using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RobotMaster.Entities;
using RobotMaster.GameComponents;
using RobotMaster.Mathematics;

namespace RobotMaster.TileEngine
{
    public delegate void CameraEventHandler(object sender, EventArgs e);

    public class Camera
    {

        #region Fields and Properties

        public event CameraEventHandler FinishedPanning; 
        public Vector2 Position;
        public Vector2 Destination;
        public Direction ScrollDirection = Direction.Left;
        float speed = 4f;

        public float Speed
        {
            get { return speed; }
            set { speed = MathHelper.Clamp(speed, 1.0f, 16.0f); }
        }

        public Matrix TransformMatrix
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(-Position, 0f));
            }
        }

        public Vector2 PanVelocity
        {
            get
            {
                Vector2 vel = Vector2.Zero;
                switch (ScrollDirection)
                {
                    case Direction.Left:
                        vel = new Vector2(speed * -1.0f, 0f);
                        break;
                    case Direction.Up:
                        vel = new Vector2(0, speed * -1.0f);
                        break;
                    case Direction.Right:
                        vel = new Vector2(speed, 0f);
                        break;
                    case Direction.Down:
                        vel = new Vector2(0f, speed);
                        break;
                }
                return vel;
            }
        }

        #endregion

        #region Constructors

        public Camera()
        {
            Position = Vector2.Zero;
            Destination = Vector2.Zero;
        }

        public Camera(Vector2 position)
        {
            Position = position;
            Destination = Vector2.Zero;
        }

        #endregion

        #region Methods

        public void PanCamera()
        {
            Vector2 dest = Position - Destination;
            //if (dest.Length() >= 0.001f)
            //{
                
            //    Position += (PanVelocity * dest);
            //}
            //else
            //{
                dest.Normalize();
                Position += (PanVelocity);
            //}

            Vector2 pos = Position;
            pos.Normalize();
            Vector2 diff = Destination - Position;

            // Finished moving, fire event
            if (diff.Length() < PanVelocity.Length())
            {
                Position = Destination;
                OnFinishedPanning(EventArgs.Empty);
            }
        }

        public void PanCamera(Vector2 velocity)
        {
            Position += velocity;
        }

        //public void PanCamera(float velocity, Vector2 Destination)
        //{
        //    Vector2 dest = Destination;
        //    dest.Normalize();
        //    Position += (velocity * dest);

        //    Vector2 pos = Position;
        //    pos.Normalize();
        //    Vector2 diff = Destination - Position;

        //    // Finished moving, fire event
        //    if (diff.Length() <= velocity)
        //    {
        //        Position = Destination;
        //        OnFinishedPanning(EventArgs.Empty);
        //    }
        //}

        private void OnFinishedPanning(EventArgs e)
        {
            if (FinishedPanning != null)
                FinishedPanning(this, e);
        }

        public void LockCamera()
        {
            Position.X = MathHelper.Clamp(Position.X,
                Session.CurrentRoom.Position.X,
                Session.CurrentRoom.Position.X + Session.CurrentRoom.Size.X - Engine.ViewportWidth);
            Position.Y = MathHelper.Clamp(Position.Y,
                Session.CurrentRoom.Position.Y,
                Session.CurrentRoom.Position.Y + Session.CurrentRoom.Size.Y - Engine.ViewportHeight);

            //Position = MegaMath.Round(Position);
        }

        public void LockToSprite(AnimatedSprite sprite)
        {
            Position.X = sprite.Position.X + sprite.Width / 2
                         - (Engine.ViewportWidth / 2);
            Position.Y = sprite.Position.Y + sprite.Height / 2
                         - (Engine.ViewportHeight / 2);

            //Position = MegaMath.Round(Position);
        }

        // TODO: Make it take in an interface that forces position on an object
        public void LockToGameObject(MegaMan mega)
        {
            Position.X = mega.Position.X + MegaMan.Width / 2
                         - (Engine.ViewportWidth / 2);
            Position.Y = mega.Position.Y + MegaMan.Height / 2
                         - (Engine.ViewportHeight / 2);

            // Uncomment this to have "smooth" motion
            Position = MegaMath.Floor(Position);
            
        }

        public void Update()
        {
            if (Session.IsScrolling)
            {
                PanCamera();
            }
        }

        #endregion
    }
}
