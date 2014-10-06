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

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenPhotosynth.RichClient
{
    /// <summary>
    /// Interaction logic for PhotoPickerControl.xaml
    /// </summary>
    public partial class PhotoPickerControl : UserControl
    {
        public static readonly DependencyProperty ThumbnailHeightProperty =
            DependencyProperty.Register("ThumbnailHeight", typeof(int), typeof(PhotoPickerControl), new PropertyMetadata(240));

        private ScrollViewer scrollViewer;       

        public PhotoPickerControl()
        {
            InitializeComponent();

            // Allow users to remove files from the photo picker by pressing the Delete key
            this.PhotoListView.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Delete)
                {
                    var photoPickerViewModel = this.DataContext as PhotoPickerViewModel;
                    if (photoPickerViewModel != null)
                    {
                        photoPickerViewModel.RemovePhotos(this.PhotoListView.SelectedItems.Cast<PhotoPickerViewModel.Photo>().ToArray());
                    }
                }
            };

            // Enable horizontal scrolling in the list view of thumbnails
            this.PhotoListView.PreviewMouseWheel += (_, e) =>
            {
                if (this.scrollViewer == null)
                {
                    this.scrollViewer = this.PhotoListView.FindVisualChild<ScrollViewer>();
                }

                if (this.scrollViewer != null)
                {
                    this.scrollViewer.ScrollToHorizontalOffset(scrollViewer.ContentHorizontalOffset + (e.Delta < 0 ? 1 : -1));
                }
            };

            this.PhotoListView.SelectionChanged += (_, e) =>
            {
                Messenger.Default.Send("PhotosSelected", this.PhotoListView.SelectedItems.Cast<PhotoPickerViewModel.Photo>().ToArray());
            };
        }

        public int ThumbnailHeight
        {
            get { return (int)GetValue(ThumbnailHeightProperty); }
            set { SetValue(ThumbnailHeightProperty, value); }
        }
    }
}
