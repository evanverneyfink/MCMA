using System;
using System.Collections.Generic;
using System.Linq;

namespace Mcma.Core
{
    public static class UriExtensions
    {
        public static IDictionary<string, string> QueryParameters(this Uri uri)
            => uri != null && !string.IsNullOrWhiteSpace(uri.Query)
                   ? uri.Query.SplitOn("&").Select(p => p.SplitOn("=")).ToDictionary(p => p[0], p => p[1])
                   : new Dictionary<string, string>();
    }
}