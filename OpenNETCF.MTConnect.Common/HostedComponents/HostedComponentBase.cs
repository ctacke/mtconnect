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
    public abstract class HostedComponentBase : IHostedComponent, INotifyPropertyChanged
    {
        private List<IHostedComponent> m_components = new List<IHostedComponent>();
        private string m_id;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual string Name { get; set; }
        public virtual ConditionCollection Conditions { get; private set; }

        public HostedComponentBase()
        {
            Conditions = new ConditionCollection();
        }
        
        public virtual string ID
        {
            set { m_id = value; }
            get { return m_id ?? Name; }
        }

        public virtual ComponentType ComponentType 
        {
            get { return ComponentType.Component; } 
        }

        public virtual List<IHostedComponent> Components
        {
            get { return m_components; }
        }


        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler == null)
            {
                Debug.WriteLine("No handler for " + propertyName);
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
