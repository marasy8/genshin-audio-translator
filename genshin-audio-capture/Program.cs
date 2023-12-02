// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

using System;
using NAudio.Wave;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting audio capture...");
        using (var capture = new WasapiLoopbackCapture())
        {
            // Event handler for when data is available
            capture.DataAvailable += (s, a) => 
            {
                // For testing, print out the length of the captured data
                Console.WriteLine($"Captured {a.BytesRecorded} bytes of data.");
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