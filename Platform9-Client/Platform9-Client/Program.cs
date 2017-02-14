using System;
using System.IO;
using System.Net.Sockets;

namespace Platform9_Client
{
    class clientOperations
    {

        static string path = "../../test.txt";
        static string operation = "PUT";
        static int iLength;
        private static void connectToServerUsingTcp(string host, int port)
        {
            try
            {
                string szMsg = host + " " + Convert.ToString(port) + " " + operation + " " + path;
                string szReceived = "";

                using (TcpClient tClient = new TcpClient(host, port))
                {
                    Console.WriteLine("Connection was established...");

                    using (NetworkStream nStream = tClient.GetStream())
                    {
                        Console.WriteLine("Stream was received from the connection...");

                        ///Sending a message
                        BinaryWriter bWriter = new BinaryWriter(nStream);
                        bWriter.Write(szMsg);
                        Console.WriteLine(szMsg + " was sent...");
                        
                        ///Receiving a message and displaying it
                        BinaryReader bReader = new BinaryReader(nStream);
                        szReceived = bReader.ReadString();

                        //Measuring the download time...
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        sendFile(host, port, path, tClient, nStream, bWriter);

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;

                        Console.WriteLine("\n\n### Upload Speed:" + (iLength / elapsedMs)*1000 + " in Bits / sec");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void sendFile(string ip, int port, string path, TcpClient tClient, NetworkStream nStream, BinaryWriter bWriter)
        {
            try
            {
                Console.WriteLine("Sending file " + path + " ...");
                
                byte[] SendingBuffer = null;

                FileStream Fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                int iBufferSize = 1024;
                int iNoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Fs.Length) / Convert.ToDouble(iBufferSize)));
                int iTotalLength = (int)Fs.Length, iCounter = 0, iCurrentPacketLength = 0, iCtr = 0;
                iLength = iTotalLength;

                for (iCounter = 0; iCounter < iNoOfPackets; iCounter++)
                {
                    if (iBufferSize < iTotalLength)
                    {
                        iCurrentPacketLength = iBufferSize;
                        iTotalLength = iTotalLength - iCurrentPacketLength;
                    }
                    else
                    {
                        iCurrentPacketLength = iTotalLength;
                    }

                    SendingBuffer = new byte[iCurrentPacketLength];
                    Fs.Read(SendingBuffer, 0, iCurrentPacketLength);

                    bWriter = new BinaryWriter(nStream);
                    bWriter.Write(SendingBuffer, 0, (int)SendingBuffer.Length);

                    iCtr += (int)SendingBuffer.Length;
                    Console.WriteLine(iCtr + " byte were written/sent ...");
                }

                Fs.Close();                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured: " + ex.Message);
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                ///Fetching the inputs from command line
                var host = args[0];
                Console.WriteLine("Host = " + host);
                
                int port = 0;
                int.TryParse(args[1], out port);
                Console.WriteLine("Port = " + port);
                
                ///Setting up connection with the server, and sending requests and receiving response...
                connectToServerUsingTcp(host, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured: " + ex.Message);
            }
        }
    }
}
