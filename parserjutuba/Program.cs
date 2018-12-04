using Google.Cloud.Speech.V1;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace parserjutuba
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
                var lines = File.ReadLines(source + category.ToString() + ".txt");
                foreach (var line in lines)
                {
                    string url = "https://www.youtube.com/watch?v=" + line;
                    yt.Convert(source, url, line);
                    StereoToMono(source + $"{line}_stereo.wav", source + $"{line}.wav");
                    Console.WriteLine(line);

                    File.Delete(source + line);
                    File.Delete(source + line + "_stereo.wav");

                    AudioToText(source + $"{line}.wav", source + category.ToString() + "/" + $"{line}.txt");
                    WaitForFile(source + line + ".wav");
                    File.Delete(source + line + ".wav");
                }
            }

            Console.WriteLine("Parsing...");
            var parser = new TextDataParser();
            foreach (Categories category in (Categories[])Enum.GetValues(typeof(Categories)))
            {
                foreach (string file in Directory.EnumerateFiles(source + category.ToString() + "/", "*.txt"))
                {
                    string fileContent = File.ReadLines(file).First();
                    parser.Parse(source, fileContent, category.ToString());
                }
            }
            Console.WriteLine("Data parsed to data.txt");

            Console.WriteLine("KONIEC");
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

            List<Task<string>> taskList = new List<Task<string>>();
            TimeSpan lastTrimEnd = TimeSpan.FromMinutes(0);
            for (int i = 1; i <= count+1; i++)
            {
                var name = "trim" + i + ".wav";
                var cutEnd = file.TotalTime - TimeSpan.FromMinutes(i) + TimeSpan.FromSeconds(i*3);
                var cutStart = lastTrimEnd + TimeSpan.FromSeconds(1.5);
                if (i == 1)
                {
                    cutStart = lastTrimEnd;
                }

                if (i == count+1)
                {
                    cutEnd = TimeSpan.FromMinutes(0);
                    if (file.TotalTime - cutStart - cutEnd > TimeSpan.FromMinutes(1))
                    {
                        cutEnd = file.TotalTime - (cutStart + TimeSpan.FromSeconds(57));
                    }
                        
                }

                lastTrimEnd = file.TotalTime - cutEnd;

                Console.WriteLine($"start: {cutStart} end: {file.TotalTime - cutEnd}");
                WavFileUtils.TrimWavFile(audioName, name, cutStart, cutEnd);
                Task<string> lastTask = Task<string>.Factory.StartNew(() => SpeechToText(name));
                taskList.Add(lastTask);
            }

            Task.WaitAll(taskList.ToArray());

            foreach (var item in taskList)
            {
                content += " " + item.Result;
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

            Console.WriteLine("Transcript of " + file + " ready.");

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    //Console.WriteLine(alternative.Transcript);
                    fileContent += alternative.Transcript;
                }
            }
            return fileContent;
        }

        public static bool IsFileReady(string filename)
        {
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void WaitForFile(string filename)
        {
            while (!IsFileReady(filename)) { }
        }

        public static void CmdExecute()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C copy /b Image1.jpg + Archive.rar Image2.jpg";
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
