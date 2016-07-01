using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace RobotMaster.Mathematics
{
    public class MegaMath
    {
        public static Vector2 Round(Vector2 v)
        {
            Vector2 r = Vector2.Zero;
            r.X = (float)Math.Round((double)v.X);
            r.Y = (float)Math.Round((double)v.Y);
            return r;
        }

        public static Vector2 Floor(Vector2 v)
        {
            Vector2 r = Vector2.Zero;
            r.X = (float)Math.Floor((double)v.X);
            r.Y = (float)Math.Floor((double)v.Y);
            return r;
        }

        public static Vector2 UnitVector(double piRadians)
        {
            return new Vector2((float)Math.Sin(piRadians), (float)Math.Cos(piRadians));
        }

        public static Vector2 Midpoint(Vector2 v1, Vector2 v2)
        {
            return (v1 + v2) / 2;
        }

        public static Texture2D CropTexture2D(Texture2D source, Rectangle area)
        {
            if (source == null)
                return null;

            Texture2D cropped = new Texture2D(source.GraphicsDevice, area.Width, area.Height);
            Color[] data = new Color[source.Width * source.Height];
            Color[] cropData = new Color[cropped.Width * cropped.Height];

            source.GetData<Color>(data);

            int index = 0;
            for (int y = area.Y; y < area.Y + area.Height; y++)
            {
                for (int x = area.X; x < area.X + area.Width; x++)
                {
                    cropData[index] = data[x + (y * source.Width)];
                    index++;
                }
            }

            cropped.SetData<Color>(cropData);

            return cropped;
        }
    }

    public struct Line
    {
        Vector2 v1;
        Vector2 v2;

        public Point p1
        {
            get { return new Point((int)v1.X, (int)v1.Y); }
        }

        public Point p2
        {
            get { return new Point((int)v2.X, (int)v2.Y); }
        }

        public Line(Vector2 v1, Vector2 v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public Line(Point p1, Point p2)
        {
            this.v1 = new Vector2(p1.X, p1.Y);
            this.v2 = new Vector2(p2.X, p2.Y);
        }

        // Assumes v1's X and Y are less than v2's
        public bool IntersectAxial(FloatRect rect)
        {
            if (v1.X == v2.X)
            {
                if (rect.Right >= v1.X && rect.Left <= v2.X &&
                    rect.Bottom >= v1.Y && rect.Top <= v2.Y)
                {
                    return true;
                }
            }

            else if (v1.Y == v2.Y)
            {
                if (rect.Left <= v2.X && rect.Right >= v1.X &&
                    rect.Top <= v1.Y && rect.Bottom >= v1.Y)
                {
                    return true;
                }
            }

            return false;
        }

        // TODO: Lazy
        //public bool Intersects(FloatRect rect)
        //{

        //}
    }

    public struct FloatRect
    {
        float x;
        float y;
        float width;
        float height;

        public Vector2 Position 
        { 
            get 
            { 
                return new Vector2(x, y); 
            }

            set
            {
                x = value.X;
                y = value.Y;
            }
        }
        public float Width { get { return width; } set { width = value; } }
        public float Height { get { return height; } set { height = value; } }
        public bool HasArea { get { return height > 0.0f && width > 0.0f; } }
        public float Left { get { return x; } }
        public float Top { get { return y; } }
        public float Right { get { return x + width; } }
        public float Bottom { get { return y + height; } }
        public float CenterX { get { return (2 * x + width) / 2; } }
        public float CenterY { get { return (2 * y + height) / 2; } }

        public FloatRect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            if (height < 0.0f) height = 0.0f;
            if (width < 0.0f) width = 0.0f;
        }

        public FloatRect(Vector2 pos, float width, float height) 
            : this(pos.X, pos.Y, width, height) { }

        public bool Intersects(FloatRect rect)
        {
            bool bottomEdge = this.y + height > rect.y;
            bool topEdge = this.y < rect.y + rect.height;
            bool leftEdge = this.x < rect.x + rect.width;
            bool rightEdge = this.x + width > rect.x;

            return (bottomEdge && topEdge && leftEdge && rightEdge);
        }

        public bool Intersects(Vector2 v)
        {
            return (v.X > this.x && v.X < Right && v.Y > this.y && v.Y < Bottom);
        }

        public FloatRect Intersection(FloatRect rect)
        {
            return (Intersection(this, rect));
        }

        public static FloatRect Intersection(FloatRect r1, FloatRect r2)
        {
            // Right-most left border of r2s
            float leftBorder = (r1.x > r2.x) ? r1.x : r2.x;

            // Left-most right border of r2s
            float rightBorder = (r1.x + r1.width < r2.x + r2.width) ? r1.x + r1.width : r2.x + r2.width;

            // Bottom-most top border of r2s
            float topBorder = (r1.y > r2.y) ? r1.y : r2.y;

            // Top-most bottom border of r2s
            float bottomBorder = (r1.y + r1.height < r2.y + r2.height) ? r1.y + r1.height : r2.y + r2.height;

            return new FloatRect(leftBorder, topBorder, rightBorder - leftBorder, bottomBorder - topBorder);
        }

        public static Vector2 GetIntersectionDepth(FloatRect rectA, FloatRect rectB)
        {
            // Calculate half sizes.
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // Calculate centers.
            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            // Calculate current and minimum-non-intersecting distances between centers.
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // If we are not intersecting at all, return (0, 0).
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            // Calculate and return intersection depths.
            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        public Vector2 Size
        {
            get
            {
                return new Vector2(Width, Height);
            }
        }

        public Vector2 Center
        {
            get
            {
                return Position + Size / 2;
            }
        }

        /// <summary>
        /// Gets the position of the center of the bottom edge of the rectangle.
        /// </summary>
        public static Vector2 GetBottomCenter(FloatRect rect)
        {
            return new Vector2(rect.Left + rect.Width / 2.0f, rect.Bottom);
        }

        public static FloatRect operator +(FloatRect rect, Vector2 translation)
        {
            return new FloatRect(rect.Left + translation.X, rect.Top + translation.Y, rect.Width, rect.Height);
        }
    }

    public class FloatRectClass
    {
        private FloatRect rectangle;

        public FloatRect FloatRect { get { return rectangle; } }
        public Vector2 Position { get { return rectangle.Position; } set { rectangle.Position = new Vector2(value.X, value.Y); } }
        public float Width { get { return rectangle.Width; } set { rectangle.Width = value; } }
        public float Height { get { return rectangle.Height; } set { rectangle.Height = value; } }
        public float Left { get { return rectangle.Left; } }
        public float Top { get { return rectangle.Top; } }
        public float Right { get { return rectangle.Left + rectangle.Width; } }
        public float Bottom { get { return rectangle.Top + rectangle.Height; } }
        public float CenterX { get { return (2 * rectangle.Left + rectangle.Width) / 2; } }
        public float CenterY { get { return (2 * rectangle.Top + rectangle.Height) / 2; } }

        public FloatRectClass(FloatRect rect)
        {
            this.rectangle = rect;
        }


    }

    public enum Direction
    {
        Left,
        Up,
        Right,
        Down
    }
}
