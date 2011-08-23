using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace OpenNETCF.MTConnect
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataItemAttribute : Attribute
    {
        public DataItemAttribute(string dataItemID)
        {
            this.Category = DataItemCategory.Event;
            this.ID = dataItemID;
            this.ItemType = "HostedProperty";
        }

        public DataItemCategory Category { get; set; }
        public string ItemType { get; set; }
        public string ID { get; set; }
    }
}
