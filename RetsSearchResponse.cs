using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace RetsExchange
{
    public class RetsSearchResponse<RetsType>
    where RetsType : new()
    {
        public int Count { get; }
        private StringBuilder _resultContents = new StringBuilder();
        public IEnumerable<RetsType> RecordStream { get => streamResults(); }
        public char Delimiter { get; } = '\t';
        public int? TotalCount { get; }

        public RetsSearchResponse(string columns, IEnumerable<string> rawData)
        {
            _resultContents.Append($"{columns}\r\n");
            // CRLF and newlines will confuse CSVHelper because fields aren't quoted.
            foreach (var rawDatum in rawData)
            {
                var cleanDatum = rawDatum.Replace("\r\n", " ").Replace("\n", " ");
                _resultContents.Append($"{cleanDatum}\r\n");
            }

            Count = rawData.Count();
        }

        public RetsSearchResponse(string columns, IEnumerable<string> rawData, string recordCount, string delimiter) : this(columns, rawData)
        {
            if (!String.IsNullOrEmpty(recordCount))
                TotalCount = Int32.Parse(recordCount);

            if (!String.IsNullOrEmpty(delimiter))
                Delimiter = (char)Int32.Parse(delimiter);
        }

        private IEnumerable<RetsType> streamResults()
        {
            // RETS doesn't quote fields; all quotes encountered are part of the field itself.
            var csvConfig = new Configuration()
            {
                Delimiter = Delimiter.ToString(),
                IgnoreQuotes = true,
            };
            // TODO change this to using declarations when C# 8 is gold
            using (var stringReader = new StringReader(_resultContents.ToString()))
            using (var csvReader = new CsvReader(stringReader, csvConfig))
            {
                foreach (var record in csvReader.GetRecords<RetsType>())
                    yield return record;
            }
        }
    }
}
