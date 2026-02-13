using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundBank.DLS {
    public class Instrument {
        public string Name = "";
        public uint BankId;
        public uint InstrumentId;
        public List<Region> Regions = new List<Region>();
    }
}
