using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.Sound;

namespace GotaSoundIO.IO.RIFF
{
    public class RiffWriter : FileWriter
    {
        Stack<long> BakOffs = new Stack<long>();
        public long CurrOffset;
        private Stack<long> BlockOffs = new Stack<long>();

        #region Constructors
        public RiffWriter(Stream output)
            : base(output) { }
        #endregion
        public void InitFile(string magic)
        {
            BakOffs = new Stack<long>();
            Write("RIFF".ToCharArray());
            Write((uint)0);
            CurrOffset = CurrentOffset = FileOffset = BaseStream.Position;
            Write(magic.ToCharArray());
        }

        public new void CloseFile()
        {
            WriteOffset(FileOffset);
        }

        public void StartChunk(string blockName)
        {
            BakOffs.Push(CurrOffset);
            Write(blockName.ToCharArray());
            Write((uint)0);
            CurrentOffset = CurrOffset = BaseStream.Position;
            BlockOffs.Push(CurrentOffset);
        }

        public void StartListChunk(string blockName)
        {
            BakOffs.Push(CurrOffset);
            Write("LIST".ToCharArray());
            Write((uint)0);
            CurrentOffset = CurrOffset = BaseStream.Position;
            BlockOffs.Push(CurrentOffset);
            Write(blockName.ToCharArray());
        }

        public void EndChunk()
        {
            WriteOffset(BlockOffs.Pop());
            CurrOffset = CurrentOffset = BakOffs.Pop();
        }

        public void WriteOffset(long basePos)
        {
            long bak = Position;
            Position = basePos;
            uint size = (uint)(bak - basePos);
            Position -= 4;
            Write(size);
            Position = bak;
        }

        public void WriteWave(RiffWave r)
        {
            r.Loops = false;
            long bak = BaseStream.Position;
            Write(r.Write());
            long bak2 = BaseStream.Position;
            BaseStream.Position = bak;
            Write("LIST".ToCharArray());
            BaseStream.Position += 4;
            Write("wave".ToCharArray());
            BaseStream.Position = bak2;
        }
    }
}
