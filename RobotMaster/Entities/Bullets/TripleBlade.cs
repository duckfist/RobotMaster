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
    public class TripleBlade : Bullet
    {
        private List<AnimatedSprite> animatedSprites;
        private static Dictionary<String, Animation> animations;
        private static Dictionary<String, Vector2> spriteOffset;

        public Vector2[] Velocities = new Vector2[3];
        public FloatRect[] HitBoxes = new FloatRect[3];
        public float Width { get { return SIZE.X; } }
        public float Height { get { return SIZE.Y; } }

        public override float Damage
        {
            get { return 2.0f; }
        }

        public override bool IsOnScreen
        {
            get
            {
                for (int i = 0; i < HitBoxes.Length; i++)
                {
                    if ((HitBoxes[i].Left < Session.Camera.Position.X + Engine.ViewportWidth) &&
                        (HitBoxes[i].Right > Session.Camera.Position.X) &&
                        (HitBoxes[i].Bottom > Session.Camera.Position.Y) &&
                        (HitBoxes[i].Top < Session.Camera.Position.Y + Engine.ViewportHeight))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public TripleBlade()
            : base()
        {

            HitBoxes[0] = new FloatRect(Position, SIZE.X, SIZE.Y);
            HitBoxes[1] = new FloatRect(Position, SIZE.X, SIZE.Y);
            HitBoxes[2] = new FloatRect(Position, SIZE.X, SIZE.Y);
            Velocities[0] = new Vector2(SPEED, 0f);
            Velocities[1] = new Vector2(SPEED * (float)Math.Cos(Math.PI / 12), SPEED * (float)Math.Sin(Math.PI / 12));
            Velocities[2] = new Vector2(SPEED * (float)Math.Cos(Math.PI / 6), SPEED * (float)Math.Sin(Math.PI / 6));

            animatedSprites = new List<AnimatedSprite>();

            AnimatedSprite blade1 = new AnimatedSprite(weaponsTexture, animations);
            blade1.CurrentAnimation = "BladeRight";
            AnimatedSprite blade2 = new AnimatedSprite(weaponsTexture, animations);
            blade2.CurrentAnimation = "BladeRight";
            AnimatedSprite blade3 = new AnimatedSprite(weaponsTexture, animations);
            blade3.CurrentAnimation = "BladeRight";

            animatedSprites.Add(blade1);
            animatedSprites.Add(blade2);
            animatedSprites.Add(blade3);


        }

        public static void Load()
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();

            // Construct bullet animations from weapons sprite sheet
            animations.Add("BladeRightUp2", new Animation(BladeRightUpRect2, Vector2.Zero));
            animations.Add("BladeRight", new Animation(BladeRightRect, Vector2.Zero));
            animations.Add("BladeRightDown2", new Animation(BladeRightDownRect2, Vector2.Zero));
            animations.Add("BladeRightUp1", new Animation(BladeRightUpRect1, Vector2.Zero));
            animations.Add("BladeRightDown1", new Animation(BladeRightDownRect1, Vector2.Zero));
            animations.Add("BladeRightUp3", new Animation(BladeRightUpRect3, Vector2.Zero));
            animations.Add("BladeRightDown3", new Animation(BladeRightDownRect3, Vector2.Zero));
            animations.Add("BladeRightUp4", new Animation(BladeRightUpRect4, Vector2.Zero));
            animations.Add("BladeRightDown4", new Animation(BladeRightDownRect4, Vector2.Zero));
            animations.Add("BladeLeftUp2", new Animation(BladeLeftUpRect2, Vector2.Zero));
            animations.Add("BladeLeft", new Animation(BladeLeftRect, Vector2.Zero));
            animations.Add("BladeLeftDown2", new Animation(BladeLeftDownRect2, Vector2.Zero));
            animations.Add("BladeLeftUp1", new Animation(BladeLeftUpRect1, Vector2.Zero));
            animations.Add("BladeLeftDown1", new Animation(BladeLeftDownRect1, Vector2.Zero));
            animations.Add("BladeLeftUp3", new Animation(BladeLeftUpRect3, Vector2.Zero));
            animations.Add("BladeLeftDown3", new Animation(BladeLeftDownRect3, Vector2.Zero));
            animations.Add("BladeLeftUp4", new Animation(BladeLeftUpRect4, Vector2.Zero));
            animations.Add("BladeLeftDown4", new Animation(BladeLeftDownRect4, Vector2.Zero));

            // Debug
            debugRectangle = new DebugRectangle(SIZE);
            debugRectTex = DebugRectangle.GetRectTexture(SIZE);
        }

        public void Activate(bool IsFacingLeft, bool IsOffGround, FloatRect MegaManRect)
        {
            InUse = true;
            if (IsFacingLeft)
            {
                for (int i = 0; i < HitBoxes.Length; i++)
                {
                    HitBoxes[i].Position = new Vector2(MegaManRect.Left - MegaManRect.Width - 6, MegaManRect.Top);
                    Velocities[i].X = Math.Abs(Velocities[i].X) * -1.0f;
                    animatedSprites[i].CurrentAnimation = "BladeLeft";
                }
            }
            else
            {
                for (int i = 0; i < HitBoxes.Length; i++)
                {
                    HitBoxes[i].Position = new Vector2(MegaManRect.Right - 2, MegaManRect.Top);
                    Velocities[i].X = Math.Abs(Velocities[i].X);
                    animatedSprites[i].CurrentAnimation = "BladeRight";
                }
            }

            if (IsOffGround)
            {
                for (int i = 0; i < HitBoxes.Length; i++)
                {
                    Velocities[i].Y = Math.Abs(Velocities[i].Y);
                    if (i > 0)
                        animatedSprites[i].CurrentAnimation += String.Format("Down{0}", i);
                }
            }
            else
            {
                for (int i = 0; i < HitBoxes.Length; i++)
                {
                    Velocities[i].Y = Math.Abs(Velocities[i].Y) * -1.0f;
                    if (i > 0)
                        animatedSprites[i].CurrentAnimation += String.Format("Up{0}", i);
                }
            }

            return;
        }

        public override bool HitTest(FloatRect rect, float HP = 0f)
        {
            for (int i = 0; i < HitBoxes.Length; i++)
            {
                if (rect.Intersects(HitBoxes[i]))
                {
                    // Only make triple blades disappear if enemy is not killed
                    if (HP > Damage)
                    {
                        HitBoxes[i].Position = new Vector2(-256.0f, -224.0f);
                    }

                    // Hit was successful with this blade
                    return true;
                }
            }
            return false;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsOnScreen && InUse)
            {
                for (int i = 0; i < HitBoxes.Length; i++)
                {
                    HitBoxes[i] += Velocities[i];
                    animatedSprites[i].Position = HitBoxes[i].Position;
                    animatedSprites[i].Update(gameTime);
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsOnScreen && InUse)
            {
                for (int i = 0; i < HitBoxes.Length; i++)
                {
                    animatedSprites[i].Draw(spriteBatch);

                    if (Session.DebugHitboxes)
                    {
                        debugRectangle.Draw(spriteBatch, HitBoxes[i].Position, Color.Yellow, debugRectTex);
                    }
                }
            }
        }

        public readonly static float SPEED = 5.0f;
        public readonly static Vector2 SIZE = new Vector2(16, 16);
        private static Rectangle BladeRightUpRect2 = new Rectangle(88,456,16,16);
        private static Rectangle BladeRightRect = new Rectangle(104,456,16,16);
        private static Rectangle BladeRightDownRect2 = new Rectangle(120,456,16,16);
        private static Rectangle BladeRightUpRect1 = new Rectangle(136,456,16,16);
        private static Rectangle BladeRightDownRect1 = new Rectangle(152,456,16,16);
        private static Rectangle BladeRightUpRect3 = new Rectangle(168,456,16,16);
        private static Rectangle BladeRightDownRect3 = new Rectangle(184,456,16,16);
        private static Rectangle BladeRightUpRect4 = new Rectangle(200,456,16,16);
        private static Rectangle BladeRightDownRect4 = new Rectangle(216,456,16,16);
        private static Rectangle BladeLeftUpRect2 = new Rectangle(88,472,16,16);
        private static Rectangle BladeLeftRect = new Rectangle(104,472,16,16);
        private static Rectangle BladeLeftDownRect2 = new Rectangle(120,472,16,16);
        private static Rectangle BladeLeftUpRect1 = new Rectangle(136,472,16,16);
        private static Rectangle BladeLeftDownRect1 = new Rectangle(152,472,16,16);
        private static Rectangle BladeLeftUpRect3 = new Rectangle(168,472,16,16);
        private static Rectangle BladeLeftDownRect3 = new Rectangle(184,472,16,16);
        private static Rectangle BladeLeftUpRect4 = new Rectangle(200,472,16,16);
        private static Rectangle BladeLeftDownRect4 = new Rectangle(216,472,16,16);

        private static DebugRectangle debugRectangle;
        private static Texture2D debugRectTex;

    }
}
