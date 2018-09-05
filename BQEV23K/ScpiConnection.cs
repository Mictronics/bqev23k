using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace BQEV23K
{
    public class ScpiConnection : IDisposable
    {
        TcpClient client;
        NetworkStream stream;
        bool isOpen = false;
        string hostname;
        int readTimeout = 1000; // ms
        public delegate void ConnectionDelegate();
        public event ConnectionDelegate Opened;
        public event ConnectionDelegate Closed;

        #region Properties
        public bool IsOpen
        {
            get
            {
                return isOpen;
            }
        }
        public string Hostname
        {
            get
            {
                return hostname;
            }
        }

        public int ReadTimeout
        {
            set
            {
                readTimeout = value; if (IsOpen) stream.ReadTimeout = value;
            }

            get
            {
                return readTimeout;
            }
        }
        #endregion

        void CheckOpen()
        {
            if (!IsOpen)
                throw new Exception("Connection not open.");
        }

        public void Write(string str)
        {
            CheckOpen();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        public void WriteLine(string str)
        {
            CheckOpen();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
            stream.Write(bytes, 0, bytes.Length);
            WriteTerminator();
        }

        void WriteTerminator()
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes("\r\n\0");
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        public string Read()
        {
            CheckOpen();
            return System.Text.Encoding.ASCII.GetString(ReadBytes());
        }

        /// <summary>
        /// Reads bytes from the socket and returns them as a byte[].
        /// </summary>
        /// <returns></returns>
        public byte[] ReadBytes()
        {
            int i = stream.ReadByte();
            byte b = (byte)i;
            int bytesToRead = 0;
            var bytes = new List<byte>();

            if ((char)b == '#')
            {
                bytesToRead = ReadLengthHeader();
                if (bytesToRead > 0)
                {
                    i = stream.ReadByte();
                    if ((char)i != '\n') // discard carriage return after length header.
                        bytes.Add((byte)i);
                }
            }

            if (bytesToRead == 0)
            {
                while (i != -1 && b != (byte)'\n')
                {
                    bytes.Add(b);
                    i = stream.ReadByte();
                    b = (byte)i;
                }
            }
            else
            {
                int bytesRead = 0;
                while (bytesRead < bytesToRead && i != -1)
                {
                    i = stream.ReadByte();
                    if (i != -1)
                    {
                        bytesRead++;
                        // record all bytes except \n if it is the last char.
                        if (bytesRead < bytesToRead || (char)i != '\n')
                            bytes.Add((byte)i);
                    }
                }
            }
            return bytes.ToArray();
        }

        int ReadLengthHeader()
        {
            int numDigits = Convert.ToInt32(new string(new char[] { (char)stream.ReadByte() }));
            string bytes = "";
            for (int i = 0; i < numDigits; ++i)
                bytes = bytes + (char)stream.ReadByte();
            return Convert.ToInt32(bytes);
        }

        public void Open(string hostname, int port)
        {
            if (IsOpen)
                Close();
            this.hostname = hostname;
            client = new TcpClient(hostname, port);
            stream = client.GetStream();
            stream.ReadTimeout = ReadTimeout;
            isOpen = true;
            Opened?.Invoke();
        }

        public void Close()
        {
            if (!isOpen)
                return;
            stream.Close();
            client.Close();
            isOpen = false;
            Closed?.Invoke();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
