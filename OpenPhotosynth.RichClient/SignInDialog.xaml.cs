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

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using OpenPhotosynth;

namespace OpenPhotosynth.RichClient
{
    /// <summary>
    /// Interaction logic for SignInDialog.xaml
    /// </summary>
    public partial class SignInDialog : Window
    {
        private string redirectUrl;

        private MicrosoftAccessInfo microsoftAccessInfo;

        private SignInDialog()
        {
            InitializeComponent();
            this.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    this.DialogResult = false;
                }
            };
            this.Browser.LoadCompleted += this.Browser_LoadCompleted;
        }

        public static async Task<MicrosoftAccessInfo> SignInAsync(string signInUrl, string redirectUrl)
        {
            var signInDialog = new SignInDialog();
            return await signInDialog.ShowDialog(Application.Current.MainWindow, signInUrl, redirectUrl);
        }

        private async Task<MicrosoftAccessInfo> ShowDialog(Window parentWindow, string signInUrl, string redirectUrl)
        {
            this.Owner = parentWindow;
            this.redirectUrl = redirectUrl;
            this.Browser.Navigate(signInUrl);
            bool result = base.ShowDialog().GetValueOrDefault();
            await Task.Yield();
            return result ? this.microsoftAccessInfo : null;
        }

        private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (this.IsLoaded && this.IsActive && e.Uri.AbsoluteUri.StartsWith(this.redirectUrl))
            {
                this.microsoftAccessInfo = MicrosoftAccessInfo.Parse(e.Uri);
                this.DialogResult = this.microsoftAccessInfo != null;
            }
        }
    }
}
