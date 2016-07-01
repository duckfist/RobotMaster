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
    class BossDoor : Obstacle
    {
        // One door is actually 2 door wide
        public readonly static Vector2 SIZE = new Vector2(32, 64);
        public override Vector2 Size { get { return SIZE; } }
        public static bool DoorPause = false;

        public bool IsVertical;
        public bool IsLocked;
        public bool IsOpening;
        public bool IsClosing;
        public CameraEventHandler FinishedPanning;

        public BossDoor(Vector2 position, bool verticalDoor = true)
            : base(position)
        {
            IsVertical = verticalDoor;
            IsLocked = false;
            IsOpening = false;
            IsClosing = false;
            //IsCollidable = false;

            FinishedPanning = new CameraEventHandler(NotifyFinishedPanning);

            LoadAnimations();
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

        public override void LoadAnimations()
        {
            // Construct animation from sprite sheet
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();

            if (IsVertical)
            {
                animations.Add("Opening", new Animation(ContentRectsVert) { FramesPerSecond = 10 });
                animations.Add("Closing", new Animation(new Rectangle[5]
                {
                    ContentVertDoor3,
                    ContentVertDoor2,
                    ContentVertDoor1,
                    ContentVertDoor0,
                    ContentVertDoor0,
                }) { FramesPerSecond = 10 });
            }
            else
            {
                //animations.Add("Default", new Animation(ContentRectsHoriz));
            }

            // Begin in opening animation, rig up scrolling events to sync with animation
            animatedSprite = new AnimatedSprite(mm10Objects, animations);
            animatedSprite.CurrentAnimation = "Opening";
            animatedSprite.AnimationEnd += new EventHandler<AnimationEndEventArgs>((o, args) =>
                {
                    animatedSprite.IsAnimating = false;
                    DoorPause = false;
                    if (animatedSprite.CurrentAnimation == "Opening")
                    {
                        animatedSprite.SetCurrentFrame("Opening", 4); // Empty space
                    }
                    else
                    {
                        animatedSprite.SetCurrentFrame("Closing", 4); // Closed
                        IsLocked = true;
                        IsCollidable = true;
                    }
                });
        }

        // Close the door when screen is finished scrolling
        public void NotifyFinishedPanning(object o, EventArgs args)
        {
            // Stop Mega Man from updating until door is finished closing
            DoorPause = true;   
            animatedSprite.CurrentAnimation = "Closing";
            animatedSprite.SetCurrentFrame("Closing", 0);
            animatedSprite.IsAnimating = true;
            Session.Camera.FinishedPanning -= FinishedPanning;
        }

        // Open the door if it is not locked, and start scrolling screen
        public override void NotifyPlayerCollision()
        {
            if (!IsLocked)
            {
                // Stop Mega Man from updating until door is finished opening
                DoorPause = true;
                IsCollidable = false;
                animatedSprite.IsAnimating = true;
                Session.Camera.FinishedPanning += FinishedPanning;
            }
        }

        public override void Update(GameTime gameTime)
        {
            animatedSprite.Position = this.Position;
            animatedSprite.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animatedSprite.Draw(spriteBatch);
            base.Draw(gameTime, spriteBatch);
        }
        
        private static Rectangle ContentVertDoor0 = new Rectangle(67, 25, 32, 64);
        private static Rectangle ContentVertDoor1 = new Rectangle(100, 25, 32, 48);
        private static Rectangle ContentVertDoor2 = new Rectangle(133, 25, 32, 32);
        private static Rectangle ContentVertDoor3 = new Rectangle(166, 25, 32, 16);
        private static Rectangle[] ContentRectsVert = { ContentVertDoor0, ContentVertDoor1, ContentVertDoor2, ContentVertDoor3, new Rectangle() };
        private static Dictionary<String, Vector2> spriteOffset = new Dictionary<string, Vector2>();
    }
}
