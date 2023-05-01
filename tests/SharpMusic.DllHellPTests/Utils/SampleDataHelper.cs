using SharpMusic.DllHellPTests.FFWrappers;

namespace SharpMusic.DllHellPTests.Utils;

public static class SampleDataHelper
{
    public static byte[,] RandomSampleData(int nbSamples, AVSampleFormat format, AVChannelLayout chLayout)
    {
        var isPackage = av_sample_fmt_is_planar(format) is 0;
        var height = isPackage ? 1 : chLayout.nb_channels;
        var width = nbSamples * av_get_bytes_per_sample(format);
        if (isPackage) width *= chLayout.nb_channels;
        var result = new byte[height, width];
        for (var i = 0; i < result.GetLength(0); i++)
        {
            for (var j = 0; j < result.GetLength(1); j++)
            {
                result[i, j] = (byte)Random.Shared.Next(0, 256);
            }
        }

        return result;
    }

    /// <summary>
    /// cut data with specify amount of samples
    /// </summary>
    /// <param name="data">will cut data</param>
    /// <param name="format">sample format of data</param>
    /// <param name="channelLayout">channel layout of data</param>
    /// <param name="amount">number of samples will cut, negative mean cut end bytes, positive mean cut start bytes, do noting when zero</param>
    /// <returns>removed samples from data, data itself when amount is 0</returns>
    public static byte[,] CutSamples(byte[,] data, AVSampleFormat format, AVChannelLayout channelLayout, int amount)
    {
        if (amount is 0) return data;
        var cutEnd = amount < 0;
        if (cutEnd) amount = -amount;

        var bps = av_get_bytes_per_sample(format);
        var amountByte = amount * bps;
        if (av_sample_fmt_is_planar(format) is 0) amountByte *= channelLayout.nb_channels;

        var height = av_sample_fmt_is_planar(format) is 0 ? 1 : data.GetLength(0);
        var result = new byte[height, data.GetLength(1) - amountByte];

        for (var i = 0; i < height; i++)
        {
            for (int j = 0, jd = cutEnd ? 0 : amountByte; j < result.GetLength(1); j++)
            {
                result[i, j] = data[i, jd++];
            }
        }

        return result;
    }

    /// <summary>
    /// cut data with specify amount of samples
    /// </summary>
    /// <param name="frame">cut data come from</param>
    /// <param name="format">sample format of data</param>
    /// <param name="channelLayout">channel layout of data</param>
    /// <param name="amount">number of samples will cut, negative mean cut end bytes, positive mean cut start bytes, do noting when zero</param>
    /// <returns>removed samples from data, data itself when amount is 0</returns>
    public static unsafe byte[,] CutSamples(AVFrame* frame, AVSampleFormat format, AVChannelLayout channelLayout,
        int amount)
    {
        var data = new FFFrame(frame).ToArray();

        var inData = av_sample_fmt_is_planar(format) is 0
            ? new byte[1, data.Length]
            : new byte[channelLayout.nb_channels, frame->nb_samples * av_get_bytes_per_sample(format)];
        Buffer.BlockCopy(data, 0, inData, 0, data.Length);
        return CutSamples(inData, format, channelLayout, amount);
    }
}