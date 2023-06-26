using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using SharpMusic.DllHellP.Abstract.Delegate;
using SharpMusic.DllHellPTests.Utils;

namespace SharpMusic.DllHellPTests.ExtensionsTests;

public class FFmpegExtensionsTests
{

    [Test]
    public void MoveNextAudioPacket_PassNonAudioPacketSequence_SkipAllPacket()
    {
        // arrange
        var formats = Enumerable.Repeat(SampleFormat.None, 5).GetEnumerator();
        var source = new CustomFFmpegSource(formats);

        // act
        source.MoveNextAudioPacket();

        // assert
        Assert.That(formats.MoveNext(), Is.False);
    }

    [Test]
    public void MoveNextAudioPacket_PassPacketSequenceContainAnAudioPacket_MoveToAudioPacket()
    {
        // arrange
        var formats = new[]
        {
            SampleFormat.None, SampleFormat.None, SampleFormat.Unsigned8,
            SampleFormat.None, SampleFormat.None, SampleFormat.None,
        };
        var source = new CustomFFmpegSource(formats.GetShareEnumerator());

        // act
        source.MoveNextAudioPacket();
        source.MoveNext();

        // assert
        Assert.That(formats.GetShareEnumerable().All(i => i is SampleFormat.None), Is.True);
    }

    [Test]
    public void MoveNextAudioPacket_PassAudioPacketFollowsNonAudioPacket_MoveToHead()
    {
        // arrange
        var formats = new[]
        {
            SampleFormat.Unsigned8, SampleFormat.None, SampleFormat.None,
            SampleFormat.None, SampleFormat.None, SampleFormat.None,
        };
        var source = new CustomFFmpegSource(formats.GetShareEnumerator());

        // act
        source.MoveNextAudioPacket();
        source.MoveNext();

        // assert
        Assert.That(formats.GetShareEnumerable().Count(), Is.EqualTo(5));
    }

    [Test]
    public void MoveNext_UsingNotExistStreamIndex_SkipAllPacket()
    {
        // arrange
        var indexOfPackets = Enumerable.Repeat(0, 5).GetEnumerator();
        var source = new CustomFFmpegSource(indexOfPackets);

        // act
        source.MoveNext(1);

        // assert
        Assert.That(indexOfPackets.MoveNext(), Is.False);
    }

    [Test]
    public void MoveNext_UsingSequenceContainAnPacketWithSpecifyStream_MoveToPacketWithSpecifyStream()
    {
        // arrange
        var indexOfPackets = new[] { 0, 0, 1, 0, 0, 0 };
        var source = new CustomFFmpegSource(indexOfPackets.GetShareEnumerator());

        // act
        source.MoveNext(1);
        source.MoveNext();

        // assert
        Assert.That(indexOfPackets.GetShareEnumerable(), Is.All.EqualTo(0));
    }

    [Test]
    public void MoveNext_PacketOfSpecifyStreamFollowsPacketOfOtherStream_MoveToHead()
    {
        // arrange
        var indexOfPackets = new[] { 1, 0, 0, 0, 0, 0 };
        var source = new CustomFFmpegSource(indexOfPackets.GetShareEnumerator());

        // act
        source.MoveNext(1);
        source.MoveNext();

        // assert
        Assert.That(indexOfPackets.GetShareEnumerable().Count(), Is.EqualTo(5));
    }

    [ExcludeFromCodeCoverage(Justification = "temp class")]
    private class CustomFFmpegSource : IFFmpegSource
    {
        private readonly IEnumerator<SampleFormat>? _formats;
        private readonly IEnumerator<int>? _indexOfPackets;
        private readonly bool _isFormat;
        private bool _disposed;

        public CustomFFmpegSource(IEnumerator<SampleFormat> formats)
        {
            _formats = formats;
            _isFormat = true;
        }

        public unsafe CustomFFmpegSource(IEnumerator<int> indexOfPackets)
        {
            _indexOfPackets = indexOfPackets;
            Stream = (AVStream*)Marshal.AllocHGlobal(sizeof(AVStream));
        }

        public unsafe bool MoveNext()
        {
            bool result;
            if (_isFormat)
            {
                result = _formats!.MoveNext();
                Format = _formats.Current;
                return result;
            }

            result = _indexOfPackets!.MoveNext();
            Stream->index = _indexOfPackets.Current;
            return result;
        }

        public IntPtr Current => IntPtr.Zero;

        public SampleFormat Format { get; private set; }

        public unsafe AVStream* Stream { get; }

        public unsafe void Dispose()
        {
            if (_isFormat || _disposed) return;
            Marshal.FreeHGlobal((IntPtr)Stream);
            _disposed = true;
        }

        ~CustomFFmpegSource()
        {
            Dispose();
        }

        #region non-impl members

        public void SetCurrentStream(int index) { }

        public int LengthStreams => default;

        public AVChannelLayout ChannelLayout => default;

        public event FFmpegSourceEofHandler? SourceEofEvent;
        public void WriteHeader(){ }
        public bool WritePacket() => default;

        public void WriteAndCloseSource() { }

        public Uri Uri => null!;
        public TimeSpan Duration => TimeSpan.Zero;
        public long BitRate => default;
        public int BitDepth => default;
        public int Channels => default;
        public int SampleRate => default;
        
        public TimeSpan Position { get; set; }

        public void ResetStream() { }

        public void SeekStream(TimeSpan time) { }

        public unsafe void AddStream(AVCodecParameters* parameters) { }

        public void Reset() { }

        object IEnumerator.Current => Current;

        #endregion
    }
}