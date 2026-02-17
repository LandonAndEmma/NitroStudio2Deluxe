using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GotaSoundIO.IO
{
    public class FileWriter : BinaryWriter
    {
        private FileHeader Header;
        public long CurrentOffset;
        public long FileOffset;
        public Stack<long> StructureOffsets = new();
        public Dictionary<string, long> Offsets = [];
        public List<long> BlockOffsets = [];
        public List<long> BlockSizes = [];
        public List<long> BlockTypes = [];
        public Dictionary<string, Reference<object>> References =
            [];

        #region
        public FileWriter(Stream output)
            : base(output)
        {
            ByteOrder = ByteOrder.LittleEndian;
        }
        #endregion
        #region BinaryDataWriter
        public ByteConverter ByteConverter { get; set; }
        public ByteOrder ByteOrder
        {
            get => ByteConverter.ByteOrder; set => ByteConverter = ByteConverter.GetConverter(value);
        }
        public Encoding Encoding { get; }
        public bool EndOfStream => BaseStream.IsEndOfStream();
        public long Length
        {
            get => BaseStream.Length; set => BaseStream.SetLength(value);
        }
        public long Position
        {
            get => BaseStream.Position; set => BaseStream.Position = value;
        }

        public long Align(int alignment, bool grow = true)
        {
            return BaseStream.Align(alignment, grow);
        }

        public void Write(bool value, BooleanDataFormat format)
        {
            BaseStream.Write(value, format, ByteConverter);
        }

        public void Write(
            IEnumerable<bool> values,
            BooleanDataFormat format = BooleanDataFormat.Byte
        )
        {
            BaseStream.Write(values, format, ByteConverter);
        }

        public void Write(DateTime value, DateTimeDataFormat format = DateTimeDataFormat.NetTicks)
        {
            BaseStream.Write(value, format, ByteConverter);
        }

        public void Write(
            IEnumerable<DateTime> values,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks
        )
        {
            BaseStream.Write(values, format, ByteConverter);
        }

        public override void Write(decimal value)
        {
            BaseStream.Write(value, ByteConverter);
        }

        public void Write(IEnumerable<decimal> values)
        {
            BaseStream.Write(values, ByteConverter);
        }

        public override void Write(double value)
        {
            BaseStream.Write(value, ByteConverter);
        }

        public void Write(IEnumerable<double> values)
        {
            BaseStream.Write(values, ByteConverter);
        }

        public void WriteEnum<T>(T value, bool strict = false)
            where T : struct, IComparable, IFormattable
        {
            BaseStream.WriteEnum<T>(value, strict, ByteConverter);
        }

        public void WriteEnums<T>(IEnumerable<T> values, bool strict = false)
            where T : struct, IComparable, IFormattable
        {
            BaseStream.WriteEnums<T>(values, strict, ByteConverter);
        }

        public override void Write(short value)
        {
            BaseStream.Write(value, ByteConverter);
        }

        public void Write(IEnumerable<short> values)
        {
            BaseStream.Write(values, ByteConverter);
        }

        public override void Write(int value)
        {
            BaseStream.Write(value, ByteConverter);
        }

        public void Write(IEnumerable<int> values)
        {
            BaseStream.Write(values, ByteConverter);
        }

        public override void Write(long value)
        {
            BaseStream.Write(value, ByteConverter);
        }

        public void Write(IEnumerable<long> values)
        {
            BaseStream.Write(values, ByteConverter);
        }

        public override void Write(float value)
        {
            BaseStream.Write(value, ByteConverter);
        }

        public void Write(IEnumerable<float> values)
        {
            BaseStream.Write(values, ByteConverter);
        }

        public void Write(string value, StringDataFormat format, Encoding encoding = null)
        {
            BaseStream.Write(value, format, encoding, ByteConverter);
        }

        public void Write(
            IEnumerable<string> values,
            StringDataFormat format = StringDataFormat.DynamicByteCount,
            Encoding encoding = null
        )
        {
            BaseStream.Write(values, format, encoding, null);
        }

        public override void Write(ushort value)
        {
            BaseStream.Write(value, ByteConverter);
        }

        public void Write(IEnumerable<ushort> values)
        {
            BaseStream.Write(values, ByteConverter);
        }

        public override void Write(uint value)
        {
            BaseStream.Write(value, ByteConverter);
        }

        public void Write(IEnumerable<uint> values)
        {
            BaseStream.Write(values, ByteConverter);
        }

        public override void Write(ulong value)
        {
            BaseStream.Write(value, ByteConverter);
        }

        public void Write(IEnumerable<ulong> values)
        {
            BaseStream.Write(values, ByteConverter);
        }
        #endregion
        public void Write(IWriteable w)
        {
            w.Write(this);
        }

        public void WriteFile(IOFile f)
        {
            Write(f.Write());
        }

        public void InitFile<T>(string magic, ByteOrder byteOrder, Version version, int numBlocks)
        {
            FileOffset = Position;
            Header = (FileHeader)Activator.CreateInstance(typeof(T));
            Header.ByteOrder = ByteOrder = byteOrder;
            Header.Version = version;
            Header.Magic = magic;
            Header.BlockOffsets = new long[numBlocks];
            Header.BlockSizes = new long[numBlocks];
            Header.BlockTypes = new long[numBlocks];
            Write(Header);
            Header.HeaderSize = Position - FileOffset;
        }

        public void CloseFile()
        {
            Header.BlockOffsets = BlockOffsets.ToArray();
            Header.BlockSizes = BlockSizes.ToArray();
            Header.BlockTypes = BlockTypes.ToArray();
            Header.FileSize = Position - FileOffset;
            long bak = Position;
            Position = FileOffset;
            Write(Header);
            Position = bak;
        }

        public void InitBlock(
            string magic,
            bool writeMagicAndSize = true,
            bool setOffset = true,
            long blockType = 0
        )
        {
            BlockOffsets.Add(Position);
            BlockTypes.Add(blockType);
            if (writeMagicAndSize)
            {
                Write(magic.ToCharArray());
                Write((uint)0);
            }
            if (setOffset)
            {
                StartStructure();
            }
        }

        public void CloseBlock(bool writeBlockSize = true)
        {
            long bak = Position;
            BlockSizes.Add(Position - BlockOffsets[^1]);
            if (writeBlockSize)
            {
                Position = BlockOffsets[^1] + 4;
                Write((uint)BlockSizes[^1]);
                EndStructure();
            }
            Position = bak;
        }

        public void StartStructure()
        {
            StructureOffsets.Push(CurrentOffset);
            CurrentOffset = Position;
        }

        public void EndStructure()
        {
            CurrentOffset = StructureOffsets.Pop();
        }

        public void InitOffset(string name)
        {
            Offsets.Add(name, Position);
            Write((uint)0);
        }

        public void CloseOffset(string name, bool absolute = false, long offsetOverride = -2)
        {
            long bak = Position;
            Position = Offsets[name];
            _ = Offsets.Remove(name);
            if (offsetOverride != -2)
            {
                Write((uint)offsetOverride);
            }
            else if (absolute)
            {
                Write((uint)(bak - FileOffset));
            }
            else
            {
                Write((uint)(bak - CurrentOffset));
            }
            Position = bak;
        }

        public void InitReference<T>(string name)
        {
            long posBak = Position;
            Reference<object> r = (Reference<object>)Activator.CreateInstance(typeof(T));
            r.InitWrite(this);
            r.Absolute = false;
            r.Identifier = 0;
            r.Offset = posBak;
            r.Size = 0;
            References.Add(name, r);
        }

        public void CloseReference(
            string name,
            int identifier = 0,
            bool absolute = false,
            long offsetOverride = -2,
            long sizeOverride = -2
        )
        {
            long bak = Position;
            Position = References[name].Offset;
            References[name].Absolute = absolute;
            References[name].Identifier = identifier;
            References[name].Size = bak - Position;
            if (sizeOverride != -2)
            {
                References[name].Size = sizeOverride;
            }
            References[name].Offset = absolute ? bak - FileOffset : bak - CurrentOffset;
            if (offsetOverride != -2)
            {
                References[name].Offset = offsetOverride;
            }
            References[name].WriteRef(this, true);
            Position = bak;
            _ = References.Remove(name);
        }

        public void WriteBitFlags(bool[] flags, int numBytes)
        {
            ulong u = 0;
            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i])
                {
                    u |= (uint)(0b1 << i);
                }
            }
            switch (numBytes)
            {
                case 1:
                    Write((byte)u);
                    break;
                case 2:
                    Write((short)u);
                    break;
                case 4:
                    Write((uint)u);
                    break;
                case 8:
                    Write(u);
                    break;
            }
        }

        public void Pad(int amount)
        {
            while ((Position - FileOffset) % amount != 0)
            {
                Write((byte)0);
            }
        }

        public void WriteNullTerminated(string s)
        {
            Write(s.ToCharArray());
            Write((byte)0);
        }

        public void WriteFixedString(string s, int amount)
        {
            string str = s[..Math.Min(amount, s.Length)];
            Write(str, StringDataFormat.Raw);
            Write(new byte[amount - str.Length]);
        }
    }
}
