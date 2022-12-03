using System.Numerics;

namespace SFML.Audio {
    public class Sound {
        public SoundStatus Status = SoundStatus.Stopped;
        public SoundBuffer SoundBuffer = new SoundBuffer(null, 0, 0);
        public float Pitch = 1;
        public float Volume = 1;
        public float PlayingOffset = 1;
        public Vector3 Position = Vector3.Zero;


        public void Play() { }
        public void Stop() { }
        public void Pause() { }
        public void Dispose() { }
    }
}
