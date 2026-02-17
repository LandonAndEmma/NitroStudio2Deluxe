using GotaSoundIO.IO;
using System.Collections.Generic;

namespace GotaSequenceLib
{
    public abstract class SequencePlatform
    {
        public abstract Dictionary<SequenceCommands, byte> CommandMap();
        public abstract Dictionary<SequenceCommands, byte> ExtendedCommands();
        public abstract ByteOrder SequenceDataByteOrder();
    }
}
