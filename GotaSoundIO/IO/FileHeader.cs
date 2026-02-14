using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundIO.IO
{
    public abstract class FileHeader : IReadable, IWriteable
    {
        public string Magic;
        public ByteOrder ByteOrder;
        public Version Version;
        public long[] BlockTypes;
        public long[] BlockOffsets;
        public long[] BlockSizes;
        public long FileSize;
        public long HeaderSize;
        public abstract void Read(FileReader r);
        public abstract void Write(FileWriter w);
    }
}
