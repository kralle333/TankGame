using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MultiShooterGame
{
	class Map
	{
		public Tile[,] tiles;
		private int _width;
		public int Width { get { return _width; } }
		private int _height;
		public int Height { get { return _height; } }
        private bool _hasWalkableTile = true;
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
        public Tile GetRandomWalkableTile()
        {
            Tile tile = null;
            _hasWalkableTile = _hasWalkableTile ? !isMapFull() : false;  
            if (_hasWalkableTile)
            {
                while (true)
                {
                    int randX=PlayScreen.random.Next(1, _width - 1);
                    int randY = PlayScreen.random.Next(1, _height - 1);
                    if(tiles[randX,randY].Type == Tile.BlockType.Walkable)
                    {
                        return tiles[randX, randY];
                    }
                }
            }
            return tile;
        }
        private bool isMapFull()
        {

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (tiles[x,y].Type == Tile.BlockType.Walkable)
                    {
                        return false;
                    }
                }
            }
            return true;
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
