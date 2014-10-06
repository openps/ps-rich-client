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

using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using OpenPhotosynth.WebClient;

namespace OpenPhotosynth.RichClient
{
    public sealed class NewPhotosynthViewModel : BindableBase
    {
        private const string ApiDocs = "https://photosynth.net/api/docs/restapi.html";

        private UploadViewModel uploadViewModel;

        public NewPhotosynthViewModel()
        {
            this.AccountViewModel = new AccountViewModel(ServiceConfiguration.Instance);
            this.AccountViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == "IsSignedIn")
                {
                    this.OnPropertyChanged("CanUpload");
                }
            };
            this.PhotoPickerViewModel = new PhotoPickerViewModel();
            this.PhotoPickerViewModel.Photos.CollectionChanged += (_, __) =>
            {
                this.OnPropertyChanged("CanUpload");
            };

            this.UploadCommand = new DelegateCommand(this.Upload);
            this.ShowApiDocsCommand = new DelegateCommand(() => Process.Start(ApiDocs));

            this.Uploader = new Uploader();
            this.uploadViewModel = new UploadViewModel(this.Uploader);

            this.Topology = OpenPhotosynth.WebClient.Topology.Spin;

            this.License = OpenPhotosynth.WebClient.License.CCAttribution;
        }

        public AccountViewModel AccountViewModel { get; private set; }

        public PhotoPickerViewModel PhotoPickerViewModel { get; private set; }

        public Uploader Uploader { get; private set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set
            {
                if (this.SetProperty(ref this.title, value))
                {
                    this.OnPropertyChanged("CanUpload");
                }
            }
        }

        private string description;
        public string Description
        {
            get { return this.description; }
            set { this.SetProperty(ref this.description, value); }
        }

        private string tags;
        public string Tags
        {
            get { return this.tags; }
            set { this.SetProperty(ref this.tags, value); }
        }

        private PrivacyLevel privacyLevel;
        public PrivacyLevel PrivacyLevel
        {
            get { return this.privacyLevel; }
            set { this.SetProperty(ref this.privacyLevel, value); }
        }

        private License license;
        public License License
        {
            get { return this.license; }
            set { this.SetProperty(ref this.license, value); }
        }

        private Topology topology;
        public Topology Topology
        {
            get { return this.topology; }
            set { this.SetProperty(ref this.topology, value); }
        }

        public ICommand UploadCommand { get; private set; }

        public ICommand ShowApiDocsCommand { get; private set; }

        public bool CanUpload
        {
            get
            {
                return this.AccountViewModel.IsSignedIn
                    && !string.IsNullOrEmpty(this.Title)
                    && !this.PhotoPickerViewModel.IsProcessingPhotos
                    && this.PhotoPickerViewModel.Photos.Count > 0
                    && !this.Uploader.IsUploading;
            }
        }

        public void Exit()
        {
            if (this.AccountViewModel.IsSignedIn)
            {
                this.AccountViewModel.SignOut();
            }
        }

        private async void Upload()
        {
            this.Uploader.UploadSynthPacketAsync(
                this.AccountViewModel.WebApi,
                new FileStorage(),
                new Uploader.SynthPacketUpload()
                {
                    Title = this.Title,
                    Description = this.Description,
                    Tags = !string.IsNullOrWhiteSpace(this.Tags) ? this.Tags.Split(',') : Enumerable.Empty<string>(),
                    PrivacyLevel = this.PrivacyLevel,
                    License = this.License,
                    Topology = this.Topology,
                    CapturedDate = this.PhotoPickerViewModel.CapturedDate,
                    PhotoPaths = this.PhotoPickerViewModel.Photos.Select(p => p.FullPath).ToList()
                },
                this.uploadViewModel.CancellationToken);
            this.uploadViewModel.ShowProgress(this.AccountViewModel.WebApi);
            this.OnPropertyChanged("CanUpload");
        }
    }
}
