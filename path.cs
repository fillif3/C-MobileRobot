using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp12
{
    class path
    {
        public static double[,] line(double x1, double y1, double x2, double y2) // Creates set of points from the first point (x1 and y1) to second point (x2 and y2)
        {
            double[,] stright = new double[2001, 2];
            for (int i = 0; i < 2001; i++) // each set of points
            {
				stright[i, 0] = x1 + i * ((x2 - x1) / 2000);
				stright[i, 1] = y1 + i * ((y2 - y1) / 2000);
                stright[i, 0] = Math.Floor(stright[i, 0]);
                stright[i, 1] = Math.Floor(stright[i, 1]);
            }
            return stright;



        }
        public static bool[,] drawing(double[,] coordinates) // on map 2000x2000, empty cell has value false. Basing on coordinates of polygon's vertices, the not empty cell gets value 1 and new map is returned
        {
			// Creating new map
            bool[,] map = new bool[2000, 2000];
            for (int i = 0; i < 2000; i++)
            {
                for (int j = 0; j < 2000; j++)
                {
                    map[i, j] = false;
                }
            }
            double[,] stright = new double[2001, 2];
            int n = coordinates.Length / 2; // amount of vertices
            for (int i = 0; i < (n - 1); i++) // for each pair of vertices
            {
                stright = line(coordinates[i, 0], coordinates[i, 1], coordinates[i + 1, 0], coordinates[i + 1, 1]); //create line
                for (int j = 0; j < 2000; j++)
                {
                    map[Convert.ToInt16(stright[j, 0]), Convert.ToInt16(stright[j, 1])] = true; //and draw line on the map
                }
            }
            stright = line(coordinates[n - 1, 0], coordinates[n - 1, 1], coordinates[0, 0], coordinates[0, 1]); // last and first vertcies
            for (int j = 0; j < 2001; j++)
            {
                map[Convert.ToInt16(stright[j, 0]), Convert.ToInt16(stright[j, 1])] = true;
            }
            for (int j = 0; j < 2000; j++) // function will look for edges, so first line must be clear
            {
                map[0, j] = false;
            }
            int[] help = new int[2000];
            for (int i = 0; i < 2000; i++) //for each point
            {
                int m = 0;
                for (int j = 1; j < 2000; j++)
                {
                    if ((map[i, j - 1] == false) && (map[i, j] == true)) //edge
                    {
                        m = m + 1;
                    }
                    help[j] = m;
                }
                if (m > 1)   // if we have two edges 
                {
                    for (int j = 1; j < 2000; j++) // for each points
                    {
                        if (help[j] == 1) // which is between them
                        {
                            map[i, j] = true; // it is obstacle
                        }
                    }
                }
            }
            return map;
        }
        public static bool[,] dilatation(bool[,] map) // We want to make obstacles bigger fo algorithm because it is easier to have bigger obstacle and treat robot as a point
        {
			// We create new empty map
            bool[,] map_dilatation = new bool[2000, 2000]; 7
            for (int i = 0; i < 2000; i++)
            {
                for (int j = 0; j < 2000; j++)
                {
                    map_dilatation[i, j] = false;
                }
            }
			// For each point on the new map
            for (int i = 1; i < 1999; i++)
            {
                for (int j = 1; j < 1999; j++)
                {
                    if (map[i - 1, j - 1] || map[i - 1, j] || map[i - 1, j + 1] || map[i, j - 1] || map[i, j] || map[i, j + 1] || map[i + 1, j - 1] || map[i + 1, j] || map[i + 1, j + 1]) map_dilatation[i, j] = true;// We chekc if point is close to obstacle, if it is, we treat it as obstacle
                }
            }
            return map_dilatation;
        }


        public static double[,] randomizer() // function creates 20 random points on the map 
        {
            Random randomizer = new Random();
            double[,] points = new double[20, 2];

            for (int i = 0; i < 20; i++)
            {
                points[i, 0] = randomizer.NextDouble() * 2000;
                points[i, 1] = randomizer.NextDouble() * 2000;
            }
            return (points);
        }

        public static double[,] checkconnections(double[,] points, bool[,] map) // Check if there is safe line between two points on the map
        {
            double[,] stright = new double[2001, 2];
            int n = points.Length / 2;
            double[,] connections = new double[n * (n - 1) / 2, 3];
            int m = 0;
            bool test;
			// For each possible pair of points
            for (int i = 1; i < n; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    test = false;
                    stright = line(points[i, 0], points[i, 1], points[i, 0], points[j, 1]); //Create line between them
                    for (int k = 0; k < 2001; k++) // for each point on the line
                    {
                        if (map[Convert.ToInt16(stright[j, 0]), Convert.ToInt16(stright[j, 1])] == true) // If there is obstacle between them
                        {
                            test = true;
                        }
                    }
                    if (test == false) // if there was no obstacle, save connection
                    {
                        connections[m, 0] = i;
                        connections[m, 1] = j;
                        m = m + 1;
                    }

                }
            }
            double[,] graph = new double[m, 3];
            for (int i = 0; i < m; i++) // for each connections, save it in the graph as [1st point, 2ndpoint, distance between them]
            {
                graph[i, 0] = connections[i, 0];
                graph[i, 1] = connections[i, 1];
                graph[i, 2] = Math.Sqrt(Math.Pow(points[Convert.ToInt16(graph[i, 0]), 0] - points[Convert.ToInt16(graph[i, 1]), 0], 2) + Math.Pow(points[Convert.ToInt16(graph[i, 0]), 1] - points[Convert.ToInt16(graph[i, 1]), 1], 2));
            }
            return graph;
        }

        public static int[] dijkstra(double[,] connections) //finds the shortest oath in the graph
        {
            bool n = true; //flag
            bool m = true; //flag
            int count_points_path = 1; //how many points does shortest path has
            int size = connections.Length / 3; 
            int maximum = 0; // how many points we have
            for (int i = 0; i < size; i++)
            {
                if (maximum < connections[i, 0])
                {
                    maximum = Convert.ToInt16(connections[i, 0]);
                }
            }
            double[,] road = new double[maximum + 1, 3]; //[distacne, if this point was checked, from whereP I came here]
			// set values according to Dijkstra algorthm
            road[0, 0] = 0;
            road[0, 1] = 0;// I was not here
            road[0, 2] = 0;// I came from 0th points
            for (int i = 1; i < maximum + 1; i++)
            {
                road[i, 0] = 10000;
                road[i, 1] = 0;
                road[i, 2] = 0;
            }
            int whereP = 0;//which point is being checked
            double distance;//to hceck which point is the closest
            while (n)
            {
                distance = 10001;
                for (int i = 0; i < maximum + 1; i++) //for all points
                {
                    if ((road[i, 0] < distance) && (road[i, 1] == 0)) //check the closest point
                    {
                        distance = road[i, 0];
                        whereP = i; //this point is going to be checkhed
                    }
                }
                road[whereP, 1] = 1; //I save information that this point was checkdf
                if (whereP == maximum) // If we check last point
                {
                    while (m) 
                    {
                        count_points_path = count_points_path + 1; // cpunt how much point we have
                        whereP = Convert.ToInt16(road[whereP, 2]); // check from where we went here
                        if (whereP == 0) // if we went from starting point, finish loop
                        {
                            m = false;
                            n = false;

                        }
                    }
                    whereP = maximum;
                }
                else
                {
                    for (int i = 0; i < size; i++) //check all connections
                    {
                        if ((connections[i, 0] == whereP) && ((connections[i, 2] + road[whereP, 0]) < road[Convert.ToInt16(connections[i, 1]), 0]))// check if new connection is clser than rpevious the closest connection
                        {
                            road[Convert.ToInt16(connections[i, 1]), 2] = whereP; //from where we went here
                            road[Convert.ToInt16(connections[i, 1]), 0] = (connections[i, 2] + road[whereP, 0]);//save distacne
                        }
                        if ((connections[i, 1] == whereP) && ((connections[i, 2] + road[whereP, 0]) < road[Convert.ToInt16(connections[i, 0]), 0]))// check if new connection is clser than rpevious the closest connection
                        {
                            road[Convert.ToInt16(connections[i, 0]), 2] = whereP; //from where we went here
                            road[Convert.ToInt16(connections[i, 0]), 0] = (connections[i, 2] + road[whereP, 0]);//save distacne
                        }
                    }
                }
            }
            int[] ordered_points = new int[count_points_path]; // find the ordered list
            for (int i = 0; i < count_points_path; i++)
            {
                ordered_points[count_points_path - i - 1] = whereP;
                whereP = Convert.ToInt16(road[whereP, 2]);
            }
            return (ordered_points);

        }


    }
}
