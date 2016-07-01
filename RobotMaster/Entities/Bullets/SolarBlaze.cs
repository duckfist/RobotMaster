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
    public class SolarBlaze : Bullet
    {
        private static List<AnimatedSprite> animatedSprites;
        private static Dictionary<String, Animation> animations;
        private static Dictionary<String, Vector2> spriteOffset;
        
        public static Vector2 SizePhase0 = new Vector2(16,16);
        public static Vector2 SizePhase1 = new Vector2(12,24);
        public static TimeSpan Delay = TimeSpan.FromMilliseconds(1000);
        public static float Deceleration = 0.12f;
        public static float Speed = 3.0f;

        private float deceleration = Deceleration;
        public DateTime SpawnTime = DateTime.MinValue;
        public Vector2[] Velocities = new Vector2[3];
        public FloatRect[] HitBoxes = new FloatRect[3];

        public override float Damage
        {
            get { return 2.0f; }
        }

        public override bool IsOnScreen
        {
            get
            {
                for (int i = 1; i < HitBoxes.Length; i++)
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

        public int Phase
        {
            get
            {
                if (DateTime.Now - SpawnTime < Delay)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }

        public SolarBlaze()
            : base()
        {

            HitBoxes[0] = new FloatRect(Position, SizePhase0.X, SizePhase0.Y);
            HitBoxes[1] = new FloatRect(Position, SizePhase1.X, SizePhase1.Y);
            HitBoxes[2] = new FloatRect(Position, SizePhase1.X, SizePhase1.Y);
            Velocities[0] = new Vector2(Speed, 0f);
            Velocities[1] = new Vector2(Speed * 1.75f, 0f);
            Velocities[2] = new Vector2(Speed * 1.75f, 0f);

        }

        public static void Load()
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();

            // Construct bullet animations from weapons sprite sheet
            animations.Add("SolarMain", new Animation(SolarMainRect, new Vector2[2] {Vector2.Zero, Vector2.Zero}));
            animations.Add("SolarLeft", new Animation(SolarLeftRect, new Vector2[2] {Vector2.Zero, Vector2.Zero}));
            animations.Add("SolarRight", new Animation(SolarRightRect, new Vector2[2] { Vector2.Zero, Vector2.Zero }));

            AnimatedSprite solar1 = new AnimatedSprite(weaponsTexture, animations);
            solar1.CurrentAnimation = "SolarMain";
            solar1.IsAnimating = true;
            AnimatedSprite solar2 = new AnimatedSprite(weaponsTexture, animations);
            solar2.CurrentAnimation = "SolarLeft";
            solar2.IsAnimating = true;
            AnimatedSprite solar3 = new AnimatedSprite(weaponsTexture, animations);
            solar3.CurrentAnimation = "SolarRight";
            solar3.IsAnimating = true;

            animatedSprites = new List<AnimatedSprite>();
            animatedSprites.Add(solar1);
            animatedSprites.Add(solar2);
            animatedSprites.Add(solar3);

            // Debug
            debugRectangle0 = new DebugRectangle(SizePhase0);
            debugRectangle1 = new DebugRectangle(SizePhase1);
            debugRectTex0 = DebugRectangle.GetRectTexture(SizePhase0);
            debugRectTex1 = DebugRectangle.GetRectTexture(SizePhase1);
        }

        public void Activate(bool IsFacingLeft, FloatRect MegaManRect)
        {
            InUse = true;

            SpawnTime = DateTime.Now;
            Position = Session.MegaMan.Position;
            
            if (IsFacingLeft)
            {
                deceleration = Math.Abs(deceleration) * -1.0f;
                Velocities[0].X = -Speed;
                HitBoxes[0].Position = Session.MegaMan.Position - new Vector2(SizePhase0.X, 0f);
            }
            else
            {
                deceleration = Math.Abs(deceleration);
                Velocities[0].X = Speed;
                HitBoxes[0].Position = Session.MegaMan.Position + new Vector2(SizePhase0.X, 0f);
            }
            Velocities[1].X = -Math.Abs(Velocities[1].X);
            Velocities[2].X = Math.Abs(Velocities[2].X);
            HitBoxes[1].Position = Session.MegaMan.Position;
            HitBoxes[2].Position = Session.MegaMan.Position;
            
        }

        public override bool HitTest(FloatRect rect, float HP = 0f)
        {
            switch (Phase)
            {
                case 0:
                    if (rect.Intersects(HitBoxes[0])) return true;
                    break;
                case 1:
                    if (rect.Intersects(HitBoxes[1])) return true;
                    else if (rect.Intersects(HitBoxes[2])) return true;
                    break;
            }

            return false;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsOnScreen && InUse)
            {

                // Phase 0
                if (Phase == 0)
                {
                    if (Math.Abs(Velocities[0].X) < Math.Abs(Deceleration) * 5)
                    {
                        Velocities[0].X = 0.0f;
                    }
                    else
                    {
                        Velocities[0].X -= deceleration;
                        HitBoxes[0].Position += Velocities[0];
                    }

                    HitBoxes[1].Position = HitBoxes[0].Position - new Vector2(0,4f);
                    HitBoxes[2].Position = HitBoxes[0].Position - new Vector2(0,4f);
                    animatedSprites[0].Position = HitBoxes[0].Position;
                    animatedSprites[0].Update(gameTime);
                }

                // Phase 1
                else 
                {
                    HitBoxes[1] += Velocities[1];
                    HitBoxes[2] += Velocities[2];

                    animatedSprites[1].Position = HitBoxes[1].Position;
                    animatedSprites[1].Update(gameTime);
                    animatedSprites[2].Position = HitBoxes[2].Position;
                    animatedSprites[2].Update(gameTime);
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsOnScreen && InUse)
            {
                // Phase 0
                if (Phase == 0)
                {
                    animatedSprites[0].Draw(spriteBatch);

                    if (Session.DebugHitboxes)
                    {
                        debugRectangle0.Draw(spriteBatch, HitBoxes[0].Position, Color.LightBlue, debugRectTex0);
                    }
                }
                // Phase 1
                else
                {
                    animatedSprites[1].Draw(spriteBatch);
                    animatedSprites[2].Draw(spriteBatch);

                    if (Session.DebugHitboxes)
                    {
                        debugRectangle1.Draw(spriteBatch, HitBoxes[1].Position, Color.LightBlue, debugRectTex1);
                        debugRectangle1.Draw(spriteBatch, HitBoxes[2].Position, Color.LightBlue, debugRectTex1);
                    }
                }
            }
        }


        //public readonly static Vector2 SIZE = new Vector2(16, 16);
        private static Rectangle solarMainRect0 = new Rectangle(114, 96, 16, 16);
        private static Rectangle solarMainRect1 = new Rectangle(130, 96, 16, 16);
        private static Rectangle solarRightRect0 = new Rectangle(102, 87, 12, 24);
        private static Rectangle solarRightRect1 = new Rectangle(90, 87, 12, 24);
        private static Rectangle solarLeftRect0 = new Rectangle(146, 88, 12, 24);
        private static Rectangle solarLeftRect1 = new Rectangle(158, 88, 12, 24);

        private static Rectangle[] SolarMainRect = { solarMainRect0, solarMainRect1 };
        private static Rectangle[] SolarRightRect = { solarRightRect0, solarRightRect1 };
        private static Rectangle[] SolarLeftRect = { solarLeftRect0, solarLeftRect1 };

        private static DebugRectangle debugRectangle0;
        private static DebugRectangle debugRectangle1;
        private static Texture2D debugRectTex0;
        private static Texture2D debugRectTex1;

    }
}
