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
using System.ComponentModel;
using System.Xml.Linq;
using System.Reflection;

namespace OpenNETCF.MTConnect
{
    public class MTConnectDescriptionAttribute : Attribute
    {
        public string Text { get; set; }

        public MTConnectDescriptionAttribute(string text)
        {
            Text = text;
        }
    }

    public static class ErrorCodeExtensions
    {
        private static Dictionary<ErrorCode, string > m_descriptionCache;

        public static XAttribute ToXmlAttribute(this ErrorCode ec)
        {
            if (m_descriptionCache == null)
            {
                m_descriptionCache = new Dictionary<ErrorCode, string>();

                var type = typeof(ErrorCode);
                var fields = type.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public);

                foreach (var fi in fields)
                {
                    var value = (ErrorCode)fi.GetValue(null);

                    var attr = (from a in fi.GetCustomAttributes(true)
                                where a is MTConnectDescriptionAttribute
                                select a as MTConnectDescriptionAttribute).FirstOrDefault();

                    m_descriptionCache.Add(value, attr.Text);
                }
            }

            return new XAttribute("errorCode", m_descriptionCache[ec]);
        }
    }

    public enum ErrorCode
    {
        [MTConnectDescription("UNAUTHORIZED")]
        Unauthorized,
        [MTConnectDescription("NO_DEVICE")]
        NoDevice,
        [MTConnectDescription("OUT_OF_RANGE")]
        OutOfRange,
        [MTConnectDescription("TOO_MANY")]
        TooMany,
        [MTConnectDescription("INVALID_URI")]
        InvalidUri,
        [MTConnectDescription("INVALID_REQUEST")]
        InvalidRequest,
        [MTConnectDescription("INTERNAL_ERROR")]
        InternalError,
        [MTConnectDescription("INVALID_PATH")]
        InvalidPath,
        [MTConnectDescription("UNSUPPORTED")]
        Unsupported
    }
}
