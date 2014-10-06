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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OpenPhotosynth.WebClient;

namespace OpenPhotosynth.RichClient
{
    public sealed class UploadViewModel : BindableBase
    {
        private CancellationTokenSource cancellationTokenSource;

        private WebApi webApi;

        public UploadViewModel(Uploader uploader)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.Uploader = uploader;
            this.CancelCommand = new DelegateCommand(async () => await this.Cancel());
            this.RetryCommand = new DelegateCommand(async () =>
            {
                await this.Uploader.ResumeUploadAsync(webApi, new FileStorage(), this.CancellationToken);
            });
        }

        private bool isCancelled;
        public bool IsCancelled
        {
            get { return this.isCancelled; }
            set
            {
                if (this.SetProperty(ref this.isCancelled, value))
                {
                    this.OnPropertyChanged("IsRetryable");
                }
            }
        }

        public bool IsRetryable
        {
            get { return this.Uploader.IsRetryable && !this.IsCancelled; }
        }

        public Uploader Uploader { get; set; }

        public ICommand RetryCommand { get; set; }

        public ICommand CancelCommand { get; set; }
        
        public CancellationToken CancellationToken { get { return this.cancellationTokenSource.Token; } }

        public void ShowProgress(WebApi webApi)
        {
            this.webApi = webApi;
            var dialog = new UploadDialog() { DataContext = this, Owner = Application.Current.MainWindow };
            dialog.Closing += async (_, __) => await this.Cancel();
            dialog.ShowDialog();
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task Cancel()
        {
            if (this.Uploader.IsUploading)
            {
                Guid id = this.Uploader.Id;
                this.IsCancelled = true;
                this.cancellationTokenSource.Cancel();
                await this.webApi.DeleteMediaAsync(id);
            }
        }
    }
}
