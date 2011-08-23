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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;

namespace OpenNETCF.MTConnect
{
    public class DataItem
    {
        public event EventHandler<DataItemValue> ValueSet;

        private DataItemCategory? m_category;
        private ComponentBase m_component;
        private Device m_device;
        private Constraint m_constraint;

        public PropertyCollection Properties { get; private set; }

        public DataItem()
        {
            Properties = new PropertyCollection();
        }

        internal DataItem(DataItem item)
        {
            Properties = new PropertyCollection(item.Properties);
            this.Constraint = item.Constraint;
            this.Category = item.Category;
            this.Device = new Device(item.Device);
            this.Component = new Component(item.Component as Component);
        }

        internal DataItem(PropertyCollection props)
        {
            Properties = props;
        }

        public DataItem(DataItemCategory category, DataItemType dataItemType, string name, string id)
            : this(category, dataItemType.ToString(), name, id)
        {
        }

        public DataItem(DataItemCategory category, string dataItemType, string name, string id, Boolean writeable)
            : this(category, dataItemType, name, id)
        {
            Writable = writeable;
        }
       public DataItem(DataItemCategory category, string dataItemType, string name, string id)
            : this(new PropertyCollection())
        {
            OpenNETCF.Validate
                .Begin()
                .IsNotNullOrEmpty(dataItemType)
                .IsNotNullOrEmpty(name)
                .IsNotNullOrEmpty(id)
                .Check();

            this.Category = category;

            switch (category)
            {
                case DataItemCategory.Sample:
                    ValueType = typeof(double);
                    break;
                case DataItemCategory.Condition:
                    ValueType = typeof(string);
                    break;
                case DataItemCategory.Event:
                    // TODO:
                    ValueType = typeof(object);
                    break;
            }

            this.Type = dataItemType.ToUpper();
            this.Name = name;
            this.ID = id;
        }

        public ComponentBase Component
        {
            get { return m_component; }
            set 
            {
                m_component = value;
                if (m_component is Device)
                {
                    m_device = m_component as Device;
                }
                else
                {
                    m_device = (m_component as Component).Device;
                }
            }
        }

        public Device Device
        {
            get { return m_device; }
            set { m_device = value; }
        }

        public string Name
        {
            get { return Properties[CommonProperties.Name]; }
            set { SetProperty(CommonProperties.Name, value); }
        }

        public string ID
        {
            get { return Properties[CommonProperties.ID]; }
            set { SetProperty(CommonProperties.ID, value); }
        }

        public string UUID
        {
            get { return Properties[CommonProperties.UUID]; }
            set { SetProperty(CommonProperties.UUID, value); }
        }

        public string Units
        {
            get { return Properties[CommonProperties.Units]; }
            set { SetProperty(CommonProperties.Units, value); }
        }

        public string NativeUnits
        {
            get { return Properties[CommonProperties.NativeUnits]; }
            set { SetProperty(CommonProperties.NativeUnits, value); }
        }

        public string NativeScale
        {
            get { return Properties[CommonProperties.NativeScale]; }
            set { SetProperty(CommonProperties.NativeScale, value); }
        }

        public string SubType
        {
            get { return Properties[CommonProperties.SubType]; }
            set { SetProperty(CommonProperties.SubType, value); }
        }

        public string Type
        {
            get  { return Properties[CommonProperties.Type]; }
            set { SetProperty(CommonProperties.Type, value); }
        }

        public string CoordinateSystem
        {
            get { return Properties[CommonProperties.CoordinateSystem]; }
            set { SetProperty(CommonProperties.CoordinateSystem, value); }
        }

        public string SignificantDigits
        {
            get { return Properties[CommonProperties.SignificantDigits]; }
            set { SetProperty(CommonProperties.SignificantDigits, value); }
        }

        public string Source
        {
            get { return Properties[CommonProperties.Source]; }
            set { SetProperty(CommonProperties.Source, value); }
        }

        public bool Writable
        {
            get 
            {
                bool value = false;
                if (Properties.ContainsProperty(ExtendedProperties.Writable))
                {
                    try
                    {
                        value = bool.Parse(Properties[ExtendedProperties.Writable]);
                    }
                    catch { }
                }
                return value;
            }
            set  {  SetProperty(ExtendedProperties.Writable, value.ToString()); }
        }

        public Type ValueType
        {
            //get
            //{
            //    bool value = false;
            //    try
            //    {
            //        value = bool.Parse(Properties[ExtendedProperties.ValueType]);
            //    }
            //    catch { }
            //    return value;
            //}
            set 
            {
                if(value.Equals(typeof(string)))
                {
                    SetProperty(ExtendedProperties.ValueType, "string"); 
                }
                else if (value.Equals(typeof(double)))
                {
                    SetProperty(ExtendedProperties.ValueType, "double");
                }
                else if (value.Equals(typeof(int)))
                {
                    SetProperty(ExtendedProperties.ValueType, "int");
                }
                else if (value.Equals(typeof(bool)))
                {
                    SetProperty(ExtendedProperties.ValueType, "boolean");
                }
                else if (value.Equals(typeof(DateTime)))
                {
                    SetProperty(ExtendedProperties.ValueType, "dateTime");
                }
                else
                {
                    SetProperty(ExtendedProperties.ValueType, null);
                }
            }
        }

        public DataItemCategory Category
        {
            get
            {
                if (!m_category.HasValue)
                {
                    // this doesn't change, so cache it
                    var p = Properties[CommonProperties.Category];
                    m_category = (DataItemCategory)Enum.Parse(typeof(DataItemCategory), p, true);
                }

                return m_category.Value;
            }
            set 
            {
                SetProperty(CommonProperties.Category, value.ToString());
                m_category = value;
            }
        }

        internal void SetProperty(string propertyName, string value)
        {
            OpenNETCF.Validate
                .Begin()
                .IsNotNullOrEmpty(propertyName)
                .Check();

            Properties[propertyName] = value;
        }

        public Constraint Constraint
        {
            get { return m_constraint; }
            set 
            {
                // TODO: check that values are meaningful for the given DataItem (constants, ranges, etc)

                m_constraint = value;
            }
        }

        internal void Validate()
        {
            if (string.IsNullOrEmpty(ID))
            {
                throw new InvalidDataItemException(this, "DataItem must have an ID");
            }

            try
            {
                var t = this.Type;
            }
            catch (Exception ex)
            {
                throw new InvalidDataItemException(this, "Invalid DataItem type: " + ex.Message);
            }

            try
            {
                var cat = this.Category;
            }
            catch(Exception ex)
            {
                throw new InvalidDataItemException(this, "Invalid DataItem category: " + ex.Message);
            }

            if (Category == DataItemCategory.Sample)
            {
                if (string.IsNullOrEmpty(Units))
                {
                    throw new InvalidDataItemException(this, "Sample category DataItems must specify Units");
                }
            }
        }

        internal static DataItem FromXElement(XNamespace ns, XElement element)
        {
            var props = ConfigParser.GetAttributes(element);
            var item = new DataItem(props);

            return item;
        }

        public XElement AsXElement(XNamespace ns)
        {
            if (ns == null) ns = string.Empty;
            var element = new XElement(ns + NodeNames.DataItem);

            foreach (var prop in Properties)
            {
                element.AddAttributeIfHasValue(prop.Key, prop.Value);
            }

            return element;
        }

        public void SetValue(string value)
        {
            SetValue(null, value, DateTime.Now);
        }

        public void SetValue(string value, DateTime time)
        {
            SetValue(null, value, time);
        }

        public void SetValue(object parameter, string value, DateTime time)
        {
            if (this.Category == DataItemCategory.Condition)
            {
                // per spec (Part 3, 3.1.1), CONDITION category values must be Normal, Warning, Fault or Unavailable
                switch (value.ToLower())
                {
                    case "normal":
                    case "warning":
                    case "fault":
                    case "unavailable":
                        break;
                    default:
                        throw new InvalidDataItemValueException(this, value, "Condition DataItems must be 'Normal', 'Warning', 'Fault' or 'Unavailable'");
                }
            }

            // TODO: check constraints?
            // TODO: check alarms?

            ValueSet.Fire(parameter, new DataItemValue(-1, this, value, time));
        }
    }
}
