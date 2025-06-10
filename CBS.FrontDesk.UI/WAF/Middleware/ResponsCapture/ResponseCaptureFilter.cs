using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Middleware.Core.ResponsCapture
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// <b>ResponseCaptureFilterStream</b> is a custom stream wrapper that intercepts and copies all data
    /// written to the HTTP response stream. It allows capturing the entire response body for logging,
    /// inspection, or auditing (e.g., by a Web Application Firewall).
    /// 
    /// The original response is written to as normal, but all content is also duplicated
    /// into an internal memory buffer accessible via <see cref="GetCapturedBody"/>.
    /// </summary>
    public class ResponseCaptureFilterStream : Stream
    {
        private readonly Stream _responseStream;              // The original HTTP response stream
        private readonly MemoryStream _copyStream = new MemoryStream();    // The internal memory copy

        /// <summary>
        /// Initializes a new instance of <see cref="ResponseCaptureFilterStream"/> with the target response stream.
        /// </summary>
        public ResponseCaptureFilterStream(Stream responseStream)
        {
            _responseStream = responseStream;
        }

        /// <summary>
        /// Returns the captured response body as a UTF-8 string.
        /// Used by WAF or diagnostics to inspect final output.
        /// </summary>
        public string GetCapturedBody()
        {
            try
            {
                _copyStream.Position = 0; // Reset memory stream to beginning
                var reader = new StreamReader(_copyStream, Encoding.UTF8, true, 1024, leaveOpen: true);
                return reader.ReadToEnd();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Overrides the Write method to write to both the response stream and memory copy.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _copyStream.Write(buffer, offset, count);       // Capture copy
            _responseStream.Write(buffer, offset, count);   // Forward to actual response stream

            System.Diagnostics.Debug.WriteLine($"[WAF] Captured {count} bytes");
        }

        // ========== Delegated Stream Members ==========

        public override bool CanRead => _responseStream.CanRead;
        public override bool CanSeek => _responseStream.CanSeek;
        public override bool CanWrite => _responseStream.CanWrite;
        public override long Length => _responseStream.Length;

        public override long Position
        {
            get => _responseStream.Position;
            set => _responseStream.Position = value;
        }

        public override void Flush() => _responseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) =>
            _responseStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) =>
            _responseStream.Seek(offset, origin);

        public override void SetLength(long value) =>
            _responseStream.SetLength(value);
    }

}