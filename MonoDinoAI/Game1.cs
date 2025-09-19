using System;
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
        private Texture2D texture;
        private Screen screen;
        private UtilsKeyboard keyboard = new UtilsKeyboard();
        private UtilsMouse mouse = new UtilsMouse();

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

            base.Initialize();
        }

        protected override void LoadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            keyboard.Update();
            mouse.Update();

            if (keyboard.IsKeyClicked(Keys.Escape)) { this.Exit(); }
            if (keyboard.IsKeyClicked(Keys.F)) { this.screen.ToggleFullScreen(this.graphics); }

            if (mouse.IsScrollingUp()) { camera.MoveZ(10f); }
            else if (mouse.IsScrollingDown()) { camera.MoveZ(-10f); }
            else if (keyboard.IsKeyClicked(Keys.R) && keyboard.IsKeyDown(Keys.LeftControl)) { camera.ResetZ(); }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            screen.Set();
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Viewport vp = GraphicsDevice.Viewport;

            sprites.Begin(camera, false);
            // Draw sprites
            sprites.End();

            shapes.Begin(camera);
            // shapes.DrawRectangle(0, 0, 50, 100, Color.Red, Shapes.FillMode.Filled, 2f);
            shapes.End();

            screen.Unset();
            screen.Present(sprites);

            base.Draw(gameTime);
        }
    }
}
