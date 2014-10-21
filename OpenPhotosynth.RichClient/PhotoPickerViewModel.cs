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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OpenPhotosynth.RichClient.Properties;
using OpenPhotosynth.WebClient;

namespace OpenPhotosynth.RichClient
{
    /// <summary>
    /// Other things we could do before uploading photos to Photosynth:
    /// - recompress photos to minimize bandwidth consumption (need to maintain EXIF data too though
    /// because this contains valuable camera information, e.g. focal length)
    /// - extract useful EXIF data like geotag
    /// - warn when photos have differing focal lengths, aspect ratios etc. that could affect the synthed result
    /// </summary>
    public class PhotoPickerViewModel : BindableBase
    {
        private const int MaxPhotoUploadLimit = 200;

        private HashSet<string> photoPaths;

        private IEnumerable<Photo> selectedPhotos;

        public PhotoPickerViewModel()
        {
            Messenger.Default.Register("PhotosSelected", o =>
            {
                this.selectedPhotos = o as IEnumerable<Photo>;
                this.OnPropertyChanged("PhotosSelected");
            });
            this.photoPaths = new HashSet<string>();
            this.Photos = new ObservableCollection<Photo>();
            this.Status = string.Format(CultureInfo.CurrentUICulture, Resources.ChoosePhotos, MaxPhotoUploadLimit);
            this.AddPhotosCommand = new DelegateCommand(this.AddPhotos);
            this.DeletePhotosCommand = new DelegateCommand(() => this.RemovePhotos(this.selectedPhotos));
            this.ClearPhotosCommand = new DelegateCommand(this.ClearPhotos);
            this.Photos.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems.Cast<Photo>())
                        {
                            this.photoPaths.Add(item.FullPath);
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.Cast<Photo>())
                        {
                            this.photoPaths.Remove(item.FullPath);
                        }

                        break;

                    case NotifyCollectionChangedAction.Reset:
                        this.photoPaths.Clear();
                        break;
                }

                if (this.Photos.Count > 0)
                {
                    this.Status = string.Format(
                        CultureInfo.CurrentUICulture, Resources.ChosenPhotosStatus, this.Photos.Count, MaxPhotoUploadLimit - this.Photos.Count);
                }
                else
                {
                    this.Status = string.Format(CultureInfo.CurrentUICulture, Resources.ChoosePhotos, MaxPhotoUploadLimit);
                }

                this.OnPropertyChanged("HasPhotos");
            };
        }

        public ObservableCollection<Photo> Photos { get; private set; }

        public DateTime CapturedDate
        {
            get
            {
                return this.Photos.Max(photo => photo.Timestamp);
            }
        }

        private string status;
        public string Status
        {
            get { return this.status; }
            set { this.SetProperty(ref this.status, value); }
        }

        private string warningMessage;
        public string WarningMessage
        {
            get { return this.warningMessage; }
            set
            {
                if (this.SetProperty(ref this.warningMessage, value))
                {
                    this.OnPropertyChanged("HasWarnings");
                }
            }
        }

        private bool isProcessingPhotos;
        public bool IsProcessingPhotos
        {
            get { return this.isProcessingPhotos; }
            set { this.SetProperty(ref this.isProcessingPhotos, value); }
        }

        public bool HasWarnings
        {
            get { return !string.IsNullOrEmpty(this.WarningMessage); }
        }

        public bool HasPhotos
        {
            get { return this.Photos.Count > 0; }
        }

        public bool PhotosSelected
        {
            get { return this.selectedPhotos != null && this.selectedPhotos.Count() > 0; }
        }

        public ICommand AddPhotosCommand { get; private set; }

        public ICommand DeletePhotosCommand { get; private set; }

        public ICommand ClearPhotosCommand { get; private set; }

        public void RemovePhotos(IEnumerable<Photo> photosToRemove)
        {
            if (photosToRemove == null)
            {
                return;
            }

            foreach (var photo in photosToRemove)
            {
                this.Photos.Remove(photo);
            }
        }

        public void AddPhotos(IList<string> paths)
        {
            if (paths.Count + this.Photos.Count > MaxPhotoUploadLimit)
            {
                this.WarningMessage = string.Format(CultureInfo.CurrentUICulture, Resources.MaxUploadWarning, MaxPhotoUploadLimit);
            }
            else
            {
                try
                {
                    this.IsProcessingPhotos = true;
                    Task.Factory.StartNew(
                        () => paths.Except(this.photoPaths).Select(path => Photo.BuildPhotoFromPath(path)).ToArray(),
                        CancellationToken.None)
                        .ContinueWith(
                            photoTask =>
                            {
                                this.IsProcessingPhotos = false;
                                foreach (var photo in photoTask.Result)
                                {
                                    this.Photos.Add(photo);
                                }
                            },
                            CancellationToken.None,
                            TaskContinuationOptions.None,
                            TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch
                {
                    this.IsProcessingPhotos = false;
                }
            }
        }

        private void AddPhotos()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPEG|*.jpg";
            dlg.Multiselect = true;
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                this.AddPhotos(dlg.FileNames);
            }
        }

        private void ClearPhotos()
        {
            this.Photos.Clear();
        }

        public sealed class Photo
        {
            public string FullPath { get; private set; }

            public ImageSource Thumbnail { get; private set; }

            public DateTime Timestamp { get; private set; }

            public static Photo BuildPhotoFromPath(string path)
            {
                var photo = new Photo() { FullPath = path };

                // Create the thumbnail 
                var thumbnail = new BitmapImage();
                thumbnail.BeginInit();
                using (var fs = new FileStream(path, System.IO.FileMode.Open))
                {
                    var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    thumbnail.StreamSource = ms;
                }

                thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                thumbnail.DecodePixelHeight = 200;
                thumbnail.EndInit();
                thumbnail.Freeze();

                photo.Thumbnail = thumbnail;

                // HACK use GDI+ API to get the capture time property
                using (var image = Bitmap.FromFile(path))
                {
                    DateTime timestamp;
                    PropertyItem timestampProperty = image.PropertyItems.FirstOrDefault(pi => pi.Type == 2 && pi.Id == 306);
                    if (timestampProperty == null || !TryParseTimestamp(timestampProperty, out timestamp))
                    {
                        timestamp = new FileInfo(path).CreationTime;
                    }

                    photo.Timestamp = timestamp;
                }

                return photo;
            }

            private static bool TryParseTimestamp(PropertyItem timestampProperty, out DateTime timestamp)
            {
                timestamp = DateTime.Now;
                string jpegTimeStamp = Encoding.ASCII.GetString(timestampProperty.Value);
                string[] parts = jpegTimeStamp.Split();
                return parts.Length == 2 && DateTime.TryParse(parts[0].Replace(':', '-') + " " + parts[1], out timestamp);
            }
        }
    }
}
