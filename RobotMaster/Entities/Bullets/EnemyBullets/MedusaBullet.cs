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
    public class MedusaBullet : Bullet
    {
        private AnimatedSprite animatedSprite;
        private static Dictionary<String, Animation> animations;
        private static Dictionary<String, Vector2> spriteOffset;

        public Vector2 Velocity;
        public FloatRect Bounds { get { return new FloatRect(Position, SIZE.X, SIZE.Y); } }
        public float Width { get { return SIZE.X; } }
        public float Height { get { return SIZE.Y; } }

        public bool IsDying = false;
        public AttributePair HP;

        public override float Damage
        {
            get { return 1.0f; }
        }

        public override bool IsOnScreen
        {
            get
            {
                return ((Position.X < Session.Camera.Position.X + Engine.ViewportWidth) &&
                    (Position.X + Width > Session.Camera.Position.X) &&
                    (Position.Y + Height > Session.Camera.Position.Y) &&
                    (Position.Y < Session.Camera.Position.Y + Engine.ViewportHeight) &&
                    !Session.IsScrolling);
            }
        }

        public MedusaBullet()
            : base()
        {
            HP = new AttributePair(1);

            animatedSprite = new AnimatedSprite(weaponsTexture, animations);
            animatedSprite.CurrentAnimation = "Bullet";
            animatedSprite.IsAnimating = true;
            animatedSprite.AnimationEnd += OnFinishedDeath;
        }

        public static void Load()
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();

            // Construct bullet animation from weapons sprite sheet
            animations.Add("Bullet", new Animation(BulletContentRect, BulletContentOffset));

            Vector2 explodeOffset = new Vector2(-1, -1);
            Animation deathAnimation = new Animation(ExplodeContentRects, new Vector2[] { explodeOffset, explodeOffset, explodeOffset }, "Death");
            deathAnimation.FramesPerSecond = 12;
            animations.Add("Death", deathAnimation);
        }

        public void Activate(bool IsFacingLeft, FloatRect EnemyRect)
        {
            InUse = true;
            HP.ReplenishAll();

            if (IsFacingLeft)
            {
                Position = new Vector2(EnemyRect.Left - Width, EnemyRect.Top + 5);
                Velocity.X = -MedusaBullet.Speed;
            }
            else
            {
                Position = new Vector2(EnemyRect.Right, EnemyRect.Top + 5);
                Velocity.X = MedusaBullet.Speed;
            }

            return;
        }

        public void Kill()
        {
            IsDying = true;
            animatedSprite.Position = this.Position;
            animatedSprite.CurrentAnimation = "Death";
            animatedSprite.SetCurrentFrame("Death", 0);
            animatedSprite.FrameTimer = TimeSpan.Zero;
        }

        public virtual void OnFinishedDeath(object sender, AnimationEndEventArgs e)
        {
            if (e.AnimationString == "Death")
            {
                Position = new Vector2(-256.0f, -224.0f);
                animatedSprite.Position = this.Position;
                animatedSprite.CurrentAnimation = "Bullet";
                IsDying = false;
            }
        }

        public override bool HitTest(FloatRect rect, float HP = 0f)
        {
            return false;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsOnScreen && InUse)
            {
                if (!IsDying)
                {
                    Position += Velocity;
                }

                animatedSprite.Position = this.Position;
                animatedSprite.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsOnScreen && InUse)
            {
                animatedSprite.Draw(spriteBatch);
            }
        }

        public readonly static float Speed = 2.5f;
        public readonly static Vector2 SIZE = new Vector2(10, 10);
        private static Rectangle BulletContentRect = new Rectangle(67, 495, 10, 10);
        private static Vector2 BulletContentOffset = Vector2.Zero;
    }
}
