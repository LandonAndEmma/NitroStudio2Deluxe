using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GotaSoundIO.IO
{
    public class FileReader : BinaryReader
    {
        public long FileOffset;
        public long CurrentOffset;
        public Stack<long> StructureOffsets = new();
        public long[] BlockOffsets;
        public long[] BlockSizes;
        public Dictionary<string, uint> Offsets = [];
        public Dictionary<string, Reference<object>> References =
            [];

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
            get => ByteConverter.ByteOrder; set => ByteConverter = ByteConverter.GetConverter(value);
        }
        public Encoding Encoding { get; }
        public bool EndOfStream => BaseStream.IsEndOfStream();
        public long Length => BaseStream.Length;
        public long Position
        {
            get => BaseStream.Position; set => BaseStream.Position = value;
        }

        public long Align(int alignment)
        {
            return BaseStream.Align(alignment, true);
        }

        public bool ReadBoolean(BooleanDataFormat format)
        {
            return BaseStream.ReadBoolean(format);
        }

        public bool[] ReadBooleans(int count, BooleanDataFormat format = BooleanDataFormat.Byte)
        {
            return BaseStream.ReadBooleans(count, format);
        }

        public DateTime ReadDateTime(DateTimeDataFormat format = DateTimeDataFormat.NetTicks)
        {
            return BaseStream.ReadDateTime(format, ByteConverter);
        }

        public DateTime[] ReadDateTimes(
            int count,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks
        )
        {
            return BaseStream.ReadDateTimes(count, format, ByteConverter);
        }

        public override decimal ReadDecimal()
        {
            return BaseStream.ReadDecimal(ByteConverter);
        }

        public decimal[] ReadDecimals(int count)
        {
            return BaseStream.ReadDecimals(count, ByteConverter);
        }

        public override double ReadDouble()
        {
            return BaseStream.ReadDouble(ByteConverter);
        }

        public double[] ReadDoubles(int count)
        {
            return BaseStream.ReadDoubles(count, ByteConverter);
        }

        public T ReadEnum<T>(bool strict = false)
            where T : struct, IComparable, IFormattable
        {
            return BaseStream.ReadEnum<T>(strict, ByteConverter);
        }

        public T[] ReadEnums<T>(int count, bool strict = false)
            where T : struct, IComparable, IFormattable
        {
            return BaseStream.ReadEnums<T>(count, strict, ByteConverter);
        }

        public override short ReadInt16()
        {
            return BaseStream.ReadInt16(ByteConverter);
        }

        public short[] ReadInt16s(int count)
        {
            return BaseStream.ReadInt16s(count, ByteConverter);
        }

        public override int ReadInt32()
        {
            return BaseStream.ReadInt32(ByteConverter);
        }

        public int[] ReadInt32s(int count)
        {
            return BaseStream.ReadInt32s(count, ByteConverter);
        }

        public override long ReadInt64()
        {
            return BaseStream.ReadInt64(ByteConverter);
        }

        public long[] ReadInt64s(int count)
        {
            return BaseStream.ReadInt64s(count, ByteConverter);
        }

        public sbyte[] ReadSBytes(int count)
        {
            return BaseStream.ReadSBytes(count);
        }

        public override float ReadSingle()
        {
            return BaseStream.ReadSingle(ByteConverter);
        }

        public float[] ReadSingles(int count)
        {
            return BaseStream.ReadSingles(count, ByteConverter);
        }

        public string ReadString(StringDataFormat format, Encoding encoding = null)
        {
            return BaseStream.ReadString(
                format,
                encoding ?? Encoding,
                ByteConverter
            );
        }

        public string ReadString(int length, Encoding encoding = null)
        {
            return BaseStream.ReadString(length, encoding ?? Encoding);
        }

        public string[] ReadStrings(int count)
        {
            return BaseStream.ReadStrings(
                count,
                StringDataFormat.DynamicByteCount,
                null,
                null
            );
        }

        public string[] ReadStrings(int count, StringDataFormat format, Encoding encoding = null)
        {
            return BaseStream.ReadStrings(
                count,
                format,
                encoding ?? Encoding,
                ByteConverter
            );
        }

        public string[] ReadStrings(int count, int length, Encoding encoding = null)
        {
            return BaseStream.ReadStrings(count, length, encoding ?? Encoding);
        }

        public override ushort ReadUInt16()
        {
            return BaseStream.ReadUInt16(ByteConverter);
        }

        public ushort[] ReadUInt16s(int count)
        {
            return BaseStream.ReadUInt16s(count, ByteConverter);
        }

        public override uint ReadUInt32()
        {
            return BaseStream.ReadUInt32(ByteConverter);
        }

        public uint[] ReadUInt32s(int count)
        {
            return BaseStream.ReadUInt32s(count, ByteConverter);
        }

        public override ulong ReadUInt64()
        {
            return BaseStream.ReadUInt64(ByteConverter);
        }

        public ulong[] ReadUInt64s(int count)
        {
            return BaseStream.ReadUInt64s(count, ByteConverter);
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
            {
                CurrentOffset = Position;
            }

            fileHeader = Read<T>();
            BlockOffsets = fileHeader.BlockOffsets;
            BlockSizes = fileHeader.BlockSizes;
        }

        public void OpenFile(bool setOffset = true)
        {
            FileOffset = Position;
            if (setOffset)
            {
                CurrentOffset = Position;
            }
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
            FileReader r = new(BaseStream)
            {
                Position = Position
            };
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
            Position = absolute ? FileOffset + offset : CurrentOffset + offset;
        }

        public void OpenOffset(string name)
        {
            Offsets.Add(name, ReadUInt32());
        }

        public void JumpToOffset(string name, bool remove = true, bool absolute = false)
        {
            Position = absolute ? FileOffset + Offsets[name] : CurrentOffset + Offsets[name];
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
            _ = Offsets.Remove(name);
            return true;
        }

        public void CloseOffset(string name)
        {
            _ = Offsets.Remove(name);
        }

        public void OpenReference<T>(string name)
        {
            Reference<object> r = Activator.CreateInstance<T>() as Reference<object>;
            r.ReadRef(this);
            References.Add(name, r);
        }

        public void JumpToReference(string name, bool remove = true)
        {
            Position = References[name].Absolute ? FileOffset + References[name].Offset : CurrentOffset + References[name].Offset;
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
            _ = References.Remove(name);
            return true;
        }

        public int ReferenceIdentifier(string name)
        {
            return References[name].Identifier;
        }

        public void CloseReference(string name)
        {
            _ = References.Remove(name);
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
            List<bool> b = [];
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
