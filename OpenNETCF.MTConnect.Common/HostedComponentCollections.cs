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
using System.ComponentModel;
using System.Diagnostics;

namespace OpenNETCF.MTConnect
{
    /// <summary>
    /// This is a simple, common utility Component for holding Linear and Rotary Axis subcomponents
    /// </summary>
    public class HostedAxes : HostedComponentBase
    {
        public HostedAxes()
            : this("axes", "axes")
        {
        }

        public HostedAxes(string id)
            : this("axes", null)
        {
        }

        public HostedAxes(string id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        public override ComponentType ComponentType
        {
            get { return OpenNETCF.MTConnect.ComponentType.Axes; }
        }
    }

    /// <summary>
    /// This is a simple, common utility Component for holding a variety of System-type subcomponents
    /// </summary>
    public class HostedSystems : HostedComponentBase
    {
        public HostedSystems()
            : this("systems", "systems")
        {
        }

        public HostedSystems(string id)
            : this("systems", null)
        {
        }

        public HostedSystems(string id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        public override ComponentType ComponentType
        {
            get { return OpenNETCF.MTConnect.ComponentType.Systems; }
        }
    }

    public class HostedController : HostedComponentBase
    {
        public HostedController()
            : this("controller", "controller")
        {
        }

        public HostedController(string id)
            : this(id, null)
        {
        }

        public HostedController(string id, string name)
        {
            Validate
                .Begin()
                .IsNotNullOrEmpty(id)
                .Check();

            this.Name = name;
            this.ID = id;
        }

        public override ComponentType ComponentType
        {
            get { return OpenNETCF.MTConnect.ComponentType.Controller; }
        }
    }

    public class HostedPath : HostedComponentBase
    {
        public HostedPath()
            : this("path", "path")
        {
        }

        public HostedPath(string id)
            : this(null, id)
        {
        }

        public HostedPath(string id, string name)
        {
            Validate
                .Begin()
                .IsNotNullOrEmpty(id)
                .Check();

            this.Name = name;
            this.ID = id;
        }

        public override ComponentType ComponentType
        {
            get { return OpenNETCF.MTConnect.ComponentType.Path; }
        }
    }
}
