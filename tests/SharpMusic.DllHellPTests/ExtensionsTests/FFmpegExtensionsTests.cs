using System.Collections;
using System.Diagnostics.CodeAnalysis;
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

    [ExcludeFromCodeCoverage(Justification = "temp class")]
    private class CustomFFmpegSource : IFFmpegSource
    {
        private readonly IEnumerator<SampleFormat> _formats;

        public CustomFFmpegSource(IEnumerator<SampleFormat> formats)
        {
            _formats = formats;
        }

        public bool MoveNext()
        {
            var result = _formats.MoveNext();
            Format = _formats.Current;
            return result;
        }

        public IntPtr Current => IntPtr.Zero;

        public SampleFormat Format { get; private set; }

        #region non-impl members

        public void SetCurrentStream(int index) { }

        public unsafe AVStream* Stream => null!;

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

        public void Dispose() { }

        #endregion
    }
}