using System;
using System.Collections.Generic;
using System.Linq;

namespace RetsExchange
{
    public class RetsSearchRequest<RetsType>
    where RetsType : new()
    {
        public CountType Count { get; set; } = CountType.NO_RECORD_COUNT;
        public FormatType Format { get; set; } = FormatType.COMPACT_DECODED;
        public string Query { get; set; }
        public SearchQueryType QueryType { get; set; } = SearchQueryType.DMQL2;
        public IEnumerable<string> Select { get; set; }
        public bool StandardNames { get; set; } = false;
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public string SearchType { get; set; }
        public string Class { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Parameters
        {
            get => buildParameterList();
        }

        public RetsSearchRequest() =>
            Select = typeof(RetsType).GetProperties().Select(p => p.Name);

        private IEnumerable<KeyValuePair<string, string>> buildParameterList()
        {
            var queryParams = new Dictionary<string, string>()
            {
                ["Count"] = Count.GetValue(),
                ["Format"] = Format.GetValue(),
                ["QueryType"] = QueryType.GetValue(),
                ["StandardNames"] = StandardNames ? "1" : "0",
                ["Class"] = Class ?? retsResource().Class,
                ["SearchType"] = SearchType ?? retsResource().SearchType,
            };

            if (!String.IsNullOrEmpty(Query))
                queryParams.Add("Query", Query);

            if (Select.Any())
                queryParams.Add("Select", String.Join(",", Select));

            if (Limit is int limitValue)
                queryParams.Add("Limit", limitValue.ToString());

            if (Offset is int offsetValue)
                queryParams.Add("Offset", offsetValue.ToString());

            return queryParams;
        }

        private RetsResource retsResource()
        {
            var attributes = Attribute.GetCustomAttributes(typeof(RetsType));
            return (RetsResource)attributes.FirstOrDefault(a => a is RetsResource);
        }
    }

    internal static class Extensions
    {
        internal static string GetValue(this FormatType formatType)
        {
            // TODO change this to switch expression when C# 8 is gold
            switch (formatType)
            {
                case FormatType.COMPACT:
                    return "COMPACT";
                case FormatType.COMPACT_DECODED:
                    return "COMPACT-DECODED";
                default:
                    throw new Exception($"Unknown FormatType encountered: {formatType}");
            }
        }

        internal static string GetValue(this SearchQueryType searchQueryType)
        {
            // TODO change this to switch expression when C# 8 is gold
            switch (searchQueryType)
            {
                case SearchQueryType.DMQL:
                    return "DMQL";
                case SearchQueryType.DMQL2:
                    return "DMQL2";
                default:
                    throw new Exception($"Unknown SearchQueryType encountered: {searchQueryType}");
            }
        }

        internal static string GetValue(this CountType countType) =>
            ((int)countType).ToString();
    }

    public enum CountType
    {
        NO_RECORD_COUNT = 0,
        RECORD_COUNT_AND_RESULTS = 1,
        RECORD_COUNT_ONLY = 2
    }

    public enum FormatType
    {
        COMPACT, COMPACT_DECODED
    }

    public enum SearchQueryType
    {
        DMQL, DMQL2
    }
}
