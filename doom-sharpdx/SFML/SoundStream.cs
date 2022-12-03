using SFML.System;

namespace SFML.Audio {
    public class SoundStream {
        public SoundStatus Status = SoundStatus.Stopped;
        public void Initialize(int channels, uint sampleRate) { }
        public void Play() { }
        public void Stop() { }
        public void Dispose() { }
        protected virtual bool OnGetData(out short[] samples) { 
            samples = new short[1];
            return false;
        }
        protected virtual void OnSeek(Time timeOffset) {

        }
    }

    public enum SoundStatus {
        Playing,
        Paused,
        Stopped
    }
}
