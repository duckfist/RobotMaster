using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RobotMaster.Mathematics;
using RobotMaster.GameComponents;

namespace RobotMaster.TileEngine
{
    public class Tile
    {
        #region Fields and Properties Region
        int tileIndex;
        int tileset;
        bool isCollidable;
        Point location;

        

        private bool standingOn; // debug mode stuff

        public TileCollision Collision
        {
            get 
            {
                if (tileIndex == -1) return TileCollision.Passable;
                return Session.CurrentMap.Tilesets[tileset].CollisionTypes[tileIndex]; 
            }
        }

        public int TileIndex
        {
            get { return tileIndex; }
            private set { tileIndex = value; }
        }
        public int Tileset
        {
            get { return tileset; }
            private set { tileset = value; }
        }
        public bool IsCollidable
        {
            get
            {
                TileCollision c = Collision;
                switch (c)
                {
                    case TileCollision.Impassable:
                        return true;
                    case TileCollision.ConveyorLeft:
                        return true;
                    case TileCollision.ConveyorRight:
                        return true;
                    default: return false;
                }
            }
        }
        public Point Location
        {
            get { return location; }
            private set { location = value; }
        }
        public bool StandingOn
        {
            get { return standingOn; }
            set { standingOn = value; }
        }
        public FloatRect HitBox
        {
            get { return new FloatRect(location.X * Engine.TileWidth, location.Y * Engine.TileHeight, Engine.TileWidth, Engine.TileHeight); }
        }

        public static readonly Vector2 Size = new Vector2(Engine.TileWidth, Engine.TileHeight);
        public static float ConveyorVelocity = 1.0f;

        #endregion

        #region Constructor Region
        public Tile(int tileIndex, int tileSet, Point location)
        {
            this.tileIndex = tileIndex;
            this.tileset = tileSet;
            this.location = location;
            standingOn = false;

        }
        #endregion

        public static void LoadContent()
        {

        }
    }

    public enum TileCollision
    {
        Passable = 0,
        Impassable = 1,
        Platform = 2,
        Ladder = 3,
        ConveyorLeft = 4,
        ConveyorRight = 5,
    }

}
