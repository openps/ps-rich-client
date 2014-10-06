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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenPhotosynth.WebClient.DataContracts;

namespace OpenPhotosynth.WebClient
{
    /// <summary>
    /// A class for managing HTTP communication with the Photosynth web service.
    /// </summary>
    public class WebApi
    {
        private const int UploadBufferSize = 16 * 1024;
        private const int DefaultNumRows = 10;
        private const int DefaultOffset = 0;

        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        private Credentials credentials;

        public WebApi(Credentials credentials)
        {
            this.credentials = credentials;
        }

        /// <summary>
        /// Gets a link viewable in a web browser to the Photosynth collection with the specified ID.
        /// </summary>
        /// <param name="id">The collection's ID</param>
        /// <returns>A link to the collection.</returns>
        public Uri GetViewLink(Guid id)
        {
            return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}/view/{1}", this.credentials.PhotosynthUrl, id));
        }

        /// <summary>
        /// Gets metadata about the Photosynth collection with the specified ID.
        /// </summary>
        /// <param name="id">The collection's ID</param>
        /// <returns>The collection's metadata.</returns>
        public async Task<Media> GetMediaAsync(Guid id)
        {
            return await this.MakeRequestAsync<Media>("/media/" + id, HttpMethod.Get);
        }

        /// <summary>
        /// Gets the user's library of Photosynth collections. If the username does not belong to the current user,
        /// only public Photosynths will be returned.
        /// </summary>
        /// <param name="username">The username of the library to fetch.</param>
        /// <param name="numRows">The number of collections to return.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="collectionTypeFilter">Flags indicating which collection types to return.</param>
        /// <returns>The user's library.</returns>
        public async Task<MediaCollection> GetUserLibraryAsync(
            string username, int numRows = DefaultNumRows, int offset = DefaultOffset, CollectionTypeFilter collectionTypeFilter = CollectionTypeFilter.All)
        {
            return await this.MakeRequestAsync<MediaCollection>(
                "/users/" + username + "/media?" + BuildPagingQueryString(numRows, offset, collectionTypeFilter), HttpMethod.Get);
        }

        /// <summary>
        /// Gets the current user's library of Photosynth collections. The current user is whoever supplied the access token as a credential.
        /// </summary>
        /// <param name="numRows">The number of collections to return.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="collectionTypeFilter">Flags indicating which collection types to return.</param>
        /// <returns>The user's library.</returns>
        public async Task<MediaCollection> GetMyLibraryAsync(
            int numRows = DefaultNumRows, int offset = DefaultOffset, CollectionTypeFilter collectionTypeFilter = CollectionTypeFilter.All)
        {
            return await this.MakeRequestAsync<MediaCollection>(
                "/me/media?" + BuildPagingQueryString(numRows, offset, collectionTypeFilter), HttpMethod.Get);
        }

        /// <summary>
        /// Creates a placeholder for a new Photosynth collection.
        /// </summary>
        /// <param name="createMediaRequest">The type of collection to create.</param>
        /// <returns>Metadata about the newly created collection.</returns>
        public async Task<CreateMediaResponse> CreateMediaAsync(CreateMediaRequest createMediaRequest)
        {
            return await this.MakeRequestAsync<CreateMediaResponse>("/media", HttpMethod.Put, createMediaRequest);
        }

        /// <summary>
        /// Adds one or more files that will be uploaded as part of a new Photosynth collection.
        /// </summary>
        /// <param name="id">The Photosynth ID of the collection to add the files to.</param>
        /// <param name="addFilesRequest">Metadata about the files being uploaded.</param>
        /// <returns>Metadata used to upload the files themselves.</returns>
        public async Task<AddFilesResponse> AddFilesAsync(Guid id, AddFilesRequest addFilesRequest)
        {
            return await this.MakeRequestAsync<AddFilesResponse>("/media/" + id + "/files", HttpMethod.Put, addFilesRequest);
        }

        /// <summary>
        /// Edits properties of the specified Photosynth collection. Can be used on newly created Photosynth collections
        /// that are not yet available for viewing, or existing Photosynth collections.
        /// </summary>
        /// <param name="id">The ID of the collection to edit.</param>
        /// <param name="editMediaRequest">The metadata to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task EditMediaAsync(Guid id, EditMediaRequest editMediaRequest)
        {
            await this.MakeRequestAsync<bool>("/media/" + id, HttpMethod.Post, editMediaRequest);
        }

        /// <summary>
        /// Deletes the specified Photosynth collection. Can only be performed by the owner of the collection.
        /// </summary>
        /// <param name="id">The ID of the collection to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteMediaAsync(Guid id)
        {
            await this.MakeRequestAsync<bool>("/media/" + id, HttpMethod.Delete);
        }

        /// <summary>
        /// Gets metadata about the current user.
        /// </summary>
        /// <returns>User metadata.</returns>
        public async Task<UserInfo> GetUserInfoAsync()
        {
            try
            {
                return await this.MakeRequestAsync<UserInfo>("/me", HttpMethod.Get);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>
        /// Uploads a file to the Photosynth service.
        /// </summary>
        /// <param name="uploadUri">The URI to upload to</param>
        /// <param name="stream">A stream representing the file to be uploaded.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <param name="progressCallback">An optional callback for reporting progress, with the number of bytes uploaded as a parameter.</param>
        /// <returns>True if the file was successfully uploaded, false otherwise.</returns>
        public async Task<bool> UploadFileAsync(Uri uploadUri, Stream stream, CancellationToken cancellationToken, Action<long> progressCallback = null)
        {
            HttpWebRequest webRequest = WebRequest.CreateHttp(uploadUri);
            webRequest.Method = HttpMethod.Post.Method;                
            string authorizationHeader = string.Format(CultureInfo.InvariantCulture, this.BuildAuthorizationHeader());
            webRequest.Headers[HttpRequestHeader.Authorization] = authorizationHeader;

            // Upload the file by copying from the file stream to the request stream.
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[UploadBufferSize];
            long bytesSent = 0;

            var writeToRequestStream = new TaskCompletionSource<object>();            
            webRequest.BeginGetRequestStream(ar =>
            {
                using (Stream requestStream = webRequest.EndGetRequestStream(ar))
                {
                    int count;
                    while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        requestStream.Write(buffer, 0, count);
                        bytesSent += count;
                        if (progressCallback != null)
                        {
                            progressCallback(bytesSent);
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }

                writeToRequestStream.SetResult(null);
            },
            null);

            await writeToRequestStream.Task;
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            // Get the web response and check the status code.
            var readResponse = new TaskCompletionSource<bool>();
            webRequest.BeginGetResponse(ar =>
            {
                try
                {
                    using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.EndGetResponse(ar))
                    {
                        readResponse.SetResult(webResponse.StatusCode == HttpStatusCode.OK);
                    }
                }
                catch (WebException)
                {
                    readResponse.SetResult(false);
                }
            },
            null);

            return await readResponse.Task;
        }

        private async Task<T> MakeRequestAsync<T>(string pathAndQueryString, HttpMethod method, object requestBody = null)
        {
            T responseBody = default(T);
            using (var client = this.BuildHttpClient())
            {
                string requestUrl = this.BuildUrl(pathAndQueryString);
                HttpResponseMessage response;
                if (method == HttpMethod.Get)
                {
                    response = await client.GetAsync(requestUrl);
                }
                else if (method == HttpMethod.Delete)
                {
                    response = await client.DeleteAsync(requestUrl);
                }
                else if (method == HttpMethod.Post)
                {
                    var content = new StringContent(JsonConvert.SerializeObject(requestBody, jsonSerializerSettings), Encoding.UTF8);
                    response = await client.PostAsync(requestUrl, content);
                }
                else if (method == HttpMethod.Put)
                {
                    var content = new StringContent(JsonConvert.SerializeObject(requestBody, jsonSerializerSettings), Encoding.UTF8);
                    response = await client.PutAsync(requestUrl, content);
                }
                else
                {
                    throw new ArgumentException("HTTP method not supported", "method");
                }

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(json))
                    {
                        responseBody = JsonConvert.DeserializeObject<T>(json);
                    }                    
                }
                else
                {
                    // Throws an exception
                    response.EnsureSuccessStatusCode();
                }
            }

            return responseBody;
        }

        private HttpClient BuildHttpClient()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var client = new HttpClient(handler);            
            client.DefaultRequestHeaders.Add("Authorization", this.BuildAuthorizationHeader());
            return client;
        }

        private string BuildUrl(string pathAndQueryString)
        {
            return this.credentials.PhotosynthUrl + "/rest/" + this.credentials.ApiVersion + pathAndQueryString;
        }

        private string BuildAuthorizationHeader()
        {
            return "Bearer " + this.credentials.AccessToken;
        }

        private static string BuildPagingQueryString(int numRows, int offset, CollectionTypeFilter collectionTypeFilter)
        {            
            StringBuilder sb = new StringBuilder();
            sb.Append("numRows=").Append(numRows).Append("&offset=").Append(offset);
            if (collectionTypeFilter != CollectionTypeFilter.All)
            {
                List<string> collectionTypes = new List<string>();
                foreach (var collectionType in new[] { CollectionTypeFilter.Panos, CollectionTypeFilter.Synths, CollectionTypeFilter.SynthPackets})
                {
                    if (collectionTypeFilter.HasFlag(collectionType))
                    {
                        collectionTypes.Add(collectionType.ToString());
                    }
                }

                if (collectionTypes.Any())
                {
                    sb.Append("&collectionTypeFilter=").Append(string.Join(",", collectionTypes));
                }
            }

            return sb.ToString();
        }
    }
}
