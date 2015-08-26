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

		public static int TileSize;

        public Map(Tile[,] tiles)
        {
            this.tiles = tiles;
            _width = tiles.GetLength(0);
            _height = tiles.GetLength(1);
            this.tiles[1, 1].SetType(Tile.BlockName.Grass);
            this.tiles[Width - 2, 1].SetType(Tile.BlockName.Grass);
            this.tiles[Width-2, Height-2].SetType(Tile.BlockName.Grass);
            this.tiles[1, Height-2].SetType(Tile.BlockName.Grass);
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
