using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundIO.IO.RIFF
{
    public class ListChunk : Chunk
    {
        public List<Chunk> Chunks = new List<Chunk>();

        public Chunk GetChunk(string magic)
        {
            return Chunks.Where(x => x.Magic.Equals(magic)).FirstOrDefault();
        }
    }
}
