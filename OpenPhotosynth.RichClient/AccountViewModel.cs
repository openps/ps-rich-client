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
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using OpenPhotosynth.WebClient;
using OpenPhotosynth.WebClient.DataContracts;
using OpenPhotosynth.RichClient.Properties;

namespace OpenPhotosynth.RichClient
{
    public class AccountViewModel : BindableBase
    {
        private ServiceConfiguration serviceConfiguration;

        private MicrosoftAccessInfo microsoftAccessInfo = null;

        private MicrosoftAccount microsoftAccount;

        public AccountViewModel(ServiceConfiguration serviceConfiguration)
        {
            this.serviceConfiguration = serviceConfiguration;
            this.microsoftAccount = new MicrosoftAccount(serviceConfiguration);            
            this.CheckInternetConnection();
            NetworkChange.NetworkAvailabilityChanged += this.NetworkChange_NetworkAvailabilityChanged;

            this.SignInCommand = new DelegateCommand(this.SignIn);
            this.SignOutCommand = new DelegateCommand(this.SignOut);
            this.LibraryCommand = new DelegateCommand(() =>
            {
                if (this.IsSignedIn)
                {
                    Process.Start(string.Format(CultureInfo.InvariantCulture, "{0}/preview/users/{1}", this.serviceConfiguration.PhotosynthUrl, this.Username));
                }
            });
        }

        ~AccountViewModel()
        {
            NetworkChange.NetworkAvailabilityChanged -= this.NetworkChange_NetworkAvailabilityChanged;
        }

        private bool isInternetConnected;
        public bool IsInternetConnected
        {
            get { return this.isInternetConnected; }
            private set
            {
                if (this.SetProperty(ref this.isInternetConnected, value))
                {
                    if (this.isInternetConnected)
                    {
                        // TBD try to sign in silently
                    }

                    this.ErrorText = null;
                    this.UpdateStatus();                    

                    this.OnPropertyChanged("CanSignIn");
                    this.OnPropertyChanged("CanSignOut");
                }
            }
        }

        public WebApi WebApi { get; private set; }

        private string status = Resources.NoInternetConnection;
        public string Status
        {
            get { return this.status; }
            private set { this.SetProperty(ref this.status, value); }
        }

        private bool isWorking;
        public bool IsWorking
        {
            get { return this.isWorking; }
            private set
            {
                if (this.SetProperty(ref this.isWorking, value))
                {
                    this.OnPropertyChanged("CanSignIn");
                    this.OnPropertyChanged("CanSignOut");
                }
            }
        }

        public bool IsSignedIn
        {
            get { return this.microsoftAccessInfo != null && this.microsoftAccessInfo.AccessToken != null; }
        }

        public bool CanSignIn
        {
            get { return this.IsInternetConnected && !this.IsSignedIn && !this.IsWorking; }
        }

        public bool CanSignOut
        {
            get { return this.IsInternetConnected && this.IsSignedIn && !this.IsWorking; }
        }

        private string errorText;
        public string ErrorText
        {
            get { return this.errorText; }
            private set
            {
                if (this.SetProperty(ref this.errorText, value))
                {
                    this.OnPropertyChanged("HasError");
                }
            }
        }

        public bool HasError
        {
            get { return !String.IsNullOrEmpty(this.ErrorText); }
        }

        private string username;
        public string Username
        {
            get { return this.username; }
            private set { this.SetProperty(ref this.username, value); }
        }

        private string description;
        public string Description
        {
            get { return this.description; }
            private set { this.SetProperty(ref this.description, value); }
        }

        private string userStats;
        public string UserStats
        {
            get { return this.userStats; }
            private set { this.SetProperty(ref this.userStats, value); }
        }

        public ICommand SignInCommand { get; private set; }

        public ICommand SignOutCommand { get; private set; }

        public ICommand LibraryCommand { get; private set; }

        public async void SignIn()
        {
            await this.SignInAsync();
        }

        private async Task SignInAsync()
        {
            try
            {
                this.ErrorText = null;
                this.IsWorking = true;
                this.Status = Resources.SigningIn;
                this.microsoftAccessInfo = await this.microsoftAccount.SignInSilentlyAsync() ?? await this.microsoftAccount.SignInAsync();
                if (this.microsoftAccessInfo == null)
                {
                    this.Status = Resources.NotSignedIn;
                }
                else
                {
                    this.WebApi = new WebApi(new Credentials(this.serviceConfiguration.PhotosynthUrl, this.microsoftAccessInfo.AccessToken));
                    UserInfo userInfo = await this.WebApi.GetUserInfoAsync();
                    if (userInfo != null)
                    {
                        this.Username = userInfo.Username;
                        this.Description = userInfo.Description;

                    }
                    else
                    {
                        await this.SignOutAsync();
                        this.ErrorText = Resources.NotPhotosynthUser;
                    }
                }
            }
            catch
            {
                this.ErrorText = Resources.CouldNotSignIn;
            }

            this.UpdateStatus();
        }

        public async void SignOut()
        {
            await this.SignOutAsync();
        }

        private async Task SignOutAsync()
        {
            try
            {
                this.WebApi = null;
                this.ErrorText = null;
                this.IsWorking = true;
                this.Status = Resources.SigningOut;
                bool signedOut = await this.microsoftAccount.SignOutAsync();
                if (signedOut)
                {
                    this.microsoftAccessInfo = null;
                    this.ClearUserInfo();
                }
            }
            catch
            {
                this.ErrorText = Resources.CouldNotSignOut;
            }

            this.UpdateStatus();
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            this.CheckInternetConnection();
        }

        private void CheckInternetConnection()
        {
            this.IsInternetConnected = NetworkInterface.GetIsNetworkAvailable();
        }

        private void UpdateStatus()
        {
            if (!this.IsInternetConnected)
            {
                this.Status = Resources.NoInternetConnection;
            }
            else
            {
                if (this.IsSignedIn)
                {
                    this.Status = Resources.SignedIn;
                }
                else
                {
                    this.Status = Resources.NotSignedIn;
                }
            }

            this.OnPropertyChanged("IsSignedIn");

            this.IsWorking = false;
        }

        private void ClearUserInfo()
        {
            this.Username = this.Description = this.UserStats = null;
        }
    }
}
