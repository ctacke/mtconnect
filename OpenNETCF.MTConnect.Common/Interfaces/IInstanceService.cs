using System;
using System.Collections.Generic;

namespace OpenNETCF.MTConnect
{
    public interface IInstanceService
    {
        List<IAdapterInstanceInfo> AdapterInstanceInfos { get; set; }
        List<string> GetAdapterInstanceNames(string name);

        bool AddAdapterInstance(string adapterName, string instanceName);
        bool RemoveAdapterInstance(string adapterName, string instanceName);

        bool Load();
        void Save();
    }
}
