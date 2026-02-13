using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.IO {
    public abstract class Reference<T> : IReadable, IWriteable {
        public T Data;
        public static implicit operator T(Reference<T> r) => r.Data;
        public virtual List<Type> DataTypes => new List<Type>();
        public abstract bool SetCurrentOffsetOnJump();
        public abstract bool NullReferenceIs0();
        public bool Absolute;
        public int Identifier;
        public long Offset = -1;
        public long Size = -1;
        private long ReferencePosition;
        public abstract void ReadRef(FileReader r);
        public abstract void WriteRef(FileWriter w, bool ignoreNullData = false);
        public void Read(FileReader r) {
            ReadRef(r);
            ReadData(r);
        }
        public void ReadData(FileReader r) {
            long bak = r.Position;
            if (Offset != 0 && Offset != -1) {
                r.Position = (Absolute ? 0 : r.CurrentOffset) + Offset;
                if (SetCurrentOffsetOnJump()) {
                    r.StructureOffsets.Push(r.CurrentOffset);
                    r.CurrentOffset = r.Position;
                }
                T obj;
                if (DataTypes.Count > 0) {
                    Type type = Identifier < DataTypes.Count ? DataTypes[Identifier] : null;
                    if (type != null) {
                        obj = (T)Activator.CreateInstance(type);
                    } else {
                        obj = default(T);
                    }
                } else {
                    obj = Activator.CreateInstance<T>();
                }
                if (obj != null) {
                    if (obj as IOFile != null) {
                        FileReader r2 = new FileReader(r.BaseStream);
                        r2.Position = r.Position;
                        ((IReadable)obj).Read(r2);
                        r.Position = r2.Position;
                    } else {
                        ((IReadable)obj).Read(r);
                    }
                }
                if (SetCurrentOffsetOnJump()) {
                    r.CurrentOffset = r.StructureOffsets.Pop();
                }
                r.Position = bak;
                Data = obj;
            }
            else {
                Data = default(T);
            }
        }
        public void InitWrite(FileWriter w) {
            ReferencePosition = w.Position;
            Offset = NullReferenceIs0() ? 0 : -1;
            Size = -1;
            WriteRef(w);
        }
        public void WriteData(FileWriter w) {
            long bak = w.Position;
            if (Data != null) {
                Offset = bak - w.CurrentOffset;
                if (SetCurrentOffsetOnJump()) {
                    w.StructureOffsets.Push(w.CurrentOffset);
                    w.CurrentOffset = w.Position;
                }
                if (Data as byte[] != null) {
                    w.Write(Data as byte[]);
                } else {
                    w.Write((IWriteable)Data);
                }
                Size = w.Position - bak;
                if (SetCurrentOffsetOnJump()) {
                    w.CurrentOffset = w.StructureOffsets.Pop();
                }
                if (DataTypes.Count > 0) {
                    for (int i = 0; i < DataTypes.Count; i++) {
                        try {
                            var h = Convert.ChangeType(Data, DataTypes[i]);
                            if (h != null) {
                                Identifier = i;
                            }
                        } catch { }
                    }
                }
            } else if (DataTypes.Count > 0) {
                Identifier = 0;
            }
            bak = w.Position;
            w.Position = ReferencePosition;
            WriteRef(w);
            w.Position = bak;
        }
        public void CloseReference(FileWriter w) {
            long bak = w.Position;
            Offset = bak - w.CurrentOffset;
            w.Position = ReferencePosition;
            WriteRef(w, true);
            w.Position = bak;
        }
        public void Write(FileWriter w) {
            InitWrite(w);
            WriteData(w);
        }
    }
}
