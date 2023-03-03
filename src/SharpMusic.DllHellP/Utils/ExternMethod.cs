using System.Runtime.InteropServices;

namespace SharpMusic.DllHellP.Utils;

public static class ExternMethod
{
    [DllImport("kernel32.dll")]
    public static extern void RtlZeroMemory(IntPtr dst, int length);
}