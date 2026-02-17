using GotaSoundIO.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GotaSoundIO
{
    public class Table<T> : IList<T>, IReadable, IWriteable
    {
        private List<T> items = [];

        public static implicit operator List<T>(Table<T> t)
        {
            return t.items;
        }

        public static explicit operator Table<T>(List<T> l)
        {
            return new Table<T>() { items = l };
        }

        public void Read(FileReader r)
        {
            uint count = r.ReadUInt32();
            if (typeof(T).Equals(typeof(ulong)))
            {
                items = r.ReadUInt64s((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(uint)))
            {
                items = r.ReadUInt32s((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(ushort)))
            {
                items = r.ReadUInt16s((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(byte)))
            {
                items = r.ReadBytes((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(long)))
            {
                items = r.ReadInt64s((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(int)))
            {
                items = r.ReadInt32s((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(short)))
            {
                items = r.ReadInt16s((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(sbyte)))
            {
                items = r.ReadSBytes((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(string)))
            {
                items = r.ReadStrings((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(char)))
            {
                items = r.ReadChars((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(float)))
            {
                items = r.ReadSingles((int)count).ConvertTo<T>().ToList();
            }
            else if (typeof(T).Equals(typeof(double)))
            {
                items = r.ReadDoubles((int)count).ConvertTo<T>().ToList();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    items.Add(r.Read<T>());
                }
            }
        }

        public void Write(FileWriter w)
        {
            w.Write((uint)items.Count);
            if (typeof(T).Equals(typeof(ulong)))
            {
                w.Write(items.ConvertTo<ulong>());
            }
            else if (typeof(T).Equals(typeof(uint)))
            {
                w.Write(items.ConvertTo<uint>());
            }
            else if (typeof(T).Equals(typeof(ushort)))
            {
                w.Write(items.ConvertTo<ushort>());
            }
            else if (typeof(T).Equals(typeof(byte)))
            {
                w.Write(items.ConvertTo<byte>().ToArray());
            }
            else if (typeof(T).Equals(typeof(long)))
            {
                w.Write(items.ConvertTo<long>());
            }
            else if (typeof(T).Equals(typeof(int)))
            {
                w.Write(items.ConvertTo<int>());
            }
            else if (typeof(T).Equals(typeof(short)))
            {
                w.Write(items.ConvertTo<short>());
            }
            else if (typeof(T).Equals(typeof(sbyte)))
            {
                w.Write(items.ConvertTo<byte>().ToArray());
            }
            else if (typeof(T).Equals(typeof(string)))
            {
                w.Write(items.ConvertTo<string>());
            }
            else if (typeof(T).Equals(typeof(char)))
            {
                w.Write(items.ConvertTo<char>().ToArray());
            }
            else if (typeof(T).Equals(typeof(float)))
            {
                w.Write(items.ConvertTo<float>());
            }
            else if (typeof(T).Equals(typeof(double)))
            {
                w.Write(items.ConvertTo<double>());
            }
            else
            {
                foreach (T i in items)
                {
                    w.Write(i as IWriteable);
                }
            }
        }

        #region ListStuff
        public T this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }
        public int Count => items.Count();
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            items.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        #endregion
    }
}
