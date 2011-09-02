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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Diagnostics;

namespace OpenNETCF.MTConnect
{
    public abstract partial class HostedAdapterBase : IHostedAdapter, IDisposable
    {
        private class MethodData
        {
            public MethodInfo MethodInfo { get; set; }
            public object Instance { get; set; }
        }

        private Dictionary<string, MethodData> m_methodDictionary;

        protected void LoadMethods()
        {
            m_methodDictionary = new Dictionary<string, MethodData>();

            // load device methods
            var deviceMethods = from m in HostedDevice.GetType().GetMethods()
                                   let ma = m.GetCustomAttributes(typeof(MTConnectMethodAttribute), true).FirstOrDefault()
                                   where ma != null
                                   select m;

            Debug.WriteLine(string.Format("Device '{0}' Methods", HostedDevice.Name));

            foreach (var method in deviceMethods)
            {
                Debug.WriteLine(" - " + method.Name);
                m_methodDictionary.Add(method.Name,
                    new MethodData
                    {
                        MethodInfo = method,
                        Instance = HostedDevice
                    }
                );
            }

            if (HostedDevice.Components != null)
            {
                foreach (var component in HostedDevice.Components)
                {
                    Debug.WriteLine(string.Format(" Component '{0}'", component.Name));

                    var componentMethods = from m in component.GetType().GetMethods()
                                           let ma = m.GetCustomAttributes(typeof(MTConnectMethodAttribute), true).FirstOrDefault()
                                           where ma != null
                                           select m;

                    foreach (var method in componentMethods)
                    {
                        Debug.WriteLine("  - " + method.Name);
                        m_methodDictionary.Add(method.Name,
                            new MethodData
                            {
                                MethodInfo = method,
                                Instance = component
                            }
                        );
                    }
                }
            }
        }

        //protected void LoadMethods_old()
        //{
        //    var allMethods = this.GetType().GetMethods();
        //    if (allMethods == null) return;
        //    m_methods = (from i in allMethods
        //                 where i.GetCustomAttributes(typeof(MTConnectMethodAttribute), false).Count() > 0
        //                 select i).ToArray();

        //    m_methodDictionary = new Dictionary<string, MethodInfo>();

        //    foreach (var method in m_methods)
        //    {

        //        object[] methodAttributes = method.GetCustomAttributes(typeof(MTConnectMethodAttribute), false);
        //        if (methodAttributes == null || methodAttributes.Length == 0) continue;
        //        m_methodDictionary.Add(method.Name, method);
        //    }
        //}

        public XElement GetMethodInfos()
        {
            if (m_methodDictionary == null || m_methodDictionary.Count == 0) return null;

            var methodsElement = new XElement("Methods");
            foreach (var method in m_methodDictionary.Values)
            {
                var methodElement = new XElement("Method",
                    new XAttribute("name", method.MethodInfo.Name),
                    new XAttribute("returnType", method.MethodInfo.ReturnType),
                    new XAttribute("adapterName", Path.GetFileNameWithoutExtension(this.GetType().Assembly.FullName))
                    );

                var parametersElement = new XElement("Parameters");

                foreach (var param in method.MethodInfo.GetParameters())
                {
                    var paramElement = new XElement("Parameter",
                        new XAttribute("name", param.Name),
                        new XAttribute("type", param.ParameterType)
                        );

                    parametersElement.Add(paramElement);
                }

                methodElement.Add(parametersElement);
                methodsElement.Add(methodElement);
            }
            return methodsElement;
        }

        public string CallMethod(string methodName, string[] parameters)
        {
            if (m_methodDictionary == null || m_methodDictionary.Count == 0) return string.Empty;
            if (!m_methodDictionary.ContainsKey(methodName)) return string.Empty;

            MethodInfo method = m_methodDictionary[methodName].MethodInfo;
            object instance = m_methodDictionary[methodName].Instance;

            ParameterInfo[] parameterInfos = method.GetParameters();
            object[] parametersArray = null;

            if (parameters == null || parameters.Length == 0)
            {
                if (parameterInfos.Length == 1)
                {
                    parametersArray = new object[] { null };
                }
            }
            else
            {
                // Get the parameters
                try
                {
                    if (parameters.Length != parameterInfos.Length)
                    {
                        // Bad Number of Arguments
                        return string.Empty;
                        //throw new ArgumentException();
                    }

                    List<object> parameterList = new List<object>();
                    int parameterCounter = 0;
                    foreach (var parameterInfo in parameterInfos)
                    {

                        // Convert each Parameter into the correct type
                        object nextParameter = null;

                        // Make sure we don't have a null
                        if (parameters[0] == null || parameterInfo.ParameterType != typeof(string) && parameters[parameterCounter] == "null")
                        {
                            return string.Empty;
                        }

                        if (parameterInfo.ParameterType == typeof(double))
                        {
                            nextParameter = double.Parse(parameters[parameterCounter]);
                        }
                        else if (parameterInfo.ParameterType == typeof(Int32))
                        {
                            nextParameter = Int32.Parse(parameters[parameterCounter]);
                        }
                        else if (parameterInfo.ParameterType == typeof(string))
                        {
                            nextParameter = parameters[parameterCounter];
                        }
                        else if (parameterInfo.ParameterType == typeof(DateTime))
                        {
                            nextParameter = DateTime.Parse(parameters[parameterCounter]);
                        }
                        else
                        {
                            // Unknown Type
                            throw new ArgumentException();
                        }
                        parameterList.Add(nextParameter);
                        parameterCounter++;

                    }
                    parametersArray = parameterList.ToArray();

                }
                catch
                {
                    throw;
                }


            }

            // Invoke the method
            try
            {
                object ret = method.Invoke(instance, parametersArray);
                if (ret == null) return string.Empty;
                else return ret.ToString();
            }
            catch
            {
                // Bad Invoke
                throw;
            }
        }
    }

}
