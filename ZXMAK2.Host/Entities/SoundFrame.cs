using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Host.Entities
{
    public class SoundFrame : ISoundFrame
    {
        public int SampleRate { get; private set; }
        public uint[][] Buffers { get; private set; }

        public SoundFrame(int sampleRate, uint[][] buffers)
        {
            SampleRate = sampleRate;
            Buffers = buffers;
        }
    }
}
