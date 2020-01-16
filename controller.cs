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
    class controller
    {
        public static void rotation(byte nrRobot, double teta) // Inputs are chosen robot's number and chosen angle (degrees). This function will send best velocity for both robot's wheels to server. If user wants robot to have chosen angle, this function should be used in loop. 
        {
            double tetaa,linkage_teta;
            byte[] data = new byte[100]; // Respond from the server about robot's position
			// Data which will be send to server
            byte[] sendData = new byte[3];
            sendData[0] = 0x5B;
            sendData[1] = 120;
            sendData[2] = 0x5D;
            data = connectionsRobot.Connect("192.168.2.20", sendData); // Receiveng data from server
            tetaa = connectionsRobot.showAngle(data, nrRobot); // current direction
            linkage_teta = teta - tetaa; // difference between current and chosen direction
			// The direction must be [-180,180] degrees
            if (linkage_teta > 180) linkage_teta = linkage_teta - 360;
            if (linkage_teta < -180) linkage_teta = linkage_teta + 360;
            double omega, v;
            v = 0; // robot's linear velocity is zero (robot shouldn't change its position)
            omega = -linkage_teta/5; //chosen angular velocity
            double[,] wv = new double[2, 1];
            wv[0, 0] = omega;
            wv[1, 0] = v;
            byte left, right;
            double r = 16, l = 50; // robot's wheels' radius and distance between robot's wheels [mm]
			// Creating matrix from equesions on velocity with kinematic constraints
            double[,] constraints = new double[2, 2];
            constraints[0, 0] = r / (2 * l);
            constraints[0, 1] = -r / (2 * l);
            constraints[1, 0] = r / 2;
            constraints[1, 1] = r / 2;
            double[,] inversedConstraints = new double[2, 2];
            inversedConstraints = matematyka.MatrixInverse(constraints); // Computing inverse matrix
            double[,] velocity = new double[2, 1];
            velocity = matematyka.MultiplyMatrix(inversedConstraints, wv); // Multiplying matrix so we know whatvelocity both wheels should have
			// Server rarely sends us information about robots position so if we want robot to be close chosen direction, we cannot order it to move to fast
            if ((Math.Abs(velocity[1, 0]) > 40) && Math.Abs(velocity[1, 0]) == Math.Abs(velocity[0, 0]))
            {
                velocity[1, 0] = 40 * Math.Sign(velocity[1, 0]);
                velocity[0, 0] = 40 * Math.Sign(velocity[0, 0]);
            }
			// Unfortunately, wheel won't move without velocity higher than 20% so we make velocity higher for low values
            if ((velocity[1, 0] < 20)&& (velocity[1, 0] > 0)) { velocity[1, 0] = velocity[1, 0] + 20; velocity[0, 0] = velocity[0, 0] - 20; }
            if ((velocity[0, 0] < 20) && (velocity[0, 0] > 0)) { velocity[0, 0] = velocity[0, 0] + 20; velocity[1, 0] = velocity[1, 0] - 20; }
			// Sending velocity to server
            left = (byte)Convert.ToSByte(Math.Floor(velocity[1, 0]));
            right = (byte)Convert.ToSByte(Math.Floor(velocity[0, 0]));
            byte[] sendData1 = new byte[5];
            sendData1[0] = 0x5B;
            sendData1[1] = 14;
            sendData1[2] = left ;
            sendData1[3] = right;
            sendData1[4] = 0x5D;
            data = connectionsRobot.Connect("192.168.2.20", sendData1);
        } 
        public static void moving(double x, double y, byte nrRobot)//Inputs are chosen robot's number and chosen position. This function will send best velocity for both robot's wheels to server. If user wants robot to have chosen position, this function should be used in loop. 
        {
            double xa, ya, tetaa, distance, teta;
            byte[] data = new byte[100];// Respond from the server about robot's position
			// Data which will be send to server
            byte[] sendData = new byte[3];
            sendData[0] = 0x5B;
            sendData[1] = 120;
            sendData[2] = 0x5D;
            data = connectionsRobot.Connect("192.168.2.20", sendData);// Receiveng data from server
			// Readin robot's pose
            xa = connectionsRobot.showX(data, nrRobot);
            ya = connectionsRobot.showY(data, nrRobot);
            tetaa = connectionsRobot.showAngle(data, nrRobot);
            double linkage_x, linkage_y, linkage_teta;
			// Checking distance between robot's pose and destination
            linkage_x = x - xa;
            linkage_y = y - ya;
            teta = matematyka.arcussinus(linkage_x, linkage_y);
            linkage_teta = teta - tetaa;
            distance = Math.Sqrt(Math.Pow(linkage_x, 2) + Math.Pow(linkage_y, 2));
			// The direction must be [-180,180] degrees
            if (linkage_teta > 180) linkage_teta = linkage_teta - 360;
            if (linkage_teta < -180) linkage_teta = linkage_teta + 360;
            double omega, vx, vy, v;
            v = distance * 3; //chosen linear velocity
            omega = -linkage_teta / 30; //chosen angular velocity
            double[,] wv = new double[2, 1];
            wv[0, 0] = omega;
            wv[1, 0] = v;
            byte left, right;
            double r = 16, l = 50;// robot's wheels' radius and distance between robot's wheels [mm]
			// Creating matrix from equesions on velocity with kinematic constraints
            double[,] constraints = new double[2, 2];
            constraints[0, 0] = r / (2 * l);
            constraints[0, 1] = -r / (2 * l);
            constraints[1, 0] = r / 2;
            constraints[1, 1] = r / 2;
			// Computing best velocity for each wheel
            double[,] inversedConstraints = new double[2, 2];
            inversedConstraints = matematyka.MatrixInverse(constraints);
            double[,] velocity = new double[2, 1];
            velocity = matematyka.MultiplyMatrix(inversedConstraints, wv);
			// Wheel's velocity cannot be higher than 100 or lower than -100 so we need to scale them if they are
            if ((Math.Abs(velocity[1, 0]) > 100) && (Math.Abs(velocity[1, 0]) > (Math.Abs(velocity[0, 0]))))
            {
                velocity[0, 0] = Convert.ToDouble(Convert.ToDouble(velocity[0, 0]) * 100 / Convert.ToDouble(Math.Abs(velocity[1, 0])));
                velocity[1, 0] = 100*Math.Sign(velocity[1, 0]);
            }
            if ((Math.Abs(velocity[0, 0]) > 100) && (Math.Abs(velocity[1, 0]) < Math.Abs(velocity[0, 0])))
            {
                velocity[1, 0] = Convert.ToDouble(Convert.ToDouble(velocity[1, 0]) * 100 / Convert.ToDouble(Math.Abs(velocity[0, 0])));
                velocity[0, 0] = 100 * Math.Sign(velocity[0, 0]);
            }
            if ((Math.Abs(velocity[1, 0]) > 100) && Math.Abs(velocity[1, 0]) == Math.Abs(velocity[0, 0]))
            {
                velocity[1, 0] = 100 * Math.Sign(velocity[1, 0]);
                velocity[0, 0] = 100 * Math.Sign(velocity[0, 0]);
            }
			// Unfortunately, wheel won't move without velocity higher than 20% so we make velocity higher for low values
            if ((Convert.ToInt16(velocity[1, 0]) < 35) && (Convert.ToInt16(velocity[0, 0]) < 35))
            {
                if (velocity[1, 0] > velocity[0, 0])
                {
                    velocity[1, 0] = Convert.ToByte(Convert.ToInt16(velocity[1, 0]) + 25);
                    velocity[0, 0] = Convert.ToByte(Convert.ToInt16(velocity[0, 0]) + 20);
                }
                else if (velocity[1, 0] > velocity[0, 0])
                {
                    velocity[1, 0] = Convert.ToByte(Convert.ToInt16(velocity[1, 0]) + 25);
                    velocity[0, 0] = Convert.ToByte(Convert.ToInt16(velocity[0, 0]) + 20);
                }
                else
                {
                    velocity[1, 0] = Convert.ToByte(Convert.ToInt16(velocity[1, 0]) + 22);
                    velocity[0, 0] = Convert.ToByte(Convert.ToInt16(velocity[0, 0]) + 22);
                }
            }
			// Sending data to server
            left = (byte)Convert.ToSByte(Math.Floor(velocity[1, 0]));
            right = (byte)Convert.ToSByte(Math.Floor(velocity[0, 0]));
            byte[] sendData1 = new byte[5];
            sendData1[0] = 0x5B;
            sendData1[1] = 14;
            sendData1[2] = left;
            sendData1[3] = right;
            sendData1[4] = 0x5D;
            data = connectionsRobot.Connect("192.168.2.20", sendData1);


        }

       // public static void regulator()
        public static double[,] trajectory(double xp, double yp, double xpv, double ypv, double xk, double yk, double xkv, double ykv, double t) // Basing on robot's positions and velocities at the start and end of path, best trajectory is computed as 3rd-degree polyminal for x and y parameters. We also need to know how much time for robot to move between starting and ending positions. Function returns computed parameters
        {	// Creating time matrix from equesion a0*t^3+a1*t^2+a2*t+a3 =x  and its deriative (same for y)
            double[,] time_matrix = new double[4, 4];
            time_matrix[0, 0] = 0;
            time_matrix[0, 1] = 0;
            time_matrix[0, 2] = 0;
            time_matrix[0, 3] = 1; 
            time_matrix[1, 0] = Math.Pow(t, 3);
            time_matrix[1, 1] = Math.Pow(t, 2);
            time_matrix[1, 2] = Math.Pow(t, 1);
            time_matrix[1, 3] = 1;
            time_matrix[2, 0] = 0;
            time_matrix[2, 1] = 0;
            time_matrix[2, 2] = 1;
            time_matrix[2, 3] = 0;
            time_matrix[3, 0] = 3 * Math.Pow(t, 2);
            time_matrix[3, 1] = 2 * Math.Pow(t, 1);
            time_matrix[3, 2] = 1;
            time_matrix[3, 3] = 0;
            double[,] time_matrix_inverse = new double[4, 4]; //inversing time matrix
            time_matrix_inverse = matematyka.MatrixInverse(time_matrix);
            double[,] positions = new double[4, 2];
            positions[0, 0] = xp; // x beginning
            positions[0, 1] = yp; // y beginning
            positions[1, 0] = xk; // x ending
            positions[1, 1] = yk; // y ending
            positions[2, 0] = xpv; // x beginning velocity
            positions[2, 1] = ypv; // y beginning velocity
            positions[3, 0] = xkv;// x ending velocity
            positions[3, 1] = ykv; // y ending velocity
            double[,] parameters = new double[4, 2];
            parameters = matematyka.MultiplyMatrix(time_matrix_inverse, positions); //Multiplaying matrix
            return parameters;
        }

        public static void follow(double[,] parameters, double tk, byte nrRobot, double ts)
		// Chosen robot moves through chosen trajctory (expressed in parameters) within time tk with dicretezetion time ts
        {
            double t = 0, x, y, a;
            int n = 0;
			double pause = 1000*ts // discete time in ms
            while (t <= tk) // if we are not finished
            {
				// Compute next position from parameters and curent time
                x = parameters[0, 0] * Math.Pow(t, 3) + parameters[1, 0] * Math.Pow(t, 2) + parameters[2, 0] * Math.Pow(t, 1) + parameters[3, 0];
                y = parameters[0, 1] * Math.Pow(t, 3) + parameters[1, 1] * Math.Pow(t, 2) + parameters[2, 1] * Math.Pow(t, 1) + parameters[3, 1];
                moving(x, y, nrRobot); // move to that position
				// some time passed
                n = n + 1; 
                t = n * ts;
				// wait
                DateTime timeout = DateTime.Now.AddMilliseconds(pause);
                while (DateTime.Now < timeout) Application.DoEvents(); // An 'active' pause that doesn't hang the thread like the EVIL thread.sleep will do. Plus it gives the GUI some CPU time allowing it to update.
            }

        }


    }
}




