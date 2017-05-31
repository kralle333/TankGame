using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MultiShooterGame.Screens;

namespace MultiShooterGame
{
		//Copy this to your new project
		class EntryPoint : Game
		{
			private GraphicsDeviceManager graphics;
			private ScreenManager screenManager;
			private GameSettings gameSettings;


			public EntryPoint()
			{
				graphics = new GraphicsDeviceManager(this);


				screenManager = new ScreenManager(this);
				gameSettings = new GameSettings(graphics);
				GameSettings.SetResolution(1366, 768, false);	
				Components.Add(screenManager);
				screenManager.AddScreen(new MenuScreen());
			}
		}
		static class Program
		{
			static void Main(string[] args)
			{
				using (EntryPoint game = new EntryPoint())
				{
					game.Run();
				}
			}
		}
}
