using System;
using System.Linq;
using NAudio.Dsp;
using NAudio.Wave;

namespace GuitarTuner.Services
{
    public class FrequencyDetector
    {
        public double DetectFrequency(string filePath)
        {
            using (var reader = new AudioFileReader(filePath))
            {
                var sampleBuffer = new float[reader.WaveFormat.SampleRate];
                int bytesRead = reader.Read(sampleBuffer, 0, sampleBuffer.Length);

                if (bytesRead == 0)
                    return 0;

                return PerformFFT(sampleBuffer);
            }
        }

        private double PerformFFT(float[] audioSamples)
        {
            int fftSize = 1024;
            Complex[] fftBuffer = new Complex[fftSize];

            for (int i = 0; i < fftSize; i++)
            {
                fftBuffer[i].X = (i < audioSamples.Length) ? (float)(audioSamples[i] * FastFourierTransform.HammingWindow(i, fftSize)) : 0;
                fftBuffer[i].Y = 0;
            }

            FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2), fftBuffer);

            int maxIndex = 0;
            double maxMagnitude = 0;

            for (int i = 0; i < fftSize / 2; i++)
            {
                double magnitude = Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
                if (magnitude > maxMagnitude)
                {
                    maxMagnitude = magnitude;
                    maxIndex = i;
                }
            }

            double frequencyResolution = 44100.0 / fftSize;
            return maxIndex * frequencyResolution;
        }

        public string GetNearestNote(double frequency)
        {
            (string Note, double Frequency)[] notes = {
                ("E2", 82.41), ("A2", 110.00), ("D3", 146.83),
                ("G3", 196.00), ("B3", 246.94), ("E4", 329.63)
            };

            var closestNote = notes.OrderBy(n => Math.Abs(n.Frequency - frequency)).First();
            return closestNote.Note;
        }

        public double GetStandardFrequency(string note)
        {
            return note switch
            {
                "E2" => 82.41,
                "A2" => 110.00,
                "D3" => 146.83,
                "G3" => 196.00,
                "B3" => 246.94,
                "E4" => 329.63,
                _ => 0
            };
        }
    }
}
