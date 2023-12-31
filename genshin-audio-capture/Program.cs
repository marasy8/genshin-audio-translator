// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

using System;
using NAudio.Wave;

class Program
{
    static Model InitializeVoskModel(string modelPath)
    {
        // Initialize Vosk Model
        Vosk.Vosk.SetLogLevel(0); // set logs for warnings
        return model = new Model("./vosk-model-small-ja-0.22");
    }
    static void Main(string[] args)
    {
        Console.WriteLine("Starting audio capture...");
        using (var capture = new WasapiLoopbackCapture())
        {
            // Event handler for when data is available
            capture.DataAvailable += (s, a) => 
            {
                // For testing, print out the length of the captured data
                // Console.WriteLine($"Captured {a.BytesRecorded} bytes of data.");

                // Console.WriteLine($"Audio Format: {capture.WaveFormat}");
                
                // Convert to format usable by Vosk Model
                using (var ms = new MemoryStream(a.Buffer))
                {
                    using (var rdr = new RawSourceWaveStream(ms, capture.WaveFormat))
                    {
                        var newFormat = new WaveFormat(16000, 1); // 16kHz mono
                        using (var resampler = new MediaFoundationResampler(rdr, newFormat))
                        {
                            
                        }
                    }
                }

            };

            // Start capturing
            capture.StartRecording();

            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // Stop capturing
            capture.StopRecording();
        }
        Console.WriteLine("Audio capture stopped.");
    }
}