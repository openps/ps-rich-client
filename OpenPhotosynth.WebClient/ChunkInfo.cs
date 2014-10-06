// The MIT License (MIT)

// Copyright (c) 2014 Alec Siu, Eric Stollnitz

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

namespace OpenPhotosynth.WebClient
{
    /// <summary>
    /// Information about a file chunk being sent up to the Photosynth upload service.
    /// </summary>
    public sealed class ChunkInfo
    {
        public ChunkInfo(Uri uploadUri, string filePath, long totalBytes)
        {
            this.UploadUri = uploadUri;
            this.FilePath = filePath;
            this.TotalBytes = totalBytes;
        }

        /// <summary>
        /// The URI the chunk should be uploaded to.
        /// </summary>
        public Uri UploadUri { get; private set; }

        /// <summary>
        /// The path to the local chunk.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Total bytes of the chunk to be uploaded.
        /// </summary>
        public long TotalBytes { get; private set; }

        /// <summary>
        /// Total number of bytes uploaded successfully so far.
        /// </summary>
        public long BytesSent { get; set; }
    }
}
