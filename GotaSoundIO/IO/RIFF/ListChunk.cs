using System.Collections.Generic;
using System.Linq;

namespace GotaSoundIO.IO.RIFF
{
    public class ListChunk : Chunk
    {
        public List<Chunk> Chunks = [];

        public Chunk GetChunk(string magic)
        {
            return Chunks.Where(x => x.Magic.Equals(magic)).FirstOrDefault();
        }
    }
}
