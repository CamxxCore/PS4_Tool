using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PSXRPC
{
    class Utils
    {
        public static byte[] SerializeHeader<T>(T header) where T : struct
        {
            int position = 0;
            int structSize = Marshal.SizeOf(typeof(T));

            byte[] rawData = new byte[structSize];

            IntPtr buffer = Marshal.AllocHGlobal(structSize);

            Marshal.StructureToPtr(header, buffer, false);
            Marshal.Copy(buffer, rawData, position, structSize);

            Marshal.FreeHGlobal(buffer);

            return rawData;
        }
    }
}
