using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
namespace Roboty_mobilne
{
    class connectionsRobot
    {
        public static NetworkStream stream;
        public static string position(byte[] data1) // decipher received data and return string which contains information about all robots
        {
            byte[] vektor = new byte[8];
            Array.Copy(data1, 1, vektor, 0, 8);
            for (int j = 1; j < 8; j++)
            {
                vektor[j] = 0;
            }
            UInt64 a = BitConverter.ToUInt64(vektor, 0); // Reading only first byte which informs how msg is
            double x, y, teta;
            string b = ("Ponizej podano pozycje robotow:"); // returned string
            string c;
            for (int i = 9; i < (int)a - 1; i = i + 25) // for each robot
            {

                Array.Copy(data1, i + 1, vektor, 0, 8);
                x = BitConverter.ToDouble(vektor, 0); // x position
                Array.Copy(data1, i + 9, vektor, 0, 8);
                y = BitConverter.ToDouble(vektor, 0); //y position
                Array.Copy(data1, i + 17, vektor, 0, 8);
                teta = BitConverter.ToDouble(vektor, 0) / 2 / 3.14 * 360; // teta position in angle
                c = "\nRobot o numerze - " + Convert.ToString(data1[i]) + ":\nx =" + Convert.ToString(x) + "\ny =" + Convert.ToString(y) + "\nteta =" + Convert.ToString(teta);
                b += c; // adding informaiotn about next robot
            }
            return (b);
        }
        public static double showX(byte[] data1, byte nrRobot) // returns robot's x position, inputs are server data and robot's number
        {
            byte[] vektor = new byte[8];
            Array.Copy(data1, 1, vektor, 0, 8);
            for (int j = 1; j < 8; j = j + 1)
            {
                vektor[j] = 0;
            }
            UInt64 a = BitConverter.ToUInt64(vektor, 0); // Reading only first byte which informs how msg is
            double b = -1; // If the chosen robot was not noticed by camera, function will return -1
            for (int i = 9; i < (int)a - 1; i = i + 25) // check each robot
            {
                if (data1[i] == nrRobot) // if chosen robot's number and current robot's nymber are same, save its x position
                {
                    Array.Copy(data1, i + 1, vektor, 0, 8);
                    b = BitConverter.ToDouble(vektor, 0);
                }
            }
            return (b);
        }
        public static double showY(byte[] data1, byte nrRobot) // returns robot's y position, inputs are server data and robot's number
        {
            byte[] vektor = new byte[8];
            Array.Copy(data1, 1, vektor, 0, 8);
            for (int j = 1; j < 8; j = j + 1)
            {
                vektor[j] = 0;
            }
            UInt64 a = BitConverter.ToUInt64(vektor, 0); // Reading only first byte which informs how msg is
            double b = -1; // If the chosen robot was not noticed by camera, function will return -1
            for (int i = 9; i < (int)a - 1; i = i + 25) // check each robot
            {
                if (data1[i] == nrRobot) // if chosen robot's number and current robot's nymber are same, save its y position
                {
                    Array.Copy(data1, i + 9, vektor, 0, 8);
                    b = BitConverter.ToDouble(vektor, 0);
                }
            }
            return (b);
        }
        public static double showAngle(byte[] data1, byte nrRobot) // returns robot's angle, inputs are server data and robot's number
        {
            byte[] vektor = new byte[8];
            Array.Copy(data1, 1, vektor, 0, 8);
            for (int j = 1; j < 8; j = j + 1)
            {
                vektor[j] = 0;
            }
            UInt64 a = BitConverter.ToUInt64(vektor, 0); // Reading only first byte which informs how msg is
            double b = -1; // If the chosen robot was not noticed by camera, function will return -1
            for (int i = 9; i < (int)a - 1; i = i + 25) // check each robot
            {
                if (data1[i] == nrRobot)// if chosen robot's number and current robot's nymber are same, save its y position
                {
                    Array.Copy(data1, i + 17, vektor, 0, 8);
                    b = BitConverter.ToDouble(vektor, 0);
                    b = b / 2 / 3.14 * 360;
                }
            }
            return (b);
        }

        private static object _lock = new object();

        public static byte[] Connect(String server, byte[] data) // connects with server
        {
            lock (_lock)
            {
                if (stream != null)
                {

                    try
                    {
                        // Create a TcpClient.
                        // Note, for this client to work you need to have a TcpServer 
                        // connected to the same address as specified by the server, 
                        // combination.
                        /// Int32 port = 10000;
                        // / TcpClient client = new TcpClient(server, port);

                        // Translate the passed message into ASCII and store it as a Byte array.
                        /// Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                        // Get a client stream for reading and writing.
                        ///  Stream stream = client.GetStream();

                        //   NetworkStream stream = client.GetStream();

                        // Send the message to the connected  TcpServer.
                        stream.Write(data, 0, data.Length);

                        // stream.Flush();
                        //Thread.Sleep(10);
                        //Console.WriteLine("Sent: {0}", message);

                        // Receive the TcpServer.response.

                        // Buffer to store the response bytes.

                        // String to store the response ASCII representation.
                        String responseData = String.Empty;
                        byte[] odczyt = new byte[256];
                        // Read the first batch of the TcpServer response bytes.
                        Int32 bytes = stream.Read(odczyt, 0, odczyt.Length);
                        string datanew;
                        datanew = BitConverter.ToString(data);
                        //responseData = System.Text.Encoding.ASCII.GetString(datanew,  0, bytes);
                        ///Console.WriteLine("Received: {0}", datanew);

                        /// Close everything.
                        ///stream.Close();
                        // /client.Close();
                        return (odczyt);
                    }
                    catch (ArgumentNullException e)
                    {
                        Console.WriteLine("ArgumentNullException: {0}", e);
                        byte[] a = new byte[3];
                        return (a);

                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("SocketException: {0}", e);
                        byte[] a = new byte[3];

                        return (a);
                    }
                }
            }
            return null;


        }


    }
}






