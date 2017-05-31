using Microsoft.Xna.Framework;
using MultiShooterGame.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame
{
	class PooledObjects
	{
		public static List<Bullet> bullets = new List<Bullet>();
		private const int cBulletCount = 1000;

        public static List<FragmentCluster> tileFragmentClusters = new List<FragmentCluster>();
        private const int cFragmentClusterCount = 100;

        public static List<LargeExplosion> explosions = new List<LargeExplosion>();
        private const int cExplosionsCount = 10;

		private PooledObjects()
		{
			
		}

		public static void Initialize()
		{
			for (int i = 0; i < cBulletCount; i++)
			{
				bullets.Add(new Bullet());
			}
            Rectangle tileRectangle = new Rectangle(0,0,16,16);
            for(int i = 0;i<cFragmentClusterCount;i++)
            {
                tileFragmentClusters.Add(new FragmentCluster("Sprites", tileRectangle,8,16,false,true,25));
            }
            for(int i = 0;i<cExplosionsCount;i++)
            {
                explosions.Add(new LargeExplosion());
            }
		}
	}
}
