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
    public class WaterShieldBubble
    {
        public readonly static Vector2 SIZE = new Vector2(8, 8);
        public static float Width { get { return SIZE.X; } }
        public static float Height { get { return SIZE.Y; } }
        public FloatRect HitBox { get { return new FloatRect(Position, Width, Height); } }

        private Vector2 position;
        public Vector2 Position { get { return position; } set { position = Sprite.Position = value; } }
        public Vector2 Velocity;
        public bool Activated;
        public bool Destroyed;
        public AnimatedSprite Sprite;

        public WaterShieldBubble(AnimatedSprite sprite)
        {
            position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Activated = false;
            Destroyed = false;
            this.Sprite = sprite;
        }
    }
    public class WaterShield : Bullet
    {
        //private List<AnimatedSprite> animatedSprites;
        private static Dictionary<String, Animation> animations;
        private static Dictionary<String, Vector2> spriteOffset;
        public static int NumBubbles = 8;
        public static int BubbleDelay = 10;
        public static double BubbleRadius = 18;
        public static double BubbleAngularVelocity = 0.08;

        private bool clockwise = false;
        public WaterShieldBubble[] Bubbles = new WaterShieldBubble[NumBubbles];
        public float Width { get { return WaterShieldBubble.SIZE.X; } }
        public float Height { get { return WaterShieldBubble.SIZE.Y; } }
        public int FrameCounter = 0;
        public bool ReadyToLaunch = false;
        public bool Launching = false;
        public int LaunchFrame = 0;
        public Vector2 LaunchPosition = Vector2.Zero;

        public override float Damage
        {
            get { return 2.0f; }
        }

        public override bool IsOnScreen
        {
            get
            {
                for (int i = 0; i < NumBubbles; i++)
                {
                    if ((Bubbles[i].HitBox.Left < Session.Camera.Position.X + Engine.ViewportWidth) &&
                        (Bubbles[i].HitBox.Right > Session.Camera.Position.X) &&
                        (Bubbles[i].HitBox.Bottom > Session.Camera.Position.Y) &&
                        (Bubbles[i].HitBox.Top < Session.Camera.Position.Y + Engine.ViewportHeight))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool AreAnyBubblesActivated
        {
            get
            {
                for (int i = 0; i < NumBubbles; i++)
                {
                    if (Bubbles[i].Activated) return true;
                }
                return false;
            }
        }
        public bool AreAnyBubblesDeactivated
        {
            get
            {
                for (int i = 0; i < NumBubbles; i++)
                {
                    if (!Bubbles[i].Activated) return true;
                }
                return false;
            }
        }

        public WaterShield()
            : base()
        {
            for (int i = 0; i < NumBubbles; i++)
            {
                AnimatedSprite bubble = new AnimatedSprite(weaponsTexture, animations);
                bubble.CurrentAnimation = "Default";
                Bubbles[i] = new WaterShieldBubble(bubble);
            }
        }

        public static void Load()
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();

            // Construct bullet animations from weapons sprite sheet
            animations.Add("Default", new Animation(ContentRectBubble, OffsetBubble));
            animations.Add("Pop0", new Animation(ContentRectPop0, Vector2.Zero));
            animations.Add("Pop1", new Animation(ContentRectPop1, Vector2.Zero));

            // Debug
            debugRectangle = new DebugRectangle(WaterShieldBubble.SIZE);
            debugRectTex = DebugRectangle.GetRectTexture(WaterShieldBubble.SIZE);
        }

        public void Activate(bool IsFacingLeft/*, Entity hostEntity*/)
        {
            Position = new Vector2(Session.MegaMan.HitBox.CenterX, Session.MegaMan.HitBox.CenterY);
            for (int i = 0; i < NumBubbles; i++)
            {
                Bubbles[i].Position = Position;
            }

            InUse = true;
            clockwise = !IsFacingLeft;
            FrameCounter = 0;
            return;
        }

        public void DeactivateBubbles()
        {
            for (int i = 0; i < NumBubbles; i++)
            {
                Bubbles[i].Activated = false;
                Bubbles[i].Destroyed = false;
            }
        }

        public void Launch()
        {
            ReadyToLaunch = false;
            Launching = true;
            LaunchFrame = FrameCounter;
            LaunchPosition = Position;
        }

        public override bool HitTest(FloatRect rect, float HP = 0f)
        {
            for (int i = 0; i < NumBubbles; i++)
            {
                if (Bubbles[i].Activated && rect.Intersects(Bubbles[i].HitBox))
                {
                    // Only make water bubbles disappear if enemy is not killed
                    if (HP > Damage)
                    {
                        Bubbles[i].Position = new Vector2(-256.0f, -224.0f);
                        Bubbles[i].Destroyed = true;
                    }

                    // Hit was successful with this bubble
                    return true;
                }
            }
            return false;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsOnScreen && InUse)
            {
                Position = new Vector2(Session.MegaMan.HitBox.CenterX, Session.MegaMan.HitBox.CenterY);

                for (int i = 0; i < NumBubbles; i++)
                {
                    // Activate next bubble, if any remain to be activated
                    if (!Bubbles[i].Activated && FrameCounter >= i * BubbleDelay)
                    {
                        Bubbles[i].Activated = true;

                        // Last bubble; All bubbles have been activated, allow launching.
                        if (i == NumBubbles - 1)
                        {
                            ReadyToLaunch = true;
                        }
                    }
                    // Move all activated bubbles
                    else
                    {
                        if (Bubbles[i].Destroyed) continue;

                        int t = FrameCounter - i * BubbleDelay;
                        double radius = BubbleRadius;
                        if (Launching)
                        {
                            radius += Math.Pow((FrameCounter - LaunchFrame) * 1.5, 1.25);
                        }
                        double x = radius * Math.Cos(t * BubbleAngularVelocity) - WaterShieldBubble.Width/2 + 1;
                        double y = radius * Math.Sin(t * BubbleAngularVelocity) - WaterShieldBubble.Height / 2;
                        if (Launching)
                        {
                            Bubbles[i].Position = LaunchPosition + new Vector2((float)x, (float)y);
                        }
                        else
                        {
                            Bubbles[i].Position = Position + new Vector2((float)x, (float)y);
                        }
                        //Bubbles[i].Sprite.Position = Bubbles[i].Position;
                    }
                }
            }

            // If all bubbles have left the screen, deactivate each to prepare for next shot.
            else if (AreAnyBubblesActivated)
            {
                Launching = false;
                ReadyToLaunch = false;
                DeactivateBubbles();
            }

            FrameCounter++;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsOnScreen && InUse)
            {
                for (int i = 0; i < NumBubbles; i++)
                {
                    if (Bubbles[i].Activated)
                    {
                        Bubbles[i].Sprite.Draw(spriteBatch);

                        if (Session.DebugHitboxes)
                        {
                            debugRectangle.Draw(spriteBatch, Bubbles[i].Position, Color.Yellow, debugRectTex);
                        }
                    }
                }
            }
        }

        private static Rectangle ContentRectBubble = new Rectangle(90, 408, 8, 8);
        private static Vector2 OffsetBubble = new Vector2(-1, -1);
        private static Rectangle ContentRectPop0 = new Rectangle(99, 406, 10, 10);
        private static Rectangle ContentRectPop1 = new Rectangle(110, 406, 10, 10);

        private static DebugRectangle debugRectangle;
        private static Texture2D debugRectTex;
    }
}
