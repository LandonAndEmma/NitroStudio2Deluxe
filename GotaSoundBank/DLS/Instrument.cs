using System.Collections.Generic;

namespace GotaSoundBank.DLS
{
    public class Instrument
    {
        public string Name = "";
        public uint BankId;
        public uint InstrumentId;
        public List<Region> Regions = [];
    }
}
