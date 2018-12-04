using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace parserjutuba
{
    class YoutubeConverter
    {
        public void Convert(string source, string url, string videoId)
        {
            var youtube = YouTube.Default;
            Console.WriteLine("Downloading video...");
            var vid = youtube.GetVideo(url);
            Console.WriteLine("video - " + vid.Title + " downloaded");
            File.WriteAllBytes(source + videoId, vid.GetBytes());

            var inputFile = new MediaFile { Filename = source + videoId };
            var outputFile = new MediaFile { Filename = $"{source + videoId}_stereo.wav" };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);

                engine.Convert(inputFile, outputFile);
            }
        }
    }
}
