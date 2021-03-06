# ffmpegcore
.NET Core Utility for leveraging the ffmpeg executable for windows (Would probably work for other OS's supported by .NET Core with a little tweaking and the matching ffmpeg binary)

To run the demo application open it in VS (created in VS2017), set the startup project to 'demo' and run it. There are a few built-in FFmpeg commands included in the utility and can be easily extended to use any command by using the 'utility.runFFMPEGCmd' method and sending in the command you would like to run.

Uses a compiled ffmpeg executable for Windows x64, if you are on a 32-bit version just download it (<a href="https://www.ffmpeg.org/download.html" target="_blank">link</a>) and replace it in the App_Data folder of the ffmpegcore project.

FFMPEG License Information: https://www.ffmpeg.org/legal.html

Included legal statement for FFmpeg: "This software uses code of <a href=http://ffmpeg.org>FFmpeg</a> licensed under the <a href=http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html>LGPLv2.1</a> and its source can be downloaded on <a href="https://github.com/justin-glorvigen/ffmpegcore" target="_blank">github</a>".
