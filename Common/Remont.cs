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

        public Remont(int timeInMagacin, DateTime timeOfExploatation, int timeOnRemont, string numberOfRemont, string idOfDevice)
        {
            TimeInMagacin = timeInMagacin;
            TimeOfExploatation = timeOfExploatation;
            TimeOnRemont = timeOnRemont;
            NumberOfRemont = numberOfRemont;
            IdOfDevice = idOfDevice;
            PartitionKey = "remont";
            RowKey = numberOfRemont;
        }

        public int TimeInMagacin { get; set; }
        public DateTime TimeOfExploatation { get; set; }
        public int TimeOnRemont { get; set; }
        public string NumberOfRemont { get; set; }
        public string IdOfDevice { get; set; }
    }
}
