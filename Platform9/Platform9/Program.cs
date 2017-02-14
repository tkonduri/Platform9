using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Platform9
{
    public class ServerMain
    {
        static int port;
        static string szIP = "";
        //Assumption: We are calculating the test.txt files speed of upload/downloading
        
        /// File name that will be saved on the server on the path where the myserver.exe is present
        const string SaveFileName = "../../DownloadedFile.txt";
        
        private static string saveFile(/*int port, */NetworkStream nStream, TcpClient tClient/*, TcpListener listener*/)
        {
            string szPUTReply = "200 OK FILE CREATED";
            int iBufferSize = 1024;
            
            try
            {
                Console.WriteLine("Saving the file...");

                byte[] RecData = new byte[iBufferSize];
                int iRecBytes = 0;

                nStream = tClient.GetStream();
                int iTotalRecBytes = 0;

                //Measuring the download time...
                var watch = System.Diagnostics.Stopwatch.StartNew();

                ///Saves the file in the current directory of the .exe file
                FileStream Fs = new FileStream(SaveFileName, FileMode.OpenOrCreate, FileAccess.Write);

                while ((iRecBytes = nStream.Read(RecData, 0, RecData.Length)) > 0)
                {
                    Fs.Write(RecData, 0, iRecBytes);
                    iTotalRecBytes += iRecBytes;
                }

                Fs.Close();

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                //szPUTReply += ", With Time taken = " + elapsedMs + " ms";

                szPUTReply += "\n\n### Download Speed:"+ (iTotalRecBytes/elapsedMs) * 8 * 1000 +" in Bits / sec";

                Console.WriteLine("File saved successfully...");
            }
            catch (Exception ex)
            {
                szPUTReply = "204 FILE NOT SAVED";
                Console.WriteLine("Error while saving the file...Exception occured: " + ex.Message);
            }

            return szPUTReply;
        }

        private static void replyRequest(string szReply, NetworkStream nStream)
        {
            try
            {
                BinaryWriter bWriter = new BinaryWriter(nStream);
                bWriter.Write(szReply);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured: " + ex.Message);
            }
        }

        private static string handlePUTRequest()
        {
            string szPUTReply = "";
            try
            {
                ///For initial confirmation...After receiving this message the client will send the file
                szPUTReply = "200 OK FILE CREATED";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured: " + ex.Message);
                szPUTReply = "204 FILE NOT SAVED";
            }
            return szPUTReply;
        }

        private static int parseRequest(NetworkStream nStream, TcpClient tClient, TcpListener listener)
        {
            int iRet = 0;
            try
            {                
                string szPUTReply = "";
                szPUTReply = handlePUTRequest();
                replyRequest(szPUTReply, nStream);
                szPUTReply = saveFile(/*port, */nStream, tClient/*, listener*/);
                Console.WriteLine(szPUTReply + " was sent...");                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured: " + ex.Message);
            }

            return iRet;
        }


        public static void handleRequest()
        {
            try
            {
                
                TcpListener listener = new TcpListener(IPAddress.Parse(szIP), port);

                ///Starting the listner
                listener.Start();

                ///Continous loop
                while (true)
                {
                    Console.WriteLine("\nWaiting for connections...");

                    ///Accept client connection
                    using (TcpClient tClient = listener.AcceptTcpClient())
                    {
                        Console.WriteLine("Connection Request accepted...");

                        using (NetworkStream nStream = tClient.GetStream())
                        {
                            BinaryReader bReader = new BinaryReader(nStream);
                            string szReceived = bReader.ReadString();

                            int iRet = parseRequest(nStream, tClient, listener);                            
                        }

                        tClient.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured: " + ex.Message);
            }
        }

        public static int Main(String[] args)
        {
            try
            {
                //ServerMain sMain = null;

                Console.WriteLine("IP address of server: " + args[0] + "\n");
                szIP = args[0];
                Console.WriteLine("Port on which server is listening: " + args[1] + "\n");
                int.TryParse(args[1], out port);
                
                ///Multithreaded call to handleRequest function
                Thread thread = new Thread(handleRequest);
                thread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured: " + ex.Message);
            }
            return 0;
        }
    }
}
