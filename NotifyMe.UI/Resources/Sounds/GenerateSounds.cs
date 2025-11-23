using System;
using System.IO;

class Program
{
    static void Generate()
    {
        // Create simple WAV files with beep sounds
        CreateBeepWav("disconnect.wav", 400, 0.3); // Low beep for disconnect
        CreateBeepWav("reconnect.wav", 800, 0.2);  // High beep for reconnect
    }

    static void CreateBeepWav(string filename, double frequency, double duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        
        using (var fs = new FileStream(filename, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            // WAV header
            bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + samples * 2);
            bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
            bw.Write(new char[4] { 'f', 'm', 't', ' ' });
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)1);
            bw.Write(sampleRate);
            bw.Write(sampleRate * 2);
            bw.Write((short)2);
            bw.Write((short)16);
            bw.Write(new char[4] { 'd', 'a', 't', 'a' });
            bw.Write(samples * 2);

            // Generate sine wave
            for (int i = 0; i < samples; i++)
            {
                double t = i / (double)sampleRate;
                double value = Math.Sin(2 * Math.PI * frequency * t);
                short sample = (short)(value * short.MaxValue * 0.3);
                bw.Write(sample);
            }
        }
    }
}
