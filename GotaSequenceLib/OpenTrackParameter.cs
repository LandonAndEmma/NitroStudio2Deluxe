using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSequenceLib
{
    public class OpenTrackParameter
    {
        public byte TrackNumber;
        public UInt24 Offset = 0;
        public SequenceCommand ReferenceCommand;

        public int Index(List<SequenceCommand> commands)
        {
            int ind = m_Index;
            if (ReferenceCommand != null)
            {
                if (ReferenceCommand.Index(commands) != -1)
                {
                    ind = ReferenceCommand.Index(commands);
                }
            }
            return ind;
        }

        public int m_Index;
        public string Label;
    }
}
