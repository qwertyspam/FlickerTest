using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FlickerTest
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Matrix matrix;
        private List<Texture2D> textures;
        private List<Sprite> players;
        private List<Sprite> sprites;
        private bool isFloor = false;
        private const int EDGE_LEN_1 = 16;
        private const int EDGE_LEN_2 = 10;
        private int edgeLength = EDGE_LEN_1;
        private int gap = 2;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
        }

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
        }
        protected override void Initialize()
        {
            base.Initialize();
            this.Window.AllowUserResizing = true;
            this.IsMouseVisible = true;
            this.initialize();
        }
        private void initialize()
        {
            // textures

            this.textures = new List<Texture2D>() {
                createTexture(this.GraphicsDevice, this.edgeLength , Color.White),
                createTexture(this.GraphicsDevice, this.edgeLength, Color.LightSkyBlue),
                createTexture(this.GraphicsDevice, this.edgeLength, Color.Magenta)};

            // players

            this.players = new List<Sprite>();
            var f = 0f;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    this.players.Add(new Sprite(new Vector2(100) + new Vector2(x * (this.edgeLength + this.gap) + f, y * (this.edgeLength + this.gap) + f), x == 0 && y == 0 ? this.textures[0] : this.textures[1]));
                    f += 0.1f;
                }
            }

            // sprites

            this.sprites = new List<Sprite>();
            f = 0f;
            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 30; x++)
                {
                    f += 0.05f;
                    if (f >= 1f)
                        f = 0;

                    this.sprites.Add(new Sprite(new Vector2(x * (this.edgeLength + this.gap) + f, y * (this.edgeLength + this.gap) + f), this.textures[2]));
                }
            }
        }
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            // screen location floor

            if (keyboardState.IsKeyDown(Keys.F1))
                this.isFloor = false;
            else if (keyboardState.IsKeyDown(Keys.F2))
                this.isFloor = true;

            // sprite edge length

            if (keyboardState.IsKeyDown(Keys.F3))
            {
                this.edgeLength = EDGE_LEN_1;
                this.initialize();
            }
            else if (keyboardState.IsKeyDown(Keys.F4))
            {
                this.edgeLength = EDGE_LEN_2;
                this.initialize();
            }

            // player velocity

            var velocity = calcVelocity(keyboardState);
            if (velocity != Vector2.Zero)
            {
                foreach (Sprite player in this.players)
                    player.Loc += velocity;
            }

            // players screen location

            foreach (Sprite player in this.players)
            {
                player.ScreenLoc = player.Loc;
                if (this.isFloor)
                    player.ScreenLoc = floor(player.ScreenLoc);
            }

            // sprites screen location

            foreach (Sprite sprite in this.sprites)
            {
                sprite.ScreenLoc = sprite.Loc;
                if (this.isFloor)
                    sprite.ScreenLoc = floor(sprite.ScreenLoc);
            }

            // camera matrix

            this.matrix = Matrix.CreateTranslation(
                new Vector3(-this.players[0].ScreenLoc.X, -this.players[0].ScreenLoc.Y, 0))
                * Matrix.CreateTranslation(new Vector3(this.GraphicsDevice.Viewport.Width * 0.5f, this.GraphicsDevice.Viewport.Height * 0.5f, 0));
        }
        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.Black);

            this.spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, this.matrix);

            foreach (var sprite in this.sprites)
                sprite.Render(this.spriteBatch);

            foreach (var player in this.players)
                player.Render(this.spriteBatch);

            this.spriteBatch.End();
        }

        private static Texture2D createTexture(GraphicsDevice device, int edgeLength, Color color)
        {
            var texture = new Texture2D(device, edgeLength, edgeLength);
            var colors = new Color[edgeLength * edgeLength];
            for (int i = 0; i < edgeLength; i++)
            {
                colors[i] = color;
                colors[edgeLength * (edgeLength - 1) + i] = color;
            }
            for (int i = 1; i < edgeLength - 1; i++)
            {
                colors[edgeLength * i] = color;
                colors[edgeLength * (i + 1) - 1] = color;
            }
            texture.SetData(colors);
            return texture;
        }
        private static Vector2 floor(Vector2 vector)
        {
            return new Vector2(
                (float)Math.Floor((decimal)vector.X),
                (float)Math.Floor((decimal)vector.Y));
        }
        private static Vector2 calcVelocity(KeyboardState keyboardState)
        {
            var newDirection = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Left))
                newDirection.X = -1;
            else if (keyboardState.IsKeyDown(Keys.F) || keyboardState.IsKeyDown(Keys.Right))
                newDirection.X = 1;
            if (keyboardState.IsKeyDown(Keys.E) || keyboardState.IsKeyDown(Keys.Up))
                newDirection.Y = -1;
            else if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Down))
                newDirection.Y = 1;

            if (newDirection != Vector2.Zero)
            {
                var velocity = newDirection * 0.5f;
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                    return newDirection * 0.5f;
                else
                    return newDirection * 0.05f;
            }
            return Vector2.Zero;
        }

        private class Sprite
        {
            public Vector2 Loc;
            public Vector2 ScreenLoc;
            public Texture2D Texture;

            public Sprite(Vector2 loc, Texture2D texture)
            {
                this.Loc = loc;
                this.ScreenLoc = loc;
                this.Texture = texture;
            }

            public void Render(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(
                    texture: this.Texture,
                    position: this.ScreenLoc,
                    sourceRectangle: null,
                    color: Color.White,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    scale: new Vector2(1),
                    effects: SpriteEffects.None,
                    layerDepth: 0f);
            }
        }
    }
}