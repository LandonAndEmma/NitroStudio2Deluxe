namespace GotaSequenceLib.Playback
{
    public interface PlayableBank
    {
        NotePlayBackInfo GetNotePlayBackInfo(int program, Notes note, byte velocity);
    }
}
