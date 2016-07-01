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
    class TestBoss : Boss
    {
        private static Dictionary<String, Vector2> spriteOffset;
        public readonly static Vector2 SIZE = new Vector2(14, 22);
        public readonly static float MaxFallVelocity = 7.0f;

        public MeterDisplay HPMeter;


        public Vector2 Velocity;
        public bool IsFacingLeft = true;
        public bool IsOnGround = false;
        public override Vector2 Size { get { return SIZE; } }

        public TestBoss(Vector2 Position)
            : base(Position)
        {
            HP = new AttributePair(28);
            HPMeter = new MeterDisplay(new AttributePair(28,0), new Vector2(40, 20), MeterDisplay.MeterColor.Boss);
            Velocity = new Vector2(0, 0);
        }

        public override void LoadAnimations()
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();
            Vector2 explodeOffset = new Vector2(4, 4);

            // Construct animation from spritesheet
            animations.Add("Default", new Animation(RectTexture, Offset));
            Animation deathAnimation = new Animation(ExplodeContentRects, new Vector2[] { explodeOffset, explodeOffset, explodeOffset }, "Death");
            deathAnimation.FramesPerSecond = 5;
            animations.Add("Death", deathAnimation);
            animatedSprite = new AnimatedSprite(mm10EnemySprites, animations);

            animatedSprite.CurrentAnimation = "Default";
            animatedSprite.IsAnimating = false;
            //animatedSprite.AnimationEnd += OnFinishedDeath;

            base.LoadAnimations();
        }

        public override void BulletCollisions(List<Bullet> bullets)
        {
            if (BossCurrentState == BossState.Fight)
            {
                foreach (Bullet b in bullets)
                {
                    // Collision against the enemy
                    if (b.InUse && b.HitTest(Bounds, HP.CurrentValue))
                    {
                        // Boss is currently damage-invulnerable
                        if (invulnCount < invulnTime)
                        {
                            continue;
                        }

                        // TODO: Solar Blaze hit limiter
                        HP.Deplete(1);

                        // Killed!
                        if (HP.CurrentValue <= 0.0f)
                        {
                            // TODO: death animation
                            //IsDying = true;
                            BossCurrentState = BossState.Dead;
                            //State = EnemyState.DeathAnimate;
                            //animatedSprite.Position = this.Position;
                            //animatedSprite.IsAnimating = true;
                            //animatedSprite.CurrentAnimation = "Death";
                            //animatedSprite.SetCurrentFrame("Death", 0);
                            //animatedSprite.FrameTimer = TimeSpan.Zero;

                            DeathExplosion.CreateEffect(Bounds.Center);
                        }
                        // "Flash" to indicate damage has been dealt, and set invulnerable
                        else
                        {
                            invulnCount = 0;
                        }
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (BossCurrentState)
            {
                case BossState.Intro:
                    Position += Velocity;
                    Velocity.Y += Gravity;
                    if (Velocity.Y > MaxFallVelocity) Velocity.Y = MaxFallVelocity;
                    CollisionResult collision = Session.CollisionCheckTilemap(Position, PosPreviousFrame, Bounds);
                    if (collision.HasLanded)
                    {
                        Position = collision.NewPosition;
                        Velocity = Vector2.Zero;
                        IsOnGround = true;
                        BossCurrentState = BossState.Health;
                    }
                    break;
                case BossState.Health:
                    fakeHP += fakeHPPerFrame;
                    if (fakeHP > HP.MaximumValue)
                    {
                        BossCurrentState = BossState.Fight;
                    }
                    break;
                case BossState.Fight:
                    if (invulnCount < invulnTime)
                    {
                        invulnCount++;
                    }
                    break;
                default: break;
            }

            if (BossCurrentState != BossState.Inactive &&
                BossCurrentState != BossState.Dead)
            {
                animatedSprite.Position = this.Position;
                animatedSprite.Update(gameTime);
            }
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            switch (BossCurrentState)
            {
                case BossState.Intro:
                    animatedSprite.Draw(spriteBatch);
                    HPMeter.Draw(spriteBatch, new AttributePair(HP.MaximumValue, 0));
                    break;
                case BossState.Health:
                    animatedSprite.Draw(spriteBatch);
                    HPMeter.Draw(spriteBatch, new AttributePair(HP.MaximumValue, fakeHP));
                    break;
                case BossState.Fight:
                    animatedSprite.Draw(spriteBatch);
                    HPMeter.Draw(spriteBatch, HP);

                    // Draw damage flash
                    if (invulnCount < invulnTime)
                    {
                        spriteDamageFlash.Position = Position;
                        spriteDamageFlash.SetCurrentFrame("Default", (invulnCount % 4) / 2); // hack-up; couldn't get the flashing to animate on its own properly
                        spriteDamageFlash.Draw(spriteBatch);
                    }
                    break;
                default: break;
            }

            base.Draw(gameTime, spriteBatch);
        }

        // Sniper joe, lol
        private static Rectangle RectTexture = new Rectangle(23, 24, 28, 24);
        private static Vector2 Offset = new Vector2(-6, -2);
    }
}
