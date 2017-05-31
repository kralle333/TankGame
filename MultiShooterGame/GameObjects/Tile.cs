using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameLibrary;
using Microsoft.Xna.Framework;
using MultiShooterGame.GameObjects;

namespace MultiShooterGame
{
	class Tile : Sprite
	{
		public enum BlockType { Solid, Walkable, WalkableCovering }
		public enum BlockName { BreakableWall,SolidWall, Grass, TallGrass,Gravel }
		private BlockType _type;
		public BlockType Type { get { return _type; } }
		private BlockName _name;
		public BlockName Name { get { return _name; } }
		private static Dictionary<BlockName, BlockType> _tileBlockTypes = new Dictionary<BlockName, BlockType>();
		private static Dictionary<BlockName, Rectangle[]> _tileSpriteMap = new Dictionary<BlockName, Rectangle[]>();
		private int _health;
        public int Health { get { return _health; } }
        private List<Tile> neighbours = new List<Tile>();

		private static bool _hasInitialized = false;
		private static int _tileSize = 32;
		public static void InitializeTilePreferences()
		{
			if (!_hasInitialized)
			{
				_tileSpriteMap.Add(BlockName.Grass, new Rectangle[] { SpriteHelper.GetSpriteRectangle(_tileSize,_tileSize, 0, 1, 0) });
				_tileSpriteMap.Add(BlockName.TallGrass, SpriteHelper.GetSpriteRectangleStrip(_tileSize, _tileSize, 0, 1, 1, 1, 2));
				_tileSpriteMap.Add(BlockName.SolidWall, new Rectangle[] { SpriteHelper.GetSpriteRectangle(_tileSize, _tileSize, 0, 2, 1) });
				_tileSpriteMap.Add(BlockName.BreakableWall, SpriteHelper.GetSpriteRectangleStrip(_tileSize, _tileSize, 0, 0,0,0,3));
				_tileSpriteMap.Add(BlockName.Gravel, new Rectangle[] { SpriteHelper.GetSpriteRectangle(_tileSize, _tileSize, 0, 1,2) });


				_tileBlockTypes.Add(BlockName.Grass, BlockType.Walkable);
				_tileBlockTypes.Add(BlockName.Gravel, BlockType.Walkable);
				_tileBlockTypes.Add(BlockName.TallGrass, BlockType.WalkableCovering);
				_tileBlockTypes.Add(BlockName.BreakableWall, BlockType.Solid);
				_tileBlockTypes.Add(BlockName.SolidWall, BlockType.Solid);
				_hasInitialized = true;
			}
			
		}
        

		public Tile(BlockName type, int xPos, int yPos)
			: base("Sprites", xPos, yPos,32,32,true, 0)
		{
			SetType(type);
		}
        public Tile(Tile toCopy):base("sprites",0,0,32,32,true,0)
        {
            position = toCopy.position;
            SetType(toCopy.Name);
            foreach(Tile neigbour in toCopy.neighbours)
            {
                neighbours.Add(new Tile(neigbour));
            }
        }

		public void SetType(BlockName type)
		{
			ClearAnimationStates();
			AddAnimationState(new SpriteState("Idle", _tileSpriteMap[type], 0));
			SetCurrentAnimationState("Idle");

			_name = type;
			_type = _tileBlockTypes[type];
			switch (_type)
			{
				case BlockType.Solid:
					_depth = SpriteHelper.GetDefaultDepth(SpriteHelper.SpriteDepth.High);
					_health = 2;
					break;
				case BlockType.WalkableCovering:
					_depth = SpriteHelper.GetDefaultDepth(SpriteHelper.SpriteDepth.High);
					break;
				case BlockType.Walkable: _depth = SpriteHelper.GetDefaultDepth(SpriteHelper.SpriteDepth.Low); break;

			}

		}
        public void SetNeighbours(List<Tile> tiles)
        {
            if(neighbours.Count!=0)
            {
                neighbours.Clear();
                Console.WriteLine("Tile already has neighbours, are you sure you want to do this?");
            }
            neighbours.AddRange(tiles);
        }
		public void Damage(Bullet b)
		{
			if (_name == BlockName.BreakableWall)
			{
                _health -= b.damage;
				if (_health <= 0)
				{
					SetType(BlockName.Gravel);
                    FragmentCluster fc = PooledObjects.tileFragmentClusters.Find(x => x.IsUsable);
                    fc.Explode(this.Center, b.directionVector,(float)(Math.PI/2),1000);
                    AudioManager.PlaySFX("TileCrumble", 0.2f);
				}
				else
				{
                    AudioManager.PlaySFX("TileHit" + PlayScreen.random.Next(1,5), 0.2f);
					NextFrame();
				}
                if(b.damage>1)
                {
                    b.damage--;
                    foreach(Tile t in neighbours)
                    {                        
                        t.Damage(b);
                    }
                }
			}
		}
	}
}
