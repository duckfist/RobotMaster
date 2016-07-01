using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RobotMaster.Mathematics;
using RobotMaster.GameComponents;
using RobotMaster.TileEngine;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RobotMaster.Entities
{
    class SmallJumper : Enemy
    {
        private static Dictionary<String, Vector2> spriteOffset;

        public bool IsFacingLeft = true;
        public bool IsJumping = true;
        public bool isOnGround = false;
        protected DateTime timeLanded = DateTime.Now;
        protected Random random;
        protected readonly static TimeSpan jumpWaitTime = TimeSpan.FromMilliseconds(350);
        protected readonly static float XSpeed = 0.9f;
        protected readonly static float JumpSpeed0 = -3.2f;
        protected readonly static float JumpSpeed1 = -5.0f;
        public Vector2 Velocity;
        

        public readonly static Vector2 SIZE = new Vector2(17, 26);
        public override Vector2 Size
        {
            get { return SIZE; }
        }
        
        public SmallJumper(Vector2 position)
            : base(position)
        {
            HP = new AttributePair(2);
            Velocity = new Vector2(0.0f, 0.0f);
        }

        public override void LoadAnimations()
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();

            Vector2 explodeOffset = new Vector2(4, 4);

            // Construct animation from sprite sheet
            animations.Add("Default", new Animation(ContentRects, ContentOffsets));
            Animation deathAnimation = new Animation(ExplodeContentRects, new Vector2[] { explodeOffset, explodeOffset, explodeOffset }, "Death");
            deathAnimation.FramesPerSecond = 12;
            animations.Add("Death", deathAnimation);

            animatedSprite = new AnimatedSprite(mm10EnemySprites, animations);
            animatedSprite.CurrentAnimation = "Default";
            animatedSprite.IsAnimating = true;
            animatedSprite.AnimationEnd += OnFinishedDeath;
        }

        public void RunAI(GameTime gameTime)
        {
            if (!IsDying)
            {
                Velocity.Y += Gravity;
                Position += Velocity;

                HandleTileCollisions();

                // Enemy is on the ground
                //if (Math.Abs(Position.Y - PosPreviousFrame.Y) < 0.001)
                if (isOnGround)
                {
                    Velocity.Y = 0;

                    // Just landed, get ready to jump again
                    if (IsJumping)
                    {
                        Velocity.X = 0f;
                        timeLanded = DateTime.Now;
                        IsJumping = false;
                    }
                    // Has been on the ground for at least 1 frame
                    else
                    {
                        // Time to jump!
                        if (DateTime.Now - timeLanded > jumpWaitTime)
                        {
                            IsJumping = true;
                            Velocity.X = (IsFacingLeft) ? -XSpeed : XSpeed;

                            // Random small jump or high jump
                            Velocity.Y = (random.Next(2) == 0) ? JumpSpeed0 : JumpSpeed1;
                        }
                    }

                } // end if isOnGround
            } // end if NOT IsDying
        }

        public override void Deactivate()
        {
            HP.ReplenishAll();
            Velocity = Vector2.Zero;
            base.Deactivate();

            animatedSprite.CurrentAnimation = "Default";
            animatedSprite.Position = this.Position;
        }

        public override void Activate()
        {
            IsFacingLeft = Session.MegaMan.HitBox.CenterX < Bounds.CenterX;
            random = new Random();
            base.Activate();
        }

        public void HandleTileCollisions()
        {
            isOnGround = false;

            CollisionResult collision = Session.CollisionCheckTilemap(Position, PosPreviousFrame, Bounds);

            if (collision.HasCollided)
            {
                isOnGround = collision.HasLanded;
                Position = collision.NewPosition;

            }
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
                if (IsFacingLeft)
                {
                    animatedSprite.FlipHorizontal = false;
                }
                else
                {
                    animatedSprite.FlipHorizontal = true;
                }
                if ((DateTime.Now - DamageFlashStart) > DamageFlashDuration)
                {
                    animatedSprite.Draw(spriteBatch);
                }
            }
        }

        public virtual void OnFinishedDeath(object sender, AnimationEndEventArgs e)
        {
            if (e.AnimationString == "Death")
            {
                Deactivate();
                IsDying = false;
            }
        }

        private static Rectangle ContentRect0 = new Rectangle(163, 99, 17, 26);
        private static Vector2 ContentOffset0 = Vector2.Zero;
        private static Rectangle ContentRect1 = new Rectangle(189, 99, 17, 26);
        private static Vector2 ContentOffset1 = Vector2.Zero;
        private static Rectangle ContentRect2 = new Rectangle(214, 99, 17, 26);
        private static Vector2 ContentOffset2 = Vector2.Zero;

        private static Rectangle[] ContentRects = { ContentRect0, ContentRect1, ContentRect2 };
        private static Vector2[] ContentOffsets = { ContentOffset0, ContentOffset1, ContentOffset2 };


    }
}
