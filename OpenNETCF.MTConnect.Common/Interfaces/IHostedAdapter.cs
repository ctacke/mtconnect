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
using System.Xml.Linq;
using System.Reflection;

namespace OpenNETCF.MTConnect
{
    public interface IHostedAdapter : IAdapter, IDisposable
    {
        string ID { get; set; }
        string AssemblyName { get; }

        IHostedDevice HostedDevice { get; }

        IAgentInterface AgentInterface { get; set; }

        void CreateDeviceAndComponents();
        void BeforeLoad();
        XElement GetMethodInfos();

        void LoadPropertiesAndMethods();
        void StartPublishing();

        /// <summary>
        /// Called after <i>this instance</i> of an IHostedAdapter has been loaded
        /// </summary>
        void AfterLoad();

        void OnError(Exception exception);
        void OnConfigurationChange();
        
        /// <summary>
        /// Called after <i>all</i> IHostedAdapters have been loaded
        /// </summary>
        void AgentInitialized();

        string CallMethod(string methodName, string[] parameters);
        void UpdateProperties();
    }
}
