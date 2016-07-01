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
    class RisingPlatform : Obstacle
    {
        private static Dictionary<String, Vector2> spriteOffset = new Dictionary<string, Vector2>();
        public readonly static Vector2 SIZE = new Vector2(32, 16);
        public override Vector2 Size { get { return SIZE; } }

        public bool IsAFallingPlatform = false;
        public bool HasActivated { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        private readonly float _maxVelocity = -3.0f;
        private readonly float _initialVelocity = 1.5f;
        private readonly float _acceleration = -0.1f;

        public RisingPlatform(Vector2 position, bool fallingPlatform = false) : base(position)
        {
            HasActivated = false;
            Acceleration = new Vector2(0f, -0.1f);

            IsAFallingPlatform = fallingPlatform;

            if (IsAFallingPlatform)
            {
                Velocity = new Vector2(0, 0);
            }
            else
            {
                Velocity = new Vector2(0, _initialVelocity);
            }
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

            if (IsAFallingPlatform)
            {
                animations.Add("Default", new Animation(ContentRectDefaultBlue));
                animations.Add("Activated", new Animation(ContentRectsAnimationBlue));
            }
            else
            {    
                animations.Add("Default", new Animation(ContentRectDefault));
                animations.Add("Activated", new Animation(ContentRectsAnimation));
            }

            animatedSprite = new AnimatedSprite(mm10Objects, animations);
            animatedSprite.CurrentAnimation = "Default";
        }

        public override void Update(GameTime gameTime)
        {
            if (HasActivated)
            {
                this.Position += this.Velocity;
                

                if (IsAFallingPlatform)
                {
                    this.Velocity -= this.Acceleration;
                    if (this.Velocity.Y > -_maxVelocity)
                    {
                        this.Velocity = new Vector2(0, -_maxVelocity);
                    }
                }
                else
                {
                    this.Velocity += this.Acceleration;
                    if (this.Velocity.Y < _maxVelocity)
                    {
                        this.Velocity = new Vector2(0, _maxVelocity);
                    }
                }
            }

            animatedSprite.Position = this.Position;
            animatedSprite.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animatedSprite.Draw(spriteBatch);
            
            base.Draw(gameTime, spriteBatch);
        }

        public override void NotifyPlayerStanding()
        {
            // First time landing on platform
            if (!HasActivated)
            {
                HasActivated = true;
                animatedSprite.CurrentAnimation = "Activated";
                animatedSprite.IsAnimating = true;
                animatedSprite.AnimationEnd +=
                    new EventHandler<AnimationEndEventArgs> ( 
                        (object o, AnimationEndEventArgs args) => 
                        {
                            animatedSprite.CurrentAnimation = "Default";
                            animatedSprite.IsAnimating = false;
                        } );
            }
            Session.MegaMan.VelocityExternal = Velocity;
            base.NotifyPlayerStanding();
        }

        private static Rectangle ContentRectDefault = new Rectangle(0, 0, (int)SIZE.X, (int)SIZE.Y);
        private static Rectangle ContentRect0 = new Rectangle(0, 18, (int)SIZE.X, (int)SIZE.Y);
        private static Rectangle ContentRect1 = new Rectangle(0, 36, (int)SIZE.X, (int)SIZE.Y);
        private static Rectangle ContentRect2 = new Rectangle(0, 54, (int)SIZE.X, (int)SIZE.Y);
        private static Rectangle ContentRect3 = new Rectangle(0, 72, (int)SIZE.X, (int)SIZE.Y);

        private static Rectangle ContentRectDefaultBlue = new Rectangle((int)SIZE.X + 2, 0, (int)SIZE.X, (int)SIZE.Y);
        private static Rectangle ContentRect0Blue = new Rectangle((int)SIZE.X + 2, 18, (int)SIZE.X, (int)SIZE.Y);
        private static Rectangle ContentRect1Blue = new Rectangle((int)SIZE.X + 2, 36, (int)SIZE.X, (int)SIZE.Y);
        private static Rectangle ContentRect2Blue = new Rectangle((int)SIZE.X + 2, 54, (int)SIZE.X, (int)SIZE.Y);
        private static Rectangle ContentRect3Blue = new Rectangle((int)SIZE.X + 2, 72, (int)SIZE.X, (int)SIZE.Y);

        private static Rectangle[] ContentRectsAnimation = { ContentRect0, ContentRect1, ContentRect2, ContentRect3 };
        private static Rectangle[] ContentRectsAnimationBlue = { ContentRect0Blue, ContentRect1Blue, ContentRect2Blue, ContentRect3Blue };
    }
}
