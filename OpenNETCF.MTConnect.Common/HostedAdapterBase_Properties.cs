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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using OpenNETCF;
using System.ComponentModel;

namespace OpenNETCF.MTConnect
{
    public abstract partial class HostedAdapterBase : IHostedAdapter, IDisposable
    {
        private class PropertyData
        {
            public PropertyInfo PropertyInfo { get; set; }
            public object Instance { get; set; }
        }

        private Dictionary<string, PropertyData> m_propertyDictionary;
        private Dictionary<string, string> m_propertyKeyMap;
        bool m_firstPublish = true;

        protected void LoadProperties()
        {
            m_propertyDictionary = new Dictionary<string, PropertyData>(StringComparer.InvariantCultureIgnoreCase);
            m_propertyKeyMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // load device properties
            var deviceProperties = from p in HostedDevice.GetType().GetProperties()
                                   let pr = p.GetCustomAttributes(typeof(MTConnectPropertyAttribute), true).FirstOrDefault()
                                   where pr != null
                                   select new DataItemProperty
                                   {
                                       PropertyInfo = p,
                                       PropertyAttribute = pr as MTConnectPropertyAttribute
                                   };

            Debug.WriteLine(string.Format("Device '{0}' Properties", HostedDevice.Name));

            foreach (var prop in deviceProperties)
            {
                Debug.WriteLine(" - " + prop.PropertyInfo.Name);

                // create a DataItem to export
                var id = CreateDataItemID(HostedDevice.Name, null, prop.PropertyInfo.Name);
                var dataItem = CreateDataItem(prop, id);
                Device.AddDataItem(dataItem);

                // cache the propertyinfo for later use
                var pd = new PropertyData
                {
                     PropertyInfo = prop.PropertyInfo,
                     Instance = HostedDevice
                };
                m_propertyDictionary.Add(id, pd);
                m_propertyKeyMap.Add(prop.PropertyInfo.Name, id);

                // wire the DataItem set to the HostedProperty setter
                if (pd.PropertyInfo.CanWrite)
                {
                    dataItem.ValueSet += delegate(object sender, DataItemValue e)
                    {
                        var n = HostedDevice as INotifyPropertyChanged;
                        // remove the change handler so our change doesn't reflect back to this point
                        if (n != null)
                        {
                            n.PropertyChanged -= PropertyChangedHandler;
                        }

                        SetMTConnectPropertyFromDataItemValueChange(pd, e.Value);

                        // rewire the change handler
                        if (n != null)
                        {
                            n.PropertyChanged += PropertyChangedHandler;
                        }
                    };
                }
            }

            var notifier = HostedDevice as INotifyPropertyChanged;
            if (notifier != null)
            {
                notifier.PropertyChanged += PropertyChangedHandler;
            }

            // load component properties
            if (HostedDevice.Components != null)
            {
                foreach (var component in HostedDevice.Components)
                {
                    Debug.WriteLine(string.Format(" Component '{0}'", component.Name));

                    var componentProperties = from p in component.GetType().GetProperties()
                                              let pr = p.GetCustomAttributes(typeof(MTConnectPropertyAttribute), true).FirstOrDefault()
                                              where pr != null
                                              select new DataItemProperty
                                              {
                                                  PropertyInfo = p,
                                                  PropertyAttribute = pr as MTConnectPropertyAttribute
                                              };

                    foreach (var prop in componentProperties)
                    {
                        Debug.WriteLine("  - " + prop.PropertyInfo.Name);

                        // create a DataItem to export
                        var id = CreateDataItemID(HostedDevice.Name, component.Name, prop.PropertyInfo.Name);

                        var dataItem = CreateDataItem(prop, id);
                        Device.Components[component.Name].AddDataItem(dataItem);

                        // cache the propertyinfo for later use
                        var pd = new PropertyData
                        {
                            PropertyInfo = prop.PropertyInfo,
                            Instance = component
                        };
                        m_propertyDictionary.Add(id, pd);
                        m_propertyKeyMap.Add(prop.PropertyInfo.Name, id);

                        // wire the DataItem set to the HostedProperty setter
                        if (pd.PropertyInfo.CanWrite)
                        {
                            dataItem.ValueSet += delegate(object sender, DataItemValue e)
                            {
                                var n = component as INotifyPropertyChanged;
                                // remove the change handler so our change doesn't reflect back to this point
                                if (n != null)
                                {
                                    n.PropertyChanged -= PropertyChangedHandler;
                                }

                                SetMTConnectPropertyFromDataItemValueChange(pd, e.Value);

                                // rewire the change handler
                                if (n != null)
                                {
                                    n.PropertyChanged += PropertyChangedHandler;
                                }
                            };
                        }
                    }

                    notifier = component as INotifyPropertyChanged;
                    if (notifier != null)
                    {
                        notifier.PropertyChanged += PropertyChangedHandler;
                    }
                }
            }
        }

        void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            var id = GetPropertyID(e.PropertyName);
            if(id != null)
            {

                // get the new property value
                var propData = m_propertyDictionary[id];
                var newValue = propData.PropertyInfo.GetValue(sender, null);

                Debug.WriteLine(string.Format("MTConnectProperty '{0}' value changed to '{1}'", e.PropertyName, newValue));
                AgentInterface.PublishData(id, newValue);
            }
        }

        private void SetMTConnectPropertyFromDataItemValueChange(PropertyData pd, string newValue)
        {
            if (pd.PropertyInfo.PropertyType == typeof(bool))
            {
                pd.PropertyInfo.SetValue(pd.Instance, bool.Parse(newValue), null);
            }
            else if (pd.PropertyInfo.PropertyType == typeof(long))
            {
                pd.PropertyInfo.SetValue(pd.Instance, long.Parse(newValue), null);
            }
            else if (pd.PropertyInfo.PropertyType == typeof(int))
            {
                pd.PropertyInfo.SetValue(pd.Instance, int.Parse(newValue), null);
            }
            else if (pd.PropertyInfo.PropertyType == typeof(double))
            {
                pd.PropertyInfo.SetValue(pd.Instance, double.Parse(newValue), null);
            }
            else if (pd.PropertyInfo.PropertyType == typeof(DateTime))
            {
                pd.PropertyInfo.SetValue(pd.Instance, DateTime.Parse(newValue), null);
            }
            else if (pd.PropertyInfo.PropertyType == typeof(TimeSpan))
            {
                pd.PropertyInfo.SetValue(pd.Instance, TimeSpan.Parse(newValue), null);
            }
            else if (pd.PropertyInfo.PropertyType.IsEnum)
            {
                pd.PropertyInfo.SetValue(pd.Instance, Enum.Parse(pd.PropertyInfo.PropertyType, newValue, true), null);
            }
            else
            {
                pd.PropertyInfo.SetValue(pd.Instance, newValue, null);
            }
        }

        public string GetPropertyID(string propertyName)
        {
            if(m_propertyKeyMap.ContainsKey(propertyName))
            {
                return m_propertyKeyMap[propertyName];
            }

            return string.Empty;
        }

        private class DataItemProperty
        {
            public PropertyInfo PropertyInfo { get; set; }
            public MTConnectPropertyAttribute PropertyAttribute { get; set; }
        }

        private DataItem CreateDataItem(DataItemProperty property, string id)
        {
            var category = DataItemCategory.Event;

            // Samples *MUST* have Units
            if (!string.IsNullOrEmpty(property.PropertyAttribute.Units))
            {
                if (property.PropertyInfo.PropertyType == typeof(double) ||
                    property.PropertyInfo.PropertyType == typeof(int) ||
                    property.PropertyInfo.PropertyType == typeof(long) ||
                    property.PropertyInfo.PropertyType == typeof(short))
                {
                    category = DataItemCategory.Sample;
                }
            }

            var type = property.PropertyInfo.PropertyType.ToString();

            var name = property.PropertyInfo.Name;

            var dataItem = new DataItem(category, type, name, id);
            dataItem.Writable = property.PropertyInfo.CanWrite;

            if (category == DataItemCategory.Sample)
            {
                dataItem.Units = property.PropertyAttribute.Units;
            }

            return dataItem;
        }

        private string CreateDataItemID(string deviceName, string componentName, string dataItemName)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                return deviceName + m_separator + dataItemName;
            }
            else
            {
                return deviceName + m_separator + componentName + m_separator + dataItemName;
            }
        }

        public bool CheckForSetProperty(DataItemValue e)
        {
            // Call Methods
            if (m_propertyDictionary == null) return false;
            if (m_propertyDictionary.Count == 0) return false;
            if (!m_propertyDictionary.ContainsKey(e.Item.ID)) return false;

            var property = m_propertyDictionary[e.Item.ID];
            if (property == null) return false;
            if (!property.PropertyInfo.CanWrite) return false;
            object newProperty = null;

            try
            {
                if (e.Value == null) return false;
                object value = property.PropertyInfo.GetValue(property.Instance, null);

                if (value != null && value.ToString() == e.Value)
                {
                    return false;
                }
                if (property.PropertyInfo.PropertyType == typeof(double))
                {
                    newProperty = (object)double.Parse(e.Value);
                }
                else if (property.PropertyInfo.PropertyType == typeof(Int32))
                {
                    newProperty = (object)Int32.Parse(e.Value);
                }
                else if (property.PropertyInfo.PropertyType == typeof(string))
                {
                    newProperty = (object)e.Value;
                }
                else if (property.PropertyInfo.PropertyType == typeof(bool))
                {
                    newProperty = (object)bool.Parse(e.Value);
                }
                else
                {
                    // Unknown Type
                    return false;
                }

                property.PropertyInfo.SetValue(property.Instance, newProperty, null);

                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("HostedAdapterBase::CheckForSetProperty Exception: " + ex.Message);
                return false;
            }
        }

        public void UpdateProperties()
        {
            try
            {
                foreach (var id in m_propertyDictionary.Keys)
                {
                    var propertyInfo = m_propertyDictionary[id].PropertyInfo;

                    object value = propertyInfo.GetValue(m_propertyDictionary[id].Instance, null);
                    if (value == null)
                    {
                        AgentInterface.PublishData(id, string.Empty, !m_firstPublish, this);
                    }
                    else
                    {
                        AgentInterface.PublishData(id, value.ToString(), !m_firstPublish, this);
                    }
                }
                m_firstPublish = false;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("HostedAdapterBase::UpdateProperties Exception: " + ex.Message);
                return;
            }
        }

    }

}
