// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

using System;
using System.Text;
using System.Timers;
using NAudio.Wave;
using Vosk;
using Newtonsoft.Json;

class Program
{
    private static StringBuilder speechBuffer = new StringBuilder();
    private static System.Timers.Timer outputTimer;

    static void Main(string[] args)
    {
        Console.WriteLine("Starting audio capture...");

        // initialize Vosk model
        Model model = InitializeVoskModel("./vosk-model-small-ja-0.22");

        // set up and enable timer
        outputTimer = new Timer(5000); // 5000 milliseconds interval or 5 seconds
        outputTimer.Elapsed += OnTimedEvent;
        outputTimer.AutoReset = true;
        outputTimer.Enabled = true;

        using (var capture = new WasapiLoopbackCapture())
        {
            // Event handler for when data is available
            capture.DataAvailable += (s, a) => 
            {
                // For testing, print out the length of the captured data
                // Console.WriteLine($"Captured {a.BytesRecorded} bytes of data.");

                // Console.WriteLine($"Audio Format: {capture.WaveFormat}");
                
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

    // function to initialize Vosk Model
    static Model InitializeVoskModel(string modelPath)
    {
        Vosk.Vosk.SetLogLevel(0); // set logs for warnings
        return new Model(modelPath);
    }

    // function for handling speech recognition
    static void RecognizeSpeech(Model model, byte[] audioData, int sampleRate)
    {
        using (var recognizer = new VoskRecognizer(model, sampleRate))
        {
            if (recognizer.AcceptWaveform(audioData, audioData.Length))
            {
                string result = recognizer.Result();
                var resultJson = JsonConvert.DeserializedObject<dynamic>(result);
                string text = resultJson.text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    speechBuffer.Append(text + " ");
                }
            }
        }
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

    // function handling the buffer, clearing and resetting based on the timer interval
    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        // if the buffer is not empty by the end of the interval, output and clear the buffer
        if (speechBuffer.Length > 0)
        {
            Console.WriteLine(speechBuffer.ToString());
            speechBuffer.Clear();
        }
    }
}