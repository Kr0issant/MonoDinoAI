using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoDinoAI
{
    public static class World
    {
        public static float GroundY;
        public static List<Obstacle> obstacles = new List<Obstacle>();
        public static int Score { get { return (int)score; } }
        public static int HighScore { get; set; } = 0;
        public static bool IsGameOver { get; set; } = false;
        public static bool IsGameOverActual { get; set; } = false;
        public static float SpawnX { get; set; }

        public static float NextObstacleX { get; set; }
        public static float NextObstacleXNormalized { get { return lastObjectPosition; } }

        private static float score = 0f;
        private static float lastScore = 0f;
        private static bool isJumpNecessary = false;
        private static float[] state;

        private static bool crossedOverObstacle = false;
        private static float lastObjectPosition = 0f;

        private static float spawnWidthMin = 100f;
        private static float spawnWidthMax = 161f;
        private static float spawnHeightMin = 20f;
        private static float spawnHeightMax = 61f;

        private static float gameSpeed = 10f;
        private static float maxGameSpeed = 20f;

        private static Random rng = new Random();

        private static int framesSinceLastSpawn = 0;
        private static int framesSinceLastDeath = 0;

        public static void Update()
        {
            if (IsGameOver)
            {
                framesSinceLastDeath++;
                if (framesSinceLastDeath > 50)
                {
                    IsGameOver = false;
                    framesSinceLastDeath = 0;
                }
            }

            score += 0.01f;
            if (obstacles.Count > 10)
            {
                obstacles.RemoveAt(0);
            }
            if (framesSinceLastSpawn > rng.Next(40, 250))
            {
                SpawnObstacle();
                framesSinceLastSpawn = 0;
            }
            foreach (Obstacle obstacle in obstacles)
            {
                if (obstacle.PosX1 < Player.PosX && obstacle.PosX2 > Player.PosX)
                {
                    if (Player.PosY <= GroundY + obstacle.Height)
                    {
                        if (Score > HighScore) { HighScore = Score; }
                        score = 0f;
                        lastScore = 0f;
                        obstacles.Clear();
                        IsGameOver = true;
                        IsGameOverActual = true;
                        break;
                    }
                } 

                obstacle.PosX1 -= gameSpeed;
                obstacle.PosX2 -= gameSpeed;
            }
            framesSinceLastSpawn++;

            state = new float[6] { 1f, 1f, 1f, (Player.VelocityY + 24f) / 48f, (Player.PosY + 140f) / 196f, gameSpeed / maxGameSpeed };

            int i = 0;
            for (i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i].PosX2 > 0)
                {
                    state[0] = obstacles[i].PosX1 / (SpawnX + spawnWidthMin);
                    state[1] = obstacles[i].Width / spawnWidthMax;
                    state[2] = obstacles[i].Height / spawnHeightMax;

                    isJumpNecessary = state[0] < 0.2f ? true : false;

                    if (state[0] > lastObjectPosition) { crossedOverObstacle = true; }
                    lastObjectPosition = state[0];

                    NextObstacleX = obstacles[i].PosX1;

                    break;
                }
            }
        }
        public static void SpawnObstacle()
        {
            obstacles.Add(new Obstacle(
                rng.Next((int)SpawnX, (int)(SpawnX + spawnWidthMin)),
                rng.Next((int)spawnWidthMin, (int)spawnWidthMax),
                rng.Next((int)spawnHeightMin, (int)spawnHeightMax)
            ));
        }

        public static float[] GetCurrentState()
        {
            return state;
        }
        public static float CalculateReward(bool isGameOver, int lastAction)
        {
            if (isGameOver) { IsGameOverActual = false; return -30f; }

            float reward = score - lastScore;

            if (reward < 0.01f && reward > -0.01f) { reward = 0.01f; }
            if (crossedOverObstacle) { reward += 10f; crossedOverObstacle = false; }
            else if (lastAction == 1 && !isJumpNecessary) { Console.Clear(); Console.WriteLine("Unnecessary Jump" + rng.Next(1, 101).ToString()); reward -= 0.5f; isJumpNecessary = true; }

            //Console.WriteLine($"Reward: {reward}");
            return reward;
        }
        public static void UpdateLastScore()
        {
            lastScore = score;
        }
    }
}
