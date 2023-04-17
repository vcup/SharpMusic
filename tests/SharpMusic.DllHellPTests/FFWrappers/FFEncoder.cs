namespace SharpMusic.DllHellPTests.FFWrappers; 

// ReSharper disable once InconsistentNaming
public unsafe class FFEncoder : IDisposable
{
    private readonly AVCodecContext* _codecCtx;

    public FFEncoder(AVCodecID codecId, AVSampleFormat format, AVChannelLayout* channelLayout, int sampleRate = 0)
    {
        AvCodec = avcodec_find_encoder(codecId);
        _codecCtx = avcodec_alloc_context3(AvCodec);
        _codecCtx->sample_fmt = format;
        av_channel_layout_copy(&_codecCtx->ch_layout, channelLayout is null ? AvCodec->ch_layouts : channelLayout);
        _codecCtx->sample_rate = sampleRate is 0
            ? AvCodec->supported_samplerates is not null
                ? *AvCodec->supported_samplerates
                : 44100
            : sampleRate;
    }

    public AVCodec* AvCodec { get; }
    public AVCodecContext* CodecCtx => _codecCtx;

    public void Dispose()
    {
        fixed (AVCodecContext** codecCtx = &_codecCtx)
        {
            avcodec_free_context(codecCtx);
        }
    }
}