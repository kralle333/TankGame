using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameLibrary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Drawing;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Menus;
using MultiShooterGame.GameObjects;

namespace MultiShooterGame.Screens
{
    class PlayerSelectScreen : GameScreen
    {
        private SpriteFont _font;
        private MenuFrame _menuFrame;

        private bool[] _gamePadSlotsUsed = new bool[4];
        private int _keyboardSlotsLeft = 1;

        private Tank.ControlScheme[] _selectedControls = new Tank.ControlScheme[8];

        private float _currentRotation = 0;
        private int _playersPlaying = 0;

        private Sprite[] _tankSprites = new Sprite[10];

        private int[] _selectedTeams = new int[4];
        private string _helpText = "Press (A)/Enter to add Player \nPress (B)/Q to remove Player \nPress (Start)/S to start game";
        
        public PlayerSelectScreen()
            : base(ScreenType.Standard)
        {

        }
        public override void LoadContent()
        {
            base.LoadContent();
            screenManager.ClearColor = Color.Black;
            _font = _contentManager.Load<SpriteFont>("Font1");

            for (int i = 0; i < 10; i++)
            {
                _tankSprites[i] = new Sprite("Sprites", 0,0, new Rectangle(96 + (32 * i), 112, 32, 32), 1);
                _tankSprites[i].Origin = new Vector2(16, 16);
                _tankSprites[i].LoadContent(_contentManager, _spriteBatch);
                _tankSprites[i].Hide();
                _tankSprites[i].rotation = _currentRotation;
            }
            for (int i = 0; i < 4; i++) _selectedTeams[i] = -1;
            _menuFrame = new MenuFrame(new Rectangle(80, 180, 1200, 320), "Menu",new Rectangle(0,0,16,16));
            for(int i = 1;i<4;i++)
            {
                _menuFrame.AddSplit((i * 300) + 80, 180,MenuFrame.SplitType.Down);
                _menuFrame.AddSplit((i * 300) + 80, 500-16, MenuFrame.SplitType.Up);
                _menuFrame.AddVerticalLine((i * 300) + 80, 196, 500-16);
            }
           
            _menuFrame.LoadContent(_contentManager);
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
                if (gamepadPressedStart || inputState.IsKeyNewPressed(Keys.S))
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
                                float rotation = _tankSprites[_selectedTeams[i]].rotation;
                                ChangeTeam(-1, i);
                                _tankSprites[_selectedTeams[i]].Show();
                                _tankSprites[_selectedTeams[i]].rotation = rotation;

                            }
                            else if (inputState.IsKeyNewPressed(Keys.Right))
                            {
                                _tankSprites[_selectedTeams[i]].Hide();
                                float rotation = _tankSprites[_selectedTeams[i]].rotation;
                                ChangeTeam(1, i);
                                _tankSprites[_selectedTeams[i]].Show();
                                _tankSprites[_selectedTeams[i]].rotation = rotation;
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

        public override void CustomDraw(GameTime gameTime)
        {
            _currentRotation += 0.03f;
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, _helpText, new Vector2(GameSettings.ScreenWidth / 2 - _font.MeasureString(_helpText).X / 2, GameSettings.ScreenHeight * 0.1f), Color.White);
            for (int i = 0; i < 4; i++)
            {
                _tankSprites[i].rotation = _currentRotation;
                _spriteBatch.DrawString(_font, "Player " + (i + 1), new Vector2(80 + 150 - (_font.MeasureString("Player " + (i + 1)).X / 2) + (i * 300), 200), Color.White);
                if (_selectedControls[i] != Tank.ControlScheme.Empty)
                {
                    _spriteBatch.DrawString(_font, _selectedControls[i].ToString(), new Vector2(80 + 150 - (_font.MeasureString(_selectedControls[i].ToString()).X / 2) + (i * 300), 250), Color.White);
                    _spriteBatch.DrawString(_font, "Change color:Left/Right", new Vector2(110 + (i * 300), 450), Color.White);
                    _tankSprites[_selectedTeams[i]].position = new Vector2(80 + (i * 300)+140, 350);
                    _tankSprites[_selectedTeams[i]].Draw(gameTime);
                }
            }
            _menuFrame.Draw(_spriteBatch, gameTime);
            _spriteBatch.End();
            base.CustomDraw(gameTime);
        }
    }
}
