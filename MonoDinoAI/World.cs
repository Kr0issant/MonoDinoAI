using Microsoft.Xna.Framework;
using MonoUtils;
using MonoUtils.Graphics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace MonoDinoAI
{
    public static class World
    {
        public static float GroundY;
        public static List<Obstacle> obstacles = new List<Obstacle>();
        public static int Score { get { return (int)score; } }

        public static float SpawnX { get; set; }

        private static float score = 0;
        private static float spawnWidth = 100f;
        private static float spawnHeight = 20f;

        private static Random rng = new Random();

        private static int framesSinceLastSpawn = 0;

        public static void Update()
        {
            score += 0.01f;
            if (obstacles.Count > 10)
            {
                obstacles.RemoveAt(0);
            }
            if (framesSinceLastSpawn > rng.Next(50, 200))
            {
                SpawnObstacle();
                framesSinceLastSpawn = 0;
            }
            foreach (Obstacle obstacle in obstacles)
            {
                obstacle.PosX1 -= 10f;
                obstacle.PosX2 -= 10f;
            }
            framesSinceLastSpawn++;
        }
        public static void SpawnObstacle()
        {
            obstacles.Add(new Obstacle(
                rng.Next((int)SpawnX, (int)(SpawnX + spawnWidth)),
                rng.Next((int)spawnWidth, (int)(spawnWidth + 61f)),
                rng.Next((int)spawnHeight, (int)(spawnHeight + 41f))
            ));
        }
    }
}
