using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RobotMaster.TileEngine;
using RobotMaster.Mathematics;

namespace RobotMaster.GameComponents
{
    public class AnimatedSprite
    {
        #region Fields and Properties
        Dictionary<String, Animation> animations;
        String currentAnimation = "";
        bool isAnimating = false;
        bool flipHorizontal = false;
        Texture2D texture;
        Vector2 position;
        Vector2 velocity;
        float speed = 2.0f;

        public event EventHandler<AnimationEndEventArgs> AnimationEnd;

        public String CurrentAnimation
        {
            get { return currentAnimation; }
            set { currentAnimation = value; }
        }
        public bool IsAnimating
        {
            get { return isAnimating; }
            set { isAnimating = value; }
        }
        public int Width
        {
            get { return animations[currentAnimation].FrameWidth; }
        }
        public int Height
        {
            get { return animations[currentAnimation].FrameHeight; }
        }
        public float Speed
        {
            get { return speed; }
            set { speed = MathHelper.Clamp(speed, 1.0f, 16.0f); }
        }
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
            }
        }
        public Vector2 Velocity
        {
            get { return velocity; }
            set
            {
                velocity = value;
                if (velocity != Vector2.Zero)
                    velocity.Normalize();
            }
        }

        public TimeSpan FrameTimer
        {
            get
            {
                return animations[currentAnimation].frameTimer;
            }
            set
            {
                animations[currentAnimation].frameTimer = value;
            }
        }
        public int Frame
        {
            get
            {
                return animations[currentAnimation].CurrentFrame;
            }
            set
            {
                animations[currentAnimation].CurrentFrame = value;
            }

        }
        public bool FlipHorizontal
        {
            get
            {
                return flipHorizontal;
            }
            set
            {
                flipHorizontal = value;
            }
        }


        #endregion


        #region Constructors
        public AnimatedSprite(Texture2D texture, Dictionary<String, Animation> animations)
        {
            this.texture = texture;
            this.animations = animations;

            foreach (var entry in animations)
            {
                entry.Value.AnimationEnd += OnAnimationEnd;
            }
        }
        #endregion


        #region Methods
        public void SetCurrentFrame(string animation, int frame)
        {
            try
            {
                animations[animation].CurrentFrame = frame;
            }
            catch (Exception e)
            {
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            if (isAnimating)
                animations[currentAnimation].Update(gameTime);
        }

        // TODO: Implement offsets for each frame of animation
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (flipHorizontal)
            {
                spriteBatch.Draw(
                    texture,
                    MegaMath.Floor(position + animations[currentAnimation].ObjectOffset),
                    //position + animations[currentAnimation].ObjectOffset,
                    animations[currentAnimation].CurrentFrameRect,
                    Color.White,
                    0f,
                    new Vector2(),
                    1f,
                    SpriteEffects.FlipHorizontally,
                    0);
            }

            else
            {
                spriteBatch.Draw(
                    texture,
                    MegaMath.Floor(position + animations[currentAnimation].ObjectOffset),
                    //position + animations[currentAnimation].ObjectOffset,
                    animations[currentAnimation].CurrentFrameRect,
                    Color.White);
            }

        }

        protected virtual void OnAnimationEnd(object sender, AnimationEndEventArgs e)
        {
            if (AnimationEnd != null && sender == animations[currentAnimation])
            {
                AnimationEnd(this, e);
            }
        }

        public void LockToViewport()
        {
            position.X = MathHelper.Clamp(position.X, 0, Session.CurrentMap.WidthInPixels - Width);
            position.Y = MathHelper.Clamp(position.Y, 0, Session.CurrentMap.HeightInPixels - Height);
        }
        #endregion
    }

    public class AnimationEndEventArgs : EventArgs
    {
        public readonly string AnimationString;

        public AnimationEndEventArgs(string animation)
        {
            AnimationString = animation;
        }
    }
}
