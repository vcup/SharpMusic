namespace SharpMusic.DllHellP.Utils;

/// <summary>
/// map audio stream format between FFmpeg SDL2
/// </summary>
/// <remarks>
/// <br/> FFmpeg             -> this            -> SDL2
/// <br/> AV_SAMPLE_FMT_None -> None
/// <br/> AV_SAMPLE_FMT_U8   -> Unsigned8       -> AUDIO_U8
/// <br/> AV_SAMPLE_FMT_S16  -> Signed16        -> AUDIO_S16SYS
/// <br/>                    -> Unsigned16      -> AUDIO_U16SYS
/// <br/> AV_SAMPLE_FMT_S32  -> Signed32        -> AUDIO_S32SYS
/// <br/> AV_SAMPLE_FMT_FLT  -> Float32         -> AUDIO_F32SYS
/// <br/> AV_SAMPLE_FMT_DBL  -> Double
/// <br/> AV_SAMPLE_FMT_U8P  -> Unsigned8Planar
/// <br/> AV_SAMPLE_FMT_S16P -> Signed16Planar
/// <br/> AV_SAMPLE_FMT_S32P -> Signed32Planar
/// <br/> AV_SAMPLE_FMT_FLTP -> Float32Planar
/// <br/> AV_SAMPLE_FMT_DBLP -> DoublePlanar
/// <br/> AV_SAMPLE_FMT_S64  -> Signed64
/// <br/> AV_SAMPLE_FMT_S64P -> Signed64Planar
/// <br/> AV_SAMPLE_FMT_NB   -> Other
/// </remarks>
public enum SampleFormat
{
    /// <summary><c>
    /// AV_SAMPLE_FMT_NONE -> None -> (doesn't exist)
    /// </c></summary>
    None,

    /// <summary><c>
    /// AV_SAMPLE_FMT_U8 -> Unsigned8 -> AUDIO_U8
    /// </c></summary>
    Unsigned8,

    /// <summary><c>
    /// AV_SAMPLE_FMT_S16 -> Signed16 -> AUDIO_S16SYS
    /// </c></summary>
    Signed16,

    /// <summary><c>
    /// (doesn't exist) -> Unsigned16 -> AUDIO_U16SYS
    /// </c></summary>
    Unsigned16,

    /// <summary><c>
    /// AV_SAMPLE_FMT_S32 -> Signed32 -> AUDIO_S32SYS
    /// </c></summary>
    Signed32,

    /// <summary><c>
    /// AV_SAMPLE_FMT_FLT -> Float32 -> AUDIO_F32SYS
    /// </c></summary>
    Float32,

    /// <summary><c>
    /// AV_SAMPLE_FMT_DBL -> Double -> (doesn't exist)
    /// </c></summary>
    Double,

    /// <summary><c>
    /// AV_SAMPLE_FMT_U8P -> Unsigned8Planar -> (doesn't exist)
    /// </c></summary>
    Unsigned8Planar,

    /// <summary><c>
    /// AV_SAMPLE_FMT_S16P -> Signed16Planar -> (doesn't exist)
    /// </c></summary>
    Signed16Planar,

    /// <summary><c>
    /// AV_SAMPLE_FMT_S32P -> Signed32Planar -> (doesn't exist)
    /// </c></summary>
    Signed32Planar,

    /// <summary><c>
    /// AV_SAMPLE_FMT_FLTP -> Float32Planar -> (doesn't exist)
    /// </c></summary>
    Float32Planar,

    /// <summary><c>
    /// AV_SAMPLE_FMT_DBLP -> DoublePlanar -> (doesn't exist)
    /// </c></summary>
    DoublePlanar,

    /// <summary><c>
    /// AV_SAMPLE_FMT_S64 -> Signed64 -> (doesn't exist)
    /// </c></summary>
    Signed64,

    /// <summary><c>
    /// AV_SAMPLE_FMT_S64P -> Signed64Planar -> (doesn't exist)
    /// </c></summary>
    Signed64Planar,

    /// <summary><c>
    /// AV_SAMPLE_FMT_NB -> Other -> (doesn't exist)
    /// </c></summary>
    Other,
}