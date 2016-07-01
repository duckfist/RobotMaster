//#define WRITING_LEVEL

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RobotMaster.GameComponents;
using RobotMaster.TileEngine;
using RobotMaster.Entities;
using RobotMaster.Mathematics;
using RobotMaster.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RobotMaster.GameScreens
{
    public class GamePlayScreen : GameScreen
    {

        #region Fields and Properties

        ScreenManager manager;
        ControlManager Controls;
        SpriteFont DebugFont;
        

        #endregion

        #region Constructors

        public GamePlayScreen(Game game, ScreenManager screenManager)
            : base(game)
        {
            Content = Game.Content;
            manager = screenManager;
            Controls = new ControlManager();
        }

        #endregion

        #region Methods

        protected override void LoadContent()
        {
            // Wire up camera event for screen scrolling
            Session.Camera.FinishedPanning += camera_IsFinishedMoving;

            //Controls.Add(lblMegaPosition);
            //Controls.Add(lblCamPosition);

            // TODO: Load every other content
            Session.LoadContent(Content);
            Entity.LoadContent(Content);
            Bullet.LoadContent(Content);
            Enemy.LoadContent(Content);
            Obstacle.LoadContent(Content);
            TileMap.LoadContent();

            // Load player animations
            MegaMan.LoadContent(Content);

            DebugFont = Content.Load<SpriteFont>("Fonts\\Mega");

            // Load shaders
            //Session.Shader = Content.Load<Effect>("Shaders\\shader");
            // TEST: Swap out the blue color of the FallingPlatform
            //Session.Shader.Parameters["SwapColor1"].SetValue(new Color(254, 0, 0).ToVector4());
            //Session.Shader.Parameters["SwapColor2"].SetValue(new Color(0, 254, 0).ToVector4());
            //Session.Shader.Parameters["SwapWeapon"].SetValue(new Color(0, 0, 254).ToVector4());
            //Session.Shader.Parameters["MegaMan1"].SetValue(new Color(0, 112, 236).ToVector4());
            //Session.Shader.Parameters["MegaMan2"].SetValue(new Color(0, 232, 216).ToVector4());
            //Session.Shader.Parameters["Weapon"].SetValue(new Color(252, 228, 160).ToVector4());

            base.LoadContent();
        }

        public void StartLevel(int levelNum)
        {
#if WRITING_LEVEL

            // Load textures
            Texture2D tilesetTexture = Content.Load<Texture2D>(@"Tiles\Tiles_QuickMan");

            // Construct tileset and collision types
            TileCollision[] tilesetQuickCol = new TileCollision[48];
            for (int i = 0; i < tilesetQuickCol.Length; i++)
            {
                tilesetQuickCol[i] = TileCollision.Impassable;
            }
            tilesetQuickCol[14] = TileCollision.Passable;
            tilesetQuickCol[15] = TileCollision.Passable;
            tilesetQuickCol[16] = TileCollision.Passable;
            tilesetQuickCol[17] = TileCollision.Passable;
            tilesetQuickCol[19] = TileCollision.Passable;
            tilesetQuickCol[20] = TileCollision.Passable;
            tilesetQuickCol[21] = TileCollision.Passable;
            tilesetQuickCol[22] = TileCollision.Passable;
            tilesetQuickCol[23] = TileCollision.Passable;

            // Construct animation key
            int[] animations = new int[48];
            for (int i = 0; i < animations.Length; i++)
            {
                animations[i] = 1;
            }
            animations[12] = 12;
            Tileset tilesetQuick = new Tileset(tilesetTexture, 6, 8, 16, 16, tilesetQuickCol, animations);

            //tilesetTexture = Content.Load<Texture2D>(@"Tiles\Tiles_Items");
            //Tileset tilesetItems = new Tileset(tilesetTexture, 5, 8, 16, 16);

            List<Tileset> tilesets = new List<Tileset>();
            tilesets.Add(tilesetQuick);
            //tilesets.Add(tilesetItems);


            // Quickman map
            int[,] coolmap =
            { 
              { 2,13,16,17,15, 7,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18},
              { 3,13,22,23,15,10,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12},
              { 3,13,16,17,15,19,19,19,19,19,19,19,20,20,20,20,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19, 7},
              { 3,13,22,23,15,15,15,15,15,15,15,15,15,14,14,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15, 7},
              { 3,13,15,15,21,15,16,17,16,17,15,21,15,14,15,15,15,15,16,17,15,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21, 7},
              { 3,13,15,15,15,21,22,23,22,23,21,15,14,14,15,15,15,15,22,23,21,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15, 7},
              { 2,13,16,17,15,21,15,15,15,15,21,21,14,14,14,15,15,21,15,15,21,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15, 7},
              { 3,13,22,23,21,15,15,15,15,15,21,21,15,14,14,14,21,21,15,15,15,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21, 7},
              { 3,13,21,15,15,14,21,15, 6, 6, 6,15,15,14,14,14,15,15,21,15,15,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21, 7},
              { 3,13,15,21,14,14,15, 6, 6,15, 6, 6,14,14,14,15,15,15,15,21,21, 4, 6, 6,15,21,21,21,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15, 7},
              { 2,13,15,14,14,14,14,15, 6,21,15,14,14, 4, 6, 6, 6, 6, 6, 6, 6, 9,15, 6,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15, 7},
              { 3,13,14,14,14,14,14,14,21,15,14,14,14, 7,18,18, 2,13,15,15,15,15,15, 6,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15, 7},
              { 3, 8, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 9,18,18, 2,13,15,15,15,15,15, 6, 6, 6, 6,15,15,15, 4, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 9},
              { 3,18,18,18, 3, 3,18,18, 3,18,18,18, 3,18,18,18, 2, 6, 6,15,15,15,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, 
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15,15,15,15,15,15, 6, 6, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15, 6, 6, 6, 6,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,06,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15, 6,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,13,15,15,15,15,15,15,15,15,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2, 8, 6, 6, 6, 6, 6, 6, 6, 6,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
              {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2, 3,18,18,18, 3,18, 3,18,13,15,15,15,15, 7, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
            };
            int mapLengthX = 48;
            int mapLengthY = 28;


            // List of lists for serialization
            List<List<int>> serializedMap = new List<List<int>>();
            for (int i = 0; i < mapLengthY; i++)
            {
                List<int> row = new List<int>();
                for (int j = 0; j < mapLengthX; j++)
                {
                    row.Add(coolmap[i, j]);
                }
                serializedMap.Add(row);
            }


            // Quick man tiles, back layer
            MapLayer layerQuick = new MapLayer(mapLengthX, mapLengthY);
            for (int y = 0; y < layerQuick.Height; y++)
            {
                for (int x = 0; x < layerQuick.Width; x++)
                {
                    Tile tile = new Tile(coolmap[y, x], 0, new Point(x, y));
                    layerQuick.SetTile(x, y, tile);
                }
            }

            // Rooms
            List<SerializedRoom> serializedRooms = new List<SerializedRoom>();
            

            // Room1 (2 screens 2 exits)
            List<Exit> exits = new List<Exit>();   // Exits for first room
            Exit exit0 = new Exit(1, Direction.Right, 1); // second screen, travel right to second room
            Exit exit1 = new Exit(1, Direction.Down, 2); // second screen, travel down to third room
            exits.Add(exit0);
            exits.Add(exit1);
            List<int> exitScreenLocations = new List<int>();
            exitScreenLocations.Add(exit0.Screen);
            exitScreenLocations.Add(exit1.Screen);
            List<int> exitDirections = new List<int>();
            exitDirections.Add((int)exit0.Direction);
            exitDirections.Add((int)exit1.Direction);
            List<int> exitDestinationRooms = new List<int>();
            exitDestinationRooms.Add(exit0.Destination);
            exitDestinationRooms.Add(exit1.Destination);
            SerializedRoom room0 = new SerializedRoom(0, new Point(0, 0), 2, exitScreenLocations, exitDirections, exitDestinationRooms);

            // Room2
            exits = new List<Exit>();   // Exits for second room
            exit0 = new Exit(0, Direction.Left, 0); // second screen, travel left to first room
            exits.Add(exit0);
            exitScreenLocations = new List<int>();
            exitScreenLocations.Add(exit0.Screen);
            exitDirections = new List<int>();
            exitDirections.Add((int)exit0.Direction);
            exitDestinationRooms = new List<int>();
            exitDestinationRooms.Add(exit0.Destination);
            SerializedRoom room1 = new SerializedRoom(1, new Point(2, 0), 1, exitScreenLocations, exitDirections, exitDestinationRooms);

            // Room3
            exits = new List<Exit>();   // Exits for second room
            exit0 = new Exit(0, Direction.Up, 0); // first screen, travel up to first room
            exits.Add(exit0);
            exitScreenLocations = new List<int>();
            exitScreenLocations.Add(exit0.Screen);
            exitDirections = new List<int>();
            exitDirections.Add((int)exit0.Direction);
            exitDestinationRooms = new List<int>();
            exitDestinationRooms.Add(exit0.Destination);
            SerializedRoom room2 = new SerializedRoom(2, new Point(1, 1), 1, exitScreenLocations, exitDirections, exitDestinationRooms);

            serializedRooms.Add(room0);
            serializedRooms.Add(room1);
            serializedRooms.Add(room2);

            // Stack layers and give to tilemap
            List<MapLayer> layers = new List<MapLayer>();
            layers.Add(layerQuick);
            //layers.Add(layerItems);
                        
            // Serialize level features for loading later
            SerializedLevel serializedLevel = new SerializedLevel();
            serializedLevel.TilesWide = 6;
            serializedLevel.TilesHigh = 8;
            serializedLevel.TileWidth = 16;
            serializedLevel.TileHeight = 16;
            serializedLevel.TexturePath = @"Tiles\Tiles_QuickMan";
            serializedLevel.TilesetAnimationKey = new List<int>();
            serializedLevel.TilesetCollisionKey = new List<int>();
            for (int i = 0; i < tilesetQuickCol.Length; i++)
            {
                serializedLevel.TilesetCollisionKey.Add((int)tilesetQuickCol[i]);
                serializedLevel.TilesetAnimationKey.Add(animations[i]);
            }
            serializedLevel.Map = serializedMap;
            serializedLevel.Rooms = serializedRooms;


            Level.Write(0, serializedLevel);

            Session.Level = new Level(0);
            Session.Level.LoadContent(Content);
            Session.CurrentMap = Session.Level.map;

#else
            Session.Level = new Level(levelNum);
            Session.Level.LoadContent(Content);
            Session.CurrentMap = Session.Level.map;
#endif

            // prepare MEGA MAN's spawn position!
            Point spawnP = new Point(Session.CurrentMap.CurrentRoom.RoomLocation.X * Engine.ViewportWidthTiles + Session.CurrentMap.SpawnTile.X,
                Session.CurrentMap.CurrentRoom.RoomLocation.Y * Engine.ViewportHeightTiles + Session.CurrentMap.SpawnTile.Y);
            Vector2 spawn = Engine.CellToVector(spawnP);
            spawn.X += (Engine.TileWidth - (MegaMan.Width / 2f));
            spawn.Y += ((Engine.TileHeight * 2) - MegaMan.Height);
            Session.MegaMan = new MegaMan(spawn);

            Session.HPMeter = new MeterDisplay(Session.MegaMan.HitPoints, new Vector2(20, 20), MeterDisplay.MeterColor.Buster);
            Session.WeaponMeter = new MeterDisplay(new AttributePair(28,14), new Vector2(28, 20), MeterDisplay.MeterColor.Weapon);
        }

        public override void Update(GameTime gameTime)
        {
            //Session.audioEngine.Update();
            HandleInput(gameTime);

            if (Session.DebugPause && !Session.DebugFrameIsAdvancing) return;

            if (!Session.IsScrolling)
            {
                // Follow Mega with the camera
                Session.Camera.LockToGameObject(Session.MegaMan);

                // Lock Camera to tilemap's boundaries
                Session.Camera.LockCamera();
            }
            
            
            if (!BossDoor.DoorPause)
            {
                Session.Camera.Update();

                // Position and udpate sprite animation state when doors aren't animating
                Session.MegaMan.Udpate(gameTime);
            }

            foreach (Enemy e in Session.Level.Enemies)
            {
                e.BulletCollisions(Session.MegaMan.Bullets);
                e.Update(gameTime);
            }
            foreach (Obstacle p in Session.Level.Obstacles)
            {
                p.Update(gameTime);
            }
            foreach (MMEffect ef in Session.Level.Effects)
            {
                ef.Update(gameTime);

            }

            
            base.Update(gameTime);

            Session.DebugFrameIsAdvancing = false;
        }

        public void HandleInput(GameTime gameTime)
        {
            // Exit Game
            if (InputManager.KeyReleased(Keys.Escape))
                manager.ChangeScreens(GameRef.StartScreen);

            // Debug Mode: Frame advance
            if (Session.DebugMode && Session.DebugPause)
            {
                if (InputManager.KeyPressed(Keys.OemPipe))
                {
                    Session.DebugFrameIsAdvancing = true;
                }
                else
                {
                    return;
                }
            }

            // Don't allow certain input changes while scrolling
            if (!Session.IsScrolling)
            {
                // Grab input, configure Mega Man's state
                if (InputManager.KeyDown(Keys.Left) ||
                    InputManager.ButtonDown(PlayerIndex.One, Buttons.LeftThumbstickLeft))
                {
                    if (!Session.MegaMan.IsClimbing)
                        Session.MegaMan.Velocity.X = -MegaMan.WalkVelocity;
                    else
                    {
                        Session.MegaMan.Velocity.X = 0.0f;
                    }
                    Session.MegaMan.IsFacingLeft = true;
                }
                else if (InputManager.KeyDown(Keys.Right) ||
                    InputManager.ButtonDown(PlayerIndex.One, Buttons.LeftThumbstickRight))
                {
                    if (!Session.MegaMan.IsClimbing)
                        Session.MegaMan.Velocity.X = MegaMan.WalkVelocity;
                    else
                    {
                        Session.MegaMan.Velocity.X = 0.0f;
                    }
                    Session.MegaMan.IsFacingLeft = false;
                }
                else Session.MegaMan.Velocity.X = 0.0f;

                // Climbing up and down
                if (InputManager.KeyPressed(Keys.Down))
                {
                    Session.MegaMan.TryGrabLadder(false);       // On ground climb down ladder
                }
                else if (InputManager.KeyPressed(Keys.Up))
                {
                    Session.MegaMan.TryGrabLadder(true);        // On ground climb up ladder
                }
                else if (InputManager.KeyDown(Keys.Up))
                {
                    if (Session.MegaMan.IsClimbing)
                    {
                        if (Session.MegaMan.IsShooting)
                            Session.MegaMan.Velocity.Y = 0.0f;
                        else
                            Session.MegaMan.Velocity.Y = -MegaMan.ClimbVelocity;
                    }
                    else if (Session.MegaMan.IsFalling)
                        Session.MegaMan.TryGrabLadder(true);    // Mid-air climb up ladder
                }
                else if (InputManager.KeyDown(Keys.Down))
                {
                    if (Session.MegaMan.IsClimbing)
                    {
                        if (Session.MegaMan.IsShooting)
                            Session.MegaMan.Velocity.Y = 0.0f;
                        else
                            Session.MegaMan.Velocity.Y = MegaMan.ClimbVelocity;
                    }
                }
                else if (Session.MegaMan.IsClimbing) Session.MegaMan.Velocity.Y = 0.0f;

                // Grab Jump input TODO: Gamepad
                if (InputManager.KeyDown(Keys.A) &&
                    !InputManager.KeyDown(Keys.Up) &&
                    !InputManager.KeyDown(Keys.Down) &&
                    Session.MegaMan.IsClimbing)
                {
                    Session.MegaMan.IsClimbing = false;
                }
                else if (InputManager.KeyDown(Keys.A) &&
                    Session.MegaMan.IsAbleToJump &&
                    !Session.MegaMan.IsJumping &&
                    !Session.MegaMan.IsFalling)
                {
                    Session.MegaMan.IsFalling = true;
                    Session.MegaMan.Velocity.Y = -MegaMan.JumpVelocity;
                    Session.MegaMan.IsJumping = true;
                }
                if (!InputManager.KeyDown(Keys.A) &&
                    Session.MegaMan.IsJumping)
                {
                    Session.MegaMan.IsJumping = false;
                    Session.MegaMan.IsFalling = true;
                    Session.MegaMan.Velocity.Y = 0.0f;
                }
                
                if (InputManager.KeyPressed(Keys.S))
                {
                    Session.MegaMan.Shoot();
                }
                if (InputManager.KeyPressed(Keys.D))
                {
                    manager.PushScreen(GameRef.PauseScreen);
                }
                if (InputManager.KeyPressed(Keys.OemPeriod))
                {
                    Session.MegaMan.NextWeapon(true);
                }
                if (InputManager.KeyPressed(Keys.OemComma))
                {
                    Session.MegaMan.NextWeapon(false);
                }
                if (InputManager.KeyPressed(Keys.J))
                {
                    Session.MegaMan.HitPoints.Deplete(1);
                }
                if (InputManager.KeyPressed(Keys.K))
                {
                    Session.MegaMan.HitPoints.ReplenishAll();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw Tilemap without a shader
            Game1.SpriteBatch.Begin(
                SpriteSortMode.Deferred,  // waits until .End() is called to draw
                BlendState.AlphaBlend,
                SamplerState.PointClamp,// Necessary for smooth scaling!
                null,
                null,
                null,
                Matrix.Multiply(Session.Camera.TransformMatrix, Game1.Scale));
            // Draw the tilemap and background
            Session.DrawMap(Game1.SpriteBatch);
            Game1.SpriteBatch.End();

            // Draw Mega Man using shader
            Game1.SpriteBatch.Begin(
                SpriteSortMode.Deferred,  // waits until .End() is called to draw
                BlendState.AlphaBlend,
                SamplerState.PointClamp,// Necessary for smooth scaling!
                null,
                null,
                Session.Shader,
                Matrix.Multiply(Session.Camera.TransformMatrix, Game1.Scale));
            Session.MegaMan.Draw(gameTime, Game1.SpriteBatch);
            Game1.SpriteBatch.End();

            // Draw all objects
            Game1.SpriteBatch.Begin(
                SpriteSortMode.Deferred,  // waits until .End() is called to draw
                BlendState.AlphaBlend,
                SamplerState.PointClamp,// Necessary for smooth scaling!
                null,
                null,
                null,
                Matrix.Multiply(Session.Camera.TransformMatrix, Game1.Scale));
            
            foreach (Enemy e in Session.Level.Enemies)
            {
                e.Draw(gameTime, Game1.SpriteBatch);
            }

            foreach (Obstacle p in Session.Level.Obstacles)
            {
                p.Draw(gameTime, Game1.SpriteBatch);
            }

            foreach (MMEffect ef in Session.Level.Effects)
            {
                ef.Draw(gameTime, Game1.SpriteBatch);
            }

            Session.HPMeter.Draw(Game1.SpriteBatch, Session.MegaMan.HitPoints);
            
            if (Session.DebugMode)
            {
                //Controls.Draw(Game1.SpriteBatch);
                //Game1.SpriteBatch.DrawString(DebugFont, String.Format("{0}", GC.GetTotalMemory(false) / 1000) /* in kilobytes */, new Vector2(25, 30) + Session.Camera.Position, Color.White);
            }
            Game1.SpriteBatch.End();
        }

        public override string ToString()
        {
            return "GamePlayScreen";
        }

        #endregion

        #region Events

        /// <summary>
        /// Move to an adjacent room in the current level.
        /// </summary>


        public void camera_IsFinishedMoving(object o, EventArgs args)
        {
            Session.IsScrolling = false;
        }

        #endregion

    }
}
