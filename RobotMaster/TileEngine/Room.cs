using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;

using RobotMaster.TileEngine;

namespace RobotMaster.TileEngine
{
    // A Room is a particular region of the level that the camera locks on to.
    // Every level is navigated by traversing a series of rooms.  A Room's height
    // will always be the Height of a single screen, but a Room's width may span
    // multiple screens.  A room has Exits, which allow the player to go to other
    // Rooms in the level.

    public class Room
    {
        /// <summary>
        /// Index to identify this Room out of the List of rooms for the Level.
        /// </summary>
        public int RoomNumber;

        /// <summary>
        /// Position of leftmost screen of this room in the level (tilemap)
        /// </summary>
        public Point RoomLocation;

        /// <summary>
        /// Room width in screens.
        /// </summary>
        public int RoomLength;

        /// <summary>
        /// All possible exits for this room.
        /// </summary>
        public List<Exit> Exits;

        public Vector2 Position
        {
            get
            {
                return new Vector2(RoomLocation.X * Engine.ViewportWidthTiles * Engine.TileWidth,
                                   RoomLocation.Y * Engine.ViewportHeightTiles * Engine.TileHeight);
            }
        }

        /// <summary>
        /// Get the dimension of the room in pixels.
        /// </summary>
        public Vector2 Size
        {
            get
            {
                // TODO: Some kind of RoomHeight, for vertically scrolling rooms (future?)
                return new Vector2(RoomLength * Engine.TileWidth * Engine.ViewportWidthTiles,
                                   Engine.TileHeight * Engine.ViewportHeightTiles);
            }
        }

        /// <summary>
        /// Define a Room in this level.
        /// </summary>
        /// <param name="location">Location of the room in the level in screens.  The upper-leftmost screen
        /// is position (0,0).  16 tiles to the right, or 256 pixels, lies the next screen, (1,0).</param>
        /// <param name="numScreens">Number of screens that this Room spans in width.  A 32-tile long room
        /// would be 2 screens wide.  A Room can only have a height of 1 screen.</param>
        /// <param name="exits">Exits out of this Room.</param>
        public Room(int roomNumber, Point location, int roomLength, List<Exit> exits)
        {
            this.RoomNumber = roomNumber;
            this.RoomLocation = location;
            this.RoomLength = roomLength;
            this.Exits = exits;
        }
    }

    [Serializable]
    public class SerializedRoom
    {
        /// <summary>
        /// Identify which Room in the level's List of Rooms this one is
        /// </summary>
        public int RoomNumber;

        /// <summary>
        /// Identify the name of the room given by the level editor
        /// </summary>
        public string RoomName;

        /// <summary>
        /// Identify which screen in the level this Room is located in
        /// </summary>
        public Point RoomLocation;

        /// <summary>
        /// Identify how many screens the room spans in length
        /// </summary>
        public int RoomWidth;

        /// <summary>
        /// Identify which screen in the room this Exit lies in
        /// </summary>
        public List<int> ExitScreenLocations;

        /// <summary>
        /// Identify which direction the room will scroll to
        /// </summary>
        public List<int> ExitDirections;

        /// <summary>
        /// Identify the destination Room that this exit leads to
        /// </summary>
        public List<int> ExitDestinationRooms;

        public SerializedRoom() { }
        public SerializedRoom(int roomNumber,
                              string roomname,
                              Point RoomLocation,
                              int roomWidth,
                              List<int> exitScreenLocations,
                              List<int> exitDirections,
                              List<int> exitDestinationRooms)
        {
            this.RoomNumber = roomNumber;
            this.RoomName = roomname;
            this.RoomLocation = RoomLocation;
            this.RoomWidth = roomWidth;
            this.ExitDestinationRooms = exitDestinationRooms;
            this.ExitDirections = exitDirections;
            this.ExitScreenLocations = exitScreenLocations;
        }
    }
}
