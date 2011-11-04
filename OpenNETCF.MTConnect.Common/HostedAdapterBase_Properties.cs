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
        private bool m_firstPublish = true;
        private object m_syncRoot = new object();
        private List<string> m_ignorePropertyChangeIDs = new List<string>();

        protected void LoadProperties()
        {
            lock (m_syncRoot)
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

                List<DataItemProperty> props = deviceProperties.ToList();

                foreach (var prop in deviceProperties)
                {
                    Debug.WriteLine(" - " + prop.PropertyInfo.Name);

                    // create a DataItem to export
                    var id = prop.PropertyAttribute.ID;
                    if (id.IsNullOrEmpty())
                    {
                        id = CreateDataItemID(HostedDevice.Name, null, prop.PropertyInfo.Name);
                    }

                    var dataItem = CreateDataItem(prop, id);
                    // see if it's publicly writable
                    if (prop.PropertyInfo.GetSetMethod() == null)
                    {
                        dataItem.Writable = false;
                    }

                    Device.AddDataItem(dataItem);
                    
                    // cache the propertyinfo for later use
                    var pd = new PropertyData
                    {
                        PropertyInfo = prop.PropertyInfo,
                        Instance = HostedDevice,
                    };
                    m_propertyDictionary.Add(id, pd);
                    var key = string.Format("{0}_{1}", HostedDevice.Name, prop.PropertyInfo.Name);
                    m_propertyKeyMap.Add(key, id);

                    // wire the DataItem set to the HostedProperty setter
                    if (pd.PropertyInfo.CanWrite)
                    {
                        dataItem.ValueSet += delegate(object sender, DataItemValue e)
                        {
                            // look to see if we're getting called from a propertychanged handler - if so we want to prevent reentrancy
                            lock (m_ignorePropertyChangeIDs)
                            {
                                if (m_ignorePropertyChangeIDs.Contains(id)) return;
                            }

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
                else
                {
                    Debug.WriteLine("notifier is null");
                }

                // load component properties
                if (HostedDevice.Components != null)
                {
                    foreach (var component in HostedDevice.Components)
                    {
                        var host = Device.Components[component.ID];
                        LoadComponentProperties(host, component);

                        // recurse one level only (that's all the spec allows)
                        if (component.Components != null)
                        {
                            foreach (var subcomponent in component.Components)
                            {
                                var subhost = host.Components[subcomponent.ID];
                                LoadComponentProperties(subhost, subcomponent);
                            }
                        }
                    }
                }
            }
        }
    
        void LoadComponentProperties(Component hostComponent, IHostedComponent component)
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
                // see if it's publicly writable
                if (prop.PropertyInfo.GetSetMethod() == null)
                {
                    dataItem.Writable = false;
                }

                hostComponent.AddDataItem(dataItem);

                // cache the propertyinfo for later use
                var pd = new PropertyData
                {
                    PropertyInfo = prop.PropertyInfo,
                    Instance = component
                };
                m_propertyDictionary.Add(id, pd);
                var key = string.Format("{0}_{1}", component.Name, prop.PropertyInfo.Name);
                m_propertyKeyMap.Add(key, id);

                // wire the DataItem set to the HostedProperty setter
                if (pd.PropertyInfo.CanWrite)
                {
                    dataItem.ValueSet += delegate(object sender, DataItemValue e)
                    {
                        // look to see if we're getting called from a propertychanged handler - if so we want to prevent reentrancy
                        lock (m_ignorePropertyChangeIDs)
                        {
                            if (m_ignorePropertyChangeIDs.Contains(id)) return;
                        }

                        var n = component as INotifyPropertyChanged;

                        // remove the change handler so our change doesn't reflect back to this point
                        try
                        {
                            if (n != null)
                            {
                                Monitor.Enter(n);
                            }

                            SetMTConnectPropertyFromDataItemValueChange(pd, e.Value);

                            // rewire the change handler
                            if (n != null)
                            {
                            }
                        }
                        finally
                        {
                            if (n != null) Monitor.Exit(n);
                        }
                    };
                }
            }

            var notifier = component as INotifyPropertyChanged;
            if (notifier != null)
            {
                Debug.WriteLine("Wired PropertyChanged on " + component.Name);
                notifier.PropertyChanged += PropertyChangedHandler;
            }
            else
            {
                Debug.WriteLine("notifier is null");
            }
        }

        void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            var id = GetPropertyID(sender, e.PropertyName);
            if(id != null)
            {
                // get the new property value
                if (!m_propertyDictionary.ContainsKey(id))
                {
                    // someone raised a property changed on something that isn't a DataItem so ignore it
                    return;
                }

                var propData = m_propertyDictionary[id];
                var newValue = propData.PropertyInfo.GetValue(sender, null);

                // Debug.WriteLine(string.Format("MTConnectProperty '{0}' value changed to '{1}'", e.PropertyName, newValue));
                lock (m_ignorePropertyChangeIDs)
                {
                    m_ignorePropertyChangeIDs.Add(id);
                    AgentInterface.PublishData(id, newValue);
                    m_ignorePropertyChangeIDs.Remove(id);
                }
            }
        }

        private void SetMTConnectPropertyFromDataItemValueChange(PropertyData pd, string newValue)
        {
            var type = pd.PropertyInfo.PropertyType;

            // can't parse a null/empty
            if ((type != typeof(string)) && (newValue.IsNullOrEmpty())) return;

            if (type == typeof(bool))
            {
                pd.PropertyInfo.SetValue(pd.Instance, bool.Parse(newValue), null);
            }
            else if (type == typeof(long))
            {
                pd.PropertyInfo.SetValue(pd.Instance, long.Parse(newValue), null);
            }
            else if (type == typeof(int))
            {
                pd.PropertyInfo.SetValue(pd.Instance, int.Parse(newValue), null);
            }
            else if (type == typeof(double))
            {
                // can't parse a null
                if (newValue.IsNullOrEmpty()) return;

                switch(newValue.ToLower())
                {
                    case "infinity":
                        pd.PropertyInfo.SetValue(pd.Instance, double.PositiveInfinity, null);
                        break;
                    case "nan":
                        pd.PropertyInfo.SetValue(pd.Instance, double.PositiveInfinity, null);
                        break;
                    default:
                        pd.PropertyInfo.SetValue(pd.Instance, double.Parse(newValue), null);
                        break;
                }
            }
            else if (type == typeof(DateTime))
            {
                pd.PropertyInfo.SetValue(pd.Instance, DateTime.Parse(newValue), null);
            }
            else if (type == typeof(TimeSpan))
            {
                pd.PropertyInfo.SetValue(pd.Instance, TimeSpan.Parse(newValue), null);
            }
            else if (type.IsEnum)
            {
                pd.PropertyInfo.SetValue(pd.Instance, Enum.Parse(pd.PropertyInfo.PropertyType, newValue, true), null);
            }
            else
            {
                pd.PropertyInfo.SetValue(pd.Instance, newValue, null);
            }
        }

        public string GetPropertyID(object parent, string propertyName)
        {
            string key;
            if (parent is IHostedDevice)
            {
                key = string.Format("{0}_{1}", (parent as IHostedDevice).Name, propertyName);
            }
            else if (parent is IHostedComponent)
            {
                key = string.Format("{0}_{1}", (parent as IHostedComponent).Name, propertyName);
            }
            else
            {
                return string.Empty;
            }

            if(m_propertyKeyMap.ContainsKey(key))
            {
                return m_propertyKeyMap[key];
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
            var category = property.PropertyAttribute.ItemCategory;

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

            var valueType = property.PropertyInfo.PropertyType;
            var type = property.PropertyAttribute.ItemType;
            var name = property.PropertyInfo.Name;

            var dataItem = new DataItem(category, type, name, id);
            dataItem.Writable = property.PropertyInfo.CanWrite;
            dataItem.ValueType = valueType;

            if (property.PropertyAttribute.ItemSubType != DataItemSubtype.NONE)
            {
                dataItem.SubType = property.PropertyAttribute.ItemSubType.ToString();
            }

            dataItem.UUID = property.PropertyAttribute.UUID;
            dataItem.Source = property.PropertyAttribute.Source;
            dataItem.SignificantDigits = property.PropertyAttribute.SignificantDigits;
            dataItem.NativeUnits = property.PropertyAttribute.NativeUnits;
            dataItem.NativeScale = property.PropertyAttribute.NativeScale;
            dataItem.CoordinateSystem = property.PropertyAttribute.CoordianteSystem;

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
                // get the current value
                object value = property.PropertyInfo.GetValue(property.Instance, null);
                // return if there's been no change
                if (value != null && value.ToString() == e.Value)
                {
                    return false;
                }

                if (property.PropertyInfo.PropertyType == typeof(double))
                {
                    // can't parse an empty string to a double
                    if(e.Value.IsNullOrEmpty()) return false;

                    newProperty = (object)double.Parse(e.Value);
                }
                else if (property.PropertyInfo.PropertyType == typeof(int))
                {
                    // can't parse an empty string to an int
                    if (e.Value.IsNullOrEmpty()) return false;

                    newProperty = (object)Int32.Parse(e.Value);
                }
                else if (property.PropertyInfo.PropertyType == typeof(string))
                {
                    newProperty = (object)e.Value;
                }
                else if (property.PropertyInfo.PropertyType == typeof(bool))
                {
                    // can't parse an empty string to a bool
                    if (e.Value.IsNullOrEmpty()) return false;

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
