using System;
using Microsoft.Xna.Framework;
using MonoUtils;

namespace MonoDinoAI
{
    internal static class Player
    {
        public static float PosX { get; set; }
        public static float PosY { get; set; }

        private const float gravity = -1.5f;
        private const float jumpForce = 25f;
        private static float verticalVelocity = 0f;

        public static void Update()
        {
            //Console.WriteLine($"{World.GroundY} | {PosY}");
            verticalVelocity += gravity;

            PosY += verticalVelocity;

            if (PosY <= World.GroundY)
            {
                PosY = World.GroundY;
                verticalVelocity = 0f;
            }
        }
        public static void Jump()
        {
            if (PosY <= World.GroundY)
            {
                verticalVelocity = jumpForce;
            }
        }
    }
}
