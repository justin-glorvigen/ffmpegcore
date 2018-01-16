using System;
using System.Collections.Generic;
using System.Text;

namespace ffmpegcore
{
    /// <summary>
    /// Holds the response from running an FFMPEG Command
    /// </summary>
    public class Response
    {
        public string OutputError { get; set; }
        public string Output { get; set; }

        public Response()
        {

        }

        public Response(string OutputError, string Output)
        {
            this.Output = Output;
            this.OutputError = OutputError;
        }
    }
}
