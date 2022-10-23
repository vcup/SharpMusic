using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using SharpMusic.DllHellP.LowLevelImpl;

namespace SharpMusic.DllHellP.Utils;

public class SdlAudioProvider
{
    private readonly IEnumerator<AVFrame> _frames;
    private readonly FFmpegResampler _resampler;
    private byte[] _audioBuffer;
    private int _index;

    public SdlAudioProvider(IEnumerable<AVFrame> frames, FFmpegResampler resampler)
    {
        _frames = frames.GetEnumerator();
        _resampler = resampler;
        _audioBuffer = Array.Empty<byte>();
    }

    public unsafe void AudioCallback(IntPtr userdata, IntPtr stream, int len)
    {
        var entry = DateTime.Now;
        while (len > 0)
        {
            if (_index >= _audioBuffer.Length)
            {
                if (!_frames.MoveNext()) break;
                var frame = _frames.Current;
                _audioBuffer = _resampler.ResampleFrame(&frame);
                _index = 0;
                Debug.Assert(_audioBuffer.Length is not 0);
            }

            var processLen = _audioBuffer.Length - _index;
            if (processLen > len)
            {
                processLen = len;
            }

                Marshal.Copy(_audioBuffer, _index, stream, processLen);

            len -= processLen;
            stream += processLen;
            _index += processLen;
        }
    }
}