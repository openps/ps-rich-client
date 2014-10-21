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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenPhotosynth.WebClient;

namespace OpenPhotosynth.RichClient
{
    /// <summary>
    /// Class for teasing apart the URL returned from Microsoft Account holding access token information.
    /// </summary>
    public sealed class MicrosoftAccessInfo
    {
        private const string UserIdParameter = "user_id";
        private const string AccessTokenParameter = "access_token";
        private const string ExpiresInParameter = "expires_in";
        private const string RefreshTokenParameter = "refresh_token";

        private MicrosoftAccessInfo(string accessToken, string refreshToken, TimeSpan expiration)
        {
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
            this.Expiration = expiration;
        }

        public string AccessToken { get; private set; }

        public string RefreshToken { get; private set; }

        public TimeSpan Expiration { get; private set; }

        public static MicrosoftAccessInfo Parse(Uri uri)
        {
            Dictionary<string, string> parameters = uri.ParseFragment();
            string accessToken, expiresIn;
            int expirationInSeconds;
            MicrosoftAccessInfo accessInfo = null;
            if (parameters.TryGetValue(AccessTokenParameter, out accessToken) &&
                parameters.TryGetValue(ExpiresInParameter, out expiresIn) &&
                Int32.TryParse(expiresIn, out expirationInSeconds))
            {
                string refreshToken;
                parameters.TryGetValue(RefreshTokenParameter, out refreshToken);
                accessInfo = new MicrosoftAccessInfo(accessToken, refreshToken, TimeSpan.FromSeconds(expirationInSeconds));
            }

            return accessInfo;
        }
    }
}
