using System;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoUtils;
using MonoUtils.Graphics;
using MonoUtils.Input;
using MonoUtils.Utility;

namespace MonoDinoAI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private Sprites sprites;
        private Shapes shapes;
        private Camera camera;
        private Screen screen;
        private UtilsKeyboard keyboard = new UtilsKeyboard();
        private UtilsMouse mouse = new UtilsMouse();

        private Texture2D smileyTexture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            sprites = new Sprites(this);
            shapes = new Shapes(this);
            screen = new Screen(this, 1280, 720);
            camera = new Camera(screen);

            World.GroundY = -screen.Height / 4 + 40;
            Player.PosX = 0;
            Player.PosY = World.GroundY;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            smileyTexture = Content.Load<Texture2D>("smiley");
        }

        protected override void Update(GameTime gameTime)
        {
            keyboard.Update();
            mouse.Update();

            if (keyboard.IsKeyClicked(Keys.Escape)) { this.Exit(); }
            if (keyboard.IsKeyClicked(Keys.F)) { this.screen.ToggleFullScreen(this.graphics); }

            if (keyboard.IsKeyClicked(Keys.Space)) { Player.Jump(); }

            Player.UpdatePlayerPhysics();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            screen.Set();
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Viewport vp = GraphicsDevice.Viewport;

            sprites.Begin(camera, false);
            sprites.Draw(smileyTexture, null, new Vector2(8, 8), new Vector2(0, Player.PosY), 0, new Vector2(5f, 5f), Color.White);
            sprites.End();

            shapes.Begin(camera);
            shapes.DrawRectangle(-screen.Width / 2, -screen.Height / 2, screen.Width, screen.Height / 4, Color.SaddleBrown);
            shapes.End();

            screen.Unset();
            screen.Present(sprites);

            base.Draw(gameTime);
        }
    }
}
