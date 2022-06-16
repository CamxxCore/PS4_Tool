using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace PSXRPC
{
    public enum PSXCommand : int
    {
        Connect,
        Notification,
        ReceiveData,
        SendData,
        GetPages,
        CallFunction,
        GetModuleBaseAndSize
    }

    public class PSXConsole : IDisposable
    {
        private const ushort _magicCode = 0x1337;

        private Socket _sender;

        private readonly IPAddress _ipAddress;

        public PSXConsole(IPAddress ipAddress)
        {
            _ipAddress = ipAddress;
        }

        public bool Connect()
        {
            IPEndPoint remoteEP = new IPEndPoint(_ipAddress, 1337);

            _sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Connect to PS4 Console
                _sender.Connect(remoteEP);

                Console.WriteLine("Connected to PS4 console at \"{0}\"",
                    _sender.RemoteEndPoint.ToString());

                return true;

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return false;
        }

        private int SendToConsole(byte[] data)
        {
            return _sender.Send(data);
        }

        private byte[] CreateHeader(PSXCommand command)
        {
            PS4_MessageHeader header = new PS4_MessageHeader
            {
                Magic = _magicCode,
                Command = (int)command
            };

            return Utils.SerializeStruct(header);
        }

        public bool SendToConsole<T>(T message) where T : IPSXNetMessage
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(CreateHeader(message.Type));

            writer.Write(message.Serialize());

            return SendToConsole(stream.ToArray()) > 0;
        }

        public short ReadInt16(long address)
        {
            var bytes = ReadBytes(address, sizeof(short));

            return BitConverter.ToInt16(bytes);
        }

        public ushort ReadUInt16(long address)
        {
            var bytes = ReadBytes(address, sizeof(ushort));

            return BitConverter.ToUInt16(bytes);
        }

        public int ReadInt32(long address)
        {
            var bytes = ReadBytes(address, sizeof(int));

            return BitConverter.ToInt32(bytes);
        }

        public uint ReadUInt(long address)
        {
            var bytes = ReadBytes(address, sizeof(uint));

            return BitConverter.ToUInt32(bytes);
        }

        public long ReadInt64(long address)
        {
            var bytes = ReadBytes(address, sizeof(long));

            return BitConverter.ToInt64(bytes);
        }

        public ulong ReadUInt64(long address)
        {
            var bytes = ReadBytes(address, sizeof(ulong));

            return BitConverter.ToUInt64(bytes);
        }

        public byte ReadByte(long address)
        {
            var bytes = ReadBytes(address, sizeof(byte));

            return bytes[0];
        }

        public bool ReadBool(long address)
        {
            var bytes = ReadBytes(address, sizeof(byte));

            return BitConverter.ToBoolean(bytes, 0);
        }

        public string ReadNullTerminatedString(long address)
        {
            if (ReadByte(address) == 0)
                return null;

            int bytesRead = -1;
            var bytes = new byte[0x1000];
            while ((bytes[++bytesRead] = ReadByte(address + bytesRead)) != 0)
            {
                if (bytesRead >= bytes.Length || bytes[bytesRead] == 0)
                    return null;
            }

            return Encoding.ASCII.GetString(bytes, 0, bytesRead);
        }


        public T ReadStruct<T>(long address) where T : struct
        {
            var structSize = Marshal.SizeOf(typeof(T));

            var bytes = ReadBytes(address, structSize);

            return Utils.DeserializeStruct<T>(bytes);
        }

        public T[] ReadStruct<T>(long address, int count) where T : struct
        {
            var structSize = Marshal.SizeOf(typeof(T));

            var arrSize = structSize * count;

            var bytes = ReadBytes(address, arrSize);

            T[] arr = new T[count];

            for (int i = 0; i < count; i++)
            {
                byte[] structBytes = new byte[structSize];

                Buffer.BlockCopy(bytes, structSize * i, structBytes, 0, structSize);

                arr[i] = Utils.DeserializeStruct<T>(structBytes);
            }

            return arr;
        }

        public byte[] ReadBytes(long address, int size)
        {
            ReadDataCommand cmd = new ReadDataCommand
            {
                Address = address,
                Size = size
            };

            SendToConsole(cmd);

            byte[] bytes = new byte[size];

            int rcv = _sender.Receive(bytes);

            // Console.WriteLine("Received {0} bytes", rcv);

            return bytes;
        }

        public bool WriteBytes(long address, byte[] bytes)
        {
            SendDataCommand cmd = new SendDataCommand
            {
                Address = address,
                Size = bytes.Length
            };

            if (!SendToConsole(cmd))
                return false;

            //Console.WriteLine("Wrote {0} bytes", bytes.Length);

            return true;
        }

        public long GetBaseAddress()
        {
            SendToConsole(new GetModuleBaseAndSizeCommand());

            byte[] bytes = new byte[0xC];

            int rcv = _sender.Receive(bytes);

            var moduleBase = BitConverter.ToInt64(bytes);

            Console.WriteLine("Module base is {0:X}", moduleBase);

            return moduleBase;
        }

        public int GetModuleMemorySize()
        {
            SendToConsole(new GetModuleBaseAndSizeCommand());

            byte[] bytes = new byte[0xC];

            int rcv = _sender.Receive(bytes);

            var moduleSize = BitConverter.ToInt32(bytes, 8);

            Console.WriteLine("Module is {0:X} bytes", moduleSize);

            return moduleSize;
        }

        /// <summary>
        /// Searches for bytes in a processes memory.
        /// </summary>
        /// <param name="needle">Byte Sequence to scan for.</param>
        /// <param name="startAddress">Address to start the search at.</param>
        /// <param name="endAddress">Address to end the search at.</param>
        /// <param name="bufferSize">Byte Buffer Size</param>
        /// <param name="firstMatch">If we should stop the search at the first result.</param>
        /// <returns></returns>
        public long[] FindBytes(byte?[] needle, long startAddress, long endAddress, bool firstMatch = false, int bufferSize = 0xFFFF)
        {
            List<long> results = new List<long>();
            long searchAddress = startAddress;

            int needleIndex = 0;
            int bufferIndex;

            while (true)
            {
                try
                {
                    byte[] buffer = ReadBytes(searchAddress, bufferSize);

                    for (bufferIndex = 0; bufferIndex < buffer.Length; bufferIndex++)
                    {
                        if (needle[needleIndex] == null)
                        {
                            needleIndex++;

                            continue;
                        }

                        if (needle[needleIndex] == buffer[bufferIndex])
                        {
                            needleIndex++;

                            if (needleIndex == needle.Length)
                            {
                                results.Add(searchAddress + bufferIndex - needle.Length + 1);

                                if (firstMatch)
                                {
                                    return results.ToArray();
                                }

                                needleIndex = 0;
                            }
                        }
                        else
                        {
                            needleIndex = 0;
                        }
                    }
                }
                catch
                {
                    break;
                }

                searchAddress += bufferSize;

                if (searchAddress > endAddress)
                {
                    break;
                }
            }

            return results.ToArray();
        }

        public void Dispose()
        {
            // Release the socket.
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}
