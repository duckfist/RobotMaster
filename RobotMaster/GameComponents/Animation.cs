using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RobotMaster.GameComponents
{
    public class Animation : ICloneable
    {
        #region Fields and Properties
        public Rectangle[] Frames;
        Vector2[] objectOffset;
        int framesPerSecond;
        TimeSpan frameLength;
        public TimeSpan frameTimer;
        int currentFrame;
        string name;

        public event EventHandler<AnimationEndEventArgs> AnimationEnd;

        public int FramesPerSecond
        {
            get { return framesPerSecond; }
            set
            {
                if (value < 1)
                    framesPerSecond = 1;
                else if (value > 60)
                    framesPerSecond = 60;
                else
                    framesPerSecond = value;
                frameLength = TimeSpan.FromSeconds(1 / (double)framesPerSecond);
            }
        }
        public Rectangle CurrentFrameRect
        {
            get { return Frames[currentFrame]; }
        }
        public int CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                currentFrame = (int)MathHelper.Clamp(value, 0, Frames.Length - 1);
            }
        }
        public int FrameWidth { get { return Frames[currentFrame].Width; } }
        public int FrameHeight { get { return Frames[currentFrame].Height; } }
        public Vector2 ObjectOffset { get { return objectOffset[currentFrame]; } }

        #endregion

        #region Constructors

        // TODO: Distinguish between animating or only having one frame.
        // TODO: Robustify frames per second.
        public Animation(Rectangle[] frames, Vector2[] objectOffset = null, string name = "DefaultName")
        {
            this.Frames = frames;
            this.name = name;
            FramesPerSecond = 8;

            if (objectOffset == null)
            {    
                this.objectOffset = new Vector2[frames.Length];   
            }
            else
            {
                this.objectOffset = objectOffset;
            }
            Reset();
        }

        public Animation(Rectangle frame, Vector2 objectOffset = new Vector2(), string name = "DefaultName")
        {
            Rectangle[] oneRect = { frame };
            this.Frames = oneRect;
            this.name = name;
            FramesPerSecond = 8;

            Vector2[] oneVec = { objectOffset };
            this.objectOffset = oneVec;

            Reset();
        }

        private Animation(Animation animation, string name = "DefaultName")
        {
            this.Frames = animation.Frames;
            this.objectOffset = animation.objectOffset;
            this.name = name;
            FramesPerSecond = 8;
        }
        private Animation()
        {
        }

        #endregion

        #region Methods 

        public void ShiftFrames(Point displacement)
        {
            for (int i = 0; i < Frames.Length; i++)
            {
                Frames[i].X += displacement.X;
                Frames[i].Y += displacement.Y;
            }
        }

        public void Update(GameTime gameTime)
        {
            frameTimer += gameTime.ElapsedGameTime;
            if (frameTimer >= frameLength)
            {
                frameTimer = TimeSpan.Zero;
                currentFrame = (currentFrame + 1) % Frames.Length;
                if (currentFrame == 0)
                {
                    if (AnimationEnd != null) AnimationEnd(this, new AnimationEndEventArgs(name));
                }
            }
        }
        public void Reset()
        {
            currentFrame = 0;
            frameTimer = TimeSpan.Zero;
        }
        #endregion       
 
        #region ICloneable Members
        public object Clone()
        {
            Animation animationClone = new Animation();
            animationClone.Frames = this.Frames;
            animationClone.FramesPerSecond = this.framesPerSecond;
            animationClone.Reset();
            return animationClone;
        }
        #endregion
    }

}
