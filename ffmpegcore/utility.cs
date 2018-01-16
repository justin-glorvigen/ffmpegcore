using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ffmpegcore
{
    public class utility
    {
        private const string videoCutCmd = "ffmpeg -y -ss {1} -i \"{0}\" -t {2} \"{3}\"";
        private const string audioReplaceCmd = "ffmpeg -y -i {0} -i \"{1}\" -map 0:v:0 -map 1:a:0 \"{2}\"";
        private const string videoConcatCmd = "ffmpeg -y -f concat -safe 0 -i \"{0}\" \"{1}\"";
        private const string addKeyFrameCmd = "ffmpeg -y -i \"{0}\" -force_key_frames {1} \"{2}\"";
        private const string fileConvertCmd = "ffmpeg -i \"{0}\" \"{1}\"";
        private const string audioStripCmd = "ffmpeg -i \"{0}\" -q:a 0 -map a \"{1}\"";

        /// <summary>
        /// Runs the input command and returns an Response object containing the result
        /// </summary>
        /// <param name="command"></param>
        /// <param name="outputToConsole"></param>
        /// <returns></returns>
        public static Response runFFMPEGCmd(string command, bool outputToConsole = false)
        {
            if (!command.ToLower().Trim().StartsWith("ffmpeg"))//Ensure the command always starts with 'ffmpeg '
            {
                command = "ffmpeg " + command;
            }

            List<string> errorOutput = new List<string>();
            List<string> output = new List<string>();

            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.Combine(System.AppContext.BaseDirectory, "App_Data"),
                    FileName = "cmd.exe",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                };

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    p.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            try
                            {
                                outputWaitHandle.Set();
                            }
                            catch (Exception ex)
                            {
                                errorOutput.Add("Process Exception: " + ex.Message);
                            }
                        }
                        else
                        {
                            output.Add(e.Data);
                        }
                    };
                    p.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            try
                            {
                                errorWaitHandle.Set();
                            }catch(Exception ex)
                            {
                                errorOutput.Add("Process Exception: " + ex.Message);
                            }
                        }
                        else
                        {
                            errorOutput.Add(e.Data);
                        }
                    };

                    p.Start();//Start the process
                    p.StandardInput.WriteLine(command);//Write the command
                    p.StandardInput.Flush();//Flush it
                    p.StandardInput.Close();//Close the stream

                    p.BeginOutputReadLine();//Start reading the output
                    p.BeginErrorReadLine();//Start reading the error output

                    if (p.WaitForExit(60000) &&
                         outputWaitHandle.WaitOne(60000) &&
                         errorWaitHandle.WaitOne(60000))
                    {
                        if (p.ExitCode != 0)//Process finished with errors
                        {
                            errorOutput.Add("Process finished with status code " + p.ExitCode.ToString());
                        }

                        return new Response(string.Join('\n', errorOutput.ToArray()), string.Join('\n', output.ToArray()));
                    }
                    else//Process timed out
                    {
                        errorOutput.Add("Process timed out, try a longer timeout, or a smaller file");
                        return new Response(string.Join('\n', errorOutput.ToArray()), string.Join('\n', output.ToArray()));
                    }
                }
            }
        }

        /// <summary>
        /// Outputs the input Response to the console
        /// </summary>
        /// <param name="response"></param>
        private static void outputFFMPEGResultToConsole(Response response)
        {
            Console.WriteLine("FFMPEG Output:");
            Console.WriteLine(response.Output);
            Console.WriteLine("FFMPEG Errors:");
            Console.WriteLine(response.OutputError);
        }

        /// <summary>
        /// Strips the audio off of the input video and returns it
        /// </summary>
        /// <param name="videoFilename"></param>
        /// <param name="format"></param>
        /// <param name="outputToConsole"></param>
        /// <returns></returns>
        public static string stripAudioFromVideo(string videoFilename, string format = "mp3", bool outputToConsole = false)
        {
            string returning = getTempFilename(format);

            string ffmpegcmd = string.Format(audioStripCmd, videoFilename, returning);
            Response result = runFFMPEGCmd(ffmpegcmd);
            if (outputToConsole)
            {
                outputFFMPEGResultToConsole(result);
            }

            return returning;
        }

        /// <summary>
        /// Replaces the input video's audio stream with the input audio stream
        /// Note:   Stretches the last video frame until the video is the same length as the audio if the audio is longer
        ///         If audio is shorter, or the same length, as the video, nothing is changed
        /// </summary>
        /// <param name="videoFilename"></param>
        /// <param name="audioFilename"></param>
        /// <returns></returns>
        public static string addAudioToVideo(string videoFilename, string audioFilename, string format = "mp4", bool outputToConsole = false)
        {
            string returning = getTempFilename();

            string ffmpegcmd = string.Format(audioReplaceCmd, videoFilename, audioFilename, returning);

            Response result = runFFMPEGCmd(ffmpegcmd);
            if (outputToConsole)
            {
                outputFFMPEGResultToConsole(result);
            }

            return returning;
        }

        /// <summary>
        /// Converts the input file to the input format and returns the temp file location for it
        /// Note: 
        ///     Only works for formats that can be converted without any ffmpeg flags being set.
        ///     If this doesn't work for your format, check the web for the correct flags and generate
        ///     your own command and run it using the runFFMPEGCmd method
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string convertFile(string inputFile, string format, bool outputToConsole = false)
        {
            string returning = getTempFilename(format);

            string ffmpegcmd = string.Format(fileConvertCmd, inputFile, returning);

            Response result = runFFMPEGCmd(ffmpegcmd);
            if (outputToConsole)
            {
                outputFFMPEGResultToConsole(result);
            }

            return returning;
        }

        /// <summary>
        /// Concatenates the input list of video files
        /// </summary>
        /// <param name="videos"></param>
        /// <param name="format"></param>
        /// <param name="outputToConsole"></param>
        /// <returns></returns>
        public static string concatVideos(string[] videos, string format = "mp4", bool outputToConsole = false)
        {
            string returning = getTempFilename();

            string[] files = videos.Select(d => "file '" + d + "'").ToArray();
            string fileLoc = Path.Combine(System.AppContext.BaseDirectory, "App_Data", getTempFilename("txt"));
            File.WriteAllLines(fileLoc, files);

            string ffmpegcmd = string.Format(videoConcatCmd, fileLoc, returning);

            Response result = runFFMPEGCmd(ffmpegcmd);
            if (outputToConsole)
            {
                outputFFMPEGResultToConsole(result);
            }

            File.Delete(fileLoc);

            return returning;
        }

        /// <summary>
        /// Cuts the input video and returns a clip including the frames between the input startTime and endTime in the input format (defaults to mp4)
        /// </summary>
        /// <param name="inputVideo"></param>
        /// <param name="startTime">in seconds</param>
        /// <param name="endTime">in seconds</param>
        /// <param name="outputToConsole"></param>
        /// <returns></returns>
        public static string cutVideo(string inputVideo, double startTime, double endTime, string format = "mp4", bool outputToConsole = false)
        {
            string returning = getTempFilename(format);

            string ffmpegcmd = string.Format(videoCutCmd, inputVideo, formatTime(startTime), formatTime(endTime - startTime), returning);

            Response result = runFFMPEGCmd(ffmpegcmd);
            if (outputToConsole)
            {
                outputFFMPEGResultToConsole(result);
            }

            return returning;
        }

        /// <summary>
        /// Formats the input time (in seconds) into following format: ss.ttt
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private static string formatTime(double time)
        {
            return time.ToString("00.000");
        }

        /// <summary>
        /// Formats the input time (in seconds) in the following format: hh:mm:ss.ttt
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private static string formatTimeLong(double time)
        {
            string returning = "";

            int hours = 0;
            if (time > 3600)
            {
                hours = (int)time / 3600;//Get the hours
                time = time % 3600;//Cut off all the hours
            }

            int mins = 0;
            if (time > 60)
            {
                mins = (int)time / 60;//Get the mins
                time = time % 60;//Cut off all of the mins
            }

            returning = hours.ToString("00") + ":" + mins.ToString("00") + ":" + time.ToString("00.000");

            return returning;
        }

        /// <summary>
        /// Generates a temp filename for the App_Data folder and returns it
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string getTempFilename(string format = "mp4")
        {
            return Path.ChangeExtension(Math.Abs(DateTime.Now.GetHashCode()).ToString(), format);
        }

    }
}
