namespace Sanford.Multimedia.Midi.Core.Sanford.Collections.Generic.UndoableList
{
    internal interface ICommand
    {
        void Execute();
        void Undo();
    }
}
