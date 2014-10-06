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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OpenPhotosynth.RichClient
{
    public sealed class MicrosoftAccount
    {
        private static readonly TimeSpan SignInTimeout = TimeSpan.FromSeconds(5);

        private static readonly TimeSpan SignOutTimeout = TimeSpan.FromSeconds(5);

        private const string AuthorizePath = "/oauth20_authorize.srf";
        private const string LogoutPath = "/oauth20_logout.srf";
        private const string TokenPath = "/oauth20_token.srf";
        private const string RedirectPath = "/oauth20_desktop.srf";

        private ServiceConfiguration serviceConfiguration;

        public MicrosoftAccount(ServiceConfiguration serviceConfiguration)
        {
            this.serviceConfiguration = serviceConfiguration;
        }

        public string RedirectUrl
        {
            get { return this.serviceConfiguration.MicrosoftLoginUrl + RedirectPath; }
        }

        public string SignInUrl
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}?client_id={2}&scope={3}&response_type=token&redirect_uri={4}",
                    this.serviceConfiguration.MicrosoftLoginUrl,
                    AuthorizePath,
                    this.serviceConfiguration.MicrosoftClientId,
                    this.serviceConfiguration.MicrosoftScope,
                    this.RedirectUrl);
            }
        }

        public string SignOutUrl
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}?client_id={2}&scope={3}&redirect_uri={4}",
                    this.serviceConfiguration.MicrosoftLoginUrl,
                    LogoutPath,
                    this.serviceConfiguration.MicrosoftClientId,
                    this.serviceConfiguration.MicrosoftScope,
                    this.RedirectUrl);
            }
        }

        public async Task<MicrosoftAccessInfo> SignInAsync()
        {
            return await SignInDialog.SignInAsync(this.SignInUrl, this.RedirectUrl);
        }

        public async Task<MicrosoftAccessInfo> SignInSilentlyAsync()
        {
            MicrosoftAccessInfo microsoftAccessInfo = null;
            var taskCompletionSource = new TaskCompletionSource<object>();
            WebBrowser webBrowser = new WebBrowser();
            webBrowser.Navigated += (sender, e) =>
            {
                if (e.Uri.AbsoluteUri.StartsWith(this.RedirectUrl))
                {
                    microsoftAccessInfo = MicrosoftAccessInfo.Parse(e.Uri);
                    taskCompletionSource.SetResult(null);
                }
            };
            webBrowser.Navigate(this.SignInUrl + "&display=none");
            await Task.WhenAny(taskCompletionSource.Task, Task.Delay(SignInTimeout));
            return microsoftAccessInfo;
        }

        public async Task<bool> SignOutAsync()
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            WebBrowser webBrowser = new WebBrowser();
            webBrowser.Navigated += (sender, e) =>
            {
                if (e.Uri.AbsoluteUri.StartsWith(this.RedirectUrl))
                {
                    taskCompletionSource.SetResult(null);
                }
            };
            webBrowser.Navigate(this.SignOutUrl);
            await Task.WhenAny(taskCompletionSource.Task, Task.Delay(SignOutTimeout));
            return taskCompletionSource.Task.IsCompleted && !taskCompletionSource.Task.IsFaulted;
        }        
    }
}
