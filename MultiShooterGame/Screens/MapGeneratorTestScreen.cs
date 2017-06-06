using MonoGameLibrary;
using MultiShooterGame.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MultiShooterGame.Screens
{
    class MapGeneratorTestScreen : GameScreen
    {
        private Map _map;
        private int _width = 42;
        private int _height = 24;

        private enum MapGenerationType { Roguelike = Keys.NumPad1, Cellular = Keys.NumPad2, Pacman = Keys.NumPad3 };
        private MapGenerationType _generationType = MapGenerationType.Cellular;

        public MapGeneratorTestScreen() : base(ScreenType.Standard)
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();
            Tile.InitializeTilePreferences();
            MapGenerator.Initialize();
            Map.TileSize = 32;
            GenerateMap();
        }

        private void GenerateMap()
        {
            switch (_generationType)
            {
                case MapGenerationType.Cellular: _map = MapGenerator.GenerateRandomCellularMap(_width, _height); break;
                case MapGenerationType.Roguelike: _map = MapGenerator.GenerateRoguelikeMap(_width, _height); break;
            }
            _map.LoadContent(_contentManager);
        }


        public override void HandleInput(InputState inputState)
        {
            base.HandleInput(inputState);
            bool regen = false;

            if (inputState.IsKeyNewPressed(Keys.NumPad1))
            {
                _generationType = MapGenerationType.Roguelike;
                regen = true;
            }
            else if (inputState.IsKeyNewPressed(Keys.NumPad2))
            {
                _generationType = MapGenerationType.Cellular;
                regen = true;
            }
            else if (inputState.IsKeyNewPressed(Keys.Left))
            {
                _width--;
                regen = true;
            }
            else if (inputState.IsKeyNewPressed(Keys.Right))
            {
                _width++;
                regen = true;
            }
            else if (inputState.IsKeyNewPressed(Keys.Up))
            {
                _height--;
                regen = true;
            }
            else if (inputState.IsKeyNewPressed(Keys.Down))
            {
                _height++;
                regen = true;
            }

            if(regen)
            {
                GenerateMap();
            }
        }
        public override void CustomDraw(GameTime gameTime)
        {
            base.CustomDraw(gameTime);
            _spriteBatch.Begin();
            _map.Draw(_spriteBatch, gameTime);
            _spriteBatch.End();
        }
    }
}
