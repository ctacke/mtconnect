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
using System.Globalization;

namespace OpenNETCF.MTConnect
{
    public interface IDataElement
    {
        string Type { get; }
        string DataItemID { get; }
        DateTime Timestamp { get; }
        int Sequence { get; }
        bool Writable { get; }
        object Value { get; set; }
    }

    public static class IDataElementExtensions
    {
        public static double? AsNullableDouble(this IDataElement evt)
        {
            if ((evt == null) || (evt.Value == null)) return null;

            if (evt.Value is string)
            {
                if ((string)evt.Value == string.Empty) return null;

                try
                {
                    return double.Parse(evt.Value as string, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                try
                {
                    return Convert.ToDouble(evt.Value);
                }
                catch
                {
                    return null;
                }
            }

        }

        public static double AsDouble(this IDataElement evt)
        {
            if ((evt == null) || (evt.Value == null)) return 0;

            if (evt.Value is string)
            {
                if ((string)evt.Value == string.Empty) return 0;

                try
                {
                    return double.Parse(evt.Value as string, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                try
                {
                    return Convert.ToDouble(evt.Value);
                }
                catch
                {
                    return 0;
                }
            }

        }

        public static int? AsNullableInt(this IDataElement evt)
        {
            if ((evt == null) || (evt.Value == null)) return null;

            if (evt.Value is string)
            {
                if ((string)evt.Value == string.Empty) return null;

                try
                {
                    return int.Parse(evt.Value as string, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                try
                {
                    return Convert.ToInt32(evt.Value);
                }
                catch
                {
                    return null;
                }
            }

        }

        public static int AsInt(this IDataElement evt)
        {
            if ((evt == null) || (evt.Value == null)) return 0;

            if (evt.Value is string)
            {
                if ((string)evt.Value == string.Empty) return 0;

                try
                {
                    return int.Parse(evt.Value as string, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                try
                {
                    return Convert.ToInt32(evt.Value);
                }
                catch
                {
                    return 0;
                }
            }

        }

        public static bool? AsNullableBool(this IDataElement evt)
        {
            if ((evt == null) || (evt.Value == null)) return null;

            if (evt.Value is string)
            {
                if ((string)evt.Value == string.Empty) return null;

                try
                {
                    return bool.Parse(evt.Value as string);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                try
                {
                    return Convert.ToBoolean(evt.Value);
                }
                catch
                {
                    return null;
                }
            }

        }

        public static bool AsBool(this IDataElement evt)
        {
            if ((evt == null) || (evt.Value == null)) return false;

            if (evt.Value is string)
            {
                if ((string)evt.Value == string.Empty) return false;

                try
                {
                    return bool.Parse(evt.Value as string);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    return Convert.ToBoolean(evt.Value);
                }
                catch
                {
                    return false;
                }
            }

        }
    }
}
