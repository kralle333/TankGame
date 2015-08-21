using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShooterGuys
{
	class PooledObjects
	{
		public static List<Bullet> bullets = new List<Bullet>();
		private const int cBulletCount = 5000;
		private PooledObjects()
		{
			
		}
		public static void Initialize()
		{
			for (int i = 0; i < cBulletCount; i++)
			{
				bullets.Add(new Bullet());
			}
		}
	}
}
