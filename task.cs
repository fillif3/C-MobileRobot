using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace Roboty_mobilne
{
    class task
    {
        public static void task1(double[,] points, byte nrRobot) // moving from from some 1st point to 2nd, from 2nd to 3rd etc.
        {
            int n = points.Length / 3;
            double angle,destination_angle,difference,x,y,destination_x,destination_y,angle_helper,difference_helper,distance;
            byte[] data = new byte[100];
            for (int i = 0; i < n; i++) // each pair of points
            {
                destination_x = points[i, 1];
                destination_y = points[i, 2];
				// check current pose
                byte[] sendData = new byte[3];
                sendData[0] = 0x5B;
                sendData[1] = 120;
                sendData[2] = 0x5D;
                data = connectionsRobot.Connect("192.168.2.20", sendData);
                x= connectionsRobot.showX(data, nrRobot);
                y = connectionsRobot.showY(data, nrRobot);
                angle = connectionsRobot.showAngle(data, nrRobot);
                destination_angle = points[i, 0];
                angle_helper = matematyka.arcussinus((destination_x - x), (destination_y - y));
                difference = angle - destination_angle;
                difference_helper = angle - angle_helper;
				// angels must be higher than -180 degrees and lower than 180 degrees
                if (difference_helper < -180) difference_helper = difference_helper + 360;
                if (difference_helper > 180) difference_helper = difference_helper - 360;
                if (difference < -180) difference = difference + 360;
                if (difference > 180) difference = difference - 360;
				// if abgle is higher than 10 degrees from best 
                while (Math.Abs(difference_helper) > 10)
                {
                    controller.rotation(nrRobot, angle_helper); // find best velocity
					// check curent difference
                    data = connectionsRobot.Connect("192.168.2.20", sendData);
                    angle = connectionsRobot.showAngle(data, nrRobot);
                    difference_helper = angle - angle_helper;
                    if (difference_helper < -180) difference_helper = difference_helper + 360;
                    if (difference_helper > 180) difference_helper = difference_helper - 360;
                    DateTime timeout = DateTime.Now.AddMilliseconds(100);
                    while (DateTime.Now < timeout) Application.DoEvents(); // An 'active' pause that doesn't hang the thread like the EVIL thread.sleep will do. Plus it gives the GUI some CPU time allowing it to update.

                }
				// Now go to the points
                double xdifference, ydifference;
                distance = Math.Sqrt(Math.Pow(destination_x - x, 2) + Math.Pow(destination_y - y, 2));
                while ((distance > 100) ) // if distance is higher tna 10 cm
                {
					// find best velocity
                    controller.moving(destination_x, destination_y, (byte)nrRobot);
					// check position
                    x = connectionsRobot.showX(data, (byte)nrRobot);
                    y = connectionsRobot.showY(data, (byte)nrRobot);
                    distance = Math.Sqrt(Math.Pow(destination_x - x, 2) + Math.Pow(destination_y - y, 2));
                    DateTime timeout = DateTime.Now.AddMilliseconds(100);
                    while (DateTime.Now < timeout) Application.DoEvents(); // An 'active' pause that doesn't hang the thread like the EVIL thread.sleep will do. Plus it gives the GUI some CPU time allowing it to update.

                }
				// stop
                byte[] sendData1 = new byte[5];
                sendData1[0] = 0x5B;
                sendData1[1] = 14;
                sendData1[2] = 0;
                sendData1[3] = 0;
                sendData1[4] = 0x5D;
                data = connectionsRobot.Connect("192.168.2.20", sendData1);
				// try to go to chosen angle 
                while (Math.Abs(difference) > 10)
                {
                    controller.rotation(nrRobot, destination_angle);
                    data = connectionsRobot.Connect("192.168.2.20", sendData);
                    angle = connectionsRobot.showAngle(data, nrRobot);
                    difference = angle - destination_angle;
                    if (difference < -180) difference = difference + 360;
                    if (difference > 180) difference = difference - 360;
                    DateTime timeout = DateTime.Now.AddMilliseconds(100);
                    while (DateTime.Now < timeout) Application.DoEvents(); // An 'active' pause that doesn't hang the thread like the EVIL thread.sleep will do. Plus it gives the GUI some CPU time allowing it to update.

                }

            }
        }

    }
}
