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
        private const int cMapWidth = 42;
        private const int cMapHeight = 22;
        private const int cTileSize = 32;

        private ShapeRenderer shapeRenderer;
        private Viewport[] usedViewPorts;
        public static bool usingSplitScreen = false;
        private Viewport guiViewPort;
        private Dictionary<Tank, SpriteText> _healthText;
        private SpriteText _wonText;
        private MenuFrame _guiFrame;

        private bool doingCountdown = true;
        private float countdownTimer = 3;

        private List<Powerup> _powerups = new List<Powerup>();
        private int _countdownToPowerup = 0;
        private int _minCountdownToPowerup = 10000;
        private int _maxCountdownToPowerup = 20000;

        private int gameTimer = 0;
        private SpriteText _timerFont;

        public PlayScreen(int numberOfPlayers, Tank.ControlScheme[] controls, int[] selectedTeams)
            : base(ScreenType.Standard)
        {
            usedControls = controls;
            teamsSelected = selectedTeams;
            this.numberOfPlayers = numberOfPlayers;
            GameRules.playerScores = new int[numberOfPlayers];
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
            screenManager.Game.IsMouseVisible = false;
            shapeRenderer = new ShapeRenderer("splitscreenborder");
            shapeRenderer.LoadContent(_contentManager);
            screenManager.ClearColor = Color.Black;
            Tile.InitializeTilePreferences();
            usingSplitScreen = false;


            Map.TileSize = cTileSize;
            MapGenerator.Initialize();
            _currentMap = MapGenerator.GenerateRoguelikeMap(cMapWidth, cMapHeight);
            _currentMap.LoadContent(_contentManager);

            SetStartingPositions(cMapWidth, cMapHeight);
            SetViewPorts(numberOfPlayers);
            camera.Move(new Vector2(-12, -Map.TileSize * 2 + 16));
            _guiFrame = new MenuFrame(new Rectangle((int)camera.Position.X + 3, (int)camera.Position.Y, (int)GameSettings.ScreenWidth - 6, (int)GameSettings.ScreenHeight), "Menu", new Rectangle(0, 0, 16, 16));
            _guiFrame.LoadContent(_contentManager);

            _wonText = new SpriteText("BigFont", "Player won", new Vector2(300, 300));
            _wonText.Depth = 1f;
            _wonText.color = Color.White;
            _wonText.LoadContent(_contentManager);
            _wonText.CenterText(new Rectangle((int)camera.Position.X, (int)camera.Position.Y, (int)GameSettings.ScreenWidth, (int)GameSettings.ScreenHeight), true, true);
            _wonText.Hide();

            _timerFont = new SpriteText("HealthFont", "Time: ", new Vector2(0, -24));
            Add(_timerFont);
            _timerFont.CenterText(new Rectangle(0, 0, (int)GameSettings.ScreenWidth, 0), true, false);

            _healthText = new Dictionary<Tank, SpriteText>();
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
                _healthText[_players[i]] = new SpriteText("HealthFont", "", new Vector2(20+(i * 300), -24));
                _healthText[_players[i]].color = Tank.ConvertTeamToColor(teamsSelected[i]);
                Add(_healthText[_players[i]]);

                _playersRemaining.Add(_players[i]);
            }


            PooledObjects.Initialize();
            PooledObjects.bullets.ForEach(b => b.LoadContent(_contentManager));
            PooledObjects.fragmentClusters.ForEach(fc => fc.LoadContent(_contentManager));
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

        private readonly List<Tank> _deadSoldiers = new List<Tank>();
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            gameTimer += gameTime.ElapsedGameTime.Milliseconds;
            _timerFont.text = "Time: " + gameTimer / 1000;
            UpdatePlayers(gameTime);
            HandleBulletCollision(gameTime);
            HandlePowerups(gameTime);
            PooledObjects.fragmentClusters.ForEach(fc => fc.Update(gameTime));
        }
        private void UpdatePlayers(GameTime gameTime)
        {
            _deadSoldiers.Clear();
            for (int i = 0; i < _playersRemaining.Count; i++)
            {
                _playersRemaining[i].Update(gameTime);
                _playersRemaining[i].HandleWallCollisions(_currentMap);
                if (_playersRemaining[i].Health <= 0)
                {
                    _healthText[_playersRemaining[i]].text = "Player " + (_playersRemaining[i].PlayerIndex + 1) + " is dead";
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
                        _playersRemaining[i].ApplyForce(20f, -directionVector);
                    }
                }
                List<Powerup> powerupsToRemove = new List<Powerup>();
                for (int j = 0; j < _powerups.Count; j++)
                {
                    if (_playersRemaining[i].CheckRectangluarCollision(_powerups[j]))
                    {
                        powerupsToRemove.Add(_powerups[j]);
                        _playersRemaining[i].GetPowerup(_powerups[j].Type);
                    }
                }
                powerupsToRemove.ForEach(x => _powerups.Remove(x));
            }
            foreach (Tank soldier in _deadSoldiers)
            {
                _playersRemaining.Remove(soldier);
            }

            if (_playersRemaining.Count < 2 && _wonText.isVisible)
            {
                _wonText.Show();
                if (_playersRemaining.Count == 0)
                {
                    _wonText.text = "Draw";
                }
                else
                {
                    _wonText.text = "Player " + (_playersRemaining[0].PlayerIndex + 1) + " Won! Press (Start)/S to start new game";
                }
            }
        }
        private void HandleBulletCollision(GameTime gameTime)
        {
            foreach (Bullet b in PooledObjects.bullets)
            {
                if (!b.isVisible)
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

                    if (possibleColliders[i].Type == Tile.BlockType.Solid && possibleColliders[i].CheckRectangluarCollision(b))
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
                            b.SetScale(remainderDamage * 2);
                        }
                        break;
                    }
                }

                for (int i = 0; i < _playersRemaining.Count; i++)
                {
                    if (_playersRemaining[i].Health > 0 && _playersRemaining[i].CheckCircularCollision(b) && b.Team != _playersRemaining[i].Team)
                    {
                        _playersRemaining[i].Damage(b);
                        b.Hide();
                    }
                }
                for (int i = 0; i < _powerups.Count; i++)
                {
                    if (b.CheckCircularCollision(_powerups[i]))
                    {
                        _powerups[i].Open();
                    }
                }
            }
        }
        private void HandlePowerups(GameTime gameTime)
        {
            _countdownToPowerup -= gameTime.ElapsedGameTime.Milliseconds;
            if ((gameTimer > 30000 && _minCountdownToPowerup == 10000) ||
                (gameTimer > 60000 && _minCountdownToPowerup == 5000))
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
                int startRange = GameRules.selectedGameType== GameRules.GameType.CollectToWin?0:1;
                Powerup newPower = new Powerup((int)randTile.position.X, (int)randTile.position.Y, (Powerup.PowerupType)random.Next(startRange, 3));
                newPower.LoadContent(_contentManager);
                _powerups.Add(newPower);
            }
        }
        public override void HandleInput(InputState inputState)
        {
            base.HandleInput(inputState);

            bool reset = false;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                _players[i].HandleInput(inputState, screenManager.currentGameTime);
                if (_playersRemaining.Count < 2)
                {
                    switch (usedControls[i])
                    {
                        case Tank.ControlScheme.Keyboard:
                            reset = inputState.IsKeyNewPressed(Keys.S);
                            break;
                        default:
                            inputState.ActivePlayerIndex = _players[i].GamePadIndex;
                            reset = inputState.IsButtonNewPressed(Buttons.Start);
                            break;
                    }
                    if (reset) ResetGame(); break;
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
                    PooledObjects.fragmentClusters.ForEach(fc => fc.Draw(_spriteBatch, gameTime));
                    _currentMap.Draw(_spriteBatch, gameTime);
                    for (int j = 0; j < numberOfPlayers; j++)
                    {
                        _players[j].Draw(_spriteBatch, gameTime);
                    }
                    _spriteBatch.End();

                }

                screenManager.GraphicsDevice.Viewport = guiViewPort;
                _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, camera.GetTransformation(screenManager.GraphicsDevice));

                for (int i = 0; i < numberOfPlayers; i++)
                {
                    for (int j = 0; j < _players[i].Health; j++)
                    {
                        _players[i].healthBar.position = _players[i].healthDrawPosition;
                        _players[i].healthBar.position.X += j * _players[i].healthBar.Width;
                        _players[i].healthBar.Draw(gameTime);
                    }
                }

                if (_playersRemaining.Count <= 1)
                {
                    _wonText.Draw(_spriteBatch, gameTime);
                }
                _guiFrame.Draw(_spriteBatch, gameTime);
                _spriteBatch.End();
            }
            else
            {
                _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, camera.GetTransformation(screenManager.GraphicsDevice));
                if (_playersRemaining.Count <= 1)
                {
                    _wonText.Draw(_spriteBatch, gameTime);
                }
                PooledObjects.bullets.ForEach(b => b.Draw(_spriteBatch, gameTime));
                PooledObjects.fragmentClusters.ForEach(fc => fc.Draw(_spriteBatch, gameTime));
                _powerups.ForEach(x => x.Draw(_spriteBatch, gameTime));
                _guiFrame.Draw(_spriteBatch, gameTime);
                _currentMap.Draw(_spriteBatch, gameTime);
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    _players[i].Draw(_spriteBatch, gameTime);
                    for (int j = 0; j < _players[i].Health; j++)
                    {
                        _players[i].healthBar.position = _players[i].healthDrawPosition;
                        _players[i].healthBar.position.X += j * _players[i].healthBar.Width;
                        _players[i].healthBar.Draw(gameTime);
                    }
                }
                _spriteBatch.End();
            }
        }

    }
}
