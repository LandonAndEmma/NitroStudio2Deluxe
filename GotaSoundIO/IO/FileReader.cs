using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundIO.IO
{
    public class FileReader : BinaryReader
    {
        public long FileOffset;
        public long CurrentOffset;
        public Stack<long> StructureOffsets = new Stack<long>();
        public long[] BlockOffsets;
        public long[] BlockSizes;
        public Dictionary<string, uint> Offsets = new Dictionary<string, uint>();
        public Dictionary<string, Reference<object>> References =
            new Dictionary<string, Reference<object>>();

        #region Constructors
        public FileReader(Stream input)
            : base(input)
        {
            ByteOrder = ByteOrder.LittleEndian;
        }
        #endregion
        #region BinaryDataReader
        public ByteConverter ByteConverter { get; set; }
        public ByteOrder ByteOrder
        {
            get { return this.ByteConverter.ByteOrder; }
            set { this.ByteConverter = ByteConverter.GetConverter(value); }
        }
        public Encoding Encoding { get; }
        public bool EndOfStream
        {
            get { return this.BaseStream.IsEndOfStream(); }
        }
        public long Length
        {
            get { return this.BaseStream.Length; }
        }
        public long Position
        {
            get { return this.BaseStream.Position; }
            set { this.BaseStream.Position = value; }
        }

        public long Align(int alignment)
        {
            return this.BaseStream.Align(alignment, true);
        }

        public bool ReadBoolean(BooleanDataFormat format)
        {
            return this.BaseStream.ReadBoolean(format);
        }

        public bool[] ReadBooleans(int count, BooleanDataFormat format = BooleanDataFormat.Byte)
        {
            return this.BaseStream.ReadBooleans(count, format);
        }

        public DateTime ReadDateTime(DateTimeDataFormat format = DateTimeDataFormat.NetTicks)
        {
            return this.BaseStream.ReadDateTime(format, this.ByteConverter);
        }

        public DateTime[] ReadDateTimes(
            int count,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks
        )
        {
            return this.BaseStream.ReadDateTimes(count, format, this.ByteConverter);
        }

        public override Decimal ReadDecimal()
        {
            return this.BaseStream.ReadDecimal(this.ByteConverter);
        }

        public Decimal[] ReadDecimals(int count)
        {
            return this.BaseStream.ReadDecimals(count, this.ByteConverter);
        }

        public override double ReadDouble()
        {
            return this.BaseStream.ReadDouble(this.ByteConverter);
        }

        public double[] ReadDoubles(int count)
        {
            return this.BaseStream.ReadDoubles(count, this.ByteConverter);
        }

        public T ReadEnum<T>(bool strict = false)
            where T : struct, IComparable, IFormattable
        {
            return this.BaseStream.ReadEnum<T>(strict, this.ByteConverter);
        }

        public T[] ReadEnums<T>(int count, bool strict = false)
            where T : struct, IComparable, IFormattable
        {
            return this.BaseStream.ReadEnums<T>(count, strict, this.ByteConverter);
        }

        public override short ReadInt16()
        {
            return this.BaseStream.ReadInt16(this.ByteConverter);
        }

        public short[] ReadInt16s(int count)
        {
            return this.BaseStream.ReadInt16s(count, this.ByteConverter);
        }

        public override int ReadInt32()
        {
            return this.BaseStream.ReadInt32(this.ByteConverter);
        }

        public int[] ReadInt32s(int count)
        {
            return this.BaseStream.ReadInt32s(count, this.ByteConverter);
        }

        public override long ReadInt64()
        {
            return this.BaseStream.ReadInt64(this.ByteConverter);
        }

        public long[] ReadInt64s(int count)
        {
            return this.BaseStream.ReadInt64s(count, this.ByteConverter);
        }

        public sbyte[] ReadSBytes(int count)
        {
            return this.BaseStream.ReadSBytes(count);
        }

        public override float ReadSingle()
        {
            return this.BaseStream.ReadSingle(this.ByteConverter);
        }

        public float[] ReadSingles(int count)
        {
            return this.BaseStream.ReadSingles(count, this.ByteConverter);
        }

        public string ReadString(StringDataFormat format, Encoding encoding = null)
        {
            return this.BaseStream.ReadString(
                format,
                encoding ?? this.Encoding,
                this.ByteConverter
            );
        }

        public string ReadString(int length, Encoding encoding = null)
        {
            return this.BaseStream.ReadString(length, encoding ?? this.Encoding);
        }

        public string[] ReadStrings(int count)
        {
            return this.BaseStream.ReadStrings(
                count,
                StringDataFormat.DynamicByteCount,
                (Encoding)null,
                (ByteConverter)null
            );
        }

        public string[] ReadStrings(int count, StringDataFormat format, Encoding encoding = null)
        {
            return this.BaseStream.ReadStrings(
                count,
                format,
                encoding ?? this.Encoding,
                this.ByteConverter
            );
        }

        public string[] ReadStrings(int count, int length, Encoding encoding = null)
        {
            return this.BaseStream.ReadStrings(count, length, encoding ?? this.Encoding);
        }

        public override ushort ReadUInt16()
        {
            return this.BaseStream.ReadUInt16(this.ByteConverter);
        }

        public ushort[] ReadUInt16s(int count)
        {
            return this.BaseStream.ReadUInt16s(count, this.ByteConverter);
        }

        public override uint ReadUInt32()
        {
            return this.BaseStream.ReadUInt32(this.ByteConverter);
        }

        public uint[] ReadUInt32s(int count)
        {
            return this.BaseStream.ReadUInt32s(count, this.ByteConverter);
        }

        public override ulong ReadUInt64()
        {
            return this.BaseStream.ReadUInt64(this.ByteConverter);
        }

        public ulong[] ReadUInt64s(int count)
        {
            return this.BaseStream.ReadUInt64s(count, this.ByteConverter);
        }
        #endregion
        public T Read<T>()
        {
            if (!typeof(IReadable).IsAssignableFrom(typeof(T)))
            {
                throw new Exception(
                    "Type \"" + typeof(T).ToString() + "\" does not implement IReadable."
                );
            }
            T r = (T)Activator.CreateInstance(typeof(T));
            (r as IReadable).Read(this);
            return r;
        }

        public string ReadNullTerminated()
        {
            string s = "";
            char c = ReadChar();
            while (c != 0)
            {
                s += c;
                c = ReadChar();
            }
            return s;
        }

        public void OpenFile<T>(out FileHeader fileHeader, bool setOffset = true)
            where T : FileHeader
        {
            FileOffset = Position;
            if (setOffset)
                CurrentOffset = Position;
            fileHeader = Read<T>();
            BlockOffsets = fileHeader.BlockOffsets;
            BlockSizes = fileHeader.BlockSizes;
        }

        public void OpenFile(bool setOffset = true)
        {
            FileOffset = Position;
            if (setOffset)
                CurrentOffset = Position;
        }

        public void OpenBlock(
            int blockNum,
            out string magic,
            out uint size,
            bool readMagicAndSize = true,
            bool setOffset = true
        )
        {
            Position = FileOffset + BlockOffsets[blockNum];
            if (setOffset)
            {
                CurrentOffset = Position;
            }
            magic = "";
            size = 0;
            if (readMagicAndSize)
            {
                magic = new string(ReadChars(4));
                size = ReadUInt32();
            }
            CurrentOffset = Position;
        }

        public T ReadFile<T>()
            where T : IOFile
        {
            FileReader r = new FileReader(BaseStream);
            r.Position = Position;
            r.CurrentOffset = r.Position;
            r.FileOffset = r.Position;
            T f = r.Read<T>();
            Position = r.Position;
            return f;
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

        public void Jump(long offset, bool absolute = false)
        {
            if (absolute)
            {
                Position = FileOffset + offset;
            }
            else
            {
                Position = CurrentOffset + offset;
            }
        }

        public void OpenOffset(string name)
        {
            Offsets.Add(name, ReadUInt32());
        }

        public void JumpToOffset(string name, bool remove = true, bool absolute = false)
        {
            if (absolute)
            {
                Position = FileOffset + Offsets[name];
            }
            else
            {
                Position = CurrentOffset + Offsets[name];
            }
            if (remove)
            {
                CloseOffset(name);
            }
        }

        public bool OffsetNull(string name)
        {
            if (!Offsets[name].Equals(0xFFFFFFFF) && !Offsets[name].Equals(0))
            {
                return false;
            }
            Offsets.Remove(name);
            return true;
        }

        public void CloseOffset(string name)
        {
            Offsets.Remove(name);
        }

        public void OpenReference<T>(string name)
        {
            Reference<object> r = Activator.CreateInstance<T>() as Reference<object>;
            r.ReadRef(this);
            References.Add(name, r);
        }

        public void JumpToReference(string name, bool remove = true)
        {
            if (References[name].Absolute)
            {
                Position = FileOffset + References[name].Offset;
            }
            else
            {
                Position = CurrentOffset + References[name].Offset;
            }
            if (remove)
            {
                CloseReference(name);
            }
        }

        public bool ReferenceNull(string name)
        {
            if (!References[name].Offset.Equals(-1) && !References[name].Offset.Equals(0))
            {
                return false;
            }
            References.Remove(name);
            return true;
        }

        public int ReferenceIdentifier(string name)
        {
            return References[name].Identifier;
        }

        public void CloseReference(string name)
        {
            References.Remove(name);
        }

        public bool[] ReadBitFlags(int numBytes, int maxArraySize = 0xFFFF)
        {
            ulong flags = 0;
            switch (numBytes)
            {
                case 1:
                    flags = ReadByte();
                    break;
                case 2:
                    flags = ReadUInt16();
                    break;
                case 4:
                    flags = ReadUInt32();
                    break;
                case 8:
                    flags = ReadUInt64();
                    break;
            }
            List<bool> b = new List<bool>();
            for (int i = 0; i < Math.Min(numBytes * 8, maxArraySize); i++)
            {
                b.Add((flags & (ulong)(0b1 << i)) > 0);
            }
            return b.ToArray();
        }

        public string ReadFixedString(int size)
        {
            return new string(ReadChars(size).Where(x => x != 0).ToArray());
        }
    }
}
