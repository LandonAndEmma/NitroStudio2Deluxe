using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.IO.RIFF {
    public class Chunk {
        public string Magic;
        public long Pos;
        public uint Size;
    }
}
