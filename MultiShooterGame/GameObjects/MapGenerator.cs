using Microsoft.Xna.Framework;
using ShooterGuys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame.GameObjects
{
    class MapGenerator
    {
        private static Vector2[] threeHor = new Vector2[] { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0) };
        private static Vector2[] threeVer = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 2) };

        private static Vector2[] LSouthWest = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        private static Vector2[] LSouthEast = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, -1) };
        private static Vector2[] LNorthWest = new Vector2[] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-1, 1) };
        private static Vector2[] LNorthEast = new Vector2[] { new Vector2(0, 0), new Vector2(0, -1), new Vector2(1, -1) };

        private static Vector2[] fourBlock = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
        private static Vector2[] point = new Vector2[] { new Vector2(0, 0) };

        private static Vector2[][] blockTypes = new Vector2[8][];


        private static int gridWidth = 9;
        private static int gridHeight = 9;
        private static int[][] grid;

        private static int blockName = -1;
        private static Random random;
        public static void Initialize()
        {
            blockTypes[0] = threeHor;
            blockTypes[1] = threeVer;
            blockTypes[2] = LSouthWest;
            blockTypes[3] = LSouthEast;
            blockTypes[4] = LNorthWest;
            blockTypes[5] = LNorthEast;
            blockTypes[6] = fourBlock;
            blockTypes[7] = point;

            random = new Random();
        }

        public static Map GenerateMap(int width, int height)
        {
            gridWidth = width/2 - 1;
            gridHeight = height / 2 - 1;
            grid = new int[gridWidth][];
            for (int i = 0; i < gridWidth; i++)
            {
                grid[i] = new int[gridHeight];
            }
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x][y] = 0;
                }
            }

            while (!isGridFull())
            {

                Vector2[] randType = blockTypes[random.Next(0, blockTypes.Length)];
                Vector2 randPos = getRandomEmptyPos();
                if (placeIfFit(randPos, randType))
                {
                    PrintGrid();
                }
            }
            int[][] map = new int[width][];
            for (int i = 0; i < width; i++)
            {
                map[i] = new int[height];
            }
            List<int> line = new List<int>();
            //Add spaces downwards
            for (int x = 0; x < gridWidth; x++)
            {
                line.Clear();
                line.Add(1);
                if (x == 0)
                {
                    line.Add(1);
                }
                else
                {
                    line.Add(0);
                }

                for (int y = 0; y < gridHeight-1; y++)
                {
                    int first = grid[x][y];
                    int second = grid[x][y + 1];

                    //First block
                    line.Add(first);

                    //Add a wall more if the next block is same type, otherwise walking spot
                    if (first != 0 && first == second)
                    {
                        line.Add(first);
                    }
                    else if (y == gridHeight - 2)
                    {
                        line.Add(second);
                    }
                    else
                    {
                        line.Add(0);
                    }
                }

                if (x == 0 || x == gridWidth - 1)
                {
                    line.Add(1);
                }
                else
                {
                    line.Add(0);
                }
                line.Add(1);
                for (int i = 0; i < line.Count; i++)
                {
                    map[x][i]= line[i];
                }
                line.Clear();
            }

            //Add spaces to the side
            for (int y = 0; y < map[0].Length; y++)
            {
                line.Clear();
                line.Add(1);
                if (y == 0 || y == map[0].Length - 1)
                {
                    line.Add(1);
                }
                else
                {
                    line.Add(0);
                }
                for (int x = 0; x < grid.Length - 1; x++)
                {
                    int first = map[x][y];
                    int second = map[x + 1][y];

                    //First block
                    if (y == map[0].Length - 1)
                    {
                        line.Add(1);
                    }
                    else if (y == map[0].Length - 2)
                    {
                        line.Add(0);
                    }
                    else
                    {
                        line.Add(second);
                    }


                    //Add a wall more if the next block is same type, otherwise walking spot
                    if (y == map[0].Length - 1)
                    {
                        line.Add(1);
                    }
                    else if (first != 0 && first == second)
                    {
                        line.Add(first);
                    }
                    else if (x == grid.Length - 2)
                    {
                        line.Add(first);
                    }
                    else
                    {
                        line.Add(0);
                    }
                }
                if (y == 0 || y == map[0].Length - 1)
                {
                    line.Add(1);
                }
                else
                {
                    line.Add(0);
                }
                line.Add(1);
                for (int i = 0; i < line.Count; i++)
                {
                    map[i][y] = line[i];
                }
                line.Clear();
            }

            Tile[,] tiles = new Tile[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int type = map[x][y];
                    if(type<0)
                    {
                        type = 0;
                    }
                    else if(type==0)
                    {
                        type = 2;
                    }
                   
                    tiles[x, y] = new Tile((Tile.BlockName)type, x * Map.TileSize, y * Map.TileSize);
                }
            }

            return new Map(tiles);
        }
        private static void PrintGrid()
        {
            string gridMessage = "\n";
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {

                    gridMessage += grid[x][y].ToString() + ",";
                }
                gridMessage += "\n";
            }
            Console.WriteLine(gridMessage);
        }
        private static Vector2 getRandomEmptyPos()
        {
            while (true)
            {
                int randX = random.Next(0, gridWidth);
                int randY = random.Next(0, gridHeight);
               
                if (grid[randX][randY] == 0)
                {
                    return new Vector2(randX,randY);
                }
            }
            return Vector2.Zero;
        }

        private static bool isGridFull()
        {

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x][y] == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool placeIfFit(Vector2 position, Vector2[] type)
        {
            Vector2 newPos = new Vector2();
            for (int i = 0; i < type.Length; i++)
            {
                newPos.X = position.X + type[i].X;
                newPos.Y = position.Y + type[i].Y;

                if (newPos.X < 0 || newPos.Y < 0 || newPos.X >= gridWidth || newPos.Y >= gridHeight || grid[(int)(newPos.X)][(int)(newPos.Y)] != 0)
                {
                    int j = i - 1;
                    while (j >= 0)
                    {
                        newPos.X = position.X + type[j].X;
                        newPos.Y = position.Y + type[j].Y;
                        grid[(int)(newPos.X)][(int)(newPos.Y)] = 0;
                        j--;
                    }
                    return false;
                }
                else
                {
                    grid[(int)(newPos.X)][(int)(newPos.Y)] = blockName;
                }
            }
            blockName--;
            return true;
        }

    }
}
