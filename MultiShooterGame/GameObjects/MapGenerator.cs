﻿using Microsoft.Xna.Framework;
using MultiShooterGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame.GameObjects
{
    class MapGenerator
    {

        private static Random random;

        private static Vector2[] directions = new Vector2[]{new Vector2(-1, 0), new Vector2(0, -1),
            new Vector2(1, 0), new Vector2(0, 1)};

        private static void SetNeighbours(Tile[,] tiles, Tile tile)
        {
            if (tile.Name == Tile.BlockName.SolidWall)
            {
                return;
            }
            int x = (int)(tile.position.X / Map.TileSize);
            int y = (int)(tile.position.Y / Map.TileSize);
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);
            List<Tile> walkableNeighbours = new List<Tile>();
            for (int d = 0; d < directions.Length; d++)
            {
                int nX = x + (int)directions[d].X;
                int nY = y + (int)directions[d].Y;
                if (nX >= 0 && nX < width && nY >= 0 && nY < height)
                {
                    if (tiles[nX, nY].Name != Tile.BlockName.SolidWall)
                    {
                        walkableNeighbours.Add(tiles[nX, nY]);
                    }
                }

            }
            tile.SetNeighbours(walkableNeighbours);
        }

        #region Cellular Map

        public static Map GenerateRandomCellularMap(int width, int height)
        {
            int iterations = 7;

            Map map = new Map(width, height);
            Map previousMap = new Map(width, height);

            //Initialize with random tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map.tiles[x, y].SetType(random.Next(0, 100) < 55 ? Tile.BlockName.Grass : Tile.BlockName.SolidWall);
                }
            }

            previousMap.CopyTileNames(map.tiles);
            for (int i = 0; i < iterations; i++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {

                        //Check if we should set the tile to ground
                        if (previousMap.NumberOfNeighbourTiles(x, y, 1, Tile.BlockName.SolidWall) >= 5)
                        {
                            map.tiles[x, y].SetType(Tile.BlockName.SolidWall);                        
                        }
                        else if(iterations<4 && previousMap.NumberOfNeighbourTiles(x, y, 2, Tile.BlockName.SolidWall) <= 2)
                        {
                            map.tiles[x, y].SetType(Tile.BlockName.Grass);
                        }
                        else
                        {
                            map.tiles[x, y].SetType(Tile.BlockName.SolidWall);
                        }

                    }
                }
                previousMap.CopyTileNames(map.tiles);
            }

            return map;
        }

        /*
        void RandomizeBushesOnGround()
        {

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (tiles[x,y] == TileTypes::Ground)
                    {
                        tiles[x,y] = tileRandomizer(_random) < 70 ? TileTypes::Ground : TileTypes::Bush;
                    }

                }
            }
        }
        
        void CreateRandomBushes(int iterations)
        {
            GameMap previousMap = *this;

            std::uniform_int_distribution<std::mt19937::result_type> tileRandomizer(1, 100);
            for (int i = 0; i < iterations; i++)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (tiles[x,y] == TileTypes::Wall)
                        {
                            continue;
                        }

                        //Check directions for ground tiles
                        int numberOfGroundtiles = previousMap.NumberOfNeighbourTiles(x, y, 2, TileTypes::Bush);

                        //Check if we should set the tile to ground
                        if (numberOfGroundtiles >= 10 || (numberOfGroundtiles == 9 && previousMap.GetTile(x, y) == TileTypes::Bush))
                        {
                            tiles[x,y] = TileTypes::Bush;
                        }
                        else
                        {
                            tiles[x,y] = TileTypes::Ground;
                        }

                    }
                }
                previousMap = *this;
            }
        }
        */
       
        #endregion

        #region RoguelikeMap
        public static Map GenerateRoguelikeMap(int width, int height)
        {
            Tile[,] tiles = new Tile[width, height];
            SetBaseLevel(tiles);
            MakeWalls(tiles, 1, 1, width - 2, height - 2);
            Decorate(tiles);
            foreach (Tile t in tiles)
            {
                SetNeighbours(tiles, t);
            }
            return new Map(tiles);
        }
        private static void Decorate(Tile[,] tiles)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);
            for (int i = 0; i < random.Next(height) + height; i++)
            {
                int xRect = random.Next(1, width - 3);
                int yRect = random.Next(1, height - 3);
                int rectWidth = random.Next(2, Math.Min(4, width - xRect));
                int rectHeight = random.Next(2, Math.Min(4, height - yRect));
                for (int x = xRect; x < xRect + rectWidth; x++)
                {
                    for (int y = yRect; y < yRect + rectHeight; y++)
                    {
                        tiles[x, y].SetType(Tile.BlockName.TallGrass);
                    }
                }
            }
            for (int triesToRemove = random.Next(width)+width; triesToRemove > 0;triesToRemove-- )
            {
                int xRect = random.Next(1, width - 3);
                int yRect = random.Next(1, height - 3);
                if(tiles[xRect,yRect].Name == Tile.BlockName.BreakableWall)
                {
                    tiles[xRect, yRect].SetType(Tile.BlockName.Grass);
                }
            }
        }
        private static void SetBaseLevel(Tile[,] tiles)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                   
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        tiles[x, y] = new Tile(Tile.BlockName.SolidWall, x * Map.TileSize, y * Map.TileSize);
                    }
                    else
                    {

                        tiles[x, y] = new Tile(Tile.BlockName.Grass, x * Map.TileSize, y * Map.TileSize);
                    }
                }
            }
        }
        private static void MakeWalls(Tile[,] tiles,int x, int y, int width, int height)
        {
            const int minSplit = 4;

            int minSplitX = x + minSplit;
            int maxSplitX = x + width - minSplit;
            int minSplitY = y + minSplit;
            int maxSplitY = y + height - minSplit;

            if (minSplitX >= maxSplitX && minSplitY >= maxSplitY)
            {
                return;
            }


            bool doVerticalSplit = false;

            if (minSplitX >= maxSplitX)
            {
                doVerticalSplit = false;
            }
            else if (minSplitY >= maxSplitY)
            {
                doVerticalSplit = true;
            }
            else
            {
                doVerticalSplit = random.Next() % 2 == 0;
            }
            int split;
            if (doVerticalSplit)
            {
                split = random.Next(x + minSplit, x + width - minSplit + 1);

                int yStart = random.Next(y + 1, y + height - 1);
                int yEnd = random.Next(yStart, y + height - 1);
                for (int i = y; i < y + height; i++)
                {
                    tiles[split, i].SetType(Tile.BlockName.BreakableWall);
                }

                MakeWalls(tiles,x, y, split - x, height);
                MakeWalls(tiles, split + 1, y, width - (split - x) - 1, height);
            }
            else
            {
                split = random.Next(y + minSplit, y + height - minSplit + 1);

                int xStart = random.Next(x + 1, x + width / 2);
                int xEnd = random.Next(xStart, x + width - 1);
                for (int i = x; i < x + width; i++)
                {
                    tiles[i, split].SetType(Tile.BlockName.BreakableWall);
                }
                MakeWalls(tiles, x, y, width, split - y);
                MakeWalls(tiles, x, split + 1, width, height - (split - y) - 1);
            }

        }
        #endregion

        #region PacmanMap
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
        public static Map GeneratePacmanMap(int width, int height)
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
            foreach (Tile t in tiles)
            {
                SetNeighbours(tiles, t);
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
        #endregion

    }
}
