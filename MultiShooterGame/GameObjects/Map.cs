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
        private Vector2[] directions = new Vector2[]{new Vector2(-1, 0), new Vector2(0, -1),
            new Vector2(1, 0), new Vector2(0, 1),
            new Vector2(-1, -1), new Vector2(-1, 1),
            new Vector2(1, -1), new Vector2(1, 1)};

        private void SetNeighbours(Tile tile)
        {
            int x = (int)(tile.position.X / TileSize);
            int y = (int)(tile.position.Y / TileSize);

            List<Tile> walkableNeighbours = new List<Tile>();
            for (int d = 0; d < directions.Length; d++)
            {
                int nX = x + (int)directions[d].X;
                int nY = y + (int)directions[d].Y;
                if (nX >= 0 && nX < _width && nY >= 0 && nY < _height)
                {
                    if (tiles[nX,nY].Type != Tile.BlockType.Solid)
                    {
                        walkableNeighbours.Add(tiles[nX,nY]);
                    }
                }

            }
            tile.SetNeighbours(walkableNeighbours);
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
