using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameLibrary;
using MonoGameLibrary.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiShooterGame.Screens;

namespace MultiShooterGame
{
    class MenuScreen : GameScreen
    {
        private MenuSystem _mainMenu;
        private Sprite _title;
        public MenuScreen()
            : base(ScreenType.Standard)
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();
            screenManager.ClearColor = Color.Black;
            _title = new Sprite("Title", (int)GameSettings.ScreenWidth / 2 - 512, 64 + 32, new Rectangle(0, 0, 1024, 128), 1);
            Add(_title);
            SpriteFont font = _contentManager.Load<SpriteFont>("HealthFont");
            _mainMenu = new MenuSystem(font, "", new Vector2(GameSettings.ScreenWidth / 2 - ((font.MeasureString("Play Local Multiplayer").X / 2)), _title.position.Y + 256), true);
            _mainMenu.MenuItemColor = Color.White;
            _mainMenu.AddItem("Play Local Multiplayer");
            _mainMenu.AddItem("Quit Game");
            _mainMenu.ToggleTitle();
            _mainMenu.ItemOffSetY *= 2;
            _mainMenu.Center(new Rectangle(0, 0, (int)GameSettings.ScreenWidth, (int)GameSettings.ScreenHeight), true, false);

            screenManager.Game.IsMouseVisible = false;
        }
        public override void HandleInput(InputState inputState)
        {
            base.HandleInput(inputState);
            if (inputState.IsKeyNewPressed(Keys.Up))
            {
                _mainMenu.GoUp();
            }
            else if (inputState.IsKeyNewPressed(Keys.Down))
            {
                _mainMenu.GoDown();
            }
            else if (inputState.IsKeyNewPressed(Keys.Enter))
            {
                _mainMenu.PressItem();
            }

            for (int i = 0; i < 4; i++)
            {
                inputState.ActivePlayerIndex = (PlayerIndex)i;
                if (inputState.GetLeftStickPosition().Y == 1)
                {
                    _mainMenu.GoUp();
                }
                else if (inputState.GetLeftStickPosition().Y < -0.5f)
                {
                    _mainMenu.GoDown();
                }
                else if (inputState.IsButtonNewPressed(Buttons.Start))
                {
                    _mainMenu.PressItem();
                }

            }

            if (_mainMenu.PressedItem.text == "Play Local Multiplayer")
            {
                screenManager.AddScreen(new PlayerSelectScreen());
            }
            else if (_mainMenu.PressedItem.text == "Quit Game")
            {
                screenManager.Game.Exit();
            }
        }
        public override void CustomDraw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _mainMenu.Draw(_spriteBatch);
            _spriteBatch.End();
        }
    }
}
