using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using ShooterGuys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame.GameObjects
{
    class FragmentCluster
    {
        class Fragment : Sprite
        {
            private float _velocity = 0;
            private Vector2 _direction = Vector2.Zero;
            private float _visibleTimer = 0;
            private float _visibleTime = 0;
            private bool _isActive = false;
            private FragmentCluster _parent;

            public Fragment(FragmentCluster parent)
                : base("Sprites", -1, -1, Rectangle.Empty, 0.45f)
            {
                _parent = parent;
            }
            public void Activate(Vector2 position, Vector2 direction, float velocity,float rotationSpeed, float visibleTime)
            {
                _origin = new Vector2(Width / 2, Height / 2);
                _isActive = true;
                this.position = position;
                _direction = direction;
                _velocity = velocity;
                this.rotationSpeed = rotationSpeed;
                _visibleTimer = 0;
                _visibleTime = visibleTime;
                
                Show();
            }

            public override void Update(GameTime gameTime)
            {
                if (_isActive)
                {
                    position += _velocity * _direction;
                    _velocity /= 2;
                    rotationSpeed /= 2;
                    _visibleTimer += gameTime.ElapsedGameTime.Milliseconds;
                    float progress = 255 * (_visibleTimer / _visibleTime);
                    color.A = (Byte)(255 - (255 * (_visibleTimer/_visibleTime)));
                    if (_visibleTimer >=_visibleTime)
                    {
                        _isActive = false;
                        _parent.RemoveFragment();
                        Hide();
                    }
                }
                base.Update(gameTime);
            }
        }

        private List<Fragment> fragments = new List<Fragment>();
        private int fragmentsRemaining;
        private bool _isUsable;
        public bool IsUsable { get { return _isUsable; } }

        public FragmentCluster(int numberOfFragments)
        {
            fragmentsRemaining = numberOfFragments;
            for (int i = 0; i < numberOfFragments; i++)
            {
                Fragment fragment = new Fragment(this);
                fragment.SetTextureRectangle(new Rectangle(PlayScreen.random.Next(32), PlayScreen.random.Next(32), PlayScreen.random.Next(8, 16), PlayScreen.random.Next(8, 16)));
                fragment.Hide();
                fragments.Add(fragment);
            }
            _isUsable = true;
        }
        public void LoadContent(ContentManager contentManager)
        {
            foreach(Fragment fragment in fragments)
            {
                fragment.LoadContent(contentManager);
            }

        }
        public void RemoveFragment()
        {
            fragmentsRemaining--;
            if(fragmentsRemaining<=0)
            {
                _isUsable = true;
            }
        }
        public void Explode(Tile tile,Vector2 direction)
        {
            foreach (Fragment fragment in fragments)
            {
                Vector2 randomPosition = direction * 16+tile.Center;// new Vector2(PlayScreen.random.Next(32), PlayScreen.random.Next(32)) + tile.position;
                float directionRadians = GeometricHelper.GetAngleFromVectorDirection(direction);
                int randomAngleThousands = PlayScreen.random.Next((int)(1000f * (directionRadians - (Math.PI / 2))), (int)(1000f * (directionRadians + (Math.PI / 2))));
                Vector2 randomDirection = GeometricHelper.GetVectorDirectionFromAngle((float)randomAngleThousands/1000f);
                float randomVelocity = (float)PlayScreen.random.NextDouble() * 20+10;
                float randomRotation = (float)PlayScreen.random.NextDouble() * 0.1f;
                float randomTime = PlayScreen.random.Next(1000)+1000;
                fragment.Activate(randomPosition, randomDirection, randomVelocity, randomRotation,1000);
            }
            fragmentsRemaining = fragments.Count;
            _isUsable = false;
        }
        public void Update(GameTime gameTime)
        {
            if(!_isUsable)
            {
                fragments.ForEach(f => f.Update(gameTime));
            }
        }
        public void Draw(SpriteBatch spriteBatch,GameTime gameTime)
        {
            foreach(Fragment fragment in fragments)
            {
                fragment.Draw(spriteBatch, gameTime);
            }
        }
    }
}
