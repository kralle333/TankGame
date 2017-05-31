using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame.GameObjects
{
    class LargeExplosion:Sprite
    {

        private FragmentCluster _tankFragments;
        private FragmentCluster _tankPartsFragments;
        public LargeExplosion():base("Explosion1",Vector2.Zero,0.3f)
        {
            SpriteState state = new SpriteState("Explode", SpriteHelper.GetSpriteRectangleStrip(64, 64, 0, 0, 28, 0, 0), 15);
            state.isAnimationLooped = false;
            AddAnimationState(state);
            _origin = new Vector2(32, 32);
            Hide();            
        }
        public override void LoadContent(ContentManager contentManager, SpriteBatch spriteBatch)
        {
            base.LoadContent(contentManager, spriteBatch);
            _contentManager = contentManager;
            _tankPartsFragments = new FragmentCluster("Sprites", new Rectangle(96, 176, 32, 32), 16, 16, true, true, 8);
            _tankPartsFragments.LoadContent(_contentManager);
        }

        public void Explode(Tank tank)
        {
            this.position = tank.position;
            _tankFragments = new FragmentCluster("Sprites", new Rectangle(96 + (32 * tank.Team), 112, 32, 32), 8, 8, true,false, 16);
            _tankFragments.LoadContent(_contentManager);
            _tankFragments.Explode(tank.position, Vector2.Zero,(float)(2*Math.PI),4000);
            _tankPartsFragments.Explode(tank.position, Vector2.Zero, (float)(Math.PI * 2),4000);
            Show();
            SetCurrentAnimationState("Explode");
            AudioManager.PlaySFX("TankExplosion1",1);
            ResumeAnimation();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if(IsVisible)
            {
                _tankPartsFragments.Update(gameTime);
                _tankFragments.Update(gameTime);
            }
        }
        public override void Draw(GameTime gameTime)
        {
            if(_isVisible)
            {
                _tankFragments.Draw(_spriteBatch, gameTime);
                _tankPartsFragments.Draw(_spriteBatch, gameTime);
                base.Draw(gameTime);                
            }
        }
    }
}
