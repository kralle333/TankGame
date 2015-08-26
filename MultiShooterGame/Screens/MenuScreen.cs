using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameLibrary;
using MonoGameLibrary.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShooterGuys.Screens;

namespace ShooterGuys
{
	class MenuScreen:GameScreen
	{
		private MenuSystem _mainMenu;
        private Sprite _title;
		public MenuScreen():base(ScreenType.Standard)
		{

		}

		public override void LoadContent()
		{
			base.LoadContent();
            screenManager.ClearColor = Color.Black;
            _title = new Sprite("Title",(int) GameSettings.ScreenWidth / 2 - 250, 64 + 32, new Rectangle(0, 0, 500, 64), 1);
            Add(_title);
            SpriteFont font = _contentManager.Load<SpriteFont>("font1");
			_mainMenu = new MenuSystem("",new Vector2(GameSettings.ScreenWidth/2-((font.MeasureString("Main Menu").X/2)),GameSettings.ScreenHeight/2),font);
            _mainMenu.UsedAlignment = MenuSystem.Alignment.Center; 
            _mainMenu.MenuItemColor = Color.White;
            _mainMenu.AddItem("Play Local Multiplayer");
			//MenuSystem durpTest = new MenuSystem("Nothing here durp",new Vector2(GameSettings.ScreenWidth/2-GameSettings.ScreenWidth/7,GameSettings.ScreenHeight/3),_contentManager.Load<SpriteFont>("Font1"));
			//durpTest.ToggleTitle();
			//_mainMenu.AddSubSystem("Player Online Multiplayer",durpTest);
			_mainMenu.AddItem("Quit Game");
			_mainMenu.ToggleTitle();

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
                if (inputState.IsButtonNewPressed(Buttons.A))
                {
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
            }

			if (_mainMenu.PressedItem == "Play Local Multiplayer")
			{
				screenManager.AddScreen(new PlayerSelectScreen());
			}
			else if (_mainMenu.PressedItem == "Quit Game")
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
