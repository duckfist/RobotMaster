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
    public class MegaBuster : Bullet
    {
        private AnimatedSprite animatedSprite;
        private static Dictionary<String, Animation> animations;
        private static Dictionary<String, Vector2> spriteOffset;

        public Vector2 Velocity;
        public FloatRect Bounds { get { return new FloatRect(Position, SIZE.X, SIZE.Y); } }
        public float Width { get { return SIZE.X; } }
        public float Height { get { return SIZE.Y; } }

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
                    (Position.Y < Session.Camera.Position.Y + Engine.ViewportHeight));
            }
        }

        public MegaBuster()
            : base()
        {
            //this.Velocity = new Vector2(left ? -Speed : Speed, 0);
            animatedSprite = new AnimatedSprite(bulletTexture, animations);
            animatedSprite.CurrentAnimation = "Bullet";
            animatedSprite.IsAnimating = true;
        }

        public static void Load()
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();

            // Construct bullet animation from weapons sprite sheet
            animations.Add("Bullet", new Animation(BulletContentRect, BulletContentOffset));

            // Debug
            debugRectangle = new DebugRectangle(SIZE);
            debugRectTex = DebugRectangle.GetRectTexture(SIZE);
        }

        public void Activate(bool IsFacingLeft, FloatRect MegaManRect)
        {
            InUse = true;
            if (IsFacingLeft)
            {
                Position = new Vector2(MegaManRect.Left - 5 - Width, MegaManRect.Top + 6);
                Velocity.X = -MegaBuster.Speed;
            }
            else
            {
                Position = new Vector2(MegaManRect.Right + 5, MegaManRect.Top + 6);
                Velocity.X = MegaBuster.Speed;
            }

            return;
        }

        public override bool HitTest(FloatRect rect, float HP = 0f)
        {
            if (rect.Intersects(Bounds))
            {
                Position = new Vector2(-256.0f, -224.0f);
                return true;
            }
            else return false;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsOnScreen && InUse)
            {
                Position += Velocity;

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

                if (Session.DebugHitboxes)
                {
                    debugRectangle.Draw(spriteBatch, Position, Color.Yellow, debugRectTex);
                }
            }
        }

        public readonly static float Speed = 4.0f;
        public readonly static Vector2 SIZE = new Vector2(8, 6);
        private static Rectangle BulletContentRect = new Rectangle(83, 394, 8, 6);
        private static Vector2 BulletContentOffset = Vector2.Zero;

        private static DebugRectangle debugRectangle;
        private static Texture2D debugRectTex;
    }
}
