using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    //[DataContract]
    public class Device : TableEntity
    {
        public Device() { }

        public Device(string id, string name, bool isOnRemont)
        {
            Id = id;
            Name = name;
            IsOnRemont = isOnRemont;
            PartitionKey = "device";
            RowKey = id;
        }

        //[DataMember]
        public string Id { get; set; }
        //[DataMember]
        public string Name { get; set; }
        //[DataMember]
        public bool IsOnRemont { get; set; }
    }
}
