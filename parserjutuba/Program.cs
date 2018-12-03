using Google.Cloud.Speech.V1;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeSpeechToText
{
    enum Categories
    {
        polityka,
        muzyka,
        sport,
        nauka_technologia
    }

    class Program
    {
        public static void Main(string[] args)
        {
            var source = @"C:/sem/";
            var yt = new YoutubeConverter();
            foreach (Categories category in (Categories[])Enum.GetValues(typeof(Categories)))
            {
                var lines = File.ReadLines(source + category.ToString() +".txt");
                foreach (var line in lines)
                {
                    string url = "https://www.youtube.com/watch?v=" + line;
                    yt.Convert(source, url, line);
                    StereoToMono(source + $"{line}_stereo.wav", source + $"{line}.wav");
                    Console.WriteLine(line);

                    File.Delete(source + line);
                    File.Delete(source + line + "_stereo.wav");

                    AudioToText(source + $"{line}.wav", source + category.ToString() + "/" + $"{line}.txt");

                    File.Delete(source + line + ".wav");
                }
            }



            Console.Read();
        }

        public static void AudioToText(string audioName, string textName)
        {
            var minute = TimeSpan.FromMinutes(1.0);
            WaveFileReader file = new WaveFileReader(audioName);

            long remainder;
            long count = Math.DivRem(file.TotalTime.Ticks, minute.Ticks, out remainder);

            TimeSpan remainderSpan = TimeSpan.FromTicks(remainder);

            Console.WriteLine(count);
            Console.WriteLine(TimeSpan.FromTicks(remainder));
            string content = "";
            Console.WriteLine(file.TotalTime);
            for (int i = 0; i <= count; i++)
            {
                var name = "trim" + i + ".wav";
                var cutEnd = TimeSpan.FromMinutes(count) - TimeSpan.FromMinutes(i + 1) + remainderSpan + TimeSpan.FromSeconds(6.5);
                if (i == count)
                {
                    cutEnd = TimeSpan.FromMinutes(0);
                }

                WavFileUtils.TrimWavFile(audioName, name, TimeSpan.FromMinutes(i), cutEnd);
                content += " " + SpeechToText(name);
            }


            using (StreamWriter writetext = new StreamWriter(textName, true, Encoding.UTF8))
            {
                writetext.WriteLine(content);
            }
        }

        public static void StereoToMono(string sourceFile, string outputFile)
        {
            using (var waveFileReader = new WaveFileReader(sourceFile))
            {
                var outFormat = new WaveFormat(waveFileReader.WaveFormat.SampleRate, 1);
                using (var resampler = new MediaFoundationResampler(waveFileReader, outFormat))
                {
                    WaveFileWriter.CreateWaveFile(outputFile, resampler);
                }
            }
        }

        public static string SpeechToText(string file)
        {
            string fileContent = "";
            var speech = SpeechClient.Create();
            var response = speech.Recognize(new RecognitionConfig()
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 44100,
                LanguageCode = "pl-PL"
            }, RecognitionAudio.FromFile(file));

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    Console.WriteLine(alternative.Transcript);
                    fileContent += alternative.Transcript;
                }
            }
            return fileContent;
        }
    }
}
