using ffmpegcore;
using System;
using System.Diagnostics;
using System.IO;

namespace demo
{
    class Program
    {
        static void Main(string[] args)
        {
            bool outputToConsole = false;
            string videoFile = Path.Combine(AppContext.BaseDirectory, "App_Data", "demo.mp4");

            //Cut video into 2 chunks
            string vidChunk1 = utility.cutVideo(videoFile, 0.0, 10.0, "mp4", outputToConsole);
            string vidChunk2 = utility.cutVideo(videoFile, 10.0, 20.0, "mp4", outputToConsole);

            //Strip audio off of both chunks
            string audioChunk1 = utility.stripAudioFromVideo(vidChunk1, "mp3", outputToConsole);
            string audioChunk2 = utility.stripAudioFromVideo(vidChunk2, "mp3", outputToConsole);

            //Replace each video's audio with the other's
            string vidChunk1AudioReplaced = utility.addAudioToVideo(vidChunk1, audioChunk2, "mp4", outputToConsole);
            string vidChunk2AudioReplaced = utility.addAudioToVideo(vidChunk2, audioChunk1, "mp4", outputToConsole);

            //Concatenate videos back together
            string finalizedVid = utility.concatVideos(new string[] { vidChunk1AudioReplaced, vidChunk2AudioReplaced }, "mp4", outputToConsole);

            //Clear out all but the finalized video
            File.Delete(Path.Combine(AppContext.BaseDirectory, "App_Data", vidChunk1));
            File.Delete(Path.Combine(AppContext.BaseDirectory, "App_Data", vidChunk2));
            File.Delete(Path.Combine(AppContext.BaseDirectory, "App_Data", audioChunk1));
            File.Delete(Path.Combine(AppContext.BaseDirectory, "App_Data", audioChunk2));
            File.Delete(Path.Combine(AppContext.BaseDirectory, "App_Data", vidChunk1AudioReplaced));
            File.Delete(Path.Combine(AppContext.BaseDirectory, "App_Data", vidChunk2AudioReplaced));

            //Open result
            string fullFilename = Path.Combine(AppContext.BaseDirectory, "App_Data", finalizedVid);
            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = fullFilename
                };

                p.Start();
            }

            Console.ReadLine();//Keep the console open to review output

        }
    }
}
