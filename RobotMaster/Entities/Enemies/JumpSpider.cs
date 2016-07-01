using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RobotMaster.Mathematics;
using RobotMaster.GameComponents;
using RobotMaster.TileEngine;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RobotMaster.Entities
{
    public enum JumpSpiderType { Big, Small }
    public enum JumpSpiderState { Ceiling, Falling, Ground }

    class JumpSpider : Enemy
    {
        private static Dictionary<String, Vector2> spriteOffset;

        protected readonly static float velCeiling = 1.6f;
        protected readonly static int JumpWaitTime = 500;
        protected readonly static float TravelLimit = 75;
        public readonly static float JumpVelocity = -3.0f;
        public readonly static float AggroRange = 10;
        public readonly static float MaxFallVelocity = 7.0f;
        public readonly static Vector2 SIZE = new Vector2(22, 14);


        public JumpSpiderType SpiderType;
        public JumpSpiderState SpiderStateInitial;
        public JumpSpiderState SpiderState;
        public Stopwatch JumpWaitStopwatch = new Stopwatch();
        public float TravelDistance = 0;
        public bool IsFacingLeft = true;
        public bool IsOnGround = false;
        public Vector2 Velocity;
        public override Vector2 Size
        {
            get { return SIZE; }
        }

        public JumpSpider(Vector2 position, JumpSpiderType type, bool onCeiling)
            : base(position)
        {
            HP = new AttributePair(14);
            Velocity = new Vector2(0.0f, 0.0f);
            
            SpiderType = type;
            SpiderStateInitial = SpiderState = (onCeiling) ? JumpSpiderState.Ceiling : JumpSpiderState.Ground;
            IsOnGround = !onCeiling;
            JumpWaitStopwatch.Start();
        }

        public override void LoadAnimations()
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();

            Vector2 explodeOffset = new Vector2(4, 4);

            // Construct animation from sprite sheet
            animations.Add("Ceiling", new Animation(CeilingRects, CeilingOffsets));
            animations.Add("Falling", new Animation(RectFalling, OffsetFalling));
            animations.Add("Jumping", new Animation(JumpingRects, JumpingOffsets));
            Animation deathAnimation = new Animation(ExplodeContentRects, new Vector2[] { explodeOffset, explodeOffset, explodeOffset }, "Death");
            deathAnimation.FramesPerSecond = 6;
            animations.Add("Death", deathAnimation);
            animatedSprite = new AnimatedSprite(mm10EnemySprites, animations);

            animatedSprite.CurrentAnimation = (SpiderState == JumpSpiderState.Ceiling) ? "Ceiling" : "Jumping";
            animatedSprite.IsAnimating = true;
            animatedSprite.AnimationEnd += OnFinishedDeath;
        }

        public void RunAI(GameTime gameTime)
        {
            if (!IsDying)
            {
                // Update position
                Position += Velocity;

                switch (SpiderState)
                {
                    case JumpSpiderState.Ceiling:

                        // Change phases to "Falling" if Mega Man is in range
                        if (Math.Abs(Session.MegaMan.HitBox.CenterX - Bounds.CenterX) < AggroRange)
                        {
                            SpiderState = JumpSpiderState.Falling;
                            Velocity = Vector2.Zero;
                            break;
                        }

                        // Spider will sit still for a moment when turning around
                        if (JumpWaitStopwatch.ElapsedMilliseconds < JumpWaitTime)
                        {
                            Velocity.X = 0f;
                            break;
                        }
                        
                        int TileX = (IsFacingLeft)
                                  ? (int)Math.Floor(Bounds.Left / (float)Engine.TileWidth)
                                  : (int)Math.Ceiling(Bounds.Right / (float)Engine.TileWidth) - 1;
                        int TileY = (int)Math.Ceiling(Bounds.CenterY / (float)Engine.TileHeight) - 1;

                        // Check if hit a wall
                        Tile tile = Session.CurrentMap.GetTileAtIndex(new Point(TileX, TileY), 0);
                        if (tile.IsCollidable)
                        {
                            TravelDistance = 0;
                            IsFacingLeft = !IsFacingLeft;
                            JumpWaitStopwatch.Restart();
                        }
                        // Check if hit a ledge
                        else
                        {
                            Tile upperTile = Session.CurrentMap.GetTileAtIndex(new Point(TileX, TileY - 1), 0);
                            if (!upperTile.IsCollidable)
                            {
                                TravelDistance = 0;
                                IsFacingLeft = !IsFacingLeft;
                                JumpWaitStopwatch.Restart();
                            }
                            // Nothing has been hit
                            else
                            {
                                // Check to see if traveled far enough to turn around
                                TravelDistance += Velocity.X;
                                if (Math.Abs(TravelDistance) > TravelLimit)
                                {
                                    // Turn around
                                    TravelDistance = -(Math.Abs(TravelDistance) - TravelLimit);
                                    IsFacingLeft = !IsFacingLeft;
                                    JumpWaitStopwatch.Restart();
                                }
                            }
                        }

                        // Set velocity
                        float _xSpeed = velCeiling;
                        _xSpeed = IsFacingLeft ? _xSpeed * -1 : _xSpeed;
                        Velocity.X = _xSpeed;
                        break;

                    case JumpSpiderState.Falling:

                        // Set animation
                        animatedSprite.CurrentAnimation = "Falling";

                        // Update velocity
                        Velocity.Y += Gravity;
                        if (Velocity.Y > MaxFallVelocity) Velocity.Y = MaxFallVelocity;
                        
                        // Check collisions to change state
                        CollisionResult collision = Session.CollisionCheckTilemap(Position, PosPreviousFrame, Bounds);
                        if (collision.HasLanded)
                        {
                            Position = collision.NewPosition;
                            Velocity = Vector2.Zero;
                            SpiderState = JumpSpiderState.Ground;
                            animatedSprite.IsAnimating = false;
                            JumpWaitStopwatch.Restart();
                            IsOnGround = true;
                        }
                        break;
                    case JumpSpiderState.Ground:
                        animatedSprite.CurrentAnimation = "Jumping";


                        // Update velocity
                        Velocity.Y += Gravity;
                        if (Velocity.Y > MaxFallVelocity) Velocity.Y = MaxFallVelocity;
                        Position += Velocity;

                        // Check collisions
                        CollisionResult col = Session.CollisionCheckTilemap(Position, PosPreviousFrame, Bounds);
                        if (col.HasCollided)
                        {

                            //// Just landed - restart wait timer
                            //if (col.HasLanded && !IsOnGround)
                            //{
                            //    JumpWaitStopwatch.Restart();
                            //    Velocity = Vector2.Zero;
                            //}

                            // Resolve collision
                            IsOnGround = col.HasLanded;
                            Position = col.NewPosition;

                            // Time to jump?
                            if (JumpWaitStopwatch.ElapsedMilliseconds > JumpWaitTime)
                            {
                                animatedSprite.IsAnimating = true;
                                animatedSprite.AnimationEnd
                                    += new EventHandler<AnimationEndEventArgs>((o, args) =>
                                    {
                                        animatedSprite.IsAnimating = false;
                                    });

                                Velocity.X = 1.0f;
                                Velocity.X = (Session.MegaMan.HitBox.CenterX < Bounds.CenterX) ? Velocity.X * -1 : Velocity.X;
                                Velocity.Y = JumpVelocity;
                            }
                            else
                            {
                                // TODO: Reliable way to check if you've hit the ceiling
                                Velocity = Vector2.Zero;
                            }


                        }
                        else
                        {
                            JumpWaitStopwatch.Restart();
                        }

                        break;
                }



            } // end if NOT IsDying
        }

        public void HandleTileCollisions()
        {
            switch (SpiderState)
            {
                case JumpSpiderState.Ceiling:

                    int TileX = (IsFacingLeft)
                              ? (int)Math.Floor(Bounds.Left / (float)Engine.TileWidth)
                              : (int)Math.Ceiling(Bounds.Right / (float)Engine.TileWidth) - 1;
                    int TileY = (int)Math.Ceiling(Bounds.CenterY / (float)Engine.TileHeight) - 1;

                    // Check if hit a wall
                    Tile tile = Session.CurrentMap.GetTileAtIndex(new Point(TileX, TileY), 0);
                    if (tile.Collision == TileCollision.Impassable)
                    {
                        IsFacingLeft = !IsFacingLeft;
                    }
                    // Check if hit a ledge
                    else
                    {
                        Tile bottomLeft = Session.CurrentMap.GetTileAtIndex(new Point(TileX, TileY + 1), 0);
                        if (!bottomLeft.IsCollidable)
                        {
                            IsFacingLeft = !IsFacingLeft;
                        }
                    }
                    break;

                default: break;
            }
        }


        public override void Deactivate()
        {
            HP.ReplenishAll();
            base.Deactivate();
        }

        public override void Activate()
        {
            IsFacingLeft = true;
            SpiderState = SpiderStateInitial;
            TravelDistance = 0;
            Velocity = Vector2.Zero;

            switch (SpiderStateInitial)
            {
                case JumpSpiderState.Ceiling:
                    IsOnGround = false;
                    animatedSprite.IsAnimating = true;
                    animatedSprite.CurrentAnimation = "Ceiling";
                    break;
                case JumpSpiderState.Falling:
                    IsOnGround = false;
                    animatedSprite.CurrentAnimation = "Falling";
                    break;
                case JumpSpiderState.Ground:
                    IsOnGround = true;
                    animatedSprite.CurrentAnimation = "Jumping";
                    animatedSprite.IsAnimating = false;
                    animatedSprite.AnimationEnd
                        += new EventHandler<AnimationEndEventArgs>((o, args) => 
                        {
                            animatedSprite.IsAnimating = false;
                        });
                    break;
                default: break;
            }
            animatedSprite.Position = this.Position;

            base.Activate();
        }

        public override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                RunAI(gameTime);
                animatedSprite.Position = this.Position;
                animatedSprite.Update(gameTime);
            }
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                animatedSprite.Draw(spriteBatch);
            }

            base.Draw(gameTime, spriteBatch);
        }

        public virtual void OnFinishedDeath(object sender, AnimationEndEventArgs e)
        {
            if (e.AnimationString == "Death")
            {
                Deactivate();
                IsDying = false;
            }
        }

        private static Rectangle RectCeiling0 = new Rectangle(847, 16, 22, 18);
        private static Rectangle RectCeiling1 = new Rectangle(875, 16, 22, 18);
        private static Vector2 OffsetCeiling = new Vector2(0, 0);
        private static Rectangle[] CeilingRects = { RectCeiling0, RectCeiling1 };
        private static Vector2[] CeilingOffsets = { OffsetCeiling, OffsetCeiling };

        private static Rectangle RectFalling = new Rectangle(847, 39, 20, 18);
        private static Vector2 OffsetFalling = new Vector2(0, 4);
        private static Rectangle[] FallingRects = { RectFalling };
        private static Vector2[] FallingOffsets = { OffsetFalling };

        private static Rectangle RectJumping0 = new Rectangle(875, 63, 22, 18);
        private static Rectangle RectJumping1 = new Rectangle(847, 63, 22, 18);
        private static Vector2 OffsetJumping = new Vector2(0, 0);
        private static Rectangle RectLanding = new Rectangle(875, 86, 22, 20);
        private static Vector2 OffsetLanding = new Vector2(0, -2);
        private static Rectangle[] JumpingRects = { RectJumping0, RectJumping1, RectLanding };
        private static Vector2[] JumpingOffsets = { OffsetJumping, OffsetJumping, OffsetLanding };


    }
}
