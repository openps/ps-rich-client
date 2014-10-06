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
using Newtonsoft.Json;

namespace OpenPhotosynth.WebClient.DataContracts
{
    public sealed class AddFileRequest
    {
        public string Extension { get; set; }

        public string Id { get; set; }

        public string RelativePath { get; set; }

        public string Hash { get; set; }

        public int ChunkCount { get; set; }

        public string Order { get; set; }
    }

    public sealed class AddFileResponse
    {
        public string ClientId { get; set; }

        public bool AlreadyUploaded { get; set; }

        public Uri PublicUri { get; set; }

        public List<FileChunk> Chunks { get; set; }
    }

    public sealed class AddFilesRequest
    {
        public AddFilesRequest()
        {
            this.Files = new List<AddFileRequest>();
        }

        public List<AddFileRequest> Files { get; set; }
    }

    public sealed class AddFilesResponse
    {
        public List<AddFileResponse> Files { get; set; }
    }

    public sealed class CreateMediaRequest
    {
        public UploadType UploadType { get; set; }
    }

    public sealed class CreateMediaResponse
    {
        public Guid Id { get; set; }

        public string JsonFilename { get; set; }

        public string ThumbFilename { get; set; }
    }

    public sealed class EditMediaRequest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int? ImageCount { get; set; }

        public GeoTag GeoTag { get; set; }

        public double? Rotation { get; set; }

        public int? MapZoomLevel { get; set; }

        public PrivacyLevel? PrivacyLevel { get; set; }

        public DateTime? CapturedDate { get; set; }

        public bool? Committed { get; set; }

        public string UploadHints { get; set; }

        public string Tags { get; set; }

        public SynthPacket SynthPacket { get; set; }

        public bool? NotifyOnComplete { get; set; }
    }

    public sealed class FileChunk
    {
        public int Id { get; set; }

        public Uri UploadUri { get; set; }

        public string Status { get; set; }
    }

    public sealed class GeoTag
    {
        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }

    public sealed class Media
    {
        public Guid Id { get; set; }

        public MediaStatus Status { get; set; }

        public Panorama Panorama { get; set; }

        public Synth Synth { get; set; }

        public SynthPacket SynthPacket { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string CollectionUrl { get; set; }

        public string ThumbnailUrl { get; set; }

        public string ViewUrl { get; set; }

        public string EditUrl { get; set; }

        public string PrivacyLevel { get; set; }

        public GeoTag GeoTag { get; set; }

        public int? MapZoomLevel { get; set; }

        public DateTime? UploadDate { get; set; }

        public DateTime? CapturedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ImageCount { get; set; }

        public string OwnerUsername { get; set; }

        public int Viewings { get; set; }

        public int FavoriteCount { get; set; }

        public int CommentCount { get; set; }

        public float? Rank { get; set; }

        public bool? Committed { get; set; }
    }

    public sealed class MediaCollection
    {
        public int TotalResults { get; set; }

        public List<Media> Collections { get; set; }
    }

    public sealed class Panorama
    {
        public Decimal Megapixels { get; set; }
    }

    public sealed class Synth
    {
        public int SynthinessScore { get; set; }
    }

    public sealed class SynthPacket
    {
        public Topology? Topology { get; set; }

        public License? License { get; set; }

        public int? StartingImageIndex { get; set; }

        public bool? ReverseDirection { get; set; }

        public string DominantColor { get; set; }

        public int? OmittedCount { get; set; }

        public string WarningsBaseUrl { get; set; }
    }

    public class UserInfo
    {
        public string Username { get; set; }

        public string Description { get; set; }

        public int FavoriteCount { get; set; }

        public int FollowerCount { get; set; }

        public int FollowingCount { get; set; }

        public UserMediaInfo Panoramas { get; set; }

        public UserMediaInfo Synths { get; set; }

        public UserMediaInfo SynthPackets { get; set; }
    }

    public class UserMediaInfo
    {
        public int PublicCount { get; set; }

        public int UnlistedCount { get; set; }
    }
}
