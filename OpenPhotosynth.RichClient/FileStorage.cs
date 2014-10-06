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
using System.IO;
using OpenPs = OpenPhotosynth.WebClient;

namespace OpenPhotosynth.RichClient
{
    public class FileStorage : OpenPs.IFileStorage
    {
        public long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public Stream GetFileStream(string path, OpenPs.FileMode mode)
        {
            FileMode fileMode;
            switch (mode)
            {
                case OpenPs.FileMode.Append:
                    fileMode = FileMode.Append;
                    break;

                case OpenPs.FileMode.Create:
                    fileMode = FileMode.Create;
                    break;

                case OpenPs.FileMode.CreateNew:
                    fileMode = FileMode.CreateNew;
                    break;

                case OpenPs.FileMode.Open:
                    fileMode = FileMode.Open;
                    break;

                case OpenPs.FileMode.OpenOrCreate:
                    fileMode = FileMode.OpenOrCreate;
                    break;

                case OpenPs.FileMode.Truncate:
                    fileMode = FileMode.Truncate;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new FileStream(path, fileMode);
        }
    }
}
