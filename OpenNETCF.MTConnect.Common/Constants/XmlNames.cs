// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://mtcagent.codeplex.com
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

namespace OpenNETCF.MTConnect
{
    public static class XmlAttributeName
    {
        public const string AgentAddress = "agentAddress";
        public const string DataItemID = "dataItemId";
        public const string Timestamp = "timestamp";
        public const string Value = "value";
        public const string Device = "device";
        public const string Component = "component";
        public const string CollectorName = "collectorName";
        public const string Type = "type";
        public const string Scale = "scale";
        public const string Offset = "offset";
        public const string Start = "start";
        public const string End = "end";
        public const string BookmarkName = "bookmarkName";
        public const string Field = "field";
        public const string Name = "name";
        public const string Frequency = "frequency";
        public const string IncludeTimestamp = "includeTimestamp";
        public const string Command = "command";
        public const string ReturnType = "returnType";
        public const string MethodName = "methodName";
        public const string AdaptersPath = "adaptersPath";
        public const string NewAdaptersPath = "newAdaptersPath";
        public const string FullName = "fullName";
        public const string Assembly = "assembly";
        public const string AdapterID = "adapterID";
        public const string DeviceID = "deviceID";
        public const string AdapterType = "adapterType";
        public const string Trigger = "trigger";
        public const string State = "state";
        public const string Sequence = "sequence";
        public const string Writable = "writable";
        public const string SubType = "subtype";
        public const string Category = "category";
        public const string Qualifier = "qualifier";
        public const string NativeCode = "nativeCode";
        public const string NativeSeverity = "nativeSeverity";

        public static System.Xml.Linq.XAttribute DataItemId { get; set; }
    }

    public static class XmlNodeName
    {
        public const string DataItemHistory = "DataItemHistory";
        public const string ItemData = "ItemData";
        public const string ItemDatas = "ItemDatas";
        public const string DataItemLocation = "DataItemLocation";
        public const string CollectorData = "CollectorData";
        public const string Conversion = "Conversion";
        public const string Controllers = "Controllers";
        public const string Bookmarks = "Bookmarks";
        public const string Bookmark = "Bookmark";
        public const string Trigger = "Trigger";
        public const string Value = "Value";
        public const string Collectors = "Collectors";
        public const string Collector = "Collector";
        public const string Item = "Item";
        public const string Items = "Items";
        public const string Connections = "Connections";
        public const string Connection = "Connection";
        public const string Source = "Source";
        public const string Destinations = "Destinations";
        public const string DataItems = "DataItems";
        public const string DataItem = "DataItem";
        public const string Parameters = "Parameters";
        public const string Parameter = "Parameter";
        public const string CallMethod = "CallMethod";
        public const string ReturnValue = "ReturnValue";
        public const string Devices = "Devices";
        public const string Adapters = "Adapters";
        public const string Adapter = "Adapter";
        public const string Methods = "Methods";
        public const string Method = "Method";
        public const string VirtualAgentCommand = "VirtualAgentCommand";
        public const string ParameterData = "ParameterData";
        public const string CollectorCommand = "CollectorCommand";
        public const string CollectorHistory = "CollectorHistory";
        public const string TimeField = "TimeField";
        
    }
}
