using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundIO.IO.RIFF
{
    public class RiffReader : FileReader
    {
        public string Magic { get; private set; }
        public List<Chunk> Chunks = new List<Chunk>();

        #region Constructors
        public RiffReader(Stream input)
            : base(input)
        {
            ReadData();
        }
        #endregion
        private void ReadData()
        {
            if (!new string(ReadChars(4)).Equals("RIFF")) { }
            ReadUInt32();
            Magic = new string(ReadChars(4));
            while (BaseStream.Position < BaseStream.Length)
            {
                Chunks.Add(ReadChunk());
            }
        }

        private Chunk ReadChunk()
        {
            string magic = new string(ReadChars(4));
            uint size = ReadUInt32();
            long bak = BaseStream.Position;
            if (size == 0)
            {
                size = (uint)(Length - bak);
            }
            if (magic.Equals("LIST"))
            {
                ListChunk l = new ListChunk();
                l.Magic = new string(ReadChars(4));
                l.Pos = BaseStream.Position;
                l.Size = size - 4;
                while (BaseStream.Position < bak + size)
                {
                    l.Chunks.Add(ReadChunk());
                }
                return l;
            }
            else
            {
                var c = new Chunk()
                {
                    Magic = magic,
                    Pos = BaseStream.Position,
                    Size = size,
                };
                ReadBytes((int)size);
                return c;
            }
        }

        public Chunk GetChunk(string magic)
        {
            return Chunks.Where(x => x.Magic.Equals(magic)).FirstOrDefault();
        }

        public void OpenChunk(Chunk c)
        {
            BaseStream.Position = c.Pos;
        }
    }
}
