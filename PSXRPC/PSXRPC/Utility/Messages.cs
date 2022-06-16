using System;
using System.Runtime.Serialization;
using System.IO;

namespace PSXRPC
{
    public abstract class IPSXNetMessage : ISerializable
    {
        public abstract byte[] Serialize();

        public abstract int Deserialize(byte[] data);

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }

    public struct PS4_MessageHeader
    {
        public int Magic;
        public int Command;
    }

    public class SendDataCommand : IPSXNetMessage
    {
        public ulong Address { get; set; }

        public ulong Size { get; set; }

        public SendDataCommand()
        { }

        public SendDataCommand(ulong address, ulong size)
        {
            Address = address;

            Size = size;
        }

        public override byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Address);

                    writer.Write(Size);
                }

                return stream.ToArray();
            }
        }

        public override int Deserialize(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                Address = reader.ReadUInt64();

                Size = reader.ReadUInt64();
            }

            return data.Length;
        }    
    }

    public class ReceiveDataCommand : IPSXNetMessage
    {
        public ulong Address { get; set; }

        public ulong Size { get; set; }

        public ReceiveDataCommand()
        { }

        public ReceiveDataCommand(ulong address, ulong size)
        {
            Address = address;

            Size = size;
        }

        public override byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Address);

                    writer.Write(Size);
                }

                return stream.ToArray();
            }
        }

        public override int Deserialize(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                Address = reader.ReadUInt64();

                Size = reader.ReadUInt64();
            }

            return data.Length;
        }
    }
}

