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

        string AgentAddress { get; }
        Agent Agent { get; }
        IHost Host { get; }
        bool Running { get; }

        void Start();
        void Stop();

        void UpdateAdapter(string requestSource, string xml);
        void SetDataByDataItemID(string requestSource, string dataItemID, string data);
    }
}
