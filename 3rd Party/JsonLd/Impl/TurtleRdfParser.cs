using System.Collections.Generic;
using JsonLD.Core;
using Newtonsoft.Json.Linq;

namespace JsonLD.Impl
{
    /// <summary>
    ///     A (probably terribly slow) Parser for turtle -&gt; the internal RdfDataset used
    ///     by JSOND-Java
    ///     TODO: this probably needs to be changed to use a proper parser/lexer
    /// </summary>
    /// <author>Tristan</author>
    public class TurtleRdfParser : IRdfParser
    {
        internal static readonly Pattern IrirefMinusContainer = Pattern.Compile("(?:(?:[^\\x00-\\x20<>\"{}|\\^`\\\\]|"
                                                                                + Core.Regex.Uchar + ")*)|" + Regex.PrefixedName);

        private static readonly Pattern PnLocalEscMatched = Pattern.Compile("[\\\\]([_~\\.\\-!$&'\\(\\)*+,;=/?#@%])"
        );

        /// <exception cref="JsonLD.Core.JsonLdError"></exception>
        public virtual RdfDataset Parse(JToken input)
        {
            if (!(input.Type == JTokenType.String))
                throw new JsonLdError(JsonLdError.Error.InvalidInput,
                                      "Invalid input; Triple RDF Parser requires a string input"
                );
            var result = new RdfDataset();
            var state = new State(this, (string)input);
            while (!string.Empty.Equals(state.line))
            {
                // check if line is a directive
                var match = Regex.Directive.Matcher(state.line);
                if (match.Find())
                {
                    if (match.Group(1) != null || match.Group(4) != null)
                    {
                        var ns = match.Group(1) != null ? match.Group(1) : match.Group(4);
                        var iri = match.Group(1) != null ? match.Group(2) : match.Group(5);
                        if (!iri.Contains(":")) iri = state.baseIri + iri;
                        iri = RdfDatasetUtils.Unescape(iri);
                        ValidateIRI(state, iri);
                        state.namespaces[ns] = iri;
                        result.SetNamespace(ns, iri);
                    }
                    else
                    {
                        var @base = match.Group(3) != null ? match.Group(3) : match.Group(6);
                        @base = RdfDatasetUtils.Unescape(@base);
                        ValidateIRI(state, @base);
                        if (!@base.Contains(":"))
                            state.baseIri = state.baseIri + @base;
                        else
                            state.baseIri = @base;
                    }

                    state.AdvanceLinePosition(match.Group(0).Length);
                    continue;
                }

                if (state.curSubject == null)
                {
                    // we need to match a subject
                    match = Regex.Subject.Matcher(state.line);
                    if (match.Find())
                    {
                        string iri;
                        if (match.Group(1) != null)
                        {
                            // matched IRI
                            iri = RdfDatasetUtils.Unescape(match.Group(1));
                            if (!iri.Contains(":")) iri = state.baseIri + iri;
                        }
                        else
                        {
                            if (match.Group(2) != null)
                            {
                                // matched NS:NAME
                                var ns = match.Group(2);
                                var name = UnescapeReserved(match.Group(3));
                                iri = state.ExpandIRI(ns, name);
                            }
                            else
                            {
                                if (match.Group(4) != null)
                                {
                                    // match ns: only
                                    iri = state.ExpandIRI(match.Group(4), string.Empty);
                                }
                                else
                                {
                                    if (match.Group(5) != null)
                                        iri = state.namer.GetName(match.Group(0).Trim());
                                    else
                                        iri = state.namer.GetName();
                                }
                            }
                        }

                        // make sure IRI still matches an IRI after escaping
                        ValidateIRI(state, iri);
                        state.curSubject = iri;
                        state.AdvanceLinePosition(match.Group(0).Length);
                    }
                    else
                    {
                        // handle blank nodes
                        if (state.line.StartsWith("["))
                        {
                            var bnode = state.namer.GetName();
                            state.AdvanceLinePosition(1);
                            state.Push();
                            state.curSubject = bnode;
                        }
                        else
                        {
                            // handle collections
                            if (state.line.StartsWith("("))
                            {
                                var bnode = state.namer.GetName();

                                // so we know we want a predicate if the collection close
                                // isn't followed by a subject end
                                state.curSubject = bnode;
                                state.AdvanceLinePosition(1);
                                state.Push();
                                state.curSubject = bnode;
                                state.curPredicate = JsonLdConsts.RdfFirst;
                            }
                            else
                            {
                                // make sure we have a subject already
                                throw new JsonLdError(JsonLdError.Error.ParseError,
                                                      "Error while parsing Turtle; missing expected subject. {line: "
                                                      + state.lineNumber + "position: " + state.linePosition + "}");
                            }
                        }
                    }
                }

                if (state.curPredicate == null)
                {
                    // match predicate
                    match = Regex.Predicate.Matcher(state.line);
                    if (match.Find())
                    {
                        var iri = string.Empty;
                        if (match.Group(1) != null)
                        {
                            // matched IRI
                            iri = RdfDatasetUtils.Unescape(match.Group(1));
                            if (!iri.Contains(":")) iri = state.baseIri + iri;
                        }
                        else
                        {
                            if (match.Group(2) != null)
                            {
                                // matched NS:NAME
                                var ns = match.Group(2);
                                var name = UnescapeReserved(match.Group(3));
                                iri = state.ExpandIRI(ns, name);
                            }
                            else
                            {
                                if (match.Group(4) != null)
                                    iri = state.ExpandIRI(match.Group(4), string.Empty);
                                else
                                    iri = JsonLdConsts.RdfType;
                            }
                        }

                        ValidateIRI(state, iri);
                        state.curPredicate = iri;
                        state.AdvanceLinePosition(match.Group(0).Length);
                    }
                    else
                    {
                        throw new JsonLdError(JsonLdError.Error.ParseError,
                                              "Error while parsing Turtle; missing expected predicate. {line: "
                                              + state.lineNumber + "position: " + state.linePosition + "}");
                    }
                }

                // expecting bnode or object
                // match BNODE values
                if (state.line.StartsWith("["))
                {
                    var bnode = state.namer.GetName();
                    result.AddTriple(state.curSubject, state.curPredicate, bnode);
                    state.AdvanceLinePosition(1);

                    // check for anonymous objects
                    if (state.line.StartsWith("]"))
                    {
                        state.AdvanceLinePosition(1);
                    }
                    else
                    {
                        // next we expect a statement or object separator
                        // otherwise we're inside the blank node
                        state.Push();
                        state.curSubject = bnode;

                        // next we expect a predicate
                        continue;
                    }
                }
                else
                {
                    // match collections
                    if (state.line.StartsWith("("))
                    {
                        state.AdvanceLinePosition(1);

                        // check for empty collection
                        if (state.line.StartsWith(")"))
                        {
                            state.AdvanceLinePosition(1);
                            result.AddTriple(state.curSubject, state.curPredicate, JsonLdConsts.RdfNil);
                        }
                        else
                        {
                            // next we expect a statement or object separator
                            // otherwise we're inside the collection
                            var bnode = state.namer.GetName();
                            result.AddTriple(state.curSubject, state.curPredicate, bnode);
                            state.Push();
                            state.curSubject = bnode;
                            state.curPredicate = JsonLdConsts.RdfFirst;
                            continue;
                        }
                    }
                    else
                    {
                        // match object
                        match = Regex.Object.Matcher(state.line);
                        if (match.Find())
                        {
                            string iri = null;
                            if (match.Group(1) != null)
                            {
                                // matched IRI
                                iri = RdfDatasetUtils.Unescape(match.Group(1));
                                if (!iri.Contains(":")) iri = state.baseIri + iri;
                            }
                            else
                            {
                                if (match.Group(2) != null)
                                {
                                    // matched NS:NAME
                                    var ns = match.Group(2);
                                    var name = UnescapeReserved(match.Group(3));
                                    iri = state.ExpandIRI(ns, name);
                                }
                                else
                                {
                                    if (match.Group(4) != null)
                                    {
                                        // matched ns:
                                        iri = state.ExpandIRI(match.Group(4), string.Empty);
                                    }
                                    else
                                    {
                                        if (match.Group(5) != null) iri = state.namer.GetName(match.Group(0).Trim());
                                    }
                                }
                            }

                            if (iri != null)
                            {
                                ValidateIRI(state, iri);

                                // we have a object
                                result.AddTriple(state.curSubject, state.curPredicate, iri);
                            }
                            else
                            {
                                // we have a literal
                                var value = match.Group(6);
                                string lang = null;
                                string datatype = null;
                                if (value != null)
                                {
                                    // we have a string literal
                                    value = UnquoteString(value);
                                    value = RdfDatasetUtils.Unescape(value);
                                    lang = match.Group(7);
                                    if (lang == null)
                                        if (match.Group(8) != null)
                                        {
                                            datatype = RdfDatasetUtils.Unescape(match.Group(8));
                                            if (!datatype.Contains(":")) datatype = state.baseIri + datatype;
                                            ValidateIRI(state, datatype);
                                        }
                                        else
                                        {
                                            if (match.Group(9) != null)
                                            {
                                                datatype = state.ExpandIRI(match.Group(9), UnescapeReserved(match.Group(10)));
                                            }
                                            else
                                            {
                                                if (match.Group(11) != null) datatype = state.ExpandIRI(match.Group(11), string.Empty);
                                            }
                                        }
                                    else
                                        datatype = JsonLdConsts.RdfLangstring;
                                }
                                else
                                {
                                    if (match.Group(12) != null)
                                    {
                                        // integer literal
                                        value = match.Group(12);
                                        datatype = JsonLdConsts.XsdDouble;
                                    }
                                    else
                                    {
                                        if (match.Group(13) != null)
                                        {
                                            // decimal literal
                                            value = match.Group(13);
                                            datatype = JsonLdConsts.XsdDecimal;
                                        }
                                        else
                                        {
                                            if (match.Group(14) != null)
                                            {
                                                // double literal
                                                value = match.Group(14);
                                                datatype = JsonLdConsts.XsdInteger;
                                            }
                                            else
                                            {
                                                if (match.Group(15) != null)
                                                {
                                                    // boolean literal
                                                    value = match.Group(15);
                                                    datatype = JsonLdConsts.XsdBoolean;
                                                }
                                            }
                                        }
                                    }
                                }

                                result.AddTriple(state.curSubject, state.curPredicate, value, datatype, lang);
                            }

                            state.AdvanceLinePosition(match.Group(0).Length);
                        }
                        else
                        {
                            throw new JsonLdError(JsonLdError.Error.ParseError,
                                                  "Error while parsing Turtle; missing expected object or blank node. {line: "
                                                  + state.lineNumber + "position: " + state.linePosition + "}");
                        }
                    }
                }

                // close collection
                var collectionClosed = false;
                while (state.line.StartsWith(")"))
                {
                    if (!JsonLdConsts.RdfFirst.Equals(state.curPredicate))
                        throw new JsonLdError(JsonLdError.Error.ParseError,
                                              "Error while parsing Turtle; unexpected ). {line: "
                                              + state.lineNumber + "position: " + state.linePosition + "}");
                    result.AddTriple(state.curSubject, JsonLdConsts.RdfRest, JsonLdConsts.RdfNil);
                    state.Pop();
                    state.AdvanceLinePosition(1);
                    collectionClosed = true;
                }

                var expectDotOrPred = false;

                // match end of bnode
                if (state.line.StartsWith("]"))
                {
                    var bnode = state.curSubject;
                    state.Pop();
                    state.AdvanceLinePosition(1);
                    if (state.curSubject == null)
                    {
                        // this is a bnode as a subject and we
                        // expect either a . or a predicate
                        state.curSubject = bnode;
                        expectDotOrPred = true;
                    }
                }

                // match list separator
                if (!expectDotOrPred && state.line.StartsWith(","))
                {
                    state.AdvanceLinePosition(1);

                    // now we expect another object/bnode
                    continue;
                }

                // match predicate end
                if (!expectDotOrPred)
                    while (state.line.StartsWith(";"))
                    {
                        state.curPredicate = null;
                        state.AdvanceLinePosition(1);

                        // now we expect another predicate, or a dot
                        expectDotOrPred = true;
                    }

                if (state.line.StartsWith("."))
                {
                    if (state.expectingBnodeClose)
                        throw new JsonLdError(JsonLdError.Error.ParseError,
                                              "Error while parsing Turtle; missing expected )\"]\". {line: "
                                              + state.lineNumber + "position: " + state.linePosition + "}");
                    state.curSubject = null;
                    state.curPredicate = null;
                    state.AdvanceLinePosition(1);

                    // this can now be the end of the document.
                    continue;
                }

                if (expectDotOrPred) continue;

                // if we're in a collection
                if (JsonLdConsts.RdfFirst.Equals(state.curPredicate))
                {
                    var bnode = state.namer.GetName();
                    result.AddTriple(state.curSubject, JsonLdConsts.RdfRest, bnode);
                    state.curSubject = bnode;
                    continue;
                }

                if (collectionClosed) continue;

                // if we get here, we're missing a close statement
                throw new JsonLdError(JsonLdError.Error.ParseError,
                                      "Error while parsing Turtle; missing expected \"]\" \",\" \";\" or \".\". {line: "
                                      + state.lineNumber + "position: " + state.linePosition + "}");
            }

            return result;
        }

        /// <exception cref="JsonLD.Core.JsonLdError"></exception>
        private void ValidateIRI(State state, string iri)
        {
            if (!IrirefMinusContainer.Matcher(iri).Matches())
                throw new JsonLdError(JsonLdError.Error.ParseError,
                                      "Error while parsing Turtle; invalid IRI after escaping. {line: "
                                      + state.lineNumber + "position: " + state.linePosition + "}");
        }

        internal static string UnescapeReserved(string str)
        {
            if (str != null)
            {
                var m = PnLocalEscMatched.Matcher(str);
                if (m.Find()) return m.ReplaceAll("$1");
            }

            return str;
        }

        private string UnquoteString(string value)
        {
            if (value.StartsWith("\"\"\"") || value.StartsWith("'''")) return JavaCompat.Substring(value, 3, value.Length - 3);

            if (value.StartsWith("\"") || value.StartsWith("'")) return JavaCompat.Substring(value, 1, value.Length - 1);
            return value;
        }

        internal class Regex
        {
            public static readonly Pattern PrefixId = Pattern.Compile("@prefix" + Core.Regex
                                                                                      .Ws1N + Core.Regex.PnameNs + Core.Regex.Ws1N + Core.Regex
                                                                                                                                         .Iriref +
                                                                      Core.Regex.Ws0N + "\\." + Core.Regex.Ws0N);

            public static readonly Pattern Base = Pattern.Compile("@base" + Core.Regex
                                                                                .Ws1N + Core.Regex.Iriref + Core.Regex.Ws0N + "\\." + Core.Regex
                                                                                                                                          .Ws0N);

            public static readonly Pattern SparqlPrefix = Pattern.Compile("[Pp][Rr][Ee][Ff][Ii][Xx]"
                                                                          + Core.Regex.Ws + Core.Regex.PnameNs + Core.Regex
                                                                                                                     .Ws + Core.Regex.Iriref +
                                                                          Core.Regex.Ws0N);

            public static readonly Pattern SparqlBase = Pattern.Compile("[Bb][Aa][Ss][Ee]" +
                                                                        Core.Regex.Ws + Core.Regex.Iriref + Core.Regex.Ws0N
            );

            public static readonly Pattern PrefixedName = Pattern.Compile("(?:" + Core.Regex
                                                                                      .PnameLn + "|" + Core.Regex.PnameNs + ")");

            public static readonly Pattern Iri = Pattern.Compile("(?:" + Core.Regex
                                                                             .Iriref + "|" + PrefixedName + ")");

            public static readonly Pattern Anon = Pattern.Compile("(?:\\[" + Core.Regex
                                                                                 .Ws + "*\\])");

            public static readonly Pattern BlankNode = Pattern.Compile(Core.Regex.BlankNodeLabel
                                                                       + "|" + Anon);

            public static readonly Pattern String = Pattern.Compile("(" + Core.Regex
                                                                              .StringLiteralLongSingleQuote + "|" + Core.Regex.StringLiteralLongQuote
                                                                    + "|" + Core.Regex.StringLiteralQuote + "|" + Core.Regex.StringLiteralSingleQuote
                                                                    + ")");

            public static readonly Pattern BooleanLiteral = Pattern.Compile("(true|false)");

            public static readonly Pattern RdfLiteral = Pattern.Compile(String + "(?:" + Core.Regex
                                                                                             .Langtag + "|\\^\\^" + Iri + ")?");

            public static readonly Pattern NumericLiteral = Pattern.Compile("(" + Core.Regex
                                                                                      .Double + ")|(" + Core.Regex.Decimal + ")|(" +
                                                                            Core.Regex.Integer
                                                                            + ")");

            public static readonly Pattern Literal = Pattern.Compile(RdfLiteral + "|" + NumericLiteral
                                                                     + "|" + BooleanLiteral);

            public static readonly Pattern Directive = Pattern.Compile("^(?:" + PrefixId + "|"
                                                                       + Base + "|" + SparqlPrefix + "|" + SparqlBase + ")");

            public static readonly Pattern Subject = Pattern.Compile("^" + Iri + "|" + BlankNode
            );

            public static readonly Pattern Predicate = Pattern.Compile("^" + Iri + "|a" + Core.Regex
                                                                                              .Ws1N);

            public static readonly Pattern Object = Pattern.Compile("^" + Iri + "|" + BlankNode
                                                                    + "|" + Literal);

            public static readonly Pattern Eoln = Pattern.Compile("(?:\r\n)|(?:\n)|(?:\r)");

            public static readonly Pattern NextEoln = Pattern.Compile("^.*(?:" + Eoln + ")" +
                                                                      Core.Regex.Ws0N);

            public static readonly Pattern CommentOrWs = Pattern.Compile("^(?:(?:[#].*(?:" +
                                                                         Eoln + ")" + Core.Regex.Ws0N + ")|(?:" + Core.Regex.Ws1N + "))"
            );

            // others
            // final public static Pattern WS_AT_LINE_START = Pattern.compile("^" +
            // WS_1_N);
            // final public static Pattern EMPTY_LINE = Pattern.compile("^" + WS +
            // "*$");
        }

        private class State
        {
            private readonly TurtleRdfParser _enclosing;

            private readonly Stack<IDictionary<string, string>> stack = new Stack<IDictionary
                <string, string>>();

            internal string baseIri = string.Empty;

            internal string curPredicate;

            internal string curSubject;

            public bool expectingBnodeClose;

            internal string line;

            internal int lineNumber;

            internal int linePosition;

            internal readonly UniqueNamer namer = new UniqueNamer("_:b");

            internal readonly IDictionary<string, string> namespaces = new Dictionary<string, string>(
            );

            /// <exception cref="JsonLD.Core.JsonLdError"></exception>
            public State(TurtleRdfParser _enclosing, string input)
            {
                this._enclosing = _enclosing;

                // int bnodes = 0;
                // {{ getName(); }}; // call
                // getName() after
                // construction to make
                // first active bnode _:b1
                line = input;
                lineNumber = 1;
                AdvanceLinePosition(0);
            }

            public virtual void Push()
            {
                stack.Push(new _Dictionary_126(this));
                expectingBnodeClose = true;
                curSubject = null;
                curPredicate = null;
            }

            public virtual void Pop()
            {
                if (stack.Count > 0)
                    foreach (var x in stack.Pop().GetEnumerableSelf())
                    {
                        curSubject = x.Key;
                        curPredicate = x.Value;
                    }

                if (stack.Count == 0) expectingBnodeClose = false;
            }

            /// <exception cref="JsonLD.Core.JsonLdError"></exception>
            private void AdvanceLineNumber()
            {
                var match = Regex.NextEoln.Matcher(line);
                if (match.Find())
                {
                    var split = match.Group(0).Split(string.Empty + Regex.Eoln);
                    lineNumber += split.Length - 1;
                    linePosition += split[split.Length - 1].Length;
                    line = JavaCompat.Substring(line, match.Group(0).Length);
                }
            }

            /// <exception cref="JsonLD.Core.JsonLdError"></exception>
            public virtual void AdvanceLinePosition(int len)
            {
                if (len > 0)
                {
                    linePosition += len;
                    line = JavaCompat.Substring(line, len);
                }

                while (!string.Empty.Equals(line))
                {
                    // clear any whitespace
                    var match = Regex.CommentOrWs.Matcher(line);
                    if (match.Find() && match.Group(0).Length > 0)
                    {
                        var eoln = Regex.Eoln.Matcher(match.Group(0));
                        var end = 0;
                        while (eoln.Find())
                        {
                            lineNumber += 1;
                            end = eoln.End();
                        }

                        linePosition = match.Group(0).Length - end;
                        line = JavaCompat.Substring(line, match.Group(0).Length);
                    }
                    else
                    {
                        break;
                    }
                }

                if (string.Empty.Equals(line) && !EndIsOK())
                    throw new JsonLdError(JsonLdError.Error.ParseError,
                                          "Error while parsing Turtle; unexpected end of input. {line: "
                                          + lineNumber + ", position:" + linePosition + "}");
            }

            private bool EndIsOK()
            {
                return curSubject == null && stack.Count == 0;
            }

            /// <exception cref="JsonLD.Core.JsonLdError"></exception>
            public virtual string ExpandIRI(string ns, string name)
            {
                if (namespaces.ContainsKey(ns))
                    return namespaces[ns] + name;
                throw new JsonLdError(JsonLdError.Error.ParseError,
                                      "No prefix found for: " + ns
                                                              + " {line: " + lineNumber + ", position:" + linePosition + "}");
            }

            private sealed class _Dictionary_126 : Dictionary<string, string>
            {
                private readonly State _enclosing;

                public _Dictionary_126(State trp)
                {
                    _enclosing = trp;
                    {
                        this[_enclosing.curSubject] = _enclosing.curPredicate;
                    }
                }
            }
        }
    }
}