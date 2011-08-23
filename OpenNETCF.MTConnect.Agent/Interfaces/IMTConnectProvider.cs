using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace OpenNETCF.MTConnect
{
    public interface IMTConnectProvider
    {
        event EventHandler<GenericEventArgs<StatusSummary>> StatusChanged;
        event EventHandler AdapterConfigurationChanged;

        string AgentAddress { get; }
        Agent Agent { get; }
        IHost Host { get; }
        bool Running { get; }

        void Start();
        void Stop();

        void SetAdapterConfiguration(string requestSource, string xml);
        void SetDataByDataItemID(string requestSource, string dataItemID, string data);
    }
}
