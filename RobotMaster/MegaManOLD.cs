using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace RobotMaster
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class MegaManOLD : DrawableGameComponent
    {
        public static float X_VELOCITY = 1.3f;
        public static float JUMP_VELOCITY = 4.0f;
        public static float FALL_ACCELERATION = 0.12f;

        Game Game;
        Texture2D SpriteSheet;
        Sprite sprite;
        SpriteBatch spriteBatch;

        Texture2D bulletTexture;
        Bullet[] bullets = new Bullet[16];
        TimeSpan bulletCooldown = TimeSpan.FromMilliseconds(100);
        TimeSpan bulletElapsed = TimeSpan.Zero;
        int lastBulletIndex;

        public MovementStates MovementState;
        public Vector2 Position;
        public Vector2 Velocity;
        public bool pressingLeft = false;
        public bool pressingRight = false;
        public bool pressingJump = false;
        public bool IsFacingLeft = true;
        
        public MegaMan(Game game) : base(game)
        {
            this.Game = game;
        }

        public override void Initialize()
        {
            base.Initialize();

            MovementState = MovementStates.Standing;
            this.Position = new Vector2(100, 100);

            sprite = new Sprite(SpriteSheet, false, FrameRegions);
            sprite.Position = this.Position;
            sprite.Scale = 1;

            for (int i = 0; i < bullets.Length; i++)
            {
                Rectangle[] frames = { new Rectangle(0, 0, 10, 6), new Rectangle(10, 0, 10, 6) };
                Bullet bullet = new Bullet(bulletTexture, true, frames);
                bullet.Active = false;
                bullet.Scale = 1;
                bullet.ZLayer = .7f;
                bullet.Origin = new Vector2(bullet.SheetFrames[0].Width / 2, 0);
                bullet.AnimationInterval = TimeSpan.FromMilliseconds(66.6666);
                bullets[i] = bullet;
            }

        }

        protected override void LoadContent()
        {
            base.LoadContent();
            SpriteSheet = Game.Content.Load<Texture2D>("Images/MegaManSheet1");
            bulletTexture = Game.Content.Load<Texture2D>("Images/BulletSheet");
            this.spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Right))
            {
                pressingRight = true;
                IsFacingLeft = false;
                Velocity.X = X_VELOCITY;
                switch (MovementState)
                {
                    case MovementStates.Standing:
                        Position.X += 1;
                        MovementState = MovementStates.Walking;
                        break;
                }
                
            }
            if (ks.IsKeyUp(Keys.Right))
            {
                if (pressingRight)
                {
                    pressingRight = false;
                    Velocity.X = 0.0f;
                    if (MovementState == MovementStates.Walking)
                    {
                        MovementState = MovementStates.Standing;
                    }
                }
            }
            if (ks.IsKeyDown(Keys.Left))
            {
                pressingLeft = true;
                IsFacingLeft = true;
                Velocity.X = -X_VELOCITY;
                switch (MovementState)
                {
                    case MovementStates.Standing:
                        Position.X -= 1;
                        MovementState = MovementStates.Walking;
                        break;
                }
            }
            if (ks.IsKeyUp(Keys.Left))
            {
                if (pressingLeft)
                {
                    pressingLeft = false;
                    Velocity.X = 0.0f;
                    if (MovementState == MovementStates.Walking)
                    {
                        MovementState = MovementStates.Standing;
                    }
                }
            }
            if (ks.IsKeyDown(Keys.A))
            {
                pressingJump = true;
                switch (MovementState)
                {
                    case MovementStates.Standing:
                    case MovementStates.Walking:
                        Velocity.Y = -JUMP_VELOCITY;
                        MovementState = MovementStates.Jumping;
                        break;
                }
            }
            if (ks.IsKeyUp(Keys.A))
            {
                if (MovementState == MovementStates.Jumping)
                {
                    MovementState = MovementStates.Falling;
                    Velocity.Y = 0.0f;
                }
            }

            if (((bulletElapsed += gameTime.ElapsedGameTime) > bulletCooldown) && ks.IsKeyDown(Keys.S))
            {
                bulletElapsed = TimeSpan.Zero;
                CreateBullet();
            }

            foreach (var bullet in bullets)
            {
                if (bullet.Active)
                {
                    bullet.Update(gameTime);
                    if (bullet.Position.X < -10 || bullet.Position.X > 266)
                    {
                        // out of bounds
                        bullet.Active = false;
                    }
                }
            }

            Position += Velocity;           // Update Mega Man object's position
            sprite.Position = Position;     // Update Sprite's display position

            if (MovementState != MovementStates.Standing && 
                MovementState != MovementStates.Walking)
            {
                Velocity.Y += FALL_ACCELERATION;

                if (Velocity.Y > 0.0f && 
                    MovementState == MovementStates.Jumping)
                {
                    MovementState = MovementStates.Falling;
                }
            }

            base.Update(gameTime);
        }

        private void CreateBullet()
        {
            for (int i = 0; i < bullets.Length; i++)
            {
                if (!bullets[i].Active)
                {
                    bullets[i].Position = Position;
                    bullets[i].Active = true;
                    if (IsFacingLeft)
                    {
                        bullets[i].Velocity = new Vector2(-1, 0);
                    }
                    else
                    {
                        bullets[i].Velocity = new Vector2(1, 0);
                    }

                    return;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // Change Mega Man's current frame based on current MovementState
            switch (MovementState)
            {
                case MovementStates.Standing:
                    if (IsFacingLeft) sprite.CurrentFrame = (int)FrameIndex.StandLeft;
                    else sprite.CurrentFrame = (int)FrameIndex.StandRight;
                    break;
                case MovementStates.Walking:
                    if (IsFacingLeft)
                    {
                        if (DateTime.Now.Millisecond < 333)
                            sprite.CurrentFrame = (int)FrameIndex.WalkLeft0;
                        else if (DateTime.Now.Millisecond < 666)
                            sprite.CurrentFrame = (int)FrameIndex.WalkLeft1;
                        else
                            sprite.CurrentFrame = (int)FrameIndex.WalkLeft2;
                    }
                    else
                    {
                        if (DateTime.Now.Millisecond < 333)
                            sprite.CurrentFrame = (int)FrameIndex.WalkRight0;
                        else if (DateTime.Now.Millisecond < 666)
                            sprite.CurrentFrame = (int)FrameIndex.WalkRight1;
                        else
                            sprite.CurrentFrame = (int)FrameIndex.WalkRight2;
                    }
                    break;
            
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            sprite.Draw(gameTime, spriteBatch);
            foreach (var bullet in bullets)
            {
                if (bullet.Active) bullet.Draw(gameTime, spriteBatch);
            }
            spriteBatch.End();
        }

        public static Rectangle[] FrameRegions = {
                                                     new Rectangle(77, 11, 21, 24),  //rect_mmStandLeft,
                                                     new Rectangle(142, 37, 20, 24), //rect_mmNudgeLeft,
                                                     new Rectangle(115, 39, 24, 22), //rect_mmWalkLeft0,
                                                     new Rectangle(97, 37, 16, 24),  //rect_mmWalkLeft1,
                                                     new Rectangle(74, 39, 21, 22),  //rect_mmWalkLeft2,
                                                     new Rectangle(236, 11, 21, 24), //rect_mmStandRight,
                                                     new Rectangle(172, 37, 20, 24), //rect_mmNudgeRight,
                                                     new Rectangle(195, 39, 24, 22), //rect_mmWalkRight0,
                                                     new Rectangle(221, 37, 16, 24), //rect_mmWalkRight1,
                                                     new Rectangle(239, 39, 21, 22), //rect_mmWalkRight2,                                                     
                                                 };

        public enum FrameIndex
        {
            StandLeft,
            NudgeLeft,
            WalkLeft0,
            WalkLeft1,
            WalkLeft2,
            StandRight,
            NudgeRight,
            WalkRight0,
            WalkRight1,
            WalkRight2
        }

        public enum MovementStates
        {
            Standing,
            Walking,
            Jumping,
            Falling
        }
    }
}
