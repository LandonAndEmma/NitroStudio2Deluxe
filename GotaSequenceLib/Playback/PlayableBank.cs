using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSequenceLib.Playback
{
    public interface PlayableBank
    {
        NotePlayBackInfo GetNotePlayBackInfo(int program, Notes note, byte velocity);
    }
}
