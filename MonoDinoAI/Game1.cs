using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoUtils;
using MonoUtils.Graphics;
using MonoUtils.Input;
using MonoUtils.Utility;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonoDinoAI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private Sprites sprites;
        private SpriteFont font;
        private Shapes shapes;
        private Camera camera;
        private Screen screen;
        private UtilsKeyboard keyboard = new UtilsKeyboard();
        private UtilsMouse mouse = new UtilsMouse();

        private float screenWidthByTwo;
        private float screenHeightByTwo;
        private float screenHeightByFour;

        private AgentClient agentClient;

        private bool waitingForAction = false;
        private int frameCount = 0;
        private const int decisionSkipFrames = 4;

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

            screenWidthByTwo = screen.Width / 2;
            screenHeightByTwo = screen.Height / 2;
            screenHeightByFour = screen.Height / 4;

            World.GroundY = -screenHeightByFour + 40;
            World.SpawnX = screenWidthByTwo + 20f;
            Player.PosX = 0;
            Player.PosY = World.GroundY;

            agentClient = new AgentClient();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("DefaultFont");
            sprites.Font = font;
            smileyTexture = Content.Load<Texture2D>("smiley");
        }

        protected override void Update(GameTime gameTime)
        {
            keyboard.Update();
            mouse.Update();

            if (keyboard.IsKeyClicked(Keys.Escape)) { this.Exit(); }
            if (keyboard.IsKeyClicked(Keys.F)) { this.screen.ToggleFullScreen(this.graphics); }

            if (keyboard.IsKeyClicked(Keys.Space)) { Player.Jump(); }

            int actionToExecute = Interlocked.Exchange(ref agentClient.PendingAction, 0);
            if (actionToExecute == 1) { Player.Jump(); }

            Player.Update();
            World.Update();

            if (!agentClient.IsAgentThinking && ++frameCount == decisionSkipFrames)
            {
                agentClient.StartAgentDecisionTask();
                frameCount = 0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            screen.Set();
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Viewport vp = GraphicsDevice.Viewport;

            sprites.Begin(camera, false);
            sprites.Draw(smileyTexture, null, new Vector2(8, 8), new Vector2(0, Player.PosY), 0, new Vector2(5f, 5f), Color.White);
            sprites.DrawString($"Score: {World.Score}", new Vector2(-screenWidthByTwo + 50, screenHeightByTwo - 50), Color.White);
            sprites.DrawString($"High Score: {World.HighScore}", new Vector2(-screenWidthByTwo + 50, screenHeightByTwo - 75), Color.White);
            if (World.IsGameOver) { sprites.DrawString("GAME OVER!", new Vector2(screenWidthByTwo - 150, screenHeightByTwo - 50), Color.Red); }
            sprites.End();

            shapes.Begin(camera);
            shapes.DrawRectangle(-screenWidthByTwo, -screenHeightByTwo, screen.Width, screenHeightByFour, Color.SaddleBrown);
            //Console.WriteLine(World.NextObstacleXNormalized);
            shapes.DrawLine(new Vector2(0, World.GroundY - 50f), new Vector2(World.NextObstacleX, World.GroundY - 50f), 5f, Color.Blue);
            foreach (Obstacle obstacle in World.obstacles)
            {
                shapes.DrawRectangle(obstacle.PosX1, World.GroundY - 40, obstacle.PosX2 - obstacle.PosX1, obstacle.Height, Color.Red);
            }
            shapes.End();

            screen.Unset();
            screen.Present(sprites);

            base.Draw(gameTime);
        }
    }
}
