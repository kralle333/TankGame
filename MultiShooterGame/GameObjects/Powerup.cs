using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGameLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame.GameObjects
{
    class Powerup:Sprite
    {
        public enum PowerupType {Pickup, Speed, AttackSpeed, BigAmmo,Mines }
        private PowerupType _type;
        public PowerupType Type { get { return _type; } }
        private const int timeToGetVisible = 2000;
        private int visibleTimer = 0;

        public Powerup(int x, int y, PowerupType powerupType)
            : base("Sprites", x, y, new Rectangle(96 + 32 * (int)powerupType, 144, 32, 32), 0.4f)
        {
            _type = powerupType;
            color.A = 0;
            visibleTimer = 0;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (visibleTimer<timeToGetVisible)
            {
                visibleTimer += gameTime.ElapsedGameTime.Milliseconds;
                float alphaValue = (255f * ((float)visibleTimer / timeToGetVisible));
                color.A = (Byte)alphaValue;
            }
            base.Draw(spriteBatch, gameTime);
        }
    }
}
