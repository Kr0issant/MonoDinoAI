using System;
using Microsoft.Xna.Framework;
using MonoUtils;

namespace MonoDinoAI
{
    public class Obstacle
    {
        public float PosX1 { get; set; }
        public float PosX2 { get; set; }
        public float PosY { get; set; }
        public float Height { get; set; }

        public Obstacle(float initialX, float width, float height)
        {
            PosX1 = initialX;
            PosX2 = initialX + width;
            Height = height;
        }
    }
}
