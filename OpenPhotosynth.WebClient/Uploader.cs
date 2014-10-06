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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenPhotosynth.WebClient.DataContracts;

namespace OpenPhotosynth.WebClient
{
    /// <summary>
    /// Data-bindable object to manage uploading of Photosynth Synth Packets for processing
    /// in the cloud.
    /// </summary>
    public class Uploader : BindableBase
    {
        /// <summary>
        /// Upload ten megabytes before estimating speed.
        /// </summary>
        private const long MinimumByteCount = 10 * 1024 * 1024; 

        public Uploader()
        {
            this.FailedChunks = new ObservableCollection<ChunkInfo>();
        }

        private bool isUploading;

        /// <summary>
        /// Gets a value indicating whether chunks are being uploaded.
        /// </summary>
        public bool IsUploading
        {
            get { return this.isUploading; }
            private set
            {
                if (this.SetProperty(ref this.isUploading, value))
                {
                    this.OnPropertyChanged("IsRetryable");
                }
            }
        }

        private string status;

        /// <summary>
        /// Gets a human-readable friendly string representing the overall status of the upload.
        /// </summary>
        public string Status
        {
            get { return this.status; }
            private set { this.SetProperty(ref this.status, value); }
        }

        private double progress;

        /// <summary>
        /// Gets upload progress as a percentage between 0 and 100.
        /// </summary>
        public double ProgressPercentage
        {
            get { return this.progress; }
            private set { this.SetProperty(ref this.progress, value); }
        }

        private string estimatedTimeRemaining;

        /// <summary>
        /// Gets an estimate of the time remaining for the upload to complete as a human-friendly English string.
        /// </summary>
        public string EstimatedTimeRemaining
        {
            get { return this.estimatedTimeRemaining; }
            private set { this.SetProperty(ref this.estimatedTimeRemaining, value); }
        }

        private Guid id;

        /// <summary>
        /// The unique Photosynth ID allocated to the newly uploaded synth packet.
        /// </summary>
        public Guid Id
        {
            get { return this.id; }
            private set { this.SetProperty(ref this.id, value); }
        }

        /// <summary>
        /// A collection of chunks that failed to upload.
        /// </summary>
        public ObservableCollection<ChunkInfo> FailedChunks { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current upload is in a retryable state.
        /// </summary>
        public bool IsRetryable { get { return !this.IsUploading && this.FailedChunks.Any(); } }

        /// <summary>
        /// Asynchronously uploads a synth packet to the Photosynth upload service for processing in the cloud.
        /// </summary>
        /// <param name="webApi">A WebApi object for handling HTTP communication with the Photosynth web service</param>
        /// <param name="fileStorage">An IFileStorage implementation for handling file I/O</param>
        /// <param name="synthPacketUpload">Metadata for the synth packet to be uploaded.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <param name="parallelism">The maximum number of chunks that can be uploaded in parallel.</param>
        /// <returns>The task object representing the asynchronous operation</returns>
        public async Task UploadSynthPacketAsync(
            WebApi webApi, IFileStorage fileStorage, SynthPacketUpload synthPacketUpload, CancellationToken cancellationToken, int parallelism = 2)
        {
            if (this.IsUploading)
            {
                throw new InvalidOperationException("Only one upload at a time per Uploader instance is allowed.");
            }

            if (string.IsNullOrWhiteSpace(synthPacketUpload.Title))
            {
                throw new ArgumentException("Must provide a Title for the SynthPacketUpload.");
            }

            try
            {
                this.IsUploading = true;
                this.Id = Guid.Empty;
                this.ProgressPercentage = 0;
                this.EstimatedTimeRemaining = string.Empty;
                this.FailedChunks.Clear();

                // Create a new Photosynth collection
                this.Status = "Creating collection...";
                var createCollectionRequest = new CreateMediaRequest() { UploadType = UploadType.SynthPacketFromRawImages };
                var createCollectionResponse = await webApi.CreateMediaAsync(createCollectionRequest);
                this.ProgressPercentage = 1;

                this.Id = createCollectionResponse.Id;

                // Add images to the collection and get upload URLs for each file
                this.Status = "Adding images to collection...";
                var addFilesRequest = new AddFilesRequest();
                for (int i = 0; i < synthPacketUpload.PhotoPaths.Count; i++)
                {
                    addFilesRequest.Files.Add(new AddFileRequest()
                    {
                        Id = i.ToString(),
                        Extension = "jpg",
                        Order = i.ToString("000"),
                        ChunkCount = 1,
                    });
                }

                var addFilesResponse = await webApi.AddFilesAsync(this.Id, addFilesRequest);
                this.ProgressPercentage = 2;

                // Get info about all the files that need uploading
                List<ChunkInfo> chunks = addFilesRequest.Files.Join(
                    addFilesResponse.Files,
                    req => req.Id,
                    resp => resp.ClientId,
                    (req, resp) =>
                    {
                        string photoPath = synthPacketUpload.PhotoPaths[Int32.Parse(resp.ClientId)];
                        FileChunk chunk = resp.Chunks.First();
                        return new ChunkInfo(chunk.UploadUri, photoPath, fileStorage.GetFileSize(photoPath));
                    }).ToList();

                // Commit the collection, no more files can be added
                this.Status = "Setting properties for the collection...";
                var editMediaRequest = new EditMediaRequest()
                {
                    Name = synthPacketUpload.Title,
                    Description = synthPacketUpload.Description,
                    ImageCount = synthPacketUpload.PhotoPaths.Count,
                    PrivacyLevel = synthPacketUpload.PrivacyLevel,
                    CapturedDate = synthPacketUpload.CapturedDate,
                    Committed = true,
                    SynthPacket = new SynthPacket()
                    {
                        License = synthPacketUpload.License,
                    },
                    UploadHints = synthPacketUpload.Topology.ToString()
                    // TODO geotag
                };
                if (synthPacketUpload.Tags.Any())
                {
                    editMediaRequest.Tags = string.Join(",", synthPacketUpload.Tags);
                }

                await webApi.EditMediaAsync(this.Id, editMediaRequest);
                this.ProgressPercentage = 3;

                var failedChunks = await this.UploadChunksAsync(webApi, fileStorage, chunks, 3, cancellationToken, parallelism);
                foreach (var failedChunk in failedChunks)
                {
                    this.FailedChunks.Add(failedChunk);
                }

                this.ProgressPercentage = 100;
                if (this.FailedChunks.Any())
                {
                    this.Status = string.Format(CultureInfo.CurrentUICulture, "{0} files failed to upload.", this.FailedChunks.Count);
                }
                else
                {
                    this.Status = string.Format(CultureInfo.CurrentUICulture, "{0} files successfully uploaded.", chunks.Count);
                }
            }
            catch (Exception e)
            {
                this.Status = string.Format(CultureInfo.CurrentUICulture, "Error: {0}", e.Message);
            }
            finally
            {
                this.IsUploading = false;
            }
        }

        /// <summary>
        /// Asynchronously resumes uploading a synth packet to the Photosynth upload service for processing in the cloud.
        /// </summary>
        /// <param name="webApi">A WebApi object for handling HTTP communication with the Photosynth web service</param>
        /// <param name="fileStorage">An IFileStorage implementation for handling file I/O</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <param name="parallelism">The maximum number of chunks that can be uploaded in parallel.</param>
        /// <returns>The task object representing the asynchronous operation</returns>
        public async Task ResumeUploadAsync(WebApi webApi, IFileStorage fileStorage, CancellationToken cancellationToken, int parallelism = 2)
        {
            if (this.IsUploading)
            {
                throw new InvalidOperationException("Only 1 upload per Uploader instance is allowed.");
            }

            if (!this.FailedChunks.Any())
            {
                return;
            }

            try
            {
                this.IsUploading = true;
                this.ProgressPercentage = 0;
                this.EstimatedTimeRemaining = string.Empty;

                var failedChunks = await this.UploadChunksAsync(webApi, fileStorage, this.FailedChunks.ToList(), 0, cancellationToken, parallelism);
                this.FailedChunks.Clear();
                foreach (var failedChunk in failedChunks)
                {
                    this.FailedChunks.Add(failedChunk);
                }

                this.ProgressPercentage = 100;
            }
            finally
            {
                this.IsUploading = false;
            }
        }

        private async Task<IEnumerable<ChunkInfo>> UploadChunksAsync(
            WebApi webApi, IFileStorage fileStorage, List<ChunkInfo> chunks, int progressBaseline, CancellationToken cancellationToken, int parallelism)
        {
            // Assume all chunks failed until successfully uploaded
            var failedChunks = new List<ChunkInfo>(chunks.ToArray());
            long totalBytes = chunks.Sum(chunk => chunk.TotalBytes);
            if (totalBytes > MinimumByteCount)
            {
                this.EstimatedTimeRemaining = "Estimating remaining time";
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Create a callback that updates the progress and remaining time estimate
            Action updateProgress = () =>
            {
                long totalBytesSent = chunks.Sum(c => c.BytesSent);
                if (totalBytesSent >= MinimumByteCount)
                {
                    long totalBytesRemaining = totalBytes - totalBytesSent;
                    TimeSpan remainingTime = TimeSpan.FromSeconds(
                        Math.Max(1, Math.Round((double)totalBytesRemaining * stopwatch.Elapsed.TotalSeconds / (double)totalBytesSent)));
                    double mbps = (double)totalBytesSent / stopwatch.Elapsed.TotalSeconds * 8e-6;
                    this.EstimatedTimeRemaining = string.Format(
                        CultureInfo.CurrentCulture, "About {0} remaining ({1:#,##0.##} Mbps)", remainingTime.FormatTimeSpan(), mbps);
                }

                double progressFraction = totalBytesSent / (double)totalBytes;
                this.ProgressPercentage = progressBaseline + 90 * progressFraction;
            };

            this.Status = "Uploading files...";
            Action<IEnumerable<ChunkInfo>> uploadChunks = chunkSlice =>
            {
                foreach (var chunk in chunkSlice)
                {
                    using (var stream = fileStorage.GetFileStream(chunk.FilePath, FileMode.Open))
                    {
                        bool uploaded = webApi.UploadFileAsync(chunk.UploadUri, stream, cancellationToken, bytesSent =>
                        {
                            chunk.BytesSent = bytesSent;                            
                            updateProgress();
                        }).Result;
                        if (uploaded)
                        {
                            chunk.BytesSent = chunk.TotalBytes;
                            lock (failedChunks)
                            {
                                failedChunks.Remove(chunk);
                            }
                        }

                        updateProgress();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            };

            // Split into n segments and assign a task to each
            int sliceCount = Math.Min(chunks.Count, parallelism);
            int sliceLength = chunks.Count / sliceCount;
            await Task.WhenAll(Enumerable.Range(0, sliceCount).Select(i =>
                Task.Run(() =>
                {
                    if (i == sliceCount - 1)
                    {
                        // The last slice may be larger than the rest
                        uploadChunks(chunks.Skip(i * sliceLength));
                    }
                    else
                    {
                        uploadChunks(chunks.Skip(i * sliceLength).Take(sliceLength));
                    }
                })).ToArray());

            // Measure the upload rate in megabits per second. (There are 8 bits per byte and 10^6 bits per megabit, so we scale bytes-per-second by 8*10^-6.)
            stopwatch.Stop();
            double averageMbps = (double)totalBytes / stopwatch.Elapsed.TotalSeconds * 8e-6;
            this.EstimatedTimeRemaining = string.Format("Average upload speed: {0:#,##0.##} Mbps", averageMbps);

            return failedChunks;
        }

        /// <summary>
        /// Metadata about the synth packet to be uploaded.
        /// </summary>
        public class SynthPacketUpload
        {
            /// <summary>
            /// The title of the synth packet.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// An optional description for the synth packet.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// The tags for the synth packet.
            /// </summary>
            public IEnumerable<string> Tags { get; set; }

            /// <summary>
            /// Whether the synth packet will be visible in search results.
            /// </summary>
            public PrivacyLevel PrivacyLevel { get; set; }

            /// <summary>
            /// The license associated with the synth packet.
            /// </summary>
            public License License { get; set; }

            /// <summary>
            /// The topology of the synth packet.
            /// </summary>
            public Topology Topology { get; set; }

            /// <summary>
            /// The date the synth packet was captured.
            /// </summary>
            public DateTime CapturedDate { get; set; }

            /// <summary>
            /// The paths to the image files that make up the synth packet.
            /// </summary>
            public IList<string> PhotoPaths { get; set; }
        }
    }
}
