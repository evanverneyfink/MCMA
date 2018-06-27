using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JsonLD.Core
{
    public class UniqueNamer
    {
        private readonly string prefix;

        private int counter;

        private JObject existing;

        /// <summary>Creates a new UniqueNamer.</summary>
        /// <remarks>
        ///     Creates a new UniqueNamer. A UniqueNamer issues unique names, keeping
        ///     track of any previously issued names.
        /// </remarks>
        /// <param name="prefix">
        ///     the prefix to use ('
        ///     <prefix>
        ///         <counter>').
        /// </param>
        public UniqueNamer(string prefix)
        {
            this.prefix = prefix;
            counter = 0;
            existing = new JObject();
        }

        /// <summary>Copies this UniqueNamer.</summary>
        /// <remarks>Copies this UniqueNamer.</remarks>
        /// <returns>a copy of this UniqueNamer.</returns>
        public virtual UniqueNamer Clone()
        {
            var copy = new UniqueNamer(prefix);
            copy.counter = counter;
            copy.existing = (JObject)JsonLdUtils.Clone(existing);
            return copy;
        }

        /// <summary>
        ///     Gets the new name for the given old name, where if no old name is given a
        ///     new name will be generated.
        /// </summary>
        /// <remarks>
        ///     Gets the new name for the given old name, where if no old name is given a
        ///     new name will be generated.
        /// </remarks>
        /// <?></?>
        /// <returns>the new name.</returns>
        public virtual string GetName(string oldName)
        {
            if (oldName != null && ((IDictionary<string, JToken>)existing).ContainsKey(oldName)) return (string)existing[oldName];
            var name = prefix + counter;
            counter++;
            if (oldName != null) existing[oldName] = name;
            return name;
        }

        public virtual string GetName()
        {
            return GetName(null);
        }

        public virtual bool IsNamed(string oldName)
        {
            return ((IDictionary<string, JToken>)existing).ContainsKey(oldName);
        }

        public virtual JObject Existing()
        {
            return existing;
        }
    }
}