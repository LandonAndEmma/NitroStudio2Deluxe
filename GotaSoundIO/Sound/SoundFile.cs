using GotaSoundIO.IO;
using System;
using System.Collections.Generic;

namespace GotaSoundIO.Sound
{
    public abstract class SoundFile : IOFile
    {
        public abstract Type[] SupportedEncodings();
        public abstract string Name();
        public abstract string[] Extensions();
        public abstract string Description();
        public abstract bool SupportsTracks();
        public abstract Type PreferredEncoding();

        public SoundFile() { }

        public SoundFile(string filePath)
            : base(filePath) { }

        public AudioData Audio = new();
        public bool Loops { get; set; }
        public uint LoopStart { get; set; }
        public uint OriginalLoopStart { get; set; }
        public uint LoopEnd { get; set; }
        public uint OriginalLoopEnd { get; set; }
        public uint SampleRate { get; set; }
        public List<TrackData> Tracks = [];

        public class TrackData
        {
            public List<int> Channels = [];
            public Dictionary<string, TrackProperty> Properties =
                [];

            public TrackData Duplicate()
            {
                TrackData t = new();
                foreach (int c in Channels)
                {
                    t.Channels.Add(c);
                }
                foreach (KeyValuePair<string, TrackProperty> p in Properties)
                {
                    t.Properties.Add(p.Key, new TrackProperty(p.Value.Type, p.Value.Data));
                }
                return t;
            }
        }

        public class TrackProperty
        {
            public object Data;
            public Type Type;

            public TrackProperty(Type type, object data)
            {
                Type = type;
                Data = data;
            }

            public T GetData<T>()
            {
                return (T)Data;
            }

            public void SetData<T>(T data)
            {
                Type = typeof(T);
                Data = data;
            }
        }

        public virtual void BeforeConversion() { }

        public virtual void AfterConversion() { }

        public void FromOtherStreamFile(SoundFile other, int targetBlockSize = -2)
        {
            Loops = other.Loops;
            LoopStart = other.LoopStart;
            LoopEnd = other.LoopEnd;
            OriginalLoopStart = other.OriginalLoopStart;
            OriginalLoopEnd = other.OriginalLoopEnd;
            SampleRate = other.SampleRate;
            Audio = other.Audio.Duplicate();
            Tracks = [];
            foreach (TrackData t in other.Tracks)
            {
                Tracks.Add(t.Duplicate());
            }
            BeforeConversion();
            if (PreferredEncoding() != null && !Audio.EncodingType.Equals(PreferredEncoding()))
            {
                Audio.Convert(
                    PreferredEncoding(),
                    targetBlockSize == -2 ? Audio.BlockSize : targetBlockSize,
                    Loops ? (int)LoopStart : -1,
                    Loops ? (int)LoopEnd : -1
                );
                AfterConversion();
                return;
            }
            Type[] e = SupportedEncodings();
            foreach (Type t in e)
            {
                if (t.Equals(other.Audio.EncodingType))
                {
                    AfterConversion();
                    return;
                }
            }
            Audio.Convert(
                SupportedEncodings()[0],
                targetBlockSize == -2 ? Audio.BlockSize : targetBlockSize,
                Loops ? (int)LoopStart : -1,
                Loops ? (int)LoopEnd : -1
            );
            AfterConversion();
        }

        public void FromOtherStreamFile(
            SoundFile other,
            Type audioEncoding,
            int targetBlockSize = -2
        )
        {
            Loops = other.Loops;
            LoopStart = other.LoopStart;
            LoopEnd = other.LoopEnd;
            OriginalLoopStart = other.OriginalLoopStart;
            OriginalLoopEnd = other.OriginalLoopEnd;
            SampleRate = other.SampleRate;
            Audio = other.Audio.Duplicate();
            Tracks = [];
            foreach (TrackData t in other.Tracks)
            {
                Tracks.Add(t.Duplicate());
            }
            BeforeConversion();
            Audio.Convert(
                audioEncoding,
                targetBlockSize == -2 ? Audio.BlockSize : targetBlockSize,
                Loops ? (int)LoopStart : -1,
                Loops ? (int)LoopEnd : -1
            );
            AfterConversion();
        }

        public T Convert<T>()
            where T : SoundFile
        {
            T ret = Activator.CreateInstance<T>();
            ret.FromOtherStreamFile(this);
            return ret;
        }

        public SoundFile Convert(Type targetType)
        {
            SoundFile ret = (SoundFile)Activator.CreateInstance(targetType);
            ret.FromOtherStreamFile(this);
            return ret;
        }

        public void AlignLoopToBlock(uint blockSamples)
        {
            uint newLoopStart = LoopStart;
            if (LoopStart % blockSamples != 0)
            {
                uint dist1 = LoopStart / blockSamples * blockSamples;
                uint dist2 = ((LoopStart / blockSamples) + 1) * blockSamples;
                bool backward = Math.Abs(dist1 - LoopStart) < Math.Abs(dist2 - LoopStart);
                if (backward || (LoopEnd + dist2) >= Audio.NumSamples)
                {
                    LoopEnd -= LoopStart - dist1;
                    LoopStart = dist1;
                }
                else
                {
                    LoopEnd += dist2 - LoopStart;
                    LoopStart = dist2;
                    if (LoopEnd > Audio.NumSamples)
                    {
                        LoopEnd = (uint)Audio.NumSamples - 1;
                    }
                }
            }
            LoopStart = newLoopStart;
        }

        public void TrimAfterLoopEnd()
        {
            if (Loops || LoopEnd != 0)
            {
                Audio.Trim((int)LoopEnd + 1);
            }
        }
    }
}
