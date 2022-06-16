using System;
using System.Net;
using System.Net.Sockets;

namespace PSXRPC
{
    public class PSXConsole
    {
        private const ushort _magicCode = 0x1337;

        private Socket _sender;

        enum PSXCommand : int
        {
            PSXCommand_Connect,
            PSXCommand_Notification,
            PSXCommand_ReceiveData,
            PSXCommand_SendData,
            PSXCommand_GetPages,
            PSXCommand_CallFunction
        }

        private IPAddress _ipAddress;

        public PSXConsole(IPAddress iPAddress)
        {
            _ipAddress = iPAddress;
        }

        private int SendHeader(PS4_MessageHeader header)
        {
            using Socket socket = new Socket(_ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Udp);

            var headerMsg = Utils.SerializeHeader(header);

            return socket.Send(headerMsg);
        }

        public bool Connect()
        {
            IPEndPoint remoteEP = new IPEndPoint(_ipAddress, 1337);

            _sender = new Socket(_ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Udp);

            try
            {
                // Connect to Remote EndPoint
                _sender.Connect(remoteEP);

                Console.WriteLine("Connected to PS4 console at \"{0}\"",
                    _sender.RemoteEndPoint.ToString());

                PS4_MessageHeader header = new PS4_MessageHeader
                {
                    Magic = _magicCode,
                    Command = (int)PSXCommand.PSXCommand_Connect
                };

                SendHeader(header);

                // Release the socket.
                _sender.Shutdown(SocketShutdown.Both);
                _sender.Close();

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

            return true;
        }
    }
}

