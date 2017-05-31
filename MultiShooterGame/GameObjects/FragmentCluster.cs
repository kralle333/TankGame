using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MultiShooterGame;
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

            public Fragment(FragmentCluster parent, string path)
                : base(path, -1, -1, Rectangle.Empty, 0.45f)
            {
                _parent = parent;
            }
            public void Activate(Vector2 position, Vector2 direction, float velocity, float rotationSpeed, float visibleTime)
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
                    color.A = (Byte)(255 - (255 * (_visibleTimer / _visibleTime)));
                    if (_visibleTimer >= _visibleTime)
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

        public FragmentCluster(string path, Rectangle spriteSheetRectangle, int minFragSize, int maxFragSize,bool fixedFragPos,bool useRandomFrags, int numberOfFragments = 10)
        {
            fragmentsRemaining = numberOfFragments;
            for (int i = 0; i < numberOfFragments; i++)
            {
                Fragment fragment = new Fragment(this, path);
                int x = spriteSheetRectangle.X;
                int y = spriteSheetRectangle.Y;
                if (useRandomFrags)
                {
                    if (!fixedFragPos)
                    {
                        x += PlayScreen.random.Next(spriteSheetRectangle.Width - maxFragSize);
                        y += PlayScreen.random.Next(spriteSheetRectangle.Height - maxFragSize);
                    }
                    else
                    {
                        int xMax = spriteSheetRectangle.Width / maxFragSize;
                        int yMax = spriteSheetRectangle.Height / maxFragSize;
                        x += PlayScreen.random.Next(xMax + 1) * maxFragSize;
                        y += PlayScreen.random.Next(yMax + 1) * maxFragSize;
                    }
                }
                else
                {
                    int w = (spriteSheetRectangle.Width / maxFragSize);
                    int xx = i %w;
                    int yy = i/w;
                    x += xx * maxFragSize;
                    y += yy * maxFragSize;
                }
               
                int width =  PlayScreen.random.Next(minFragSize, maxFragSize);
                int height = PlayScreen.random.Next(minFragSize, maxFragSize);
                fragment.SetTextureRectangle(new Rectangle(x, y,width,height));
                fragment.Hide();
                fragments.Add(fragment);
            }
            _isUsable = true;
        }
        public void LoadContent(ContentManager contentManager)
        {
            foreach (Fragment fragment in fragments)
            {
                fragment.LoadContent(contentManager);
            }

        }
        public void RemoveFragment()
        {
            fragmentsRemaining--;
            if (fragmentsRemaining <= 0)
            {
                _isUsable = true;
            }
        }
        public void Explode(Vector2 position, Vector2 direction,float spreadInRadians,float maxLifeTime)
        {
            foreach (Fragment fragment in fragments)
            {
                Vector2 randomPosition = direction * 16 + position;
                float directionRadians = GeometricHelper.GetAngleFromVectorDirection(direction);
                int randomAngleThousands = PlayScreen.random.Next((int)(1000f * (directionRadians - spreadInRadians)), (int)(1000f * (directionRadians + spreadInRadians)));
                Vector2 randomDirection = GeometricHelper.GetVectorDirectionFromAngle((float)randomAngleThousands / 1000f);
                float randomVelocity = (float)PlayScreen.random.NextDouble() * 20 + 10;
                float randomRotation = (float)PlayScreen.random.NextDouble() * 0.1f;
                float randomTime = PlayScreen.random.Next((int)(maxLifeTime / 2)) + maxLifeTime/2;
                fragment.Activate(randomPosition, randomDirection, randomVelocity, randomRotation, 1000);
            }
            fragmentsRemaining = fragments.Count;
            _isUsable = false;
        }
        public void Update(GameTime gameTime)
        {
            if (!_isUsable)
            {
                fragments.ForEach(f => f.Update(gameTime));
            }
        }
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!_isUsable)
            {
                foreach (Fragment fragment in fragments)
                {
                     fragment.Draw(spriteBatch, gameTime);
                }
            }
        }
    }
}
