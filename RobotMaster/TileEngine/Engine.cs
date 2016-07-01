using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RobotMaster.Mathematics;

namespace RobotMaster.TileEngine
{
    public class Engine
    {
        #region Fields and Properties

        static int tileWidth;
        static int tileHeight;
        static int viewportWidth;
        static int viewportHeight;

        public static int TileWidth
        {
            get { return tileWidth; }
        }
        public static int TileHeight
        {
            get { return tileHeight; }
        }
        public static int ViewportWidth
        {
            get { return viewportWidth; }
        }
        public static int ViewportHeight
        {
            get { return viewportHeight; }
        }
        public static int ViewportWidthTiles
        {
            get { return viewportWidth / tileWidth; }
        }
        public static int ViewportHeightTiles
        {
            get { return viewportHeight / tileHeight; }
        }

        public static Vector2 TileSizeVector
        {
            get { return new Vector2(tileWidth, tileHeight); }
        }
        public static Vector2 ViewportVector
        {
            get { return new Vector2(viewportWidth, viewportHeight); }
        }

        #endregion

        #region Constructors
        public Engine(int tileWidth, int tileHeight, Game game)
        {
            Engine.tileWidth = tileWidth;
            Engine.tileHeight = tileHeight;
            viewportHeight = Game1.WINDOW_HEIGHT;
            viewportWidth = Game1.WINDOW_WIDTH;
        }

        #endregion

        #region Methods

        public static Point VectorToCell(Vector2 position)
        {
            return new Point((int)position.X / tileWidth, (int)position.Y / tileHeight);
        }
        public static Vector2 CellToVector(Point cell)
        {
            return new Vector2(cell.X * tileWidth, cell.Y * tileHeight);
        }

        public static FloatRect GetTileRect(int x, int y)
        {
            return new FloatRect(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
        }
        public static FloatRect GetTileRect(Vector2 pos)
        {
            return new FloatRect((int)(pos.X / tileWidth) * tileWidth, (int)(pos.Y / tileHeight) * tileHeight, tileWidth, tileHeight);
        }
        #endregion
    }

}
