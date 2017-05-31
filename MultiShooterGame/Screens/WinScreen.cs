using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MultiShooterGame.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame.Screens
{
    class WinScreen:GameScreen
    {
        private SpriteText winner;
        private SpriteText goBack;
        private Tank _tank;
        private float timer;
        public WinScreen(Tank tank):base(ScreenType.Standard,500,500)
        {
            _tank = tank;
            _tank.ChangeScale(4);
           
            winner = new SpriteText("BigFont", "Congratulations!", new Vector2(200, 200));
            goBack = new SpriteText("HealthFont", "Press Enter/Start to go back to the main menu", new Vector2(0, GameSettings.ScreenHeight - 220));
           
        }

        public override void LoadContent()
        {
            base.LoadContent();
            Add(_tank);
            Add(winner);
            Add(goBack);
            timer = 1000;
            winner.CenterText(new Rectangle(0, 200, (int)GameSettings.ScreenWidth, 0), true, false);
            goBack.CenterText(new Rectangle(0, 0, (int)GameSettings.ScreenWidth, 0), true, false);
            _tank.position = new Vector2(GameSettings.ScreenWidth / 2 - (_tank.Width * 4 / 2), GameSettings.ScreenHeight / 2 - (_tank.Width * 4 / 2));
        }

        public override void HandleInput(InputState inputState)
        {
            base.HandleInput(inputState);
            if(inputState.IsButtonNewPressed(Buttons.Start) || inputState.IsKeyNewPressed(Keys.Enter))
            {
                screenManager.AddScreen(new MenuScreen());
            }
        }
        public override void CustomDraw(GameTime gameTime)
        {
            base.CustomDraw(gameTime);
            if (timer<0)
            {
                timer = 1000;
                winner.position = new Vector2(PlayScreen.random.Next(200, 400), PlayScreen.random.Next(200, 400));
            }
        }
    }
}
