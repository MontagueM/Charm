using System;
using System.Runtime.InteropServices;

namespace RevorbStd
{
    public static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RevorbFile
        {
            public IntPtr Start;
            public IntPtr Cursor;
            public long Size;
        }

        public enum RevorbResult : int
        {
            Success,
            NotOggError,
            FirstPageError,
            FirstPacketError,
            HeaderError,
            TruncatedError,
            SecondaryHeaderError,
            WriteHeaderError,
            CorruptError,
            BitstreamCorruptError,
            WriteError,
            EOSWriteError
        }

        [DllImport("ThirdParty/librevorb.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int revorb(ref RevorbFile fi, ref RevorbFile fo);
    }
}
