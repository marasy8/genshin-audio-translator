// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

using System;
using System.IO;
using NAudio.Wave;
using Vosk;

class Program
{
    // // function for handling speech recognition
    // static void RecognizeSpeech(Model model, byte[] audioData, int sampleRate)
    // {
    //     using (var recognizer = new VoskRecognizer(model, sampleRate))
    //     {
    //         if (recognizer.AcceptWaveform(audioData, audioData.Length))
    //         {
    //             string result = recognizer.Result();
    //             Console.WriteLine(result); // Print the recognized text
    //         }
    //         else
    //         {
    //             string partial = recognizer.PartialResult();
    //             Console.WriteLine(partial); // Print partial results
    //         }

    //         string finalResult = recognizer.FinalResult();
    //         Console.WriteLine(finalResult);
    //     }
    // }

    // Vosk Demo Byte Buffer

    public static void DemoBytes(Model, model)
    {
        VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
        rec.setMaxAlternatives(0);
        rec.setWords(true);
        
        using(Stream source = File.OpenRead("test.wav")) {
            // change the file to be read 
            byte[] buffer = new byte[4096];
            int bytesRead;
            while((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0) {
                if (rec.AcceptWaveform(buffer, bytesRead)) {
                    Console.WriteLine(rec.Result());
                } else {
                    Console.WriteLine(rec.PartialResult());
                }
            }
        }
        Console.WriteLine(rec.FinalResult());
    }

    // function to convert the audio data into the right format
    static byte[] ProcessedAudioData(byte[] buffer, WaveFormat format)
    {
        using (var ms = new MemoryStream(buffer))
        {
            using (var rdr = new RawSourceWaveStream(ms, format))
            {
                var newFormat = new WaveFormat(16000, 1); // 16kHz mono
                using (var resampler = new MediaFoundationResampler(rdr, newFormat))
                {
                    resampler.ResamplerQuality = 60; // quality can be adjusted here, higher means more cpu intensive
                    using (var resampledMs = new MemoryStream())
                    {
                        WaveFileWriter.WriteWavFileToStream(resampledMs, resampler);
                        
                        return resampledMs.ToArray();
                    }
                }
            }
        }
    }
    static void Main(string[] args)
    {
        Console.WriteLine("Starting audio capture...");

        // initialize Vosk model
        Vosk.Vosk.SetLogLevel(0);
        Model model = new Model("./vosk-model-small-ja-0.22");

        using (var capture = new WasapiLoopbackCapture())
        {
            // Event handler for when data is available
            capture.DataAvailable += (s, a) => 
            {
                // For testing, print out the length of the captured data
                Console.WriteLine($"Captured {a.BytesRecorded} bytes of data.");

                Console.WriteLine($"Audio Format: {capture.WaveFormat}");
                
                byte[] processedAudioData = ProcessedAudioData(a.Buffer, capture.WaveFormat);
                RecognizeSpeech(model, processedAudioData, 16000);
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