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

        private bool _isOpened = false;
        public Powerup(int x, int y, PowerupType powerupType):base("Sprites",x,y,new Rectangle(64,0,32,32),0.4f)
        {
            _type = powerupType;
        }
        
        public void Open()
        {
            if(!_isOpened)
            {
                _isOpened = true;
                SetTextureRectangle(new Rectangle(96+32*(int)_type,144,32,32));
            }
        }
    }
}
