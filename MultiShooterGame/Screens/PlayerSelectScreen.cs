using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameLibrary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Drawing;
using Microsoft.Xna.Framework.Input;

namespace ShooterGuys.Screens
{
    class PlayerSelectScreen : GameScreen
    {
        private SpriteFont _font;
        private ShapeRenderer _shapeRenderer;

        private bool[] _gamePadSlotsUsed = new bool[4];
        private int _keyboardSlotsLeft = 1;

        private Tank.ControlScheme[] _selectedControls = new Tank.ControlScheme[8];

        private int _playersPlaying = 0;

        private Tank[] _tankSprites = new Tank[8];
        private int[] _selectedTeams = new int[8];
        private string _helpText = "Press (A)/Enter to add Player \nPress (B)/Q to remove Player \nPress (Start)/Space to start game";
        public PlayerSelectScreen()
            : base(ScreenType.Standard)
        {

        }
        public override void LoadContent()
        {
            base.LoadContent();
            screenManager.ClearColor = Color.DarkGreen;
            _font = _contentManager.Load<SpriteFont>("Font1");
            _shapeRenderer = new ShapeRenderer("splitscreenborder");
            _shapeRenderer.LoadContent(_contentManager);
            for (int i = 0; i < 8; i++)
            {
                _tankSprites[i] = new Tank(i,new Vector2(300 + (i * 300), 300), i, Tank.ControlScheme.Empty, new Rectangle(0, 0, (int)GameSettings.GetResolution().X, (int)GameSettings.GetResolution().Y));
                _tankSprites[i].camera = this.camera;
                _tankSprites[i].LoadContent(_contentManager, _spriteBatch);
                _tankSprites[i].Hide();
                _tankSprites[i].rotationSpeed =0.05f;
                _selectedTeams[i] = -1;
            }
        }

        public override void HandleInput(InputState inputState)
        {
            base.HandleInput(inputState);
            HandleGameEntering(inputState);
            HandleGameExiting(inputState);
            HandleGameStarting(inputState);
            HandleColorSelection(inputState);
        }
        private void HandleGameEntering(InputState inputState)
        {

            if (_playersPlaying < 4)
            {
                if (inputState.IsKeyNewPressed(Keys.Enter) && _keyboardSlotsLeft > 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (_selectedControls[i] == Tank.ControlScheme.Empty)
                        {
                            _keyboardSlotsLeft--;
                            _selectedControls[i] = Tank.ControlScheme.Keyboard;
                            ChangeTeam(1, i);
                            _tankSprites[_selectedTeams[i]].Show();
                            _playersPlaying++;
                            break;
                        }
                    }

                }
                for (int i = 0; i < 4; i++)
                {
                    inputState.ActivePlayerIndex = (PlayerIndex)i;
                    if (!_gamePadSlotsUsed[i] && inputState.IsButtonNewPressed(Buttons.A))
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if (_selectedControls[j] == Tank.ControlScheme.Empty)
                            {
                                _gamePadSlotsUsed[i] = true;
                                _selectedControls[j] = (Tank.ControlScheme)(i + 3);
                                ChangeTeam(1, j);
                                _tankSprites[_selectedTeams[j]].Show();
                                _playersPlaying++;
                                break;
                            }
                        }
                    }
                }
            }
        }
        private void HandleGameExiting(InputState inputState)
        {
            if (_playersPlaying > 0)
            {
                for (int i = 0; i < _playersPlaying; i++)
                {
                    if (_selectedControls[i] == Tank.ControlScheme.Keyboard)
                    {
                        if (inputState.IsKeyNewPressed(Keys.Q))
                        {
                            _selectedControls[i] = Tank.ControlScheme.Empty;
                            _selectedTeams[i] = -1;
                            _tankSprites[i].Hide();
                            _keyboardSlotsLeft++;
                            _playersPlaying--;

                        }
                    }
                    else if (_selectedControls[i] != Tank.ControlScheme.Empty)
                    {
                        inputState.ActivePlayerIndex = (PlayerIndex)(int)(_selectedControls[i] - 3);
                        if (inputState.IsButtonNewPressed(Buttons.B))
                        {
                            _gamePadSlotsUsed[(int)inputState.ActivePlayerIndex] = false;
                            _selectedControls[i] = Tank.ControlScheme.Empty;
                            _tankSprites[i].Hide();
                            _selectedTeams[i] = -1;
                            _playersPlaying--;
                        }
                    }
                }
            }
        }
        private void HandleGameStarting(InputState inputState)
        {

            if (_playersPlaying > 1)
            {
                bool gamepadPressedStart = false;
                for (int i = 0; i < 4; i++)
                {
                    inputState.ActivePlayerIndex = (PlayerIndex)i;
                    if (_gamePadSlotsUsed[i] && inputState.IsButtonNewPressed(Buttons.Start))
                    {
                        gamepadPressedStart = true;
                        break;
                    }
                }
                if (gamepadPressedStart || inputState.IsKeyNewPressed(Keys.Space))
                {
                    screenManager.AddScreen(new PlayScreen(_playersPlaying, _selectedControls, _selectedTeams));
                }
            }
        }
        private void HandleColorSelection(InputState inputState)
        {
            if (_playersPlaying > 0)
            {
                for (int i = 0; i < _playersPlaying; i++)
                {
                    switch (_selectedControls[i])
                    {
                        case Tank.ControlScheme.Empty: break;
                        case Tank.ControlScheme.Keyboard:
                            if (inputState.IsKeyNewPressed(Keys.Left))
                            {
                                _tankSprites[_selectedTeams[i]].Hide();
                                ChangeTeam(-1, i);
                                _tankSprites[_selectedTeams[i]].Show();
                            }
                            else if (inputState.IsKeyNewPressed(Keys.Right))
                            {
                                _tankSprites[_selectedTeams[i]].Hide();
                                ChangeTeam(1, i);
                                _tankSprites[_selectedTeams[i]].Show();
                            }
                            break;
                        default:
                            inputState.ActivePlayerIndex = (PlayerIndex)(int)(_selectedControls[i] - 3);
                            if (inputState.IsButtonNewPressed(Buttons.LeftThumbstickLeft))
                            {
                                _tankSprites[_selectedTeams[i]].Hide();
                                ChangeTeam(-1, i);
                                _tankSprites[_selectedTeams[i]].Show();
                            }
                            else if (inputState.IsButtonNewPressed(Buttons.LeftThumbstickRight))
                            {
                                _tankSprites[_selectedTeams[i]].Hide();
                                ChangeTeam(1, i);
                                _tankSprites[_selectedTeams[i]].Show();
                            }
                            break;

                    }
                }
            }
        }
        private void ChangeTeam(int spaces, int playerIndex)
        {
            bool foundNewTeam = false;
            int newTeam = _selectedTeams[playerIndex];
            for (int i = 0; i < _tankSprites.Length; i++)
            {
                newTeam += spaces;
                if (newTeam < 0) { newTeam = _tankSprites.Length-1; }
                else if (newTeam > _tankSprites.Length-1) { newTeam = 0; }
                foundNewTeam = true;
                for (int j = 0; j < _selectedTeams.Length; j++)
                {
                    if (playerIndex != j)
                    {
                        if (_selectedTeams[j] == newTeam)
                        {
                            foundNewTeam = false;
                            break;
                        }
                    }
                }

                if (foundNewTeam)
                {
                    _selectedTeams[playerIndex] = newTeam;
                    break;
                }
            }
        }

        public override void CustomDraw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, _helpText, new Vector2(GameSettings.ScreenWidth / 2 - _font.MeasureString(_helpText).X / 2, GameSettings.ScreenHeight * 0.1f), Color.Black);
            for (int i = 0; i < 4; i++)
            {
                _shapeRenderer.DrawRectangle(_spriteBatch, 80 + (i * 300), 180, 300, 500);
                _spriteBatch.DrawString(_font, "Player " + (i + 1), new Vector2(80 + 150 - (_font.MeasureString("Player " + (i + 1)).X / 2) + (i * 300), 200), Color.Black);
                if (_selectedControls[i] != Tank.ControlScheme.Empty)
                {
                    _spriteBatch.DrawString(_font, _selectedControls[i].ToString(), new Vector2(80+150-(_font.MeasureString(_selectedControls[i].ToString()).X/2)+(i * 300), 250), Color.Black);
                    _spriteBatch.DrawString(_font, "Change color:Left/Right", new Vector2(110 + (i * 300), 450), Color.Black);
                    _tankSprites[_selectedTeams[i]].position = new Vector2(80 + (i * 300)+140, 350);
                    _tankSprites[_selectedTeams[i]].Draw(gameTime);
                }
            }

            _spriteBatch.End();
            base.CustomDraw(gameTime);
        }
    }
}
