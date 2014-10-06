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

using System.Runtime.Serialization;

namespace OpenPhotosynth.WebClient
{
    public enum CollectionTypeFilter
    {
        None = 0,
        Synths = 1,
        Panos = 2,
        SynthPackets = 8,
        All = Synths | Panos | SynthPackets,
    }

    public enum License
    {
        AllRightsReserved = 0,

        CCAttribution = 1,

        CCAttributionShareAlike = 2,

        CCAttributionNoDerivatives = 3,

        CCAttributionNonCommercial = 4,

        CCAttributionNonCommercialShareAlike = 5,

        CCAttributionNonCommercialNoDerivatives = 6,

        PublicDomain = 7
    }

    [DataContract]
    public enum MediaStatus
    {
        [EnumMember]
        Available,

        [EnumMember]
        Pending,

        [EnumMember]
        Committed,

        [EnumMember]
        Queued,

        [EnumMember]
        Processing,

        [EnumMember]
        Succeeded,

        [EnumMember]
        InsufficientCameras,

        [EnumMember]
        Failed,

        [EnumMember]
        Unknown,

        [EnumMember]
        UserCanceled
    }

    public enum PrivacyLevel
    {
        Public = 0,
        Unlisted = 1
    }

    public enum Topology : byte
    {
        [EnumMember(Value = "Unknown")]
        Unknown = 0,

        [EnumMember(Value = "Spin")]
        Spin = 1,

        [EnumMember(Value = "Panorama")]
        Panorama = 2,

        [EnumMember(Value = "Wall")]
        Wall = 3,

        [EnumMember(Value = "Walk")]
        Walk = 4,
    }

    public enum UploadType : byte
    {
        None = 0,

        [EnumMember(Value = "CubeFacePano")]
        PanoramaFromCubeFaces = 1,

        [EnumMember(Value = "RawImagePano")]
        PanoFromRawImages = 2,

        [EnumMember(Value = "RawImageSpin")]
        SynthPacketFromRawImages = 3,

        [EnumMember(Value = "SynthPacketProcessed")]
        SynthPacketProcessed = 4
    }
}
