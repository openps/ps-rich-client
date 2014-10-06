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

using System.Configuration;

namespace OpenPhotosynth.RichClient
{
    public sealed class ServiceConfiguration
    {
        private static ServiceConfiguration instance = new ServiceConfiguration()
        {
            PhotosynthUrl = ConfigurationManager.AppSettings["PhotosynthUrl"],
            MicrosoftLoginUrl = ConfigurationManager.AppSettings["MicrosoftLoginUrl"],
            MicrosoftClientId = ConfigurationManager.AppSettings["MicrosoftClientId"]            
        };

        private ServiceConfiguration()
        {
        }

        public static ServiceConfiguration Instance
        {
            get { return instance; }
        }

        public string PhotosynthUrl { get; private set; }

        public string MicrosoftLoginUrl { get; private set; }

        public string MicrosoftClientId { get; private set; }

        public string MicrosoftScope
        {
            // This app needs both Read and Write privileges in order to upload new Photosynths
            get { return "Photosynth.Read Photosynth.Write"; }
        }
    }
}
