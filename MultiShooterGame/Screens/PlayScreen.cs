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
using Microsoft.Xna.Framework.Content;
using MonoGameLibrary.Menus;
using MultiShooterGame.Screens;

namespace MultiShooterGame
{
    class PlayScreen : GameScreen
    {
        public static Random random = new Random();

        private Tank[] _players = new Tank[4];
        private List<Tank> _playersRemaining = new List<Tank>();
        private List<Tuple<Vector2, float>> startingLocations = new List<Tuple<Vector2, float>>();
        private int numberOfPlayers = 0;
        private Tank.ControlScheme[] usedControls;
        private int[] teamsSelected;

        private Map _currentMap;
        public static int MapWidth = 42;
        public static int MapHeight = 21;
        private const int cTileSize = 32;

        private Viewport[] usedViewPorts;
        public static bool usingSplitScreen = false;
        private Viewport guiViewPort;
        private Camera2D _guiCamera;
        private MenuFrame _guiFrame;
        private ResultsScreen _resultsScreen;
        private SpriteText _wonText;
        private bool _isRoundOver = false;

        private bool doingCountdown = true;
        private float countdownTimer = 3;

        private List<Powerup> _powerups = new List<Powerup>();
        private int _countdownToPowerup = 0;
        private int _minCountdownToPowerup = 10000;
        private int _maxCountdownToPowerup = 20000;

        private int gameTimer = 0;
        private SpriteText _timerFont;

        public PlayScreen(int numberOfPlayers, Tank.ControlScheme[] controls, int[] selectedTeams)
            : base(ScreenType.Standard,200,0)
        {
            usedControls = controls;
            teamsSelected = selectedTeams;
            this.numberOfPlayers = numberOfPlayers;
            GameRules.playerScores = new int[numberOfPlayers];
        }

        private void StartNextRound()
        {
            startingLocations.Clear();
            _playersRemaining.Clear();
            GameRules.totalTimeUsed += gameTimer;
            _powerups.Clear();
            gameTimer = 0;

            SetStartingPositions(MapWidth, MapHeight);
            _isRoundOver = false;
            Tank winner = null;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (GameRules.playerScores[i] >= GameRules.numberToWin)
                {
                    winner = _players[i];
                    ShowWinner(winner);
                    screenManager.AddScreen(new WinScreen(winner));
                    //return;
                }
            }

            //Make more advanced here
            switch (GameRules._mapSelectionSetting)
            {
                case GameRules.MapSelectionSetting.RandomGenerated:
                    _currentMap = MapGenerator.GenerateRandomCellularMap(MapWidth, MapHeight);
                    break;
                case GameRules.MapSelectionSetting.Shuffle:
                    break;
            }
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
        public void ShowWinner(Tank winner)
        {

        }
        public override void LoadContent()
        {
            base.LoadContent();
            screenManager.Game.IsMouseVisible = false;
            ShapeRenderer.graphicsDevice = screenManager.GraphicsDevice;
            screenManager.ClearColor = Color.Black;
            Tile.InitializeTilePreferences();
            usingSplitScreen = false;

            Map.TileSize = cTileSize;
            MapGenerator.Initialize();
            _currentMap = MapGenerator.GenerateRandomCellularMap(MapWidth, MapHeight);
            _currentMap.LoadContent(_contentManager);

            SetStartingPositions(MapWidth, MapHeight);
            SetViewPorts(numberOfPlayers);
            camera.Move(new Vector2(-12, -3 * Map.TileSize + 16));
            LoadGui();

            //Load players
            for (int i = 0; i < numberOfPlayers; i++)
            {
                int randomPosition = random.Next(0, startingLocations.Count);
                _players[i] = new Tank(i, startingLocations[randomPosition].Item1, teamsSelected[i], usedControls[i], usedViewPorts[i].Bounds);
                _players[i].Rotation = startingLocations[randomPosition].Item2;
                _players[i].LoadContent(_contentManager, _spriteBatch);
                _players[i].ResetHealth();
                startingLocations.RemoveAt(randomPosition);
                if (!usingSplitScreen)
                {
                    _players[i].camera = this.camera;
                }

                _playersRemaining.Add(_players[i]);
            }


            PooledObjects.Initialize();
            PooledObjects.bullets.ForEach(b => b.LoadContent(_contentManager));
            PooledObjects.tileFragmentClusters.ForEach(fc => fc.LoadContent(_contentManager));
            PooledObjects.explosions.ForEach(e => e.LoadContent(_contentManager));
        }
        private void LoadGui()
        {
            _guiCamera = new Camera2D();
            _resultsScreen = new ResultsScreen(teamsSelected);
            _guiFrame = new MenuFrame(new Rectangle(0, 0, (int)GameSettings.ScreenWidth - 6, (int)GameSettings.ScreenHeight), "Menu", new Rectangle(0, 0, 16, 16));
            _guiFrame.AddHorizontalLine(16, (int)GameSettings.ScreenWidth - 22, (int)(2 * 32));
            _guiFrame.AddSplit(0, (int)(2 * 32), MenuFrame.SplitType.Right);
            _guiFrame.AddSplit((int)GameSettings.ScreenWidth - 22, (int)(2 * 32), MenuFrame.SplitType.Left);
            for (int i = 1; i <= 2; i++)
            {
                _guiFrame.AddSplit((i * 290), 0, MenuFrame.SplitType.Down);
                _guiFrame.AddSplit((i * 290), (int)(2 * 32), MenuFrame.SplitType.Up);
                _guiFrame.AddVerticalLine((i * 290), 16, 2 * 32);
            }
            for (int i = 3; i <= 4; i++)
            {
                _guiFrame.AddSplit((i * 290) - 90, 0, MenuFrame.SplitType.Down);
                _guiFrame.AddSplit((i * 290) - 90, (int)(2 * 32), MenuFrame.SplitType.Up);
                _guiFrame.AddVerticalLine((i * 290) - 90, 16, 2 * 32);
            }

            _guiFrame.LoadContent(_contentManager);


            _wonText = new SpriteText("BigFont", "Player won", new Vector2(300, 300));
            _wonText.Depth = 1f;
            _wonText.color = Color.White;
            _wonText.LoadContent(_contentManager);
            _wonText.CenterText(new Rectangle((int)camera.Position.X, (int)camera.Position.Y, (int)GameSettings.ScreenWidth, (int)GameSettings.ScreenHeight), true, true);
            _wonText.Hide();

            _timerFont = new SpriteText("HealthFont", "Time: ", new Vector2(0, 0));
            _timerFont.LoadContent(_contentManager, _spriteBatch);
            _timerFont.CenterText(new Rectangle(0, 16, (int)GameSettings.ScreenWidth, 16 * 3), true, true);
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
                    usedViewPorts[0] = new Viewport(0, Map.TileSize * 2 + 16, screenWidth / 2, screenHeight - Map.TileSize * 2 + 16);
                    usedViewPorts[1] = new Viewport(usedViewPorts[0].Width, -Map.TileSize * 2 + 16, screenWidth / 2, screenHeight - Map.TileSize * 2 + 16);
                    break;
                case 3:
                    usedViewPorts[0] = new Viewport(0, Map.TileSize * 2 + 16, screenWidth / 2, screenHeight / 2);
                    usedViewPorts[1] = new Viewport(usedViewPorts[0].Width, 0, screenWidth / 2, screenHeight / 2);
                    usedViewPorts[2] = new Viewport(0, usedViewPorts[1].Height, screenWidth / 2, screenHeight / 2); break;
                case 4:
                    usedViewPorts[0] = new Viewport(0, Map.TileSize * 2 + 16, screenWidth / 2, screenHeight / 2);
                    usedViewPorts[1] = new Viewport(usedViewPorts[0].Width, Map.TileSize * 2 + 16, screenWidth / 2, screenHeight / 2);
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
            startingLocations.Add(new Tuple<Vector2, float>(new Vector2(offsetX, offsetY), 0));
            startingLocations.Add(new Tuple<Vector2, float>(new Vector2((mapWidth * Map.TileSize) - offsetX, offsetY), (float)Math.PI));
            startingLocations.Add(new Tuple<Vector2, float>(new Vector2(offsetX, (mapHeight * Map.TileSize) - offsetY), 0));
            startingLocations.Add(new Tuple<Vector2, float>(new Vector2((mapWidth * Map.TileSize) - offsetX, (mapHeight * Map.TileSize) - offsetY), (float)Math.PI));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!_isRoundOver)
            {
                gameTimer += gameTime.ElapsedGameTime.Milliseconds;
                _timerFont.text = "Time: " + gameTimer / 1000;
            }
            UpdatePlayers(gameTime);
            HandleBulletCollision(gameTime);
            HandlePowerups(gameTime);
            PooledObjects.tileFragmentClusters.ForEach(fc => fc.Update(gameTime));
            PooledObjects.explosions.ForEach(e => e.Update(gameTime));
            if (_playersRemaining.Count <= 1 && !_isRoundOver)
            {
                _isRoundOver = true;
                if (GameRules.selectedGameType == GameRules.GameType.WinRoundsToWin)
                {
                    GameRules.playerScores[_playersRemaining[0].PlayerIndex]++;
                }
                screenManager.AddScreen(_resultsScreen);
                _resultsScreen.Show();
            }

            if (_isRoundOver && !_resultsScreen.isVisible)
            {
                StartNextRound();
            }
        }
        private void UpdatePlayers(GameTime gameTime)
        {
            _playersRemaining.RemoveAll(x => x.Health <= 0);
            for (int i = 0; i < _playersRemaining.Count; i++)
            {
                _playersRemaining[i].Update(gameTime);
                _playersRemaining[i].HandleWallCollisions(_currentMap);
                foreach (Tank otherSoldier in _playersRemaining)
                {
                    if (otherSoldier != _playersRemaining[i] &&
                        Vector2.Distance(otherSoldier.position, _playersRemaining[i].position) < otherSoldier.Width)
                    {
                        Vector2 directionVector = otherSoldier.position - _playersRemaining[i].position;
                        directionVector.Normalize();
                        Console.WriteLine(directionVector);
                        _playersRemaining[i].ApplyForce(16f, -directionVector);

                    }
                }
                if (_playersRemaining[i].HasRoomForPowerup)
                {
                    List<Powerup> powerupsToRemove = new List<Powerup>();
                    for (int j = 0; j < _powerups.Count; j++)
                    {
                        if (_playersRemaining[i].IsCollidingWith(_powerups[j]))
                        {
                            powerupsToRemove.Add(_powerups[j]);
                            _playersRemaining[i].GetPowerup(_powerups[j]);
                        }
                    }
                    powerupsToRemove.ForEach(x => _powerups.Remove(x));
                }
            }
        }
        private void HandleBulletCollision(GameTime gameTime)
        {
            foreach (Bullet b in PooledObjects.bullets)
            {
                if (!b.IsVisible)
                {
                    continue;
                }

                b.Update(gameTime);
                int tileX = (int)Math.Round(b.Center.X / Map.TileSize);
                int tileY = (int)Math.Round(b.Center.Y / Map.TileSize);
                if (tileX >= _currentMap.Width || tileY >= _currentMap.Height || tileX < 0 || tileY < 0)
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
                    if (possibleColliders[i] == null) continue;

                    if (possibleColliders[i].Type == Tile.BlockType.Solid && possibleColliders[i].IsCollidingWith(b))
                    {

                        int remainderDamage = b.damage - possibleColliders[i].Health;
                        possibleColliders[i].Damage(b);
                        if (remainderDamage <= 0)
                        {
                            b.Hide();
                        }
                        else
                        {
                            b.damage = remainderDamage;
                            //b.SetScale(remainderDamage * 2);
                        }
                        break;
                    }
                }

                for (int i = 0; i < _playersRemaining.Count; i++)
                {
                    if (_playersRemaining[i].Health > 0 && _playersRemaining[i].IsCollidingCircularlyWith(b) && b.Team != _playersRemaining[i].Team)
                    {
                        _playersRemaining[i].Damage(b);
                        b.Hide();
                    }
                }
            }
        }
        private void HandlePowerups(GameTime gameTime)
        {
            _countdownToPowerup -= gameTime.ElapsedGameTime.Milliseconds;
            if ((gameTimer > 60000 && _minCountdownToPowerup == 10000) ||
                (gameTimer > 120000 && _minCountdownToPowerup == 5000))
            {
                _minCountdownToPowerup /= 2;
                _maxCountdownToPowerup /= 2;
            }
            if (_countdownToPowerup <= 0)
            {
                _countdownToPowerup = random.Next(_minCountdownToPowerup, _maxCountdownToPowerup);
                Tile randTile = null;
                bool foundTile = false;
                do
                {
                    randTile = _currentMap.GetRandomWalkableTile();
                    if (randTile == null)
                    {
                        break;
                    }
                    foundTile = true;
                    for (int i = 0; i < _powerups.Count; i++)
                    {
                        if (randTile.position == _powerups[i].position)
                        {
                            foundTile = false;
                            break;
                        }
                    }
                } while (!foundTile);
                int startRange = GameRules.selectedGameType == GameRules.GameType.CollectToWin ? 0 : 1;
                Powerup newPower = new Powerup((int)randTile.position.X, (int)randTile.position.Y, (Powerup.PowerupType)random.Next(startRange, 4));
                newPower.LoadContent(_contentManager);
                _powerups.Add(newPower);
            }
        }
        public override void HandleInput(InputState inputState)
        {
            base.HandleInput(inputState);

            for (int i = 0; i < numberOfPlayers; i++)
            {
                _players[i].HandleInput(inputState, screenManager.currentGameTime);
                if (_isRoundOver)
                {
                    switch (usedControls[i])
                    {
                        case Tank.ControlScheme.Keyboard:
                            if (inputState.IsKeyNewPressed(Keys.S))
                            {
                                StartNextRound();
                            }
                            break;
                        default:
                            inputState.ActivePlayerIndex = _players[i].GamePadIndex;
                            if (inputState.IsButtonNewPressed(Buttons.Start))
                            {
                                StartNextRound();
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
                    float newCameraX = _players[i].camera.Position.X;
                    float newCameraY = _players[i].camera.Position.Y;
                    if (_players[i].camera.Position.X < 0) newCameraX = 0;
                    if (_players[i].camera.Position.Y < 0) newCameraY = 0;
                    if (_players[i].camera.Position.X + usedViewPorts[i].Width > GameSettings.ScreenWidth)
                    {
                        newCameraX = GameSettings.ScreenWidth - usedViewPorts[i].Width;
                    }
                    if (_players[i].camera.Position.Y + usedViewPorts[i].Height > GameSettings.ScreenHeight)
                    {
                        newCameraY = GameSettings.ScreenHeight - usedViewPorts[i].Height;
                    }
                    _players[i].camera.Position = new Vector2(newCameraX, newCameraY);

                    _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, _players[i].camera.GetTransformation(screenManager.GraphicsDevice));
                    PooledObjects.bullets.ForEach(b => b.Draw(_spriteBatch, gameTime));
                    PooledObjects.tileFragmentClusters.ForEach(fc => fc.Draw(_spriteBatch, gameTime));
                    _currentMap.Draw(_spriteBatch, gameTime);
                    for (int j = 0; j < numberOfPlayers; j++)
                    {
                        _players[j].Draw(_spriteBatch, gameTime);
                    }
                    _spriteBatch.End();

                }

                screenManager.GraphicsDevice.Viewport = guiViewPort;
                _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, camera.GetTransformation(screenManager.GraphicsDevice));


                if (_isRoundOver)
                {
                    _wonText.Draw(_spriteBatch, gameTime);
                }
                _guiFrame.Draw(_spriteBatch, gameTime);
                _spriteBatch.End();
            }
            else
            {
                _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, camera.GetTransformation(screenManager.GraphicsDevice));
                PooledObjects.bullets.ForEach(b => b.Draw(_spriteBatch, gameTime));
                PooledObjects.tileFragmentClusters.ForEach(fc => fc.Draw(_spriteBatch, gameTime));
                PooledObjects.explosions.ForEach(e => e.Draw(_spriteBatch, gameTime));
                _powerups.ForEach(x => x.Draw(_spriteBatch, gameTime));
                _currentMap.Draw(_spriteBatch, gameTime);
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    _players[i].Draw(_spriteBatch, gameTime);
                }
                _spriteBatch.End();
                _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, _guiCamera.GetTransformation(screenManager.GraphicsDevice));
                _guiFrame.Draw(_spriteBatch, gameTime);
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    _players[i].DrawStatsBar(gameTime);
                }
                if (_isRoundOver)
                {
                    _wonText.Draw(_spriteBatch, gameTime);
                }
                _timerFont.Draw(gameTime);
                _spriteBatch.End();
            }
        }

    }
}
