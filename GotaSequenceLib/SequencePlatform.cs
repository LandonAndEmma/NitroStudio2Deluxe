using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace GotaSequenceLib
{
    public abstract class SequencePlatform
    {
        public abstract Dictionary<SequenceCommands, byte> CommandMap();
        public abstract Dictionary<SequenceCommands, byte> ExtendedCommands();
        public abstract ByteOrder SequenceDataByteOrder();
    }
}
