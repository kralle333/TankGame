using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameLibrary;
using MonoGameLibrary.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiShooterGame.GameObjects;

namespace ShooterGuys
{
	class PlayScreen : GameScreen
	{
		private Tank[] _players = new Tank[4];
		private List<Tank> _playersRemaining = new List<Tank>();
		private Map _currentMap;
		private const int cMapWidth = 42;
		private const int cMapHeight = 20;
		private const int cTileSize = 32;

		private ShapeRenderer shapeRenderer;

		private int numberOfPlayers = 0;
		private Tank.ControlScheme[] usedControls;
		private int[] teamsSelected;

		private Viewport[] usedViewPorts;
		public static bool usingSplitScreen = false;

        private List<Tuple<Vector2, float>> startingLocations = new List<Tuple<Vector2, float>>();
		private Random random = new Random();
		private Viewport guiViewPort;
		private Dictionary<Tank,SpriteText> _healthText;
		private SpriteText _wonText;

		public PlayScreen(int numberOfPlayers, Tank.ControlScheme[] controls, int[] selectedTeams)
			: base(ScreenType.Standard)
		{
			usedControls = controls;
            teamsSelected = selectedTeams;
			this.numberOfPlayers = numberOfPlayers;
		}

		private void ResetGame()
		{
			startingLocations.Clear();
			_deadSoldiers.Clear();
			_playersRemaining.Clear();
			SetStartingPositions(cMapWidth, cMapHeight);
            _currentMap = MapGenerator.GenerateRoguelikeMap(cMapWidth, cMapHeight);
            _currentMap.LoadContent(_contentManager);
			for (int i = 0; i < numberOfPlayers; i++)
			{
				_players[i].Reset();
				int randomPosition = random.Next(0, startingLocations.Count);
				_players[i].position = startingLocations[randomPosition].Item1;
                _players[i].Rotation = startingLocations[randomPosition].Item2;
				startingLocations.RemoveAt(randomPosition);
				_playersRemaining.Add(_players[i]);
			}
		}
		public override void LoadContent()
		{
			base.LoadContent();
			screenManager.Game.IsMouseVisible = true;
			shapeRenderer = new ShapeRenderer("splitscreenborder");
			shapeRenderer.LoadContent(_contentManager);
			screenManager.ClearColor = Color.DarkGreen;
			Tile.InitializeTilePreferences();

			Map.TileSize = cTileSize;
            MapGenerator.Initialize();
            _currentMap = MapGenerator.GenerateRoguelikeMap(cMapWidth, cMapHeight);
			_currentMap.LoadContent(_contentManager);

			SetStartingPositions(cMapWidth, cMapHeight);
			SetViewPorts(numberOfPlayers);
			camera.Move(new Vector2(-10, -Map.TileSize * 2));

			_wonText = new SpriteText("BigFont", "Player won", new Vector2(300, 300));
			_wonText.Depth = 1f;
			_wonText.LoadContent(_contentManager);
			_wonText.Hide();

			_healthText = new Dictionary<Tank, SpriteText>();
			for (int i = 0; i < numberOfPlayers; i++)
			{
				int randomPosition = random.Next(0, startingLocations.Count);
				_players[i] = new Tank(i,startingLocations[randomPosition].Item1, teamsSelected[i], usedControls[i], usedViewPorts[i].Bounds);
                _players[i].Rotation = startingLocations[randomPosition].Item2;
                _players[i].LoadContent(_contentManager,_spriteBatch);
                _players[i].ResetHealth();
				_healthText[_players[i]] = new SpriteText("HealthFont", "", new Vector2(50 + (i*300), -25));
                _healthText[_players[i]].color = Tank.ConvertTeamToColor(teamsSelected[i]);
				Add(_healthText[_players[i]]);
				startingLocations.RemoveAt(randomPosition);
				if (!usingSplitScreen)
				{
					_players[i].camera = this.camera;
				}
				_playersRemaining.Add(_players[i]);
			}


			PooledObjects.Initialize();
			PooledObjects.bullets.ForEach(b => b.LoadContent(_contentManager));

		}

		private void SetViewPorts(int numberOfPlayers)
		{
			usedViewPorts = new Viewport[numberOfPlayers];
			int screenWidth = (int)GameSettings.GetResolution().X;
			int screenHeight = (int)GameSettings.GetResolution().Y;
			switch (numberOfPlayers)
			{
				case 1:
					usedViewPorts[0] = new Viewport(0, 0, screenWidth, screenHeight);
					break;
				case 2:
					usedViewPorts[0] = new Viewport(0, 0, screenWidth / 2, screenHeight);
					usedViewPorts[1] = new Viewport(usedViewPorts[0].Width, 0, screenWidth / 2, screenHeight);
					break;
				case 3:
					usedViewPorts[0] = new Viewport(0, 0, screenWidth / 2, screenHeight / 2);
					usedViewPorts[1] = new Viewport(usedViewPorts[0].Width, 0, screenWidth / 2, screenHeight / 2);
					usedViewPorts[2] = new Viewport(0, usedViewPorts[1].Height, screenWidth / 2, screenHeight / 2); break;
				case 4:
					usedViewPorts[0] = new Viewport(0, 0, screenWidth / 2, screenHeight / 2);
					usedViewPorts[1] = new Viewport(usedViewPorts[0].Width, 0, screenWidth / 2, screenHeight / 2);
					usedViewPorts[2] = new Viewport(0, usedViewPorts[1].Height, screenWidth / 2, screenHeight / 2);
					usedViewPorts[3] = new Viewport(usedViewPorts[0].Width, usedViewPorts[1].Height, screenWidth / 2, screenHeight / 2);
					break;
			}
			guiViewPort = new Viewport(0, 0, screenWidth, screenHeight);
		}
		private void SetStartingPositions(int mapWidth, int mapHeight)
		{
            float offsetX = Map.TileSize * 1.5f;
            float offsetY = Map.TileSize * 1.5f;
            startingLocations.Add(new Tuple<Vector2,float>(new Vector2(offsetX, offsetY ),0));
            startingLocations.Add(new Tuple<Vector2, float>(new Vector2((mapWidth * Map.TileSize) - offsetX, offsetY), (float)Math.PI));
			startingLocations.Add(new Tuple<Vector2, float>(new Vector2(offsetX, (mapHeight * Map.TileSize) - offsetY),0));
            startingLocations.Add(new Tuple<Vector2, float>(new Vector2((mapWidth * Map.TileSize) - offsetX, (mapHeight * Map.TileSize) - offsetY),(float)Math.PI));
		}

		private readonly List<Tank> _deadSoldiers = new List<Tank>();
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			_deadSoldiers.Clear();
			for (int i = 0; i < _playersRemaining.Count; i++)
			{
				_playersRemaining[i].Update(gameTime);
				_playersRemaining[i].HandleWallCollisions(_currentMap);
				if (_playersRemaining[i].Health <= 0)
				{
					_healthText[_playersRemaining[i]].text = "Player is dead";
					_playersRemaining[i].Hide();
					_deadSoldiers.Add(_playersRemaining[i]);
					continue;
				}
				else
				{
                    _healthText[_playersRemaining[i]].text = "Player " + (_playersRemaining[i].PlayerIndex + 1) + ":";
				}
				foreach (Tank otherSoldier in _playersRemaining)
				{
					if (otherSoldier != _playersRemaining[i] &&
						Vector2.Distance(otherSoldier.position, _playersRemaining[i].position) < otherSoldier.Width)
					{
						Vector2 directionVector = otherSoldier.position - _playersRemaining[i].position;
						directionVector.Normalize();
						_playersRemaining[i].ApplyForce(5f, -directionVector);
					}
				}
			}
			foreach (Tank soldier in _deadSoldiers)
			{
				_playersRemaining.Remove(soldier);
			}

			if (_playersRemaining.Count < 2)
			{
				_wonText.Show();
				if (_playersRemaining.Count == 0)
				{
					_wonText.text = "Draw";
				}
				else
				{
					_wonText.text = _playersRemaining[0].color + " Won!";
				}
			}
			foreach (Bullet b in PooledObjects.bullets)
			{
				if (b.isVisible)
				{
					b.Update(gameTime);
					int tileX = (int)Math.Round(b.Center.X / Map.TileSize);
					int tileY = (int)Math.Round(b.Center.Y / Map.TileSize);
                    if(tileX>=_currentMap.Width || tileY>=_currentMap.Height || tileX<0 || tileY<0)
                    {
                        b.Hide();
                        continue;
                    }
					Tile[] possibleColliders = new Tile[5];
					possibleColliders[0] = tileX - 1 >= 0 ? _currentMap.tiles[tileX - 1, tileY] : null;
					possibleColliders[1] = tileX + 1 < _currentMap.Width ? _currentMap.tiles[tileX + 1, tileY] : null;
					possibleColliders[2] = tileY - 1 >= 0 ? _currentMap.tiles[tileX, tileY - 1] : null;
					possibleColliders[3] = tileY + 1 < _currentMap.Height ? _currentMap.tiles[tileX, tileY + 1] : null;
					possibleColliders[4] = _currentMap.tiles[tileX, tileY];

					for (int i = 0; i < possibleColliders.Length; i++)
					{
						if (possibleColliders[i] != null)
						{
							if (possibleColliders[i].Type == Tile.BlockType.Solid && possibleColliders[i].CheckRectangluarCollision(b))
							{
								possibleColliders[i].Damage(b);
								b.Hide();
								break;
							}
						}
					}

					for (int i = 0; i < numberOfPlayers; i++)
					{
						if (_players[i].Health > 0 && _players[i].CheckCircularCollision(b) && b.Team != _players[i].Team)
						{
							_players[i].Damage(b);
							b.Hide();
						}
					}

				}
			}
		}
		public override void HandleInput(InputState inputState)
		{
			base.HandleInput(inputState);
			for (int i = 0; i < numberOfPlayers; i++)
			{
				_players[i].HandleInput(inputState, screenManager.currentGameTime);
				if (_playersRemaining.Count <= 1)
				{
					switch (usedControls[i])
					{
						case Tank.ControlScheme.Empty: break;
						case Tank.ControlScheme.Keyboard:
							if (inputState.IsKeyNewPressed(Keys.Enter))
							{
								ResetGame();
							}
							break;
						default:
							inputState.ActivePlayerIndex = _players[i].GamePadIndex;
							if (inputState.IsButtonNewPressed(Buttons.Start))
							{
								ResetGame();
							}
							break;
					}
				}
			}
		}

		public override void CustomDraw(GameTime gameTime)
		{

			if (usingSplitScreen)
			{
                for (int i = 0; i < _playersRemaining.Count; i++)
                {
                    screenManager.GraphicsDevice.Viewport = usedViewPorts[i];
                    _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, _players[i].camera.GetTransformation(screenManager.GraphicsDevice));
                    if (_playersRemaining.Count <= 1)
                    {
                        _wonText.Draw(_spriteBatch, gameTime);
                    }
                    PooledObjects.bullets.ForEach(b => b.Draw(_spriteBatch, gameTime));


                    _currentMap.Draw(_spriteBatch, gameTime);
                    for (int j = 0; j < numberOfPlayers; j++)
                    {
                        _players[j].Draw(_spriteBatch, gameTime);
                    }
                    _spriteBatch.End();

                }
               
				screenManager.GraphicsDevice.Viewport = guiViewPort;
				_spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, camera.GetTransformation(screenManager.GraphicsDevice));
				for (int i = 0; i < _playersRemaining.Count; i++)
				{
					shapeRenderer.DrawRectangle(_spriteBatch, usedViewPorts[i].Bounds);
				}
				_spriteBatch.End();
			}
            else
            {
                _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, camera.GetTransformation(screenManager.GraphicsDevice));
                if (_playersRemaining.Count <= 1)
                {
                    _wonText.Draw(_spriteBatch, gameTime);
                }
                PooledObjects.bullets.ForEach(b => b.Draw(_spriteBatch, gameTime));
                
                _currentMap.Draw(_spriteBatch, gameTime);
                for (int j = 0; j < numberOfPlayers; j++)
                {
                    _players[j].Draw(_spriteBatch, gameTime);
                }
                _spriteBatch.End();
            }
		}

	}
}
