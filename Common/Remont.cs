using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Remont : TableEntity
    {
        public Remont() { }

        public Remont(int timeInMagacin, int timeOfExploatation, int timeOnRemont, string numberOfRemont)
        {
            TimeInMagacin = timeInMagacin;
            TimeOfExploatation = timeOfExploatation;
            TimeOnRemont = timeOnRemont;
            NumberOfRemont = numberOfRemont;
            PartitionKey = "remont";
            RowKey = numberOfRemont;
        }

        public int TimeInMagacin { get; set; }
        public int TimeOfExploatation { get; set; }
        public int TimeOnRemont { get; set; }
        public string NumberOfRemont { get; set; }
    }
}
