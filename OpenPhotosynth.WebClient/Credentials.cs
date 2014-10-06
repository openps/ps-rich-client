﻿// The MIT License (MIT)

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

namespace OpenPhotosynth.WebClient
{
    /// <summary>
    /// Credentials needed to access the Photosynth service.
    /// </summary>
    public class Credentials
    {
        public Credentials(string photosynthUrl, string accessToken, string apiVersion = null)
        {
            this.PhotosynthUrl = photosynthUrl;
            this.AccessToken = accessToken;
            this.ApiVersion = apiVersion ?? "2014-08";
        }

        /// <summary>
        /// The base URL of the Photosynth service.
        /// </summary>
        public string PhotosynthUrl { get; private set; }

        /// <summary>
        /// The API version to be used.
        /// </summary>
        public string ApiVersion { get; private set; }

        /// <summary>
        /// The access token used to authenticate to the Photosynth service.
        /// </summary>
        public string AccessToken { get; private set; }
    }
}
