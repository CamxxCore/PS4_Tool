using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
        CallFunction
    }

    public class PSXConsole : IDisposable
    {
        private const ushort _magicCode = 0x1337;

        private Socket _sender;

        private readonly IPAddress _ipAddress;

        public PSXConsole(IPAddress iPAddress)
        {
            _ipAddress = iPAddress;
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

            return Utils.SerializeHeader(header);
        }

        public bool SendToConsole<T>(T message) where T : IPSXNetMessage
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(CreateHeader(message.Type));

            writer.Write(message.Serialize());

            return SendToConsole(stream.ToArray()) > 0;
        }

        public short ReadInt16(ulong address)
        {
            var bytes = ReadBytes(address, sizeof(short));

            return BitConverter.ToInt16(bytes);
        }

        public ushort ReadUInt16(ulong address)
        {
            var bytes = ReadBytes(address, sizeof(ushort));

            return BitConverter.ToUInt16(bytes);
        }

        public int ReadInt(ulong address)
        {
            var bytes = ReadBytes(address, sizeof(int));

            return BitConverter.ToInt32(bytes);
        }

        public uint ReadUInt(ulong address)
        {
            var bytes = ReadBytes(address, sizeof(uint));

            return BitConverter.ToUInt32(bytes);
        }

        public ulong ReadUInt64(ulong address)
        {
            var bytes = ReadBytes(address, sizeof(ulong));

            return BitConverter.ToUInt64(bytes);
        }

        public byte ReadByte(ulong address)
        {
            var bytes = ReadBytes(address, sizeof(byte));

            return bytes[0];
        }

        public bool ReadBool(ulong address)
        {
            var bytes = ReadBytes(address, sizeof(byte));

            return BitConverter.ToBoolean(bytes, 0);
        }

        public string ReadString(ulong address)
        {
            int bytesRead = -1;
            var bytes = new byte[0x1000];
            while ((bytes[++bytesRead] = ReadByte(address + (uint)bytesRead)) != 0)
            {
                if (bytesRead >= bytes.Length)
                    return null;
            }

            return Encoding.ASCII.GetString(bytes);
        }

        public byte[] ReadBytes(ulong address, uint size)
        {
            ReadDataCommand cmd = new ReadDataCommand
            {
                Address = address,
                Size = size
            };

            SendToConsole(cmd);

            byte[] bytes = new byte[size];

            int rcv = _sender.Receive(bytes);

            Console.WriteLine("Received {0} bytes", rcv);

            return bytes;
        }

        public bool WriteBytes(ulong address, byte[] bytes)
        {
            SendDataCommand cmd = new SendDataCommand
            {
                Address = address,
                Size = (ulong)bytes.Length
            };

            if (!SendToConsole(cmd))
                return false;

            Console.WriteLine("Wrote {0} bytes", bytes.Length);

            return true;
        }

        public void Dispose()
        {
            // Release the socket.
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}
