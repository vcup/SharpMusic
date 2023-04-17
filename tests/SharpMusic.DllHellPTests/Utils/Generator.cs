namespace SharpMusic.DllHellPTests.Utils;

public static class Generator
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
}