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
    class MedusaHead : Enemy
    {
        private static Dictionary<String, Vector2> spriteOffset = new Dictionary<string, Vector2>();

        private MedusaBullet bullet;

        public Vector2 Velocity;

        public readonly static float Speed = 1.5f;
        private float sinX = 0.0f;
        private readonly static float sinInc = 0.0666f;
        private readonly static float sinScale = 1.5f;

        public readonly static Vector2 SIZE = new Vector2(20, 20);
        public override Vector2 Size
        {
            get { return SIZE; }
        }

        public MedusaHead(Vector2 position)
            : base(position)
        {
            HP = new AttributePair(1);
            Velocity = new Vector2(Speed, 0.0f);
            bullet = new MedusaBullet();
        }

        public override void LoadAnimations()
        {
            // Construct animation from sprite sheet
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();
            Vector2 explodeOffset = new Vector2(4, 4);
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
                sinX += sinInc;
                Velocity.Y = sinScale * (float)Math.Sin(sinX);

                Position += Velocity;

                if (!bullet.InUse)
                {
                    bullet.Activate(Velocity.X < 0, Bounds);
                }
            }
        }

        public override void Deactivate()
        {
            sinX = 0f;
            HP.ReplenishAll();
            base.Deactivate();

            animatedSprite.CurrentAnimation = "Default";
            animatedSprite.Position = this.Position;
        }

        public override void Activate()
        {
            if (Session.MegaMan.HitBox.CenterX < Bounds.CenterX)
            {
                Velocity = new Vector2(-Speed, 0.0f);
            }
            else
            {
                Velocity = new Vector2(Speed, 0.0f);
            }
            base.Activate();
        }

        public override void BulletCollisions(List<Bullet> bullets)
        {

            base.BulletCollisions(bullets);
            
            // Collision against the enemy's bullets, if any are active
            if (bullet.InUse && !bullet.IsDying)
            {
                foreach (Bullet b in bullets)
                {
                    // If enemy bullet hits a player bullet, while player bullet is in use
                    if (b.InUse && b.HitTest(bullet.Bounds))
                    {
                        bullet.HP.Deplete(b.Damage);

                        if (bullet.HP.CurrentValue <= 0.0f)
                        {
                            bullet.Kill();
                        }
                    }
                }
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

            bullet.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                if (Velocity.X < 0.0f)
                {
                    animatedSprite.FlipHorizontal = false;
                }
                else
                {
                    animatedSprite.FlipHorizontal = true;
                }

                animatedSprite.Draw(spriteBatch);
                
            }
            bullet.Draw(gameTime, spriteBatch);
        }

        public virtual void OnFinishedDeath(object sender, AnimationEndEventArgs e)
        {
            if (e.AnimationString == "Death")
            {
                Deactivate();
                IsDying = false;
            }
        }

        private static Rectangle ContentRect0 = new Rectangle(21, 101, 20, 20);
        private static Vector2 ContentOffset0 = Vector2.Zero;
        private static Rectangle ContentRect1 = new Rectangle(51, 101, 20, 20);
        private static Vector2 ContentOffset1 = Vector2.Zero;
        private static Rectangle ContentRect2 = new Rectangle(79, 101, 20, 20);
        private static Vector2 ContentOffset2 = Vector2.Zero;
        private static Rectangle ContentRect3 = new Rectangle(110, 101, 20, 20);
        private static Vector2 ContentOffset3 = Vector2.Zero;

        private static Rectangle[] ContentRects = { ContentRect0, ContentRect1, ContentRect2, ContentRect3 };
        private static Vector2[] ContentOffsets = { ContentOffset0, ContentOffset1, ContentOffset2, ContentOffset3 };


    }
}
