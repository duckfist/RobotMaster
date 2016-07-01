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
    class Zoomer : Enemy
    {
        private static Dictionary<String, Vector2> spriteOffset;

        protected readonly static float XSpeed = 0.5f;
        protected readonly static float XSpeedFast = 2.0f;
        protected readonly static int TotalFreezeTime = 2000;
        public readonly static Vector2 SIZE = new Vector2(16, 8);

        public bool IsFacingLeft = true;
        public bool IsFrozen = false;
        public Stopwatch FreezeTimer = new Stopwatch();
        public Vector2 Velocity;
        public override Vector2 Size
        {
            get { return SIZE; }
        }
        
        public Zoomer(Vector2 position)
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
            deathAnimation.FramesPerSecond = 6;
            animations.Add("Death", deathAnimation);
            animatedSprite = new AnimatedSprite(mm9EnemySprites, animations);
            animatedSprite.CurrentAnimation = "Default";
            animatedSprite.IsAnimating = true;
            animatedSprite.AnimationEnd += OnFinishedDeath;
        }

        public void RunAI(GameTime gameTime)
        {
            if (!IsDying)
            {
                // Unfreeze after 2 seconds
                if (IsFrozen && FreezeTimer.ElapsedMilliseconds > TotalFreezeTime)
                {
                    IsFrozen = false;
                }

                float _xSpeed;

                // Test if on same y-position as mega man
                if (Math.Abs(Session.MegaMan.HitBox.Bottom - Bounds.CenterY) < 8)
                {
                    _xSpeed = XSpeedFast;
                }
                else
                {
                    _xSpeed = XSpeed;
                }

                _xSpeed = IsFacingLeft ? _xSpeed * -1 : _xSpeed;
                if (IsFrozen) _xSpeed = 0.0f;

                Velocity.X = _xSpeed;
                Position += Velocity;

                HandleTileCollisions();

            } // end if NOT IsDying
        }

        public void HandleTileCollisions()
        {
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
        }

        public override void BulletCollisions(List<Bullet> bullets)
        {
            if (IsActive && !IsDying)
            {
                foreach (Bullet b in bullets)
                {
                    int fakeHP = 100;
                    // Weaknessbullet wb = b as Weaknessbullet
                    // if (weaknessbullet != null) fakeHP = hp;

                    // Collision against the enemy
                    if (b.InUse && b.HitTest(Bounds, fakeHP))
                    {
                        // (if weaknessbullet != null) HP.Deplete(b.Damage);

                        // Killed!
                        if (HP.CurrentValue <= 0.0f)
                        {
                            // TODO: death animation
                            IsDying = true;
                            State = EnemyState.DeathAnimate;
                            animatedSprite.Position = this.Position;
                            animatedSprite.CurrentAnimation = "Death";
                            animatedSprite.SetCurrentFrame("Death", 0);
                            animatedSprite.FrameTimer = TimeSpan.Zero;
                        }
                        else
                        {
                            // Freeze the enemy!
                            IsFrozen = true;
                            FreezeTimer.Restart();
                        }
                    }
                }
            }
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
            IsFacingLeft = true;
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
                animatedSprite.IsAnimating = IsFrozen ? false : true;
                animatedSprite.Draw(spriteBatch);
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

        private static Rectangle ContentRect0 = new Rectangle(165, 226, 16, 8);
        private static Vector2 ContentOffset0 = Vector2.Zero;
        private static Rectangle ContentRect1 = new Rectangle(185, 226, 16, 8);
        private static Vector2 ContentOffset1 = Vector2.Zero;

        private static Rectangle[] ContentRects = { ContentRect0, ContentRect1 };
        private static Vector2[] ContentOffsets = { ContentOffset0, ContentOffset1 };


    }
}
