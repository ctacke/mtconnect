// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://mtconnect.codeplex.com
//
// Copyright (c) 2010 OpenNETCF Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// -------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.Net.MTConnect
{
    internal static class RequestParser
    {
        public static IResourceRequest ParseRequest(Uri requestUri)
        {
            return ParseRequest(requestUri, "/");
        }

        public static IResourceRequest ParseRequest(Uri requestUri, string rootPath)
        {
            if (!rootPath.EndsWith("/"))
            {
                rootPath += "/";
            }

            // determine the resource name (device or "all")
            var pathSegments = GetSegments(rootPath);
            int s = 0;

            while ((s < pathSegments.Length) &&
                (pathSegments[s].ToLower() == requestUri.Segments[s].ToLower()))
            {
                s++;
            }

            string resourceName;

            if (requestUri.Segments.Length == s + 1)
            {
                resourceName = "*";
            }
            else if (requestUri.Segments.Length == s + 2)
            {
                resourceName = requestUri.Segments[s];
            }
            else
            {
                // bad request
                string badPath = string.Empty;

                for (int i = s; i < requestUri.Segments.Length; i++)
                {
                    badPath += requestUri.Segments[i];
                }

                    throw new InvalidRequestPathException(string.Format("Invalid request path '{0}'", badPath));
            }

            IResourceRequest request;

            // validate that it's a supported request
            try
            {
                request = GenerateRequest(requestUri.Segments.Last(), resourceName);
            }
            catch
            {
                // TODO return a valid MTConnect error
                throw;
            }

            // parse out parameters
            request.SetParameters( Uri.UnescapeDataString(requestUri.Query));

            return request;
        }

        private static IResourceRequest GenerateRequest(string typeString, string resourceName)
        {
            switch (typeString.ToLower())
            {
                case "probe":
                    return new ProbeRequest() { ResourceName = resourceName.Replace("/", string.Empty) };
                case "current":
                    return new CurrentRequest() { ResourceName = resourceName.Replace("/", string.Empty) };
                case "sample":
                    return new SampleRequest() { ResourceName = resourceName.Replace("/", string.Empty) };
                default:
                    throw new InvalidRequestCommandException(string.Format("Unknown command '{0}'", typeString));
            }
        }

        private static string[] GetSegments(string path)
        {
            var list = new List<string>();

            int index = path.IndexOf('/');
            int start = 0;

            while ((index >= 0) && ((start + index) < path.Length))
            {
                list.Add(path.Substring(start, index - start + 1));
                start = index + 1;
                index = path.IndexOf('/', start);
            }

            if (start < path.Length)
            {
                list.Add(path.Substring(start));
            }

            return list.ToArray();
        }
    }
}
