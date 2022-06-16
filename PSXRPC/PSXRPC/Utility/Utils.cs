using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PSXRPC
{
    class Utils
    {
        public static byte[] SerializeStruct<T>(T header) where T : struct
        {
            int position = 0;
            int structSize = Marshal.SizeOf(header);

            byte[] rawData = new byte[structSize];

            IntPtr buffer = Marshal.AllocHGlobal(structSize);

            Marshal.StructureToPtr(header, buffer, false);
            Marshal.Copy(buffer, rawData, position, structSize);

            Marshal.FreeHGlobal(buffer);

            return rawData;
        }

        public static T DeserializeStruct<T>(byte[] bytes) where T : struct
        {
            int position = 0;
          
            int structSize = Marshal.SizeOf(typeof(T));

            IntPtr buffer = Marshal.AllocHGlobal(structSize);

            Marshal.Copy(bytes, position, buffer, structSize);

            var structObj = (T)Marshal.PtrToStructure(buffer, typeof(T));

            Marshal.FreeHGlobal(buffer);

            return structObj;
        }
    }
}
