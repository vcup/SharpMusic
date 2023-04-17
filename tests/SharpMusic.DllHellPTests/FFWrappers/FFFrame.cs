using System.Collections;

namespace SharpMusic.DllHellPTests.FFWrappers;

// ReSharper disable once InconsistentNaming
public unsafe class FFFrame : IEnumerable<byte>, IDisposable
{
    private readonly AVFrame* _frame;

    public FFFrame(AVSampleFormat format, AVChannelLayout channelLayout, int samples)
    {
        _frame = av_frame_alloc();
        _frame->format = (int)format;
        av_channel_layout_copy(&_frame->ch_layout, &channelLayout);
        _frame->nb_samples = samples;
        av_frame_get_buffer(_frame, 0);
    }

    public AVFrame* Frame => _frame;

    public void Dispose()
    {
        fixed (AVFrame** frame = &_frame)
        {
            av_frame_free(frame);
        }
    }

    public IEnumerator<byte> GetEnumerator()
    {
        return new Enumerator(_frame);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class Enumerator : IEnumerator<byte>
    {
        private readonly AVFrame* _frame;
        private readonly int _lenght;
        private readonly bool _isPacket;
        private uint _index;
        private uint _channel;

        public Enumerator(AVFrame* frame)
        {
            _frame = frame;
            _lenght = frame->nb_samples * av_get_bytes_per_sample((AVSampleFormat)frame->format);
            _isPacket = av_sample_fmt_is_planar((AVSampleFormat)frame->format) is 0;
            if (_isPacket) _lenght *= _frame->ch_layout.nb_channels;
        }

        public bool MoveNext()
        {
            if (_index >= _lenght)
            {
                if (_isPacket) return false;
                if (++_channel >= _frame->ch_layout.nb_channels) return false;
                _index = 0;
            }

            Current = _frame->extended_data[_channel][_index++];
            return true;
        }

        public void Reset() => _index = 0;

        public byte Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}