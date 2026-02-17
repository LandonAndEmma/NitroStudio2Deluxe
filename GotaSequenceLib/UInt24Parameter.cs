using System.Collections.Generic;

namespace GotaSequenceLib
{
    public class UInt24Parameter
    {
        public UInt24 Offset = 0;
        public SequenceCommand ReferenceCommand;

        public int Index(List<SequenceCommand> commands)
        {
            return ReferenceCommand == null ? m_Index : ReferenceCommand.Index(commands);
        }

        public int m_Index;
        public string Label;
    }
}
