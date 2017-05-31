using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Drawing;
using MonoGameLibrary.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame.Screens
{
    class ResultsScreen : GameScreen
    {
        private Sprite[] _tankSprites;
        private HealthBar[] _progressBars;
        private MenuFrame _resultsFrame;
        private SpriteText _resultsText;
        private SpriteText _pressToContinue;
        private bool _hasLoaded = false;
        public bool isVisible = false;
        private Texture2D _menuBackground;
        private Rectangle _backRectangle = new Rectangle(0, 0, 16, 16);
        private int _timerToHandlingInput = 1000;

        public ResultsScreen(int[] teams)
            : base(ScreenType.ActivePopup)
        {
            Rectangle position = new Rectangle((int)(32 * 16),
                                               (int)(10 * 16),
                                               (int)(21 * 16),
                                               (int)(80 * 4));
            Rectangle menuRectangle = new Rectangle(0, 0, 16, 16);

            _resultsFrame = new MenuFrame(position, "Menu", menuRectangle);
            _tankSprites = new Sprite[teams.Length];
            int numberOfPlayers = 0;
            for (int i = 0; i < teams.Length; i++)
            {
                _tankSprites[i] = new Sprite("Sprites", 0, 0, 1);
                _tankSprites[i].SetTextureRectangle(new Rectangle(96 + (32 * teams[i]), 112, 32, 32));
                numberOfPlayers = teams[i] == -1 ? numberOfPlayers : numberOfPlayers + 1;
            }
            _progressBars = new HealthBar[numberOfPlayers];
            _resultsFrame.AddSplit(_resultsFrame.XPos, _resultsFrame.YPos + 64, MenuFrame.SplitType.Right);
            _resultsFrame.AddSplit(_resultsFrame.XPos + _resultsFrame.Width - 16, _resultsFrame.YPos + 64, MenuFrame.SplitType.Left);
            _resultsFrame.AddHorizontalLine(_resultsFrame.XPos + 16, _resultsFrame.XPos + _resultsFrame.Width - 16, _resultsFrame.YPos + 64);
        }
        public void Reset()
        {
            for (int i = 0; i < _progressBars.Length; i++)
            {
                _progressBars[i].Percent = 0;
            }
            isVisible = false;
            _pressToContinue.SetAlpha(0);
        }
        public void Show()
        {
            for (int i = 0; i < _progressBars.Length; i++)
            {
                _progressBars[i].ChangePercentage(((float)GameRules.playerScores[i] / GameRules.numberToWin) * 100,PlayScreen.random.Next(1000,2000));
            }
            _timerToHandlingInput = 1000;
            _pressToContinue.FadeIn(1000, 1);
            isVisible = true;
        }
        public override void LoadContent()
        {
            base.LoadContent();
            if (!_hasLoaded)
            {
                _resultsFrame.LoadContent(_contentManager);
                for (int i = 0; i < _tankSprites.Length; i++)
                {
                    _tankSprites[i].position.X = _resultsFrame.XPos + 32;
                    _tankSprites[i].position.Y = _resultsFrame.YPos + (48 * i) + 96;
                    _tankSprites[i].LoadContent(_contentManager, _spriteBatch);
                }
                for (int i = 0; i < _progressBars.Length; i++)
                {
                    _progressBars[i] = new HealthBar((int)_tankSprites[i].position.X + 32, (int)_tankSprites[i].position.Y - 1, _resultsFrame.Width - 100, 32, Color.Gold, Color.Gray, Color.White);
                    _progressBars[i].Percent = 0;
                }
                _resultsText = new SpriteText("HealthFont", "Results", new Vector2(0, 0));
                _resultsText.LoadContent(_contentManager, _spriteBatch);
                _resultsText.CenterText(new Rectangle(_resultsFrame.XPos, _resultsFrame.YPos + 16, _resultsFrame.Width, 64), true, true);
                _pressToContinue = new SpriteText("HealthFont", "Press S/Start to continue", _resultsText.position);
                _pressToContinue.position.Y += 224;
                _pressToContinue.position.X = _resultsFrame.XPos + 16;
                _pressToContinue.LoadContent(_contentManager, _spriteBatch);
                _pressToContinue.SetAlpha(0);
                Color[] whiteSquare = new Color[16 * 16];
                for (int i = 0; i < whiteSquare.Length; i++)
                {
                    whiteSquare[i] = Color.White;
                }
                _menuBackground = new Texture2D(screenManager.GraphicsDevice, 16, 16);
                _menuBackground.SetData<Color>(whiteSquare);
                _hasLoaded = true;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_timerToHandlingInput > 0)
            {
                _timerToHandlingInput -= gameTime.ElapsedGameTime.Milliseconds;
                if (_timerToHandlingInput <= 0)
                {
                    _pressToContinue.FadeIn(1000, 1);
                }
            }
        }
        public override void HandleInput(InputState inputState)
        {
            base.HandleInput(inputState);
            if (_timerToHandlingInput <= 0)
            {
                bool gamepadPressedStart = false;
                for (int i = 0; i < 4; i++)
                {
                    inputState.ActivePlayerIndex = (PlayerIndex)i;
                    if (inputState.IsButtonNewPressed(Buttons.Start))
                    {
                        gamepadPressedStart = true;
                        break;
                    }
                }
                if (gamepadPressedStart || inputState.IsKeyNewPressed(Keys.S))
                {
                    Reset();
                    screenManager.RemoveScreen(this);
                }
            }

        }
        public override void CustomDraw(GameTime gameTime)
        {
            base.CustomDraw(gameTime);
            _spriteBatch.Begin();

            for (int x = _resultsFrame.XPos + 16; x < _resultsFrame.XPos + _resultsFrame.Width - 16; x += 16)
            {
                for (int y = _resultsFrame.YPos + 16; y < _resultsFrame.YPos + _resultsFrame.Height; y += 16)
                {
                    _spriteBatch.Draw(_menuBackground, new Vector2(x, y), _backRectangle, Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.6f);
                }
            }
            _resultsFrame.Draw(screenManager.spriteBatch, gameTime);
            _resultsText.Draw(gameTime);
            if (_pressToContinue.IsVisible)
            {
                _pressToContinue.Draw(gameTime);
            }

            for (int i = 0; i < _progressBars.Length; i++)
            {
                _tankSprites[i].Draw(gameTime);
                _progressBars[i].Draw(_spriteBatch, gameTime);
            }
            _spriteBatch.End();
        }
    }
}
