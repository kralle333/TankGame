using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ShooterGuys
{
	class Map
	{
		public Tile[,] tiles;
		private int _width;
		public int Width { get { return _width; } }
		private int _height;
		public int Height { get { return _height; } }
		private Random random = new Random();
		private bool _isEmpty = true;

		public static int TileSize;

		public Map(int width, int height)
		{
			_width = width;
			_height = height;
			tiles = new Tile[width, height];
			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _height; y++)
				{
					if (x == 0 || y == 0 || x == _width - 1 || y == _height - 1)
					{
                        tiles[x, y] = new Tile(Tile.BlockName.SolidWall, x * TileSize, y * TileSize);
					}
					else
					{

                        tiles[x, y] = new Tile(Tile.BlockName.Grass, x * TileSize,y * TileSize);
					}
				}
			}

		}
		
        public Map(Tile[,] tiles)
        {
            this.tiles = tiles;
            _width = tiles.GetLength(0);
            _height = tiles.GetLength(1);
        }
        
        private void SetBaseLevel()
		{
			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _height; y++)
				{
					if (x == 0 || y == 0 || x == _width - 1 || y == _height - 1)
					{
						tiles[x, y].SetType(Tile.BlockName.SolidWall);
					}
					else
					{

						tiles[x, y].SetType(Tile.BlockName.Grass);
					}
				}
			}
			_isEmpty = true;
		}


		public void GenerateRandomMap()
		{
			if (!_isEmpty)
			{
				SetBaseLevel();
			}
			for (int i = 0; i < random.Next(10) + 10; i++)
			{
				int xRect = random.Next(1, Width - 3);
				int yRect = random.Next(1, Height - 3);
				int rectWidth = random.Next(2, Math.Min(4,Width - xRect));
				int rectHeight = random.Next(2,  Math.Min(4,Height - yRect));
				for (int x = xRect; x < xRect + rectWidth; x++)
				{
					for (int y = yRect; y < yRect+rectHeight; y++)
					{
						tiles[x, y].SetType(Tile.BlockName.TallGrass);
					}
				}
			}

			MakeWalls(1, 1, _width-2, _height-2);
			_isEmpty = false;
		}
		public void MakeWalls(int x, int y, int width, int height)
		{
			const int minSplit = 4;

			int minSplitX = x + minSplit;
			int maxSplitX = x + width - minSplit;
			int minSplitY = y + minSplit;
			int maxSplitY = y + height - minSplit;

			if (minSplitX >=maxSplitX && minSplitY >= maxSplitY)
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
				split = random.Next(x + minSplit, x + width - minSplit+1);

				int yStart = random.Next(y+1, y + height-1);
				int yEnd = random.Next(yStart, y + height-1);
				for (int i = y; i < y+height; i++)
				{
					tiles[split, i].SetType(Tile.BlockName.BreakableWall);
				}

				MakeWalls(x, y, split - x, height);
				MakeWalls(split + 1, y, width - (split - x) - 1, height);
			}
			else
			{
				split = random.Next(y + minSplit, y + height - minSplit+1);

				int xStart = random.Next(x+1, x + width/2);
				int xEnd = random.Next(xStart, x + width-1);
				for (int i = x; i < x+width; i++)
				{
					tiles[i, split].SetType(Tile.BlockName.BreakableWall);
				}
				MakeWalls(x, y, width, split - y);
				MakeWalls(x, split + 1, width,height - (split -y) - 1);
			}

		}

		public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
		{
			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _height; y++)
				{
					tiles[x, y].Draw(spriteBatch, gameTime);
				}
			}
		}

		public void LoadContent(ContentManager contentManager)
		{
			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _height; y++)
				{
					tiles[x, y].LoadContent(contentManager);
				}
			}
		}
	}
}
