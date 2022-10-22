using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using SharpMusic.DllHellP.LowLevelImpl;

namespace SharpMusic.DllHellP.Utils;

public class SdlAudioProvider
{
    private readonly IEnumerable<AVFrame> _frames;
    private readonly FFmpegResampler _resampler;
    private byte[] _audioBuffer;
    private int _index;

    public SdlAudioProvider(IEnumerable<AVFrame> frames, FFmpegResampler resampler)
    {
        _frames = frames;
        _resampler = resampler;
        _audioBuffer = Array.Empty<byte>();
    }

    public unsafe void AudioCallback(IntPtr userdata, IntPtr stream, int len)
    {
        while (len > 0)
        {
            foreach (var frame in _frames)
            {
                if (_index >= _audioBuffer.Length)
                {
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
                if (len is 0) break;
            }
        }
    }
}