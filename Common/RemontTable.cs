using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class RemontTable : TableEntity
    {
        public RemontTable() { }

        public RemontTable(int timeInMagacin, int timeOfExploatation, int timeOnRemont, int numberOfRemont)
        {
            TimeInMagacin = timeInMagacin;
            TimeOfExploatation = timeOfExploatation;
            TimeOnRemont = timeOnRemont;
            NumberOfRemont = numberOfRemont;
        }


        [DataMember]
        public int TimeInMagacin { get; set; }
        [DataMember]
        public int TimeOfExploatation { get; set; }
        [DataMember]
        public int TimeOnRemont { get; set; }
        [DataMember]
        [Key]
        public int NumberOfRemont { get; set; }
    }
}
