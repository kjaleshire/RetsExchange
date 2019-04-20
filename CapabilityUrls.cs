using System;
using System.Collections.Generic;

namespace RetsExchange
{
    internal class CapabilityUrls
    {
        internal Uri GetObjectPath { get; }
        internal Uri LoginPath { get; }
        internal Uri LogoutPath { get; }
        internal Uri SearchPath { get; }
        internal Uri GetMetadataPath { get; }
        internal Uri UpdatePath { get; }
        internal Uri PostObjectPath { get; }

        internal CapabilityUrls(Uri baseUrl, string urlStrings)
        {
            var urlEntries = urlStrings.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            foreach (var urlEntry in urlEntries)
            {
                var urlEntryArray = urlEntry.Split('=');
                var pathName = urlEntryArray[0];
                var urlString = urlEntryArray[1];
                if (!Uri.IsWellFormedUriString(urlString, UriKind.Absolute))
                    continue;
                var url = new Uri(urlString);
                var path = baseUrl.MakeRelativeUri(url);

                switch (pathName)
                {
                    case "Login":
                        LoginPath = path;
                        break;
                    case "Logout":
                        LogoutPath = path;
                        break;
                    case "Search":
                        SearchPath = path;
                        break;
                    case "GetMetadata":
                        GetMetadataPath = path;
                        break;
                    case "GetObject":
                        GetObjectPath = path;
                        break;
                    case "Update":
                        UpdatePath = path;
                        break;
                    case "PostObject":
                        PostObjectPath = path;
                        break;
                }
            }
        }
    }

}
