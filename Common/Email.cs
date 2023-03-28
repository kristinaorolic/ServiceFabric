using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Email : TableEntity
    {
        public string Sender { get; set; }
        public string Contents { get; set; }
        public bool Successful { get; set; }
        public Email(string id)
        {
            PartitionKey = "email";
            RowKey = id;
        }
        public Email() { }
    }
}
