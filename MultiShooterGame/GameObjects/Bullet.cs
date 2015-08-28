using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameLibrary;

namespace MultiShooterGame
{
	class Bullet : Sprite
	{

		public Vector2 directionVector;
		private float _speed;
		public int damage = 0;
		private float _lifeTimer;
		private const float cTimeToLive = 1000;
        private int _team = -1;
        public int Team { get { return _team; } }
		public Bullet()
			: base("Sprites", 0, 0, new Rectangle(64,64,8,6), 0.8f)
		{
            SetOrigin(4, 3f);
			Hide();
		}

		public void Activate(Vector2 fromPosition, Vector2 direction, float speed,float size, int team)
		{
			Show();
            _speed = 10;
            _team = team;
            position = fromPosition+(direction * 10);
            SetTextureRectangle(new Rectangle(112+(32*team), 80, 8, 6));
            rotation = GeometricHelper.GetAngleFromVectorDirection(direction);
			directionVector = direction;
			SetScale(size*2);
            damage = (int)size;
			_lifeTimer = cTimeToLive;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (isVisible)
			{
				position += directionVector * _speed;
				_lifeTimer -= gameTime.ElapsedGameTime.Milliseconds;
				if (_lifeTimer < 0)
				{
					Hide();
				}
			}
		}
	}
}
