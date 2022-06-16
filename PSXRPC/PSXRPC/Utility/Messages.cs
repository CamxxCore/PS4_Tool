using System;
using System.Runtime.Serialization;
using System.IO;

namespace PSXRPC
{
    public abstract class IPSXNetMessage : ISerializable
    {
        public abstract PSXCommand Type { get; }

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

    public class GetModuleBaseAndSizeCommand : IPSXNetMessage
    {
        public override PSXCommand Type => PSXCommand.GetModuleBaseAndSize;

        public GetModuleBaseAndSizeCommand()
        { }

        public override byte[] Serialize()
        {
            return new byte[0];
        }

        public override int Deserialize(byte[] data)
        {
            return 0;
        }
    }

    public class SendDataCommand : IPSXNetMessage
    {
        public override PSXCommand Type => PSXCommand.SendData;

        public long Address { get; set; }

        public long Size { get; set; }

        public SendDataCommand()
        { }

        public SendDataCommand(long address, long size)
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
                Address = reader.ReadInt64();

                Size = reader.ReadInt64();
            }

            return data.Length;
        }    
    }

    public class ReadDataCommand : IPSXNetMessage
    {
        public override PSXCommand Type => PSXCommand.ReceiveData;
        
        public long Address { get; set; }

        public long Size { get; set; }

        public ReadDataCommand()
        { }

        public ReadDataCommand(long address, long size)
        {
            Address = address;

            Size = size;
        }

        public override byte[] Serialize()
        {
            using MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Address);

                writer.Write(Size);
            }

            return stream.ToArray();
        }

        public override int Deserialize(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                Address = reader.ReadInt64();

                Size = reader.ReadInt64();
            }

            return data.Length;
        }
    }
}
