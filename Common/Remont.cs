using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class Remont
    {
        public Remont() { }

        public Remont(int timeInMagacin, int timeOfExploatation, int timeOnRemont, int numberOfRemont)
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
        public int NumberOfRemont { get; set; }
    }
}
