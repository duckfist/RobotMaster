using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;
using System.Xml.Serialization;
using System.Windows.Threading;
using System.IO;

using RobotMaster;
using RobotMaster.TileEngine;
using RobotMaster.Entities;

namespace MMEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly int ViewportWidth = 256;
        public static readonly int ViewportHeight = 224;
        public static int ScaleTileset = 2;
        public static int ScaleTilemap = 3;

        public static int RoomIndexOffset = 0;


        int animationFrame = 0;
        DispatcherTimer dt;

        public SerializedLevel serialized = new SerializedLevel();
        public List<int[,]> roomMaps = new List<int[,]>(); // Arrays of maps for each room
        public List<int[,]> roomEnemies = new List<int[,]>(); // Arrays of enemy positions for each room
        public List<int[,]> roomObstacles = new List<int[,]>(); // Arrays of obstacle positions

        public NewTileset NewLevelWizard;
        public VisibleGrid vgrdTileset;                  // Visible, clickable Grid for the Tileset
        public Image[,] imgTilesInGrid = new Image[1,1]; // Images for each tile used in grdRoomViewer
        public Image[,] imgEnemiesInGrid = new Image[1, 1]; // Images for each enemy used in grdRoomViewer
        public Image[,] imgObstaclesInGrid = new Image[1, 1];
        public List<System.Windows.Shapes.Rectangle> screenRects = new List<System.Windows.Shapes.Rectangle>();
        public List<RadioButton> collisionButtons = new List<RadioButton>();

        public bool SetSpawnPointMode = false;
        public bool IsLevelLoaded = false;
        public Layers CurrentLayer = Layers.TileLayer;

        public int SelectedRoomIndex
        {
            get { return lbxRooms.SelectedIndex; }
        }
        public int SelectedExitIndex
        {
            get { return lbxExits.SelectedIndex; }
        }
        public int SelectedTilesetX { get { return vgrdTileset.SelectedColumn; } }
        public int SelectedTilesetY { get { return vgrdTileset.SelectedRow; } }
        public int ScreenTilesX { get { return (ViewportWidth / serialized.TileWidth); } }    // Screen width in tiles
        public int ScreenTilesY { get { return (ViewportHeight / serialized.TileHeight); } }  // Screen height in tiles

        public MainWindow()
        {
            InitializeComponent();

            // Add custom controls to their respective containers
            vgrdTileset = new VisibleGrid(1, 1, 1, 1);
            grdTilesetPalette.Children.Add(vgrdTileset);
            imgTilesInGrid[0, 0] = new Image();
            imgEnemiesInGrid[0, 0] = new Image();
            imgObstaclesInGrid[0, 0] = new Image();

            // Save radio buttons to a convenient list
            collisionButtons.Add(radPassable);
            collisionButtons.Add(radImpassable);
            collisionButtons.Add(radPlatform);
            collisionButtons.Add(radLadder);
            collisionButtons.Add(radConveyorLeft);
            collisionButtons.Add(radConveyorRight);

            // Start animation thread
            dt = new DispatcherTimer();
            dt.Tick += new EventHandler(AnimateStuff);
            dt.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dt.Start();

            EditorLevel.UnsavedChanges = false;
        }

        #region Event Handlers - Toolbar and Menu

        private void menNewLevel_Click(object sender, RoutedEventArgs e)
        {
            // Prompt user to save before opening a new level
            if (EditorLevel.UnsavedChanges)
            {
                UnsavedChanges window = new UnsavedChanges(this);
                switch (window.ShowCustomDialog())
                {
                    case SaveResult.Yes:
                        // Try to save before opening new level: If user cancels the SaveLevelDialog, this UnsavedChanges dialog is also canceled.
                        bool? saveDialog = SaveLevelDialog();
                        if (saveDialog == false)
                        {
                            return;
                        }
                        break;
                    case SaveResult.No:
                        // User doesn't want to save, proceed with new level and discard changes.
                        break;
                    case SaveResult.Cancel:
                        // User doesn't want to save, but doesn't want to open a new level anymore.
                        return;
                }
            }

            NewLevelWizard = new NewTileset(serialized);
            NewLevelWizard.ShowDialog();

            // Has gone through the wizard and set up a new level
            if (NewLevelWizard.DialogResult == true)
            {
                UnloadLevel();

                // TODO: Have starting values reflect that which was selected in the dialog
                // Set up simple starting tilemap in the first room
                int[,] firstRoom = new int[16, 14];
                int[,] firstRoomEnemies = new int[16, 14];
                int[,] firstRoomObstacles = new int[16, 14];
                for (int y = 0; y < 14; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        firstRoom[x, y] = -1;
                        firstRoomEnemies[x, y] = -1;
                        firstRoomObstacles[x, y] = -1;
                    }
                }
                roomMaps.Add(firstRoom);
                roomEnemies.Add(firstRoomEnemies);
                roomObstacles.Add(firstRoomObstacles);

                // Load rooms into UI elements, select first room (populating the grid)
                LoadRoomsAndTileset();
                DisplayTilemapGrid();
                IsLevelLoaded = true;
                tbxStatus.Text = String.Format("Created new level");
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void menOpenLevel_Click(object sender, RoutedEventArgs e)
        {
            // Prompt user to save before opening a new level
            if (EditorLevel.UnsavedChanges)
            {
                UnsavedChanges window = new UnsavedChanges(this);
                switch (window.ShowCustomDialog())
                {
                    case SaveResult.Yes:
                        // Try to save before opening new level: If user cancels the SaveLevelDialog, this UnsavedChanges dialog is also canceled.
                        bool? saveDialog = SaveLevelDialog();
                        if (saveDialog == false)
                        {
                            return;
                        }
                        break;
                    case SaveResult.No:
                        // User doesn't want to save, proceed with new level and discard changes.
                        break;
                    case SaveResult.Cancel:
                        // User doesn't want to save, but doesn't want to open a new level anymore.
                        return;
                }
            }

            // Open file browser to select a tileset image
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.CurrentDirectory + "\\Levels";
            dlg.FileName = "level0.xml";
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Map Files (.xml)|*.xml";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                String path = dlg.FileName;
                SerializedLevel level = null;

                // Deserialize level from file
                try
                {
                    FileStream stream = new FileStream(path, System.IO.FileMode.Open);
                    XmlSerializer serializer = new XmlSerializer(typeof(SerializedLevel));
                    level = (SerializedLevel)serializer.Deserialize(stream);
                    stream.Close();
                    serialized = level;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Oh no! Corrupted map file.");
                    return;
                }

                UnloadLevel();
                LoadLevel();
                tbxStatus.Text = String.Format("Opened level \"{0}\"", dlg.SafeFileName);
                EditorLevel.UnsavedChanges = false;
            }
        }

        private bool? SaveLevelDialog()
        {
            dlgSaveLevel save = new dlgSaveLevel();
            bool? result = save.ShowDialog();
            if (result == true)
            {
                // Save copy of tileset image
                string tilesetFileName = System.IO.Path.GetFileName(serialized.TexturePath);
                string targetTilesetPath = "Content\\Tiles";
                string targetFile = System.IO.Path.Combine(targetTilesetPath, tilesetFileName);
                if (!Directory.Exists(targetTilesetPath))
                {
                    Directory.CreateDirectory(targetTilesetPath);
                }
                if (serialized.TexturePath != targetFile && !serialized.TexturePath.Contains(targetFile))
                {
                    File.Copy(serialized.TexturePath, targetFile, true);
                }

                // Point texture path to new copy
                serialized.TexturePath = targetFile;

                // Find the bounds for the entire level
                int maxTilemapHeight = 0;
                int maxTilemapWidth = 0;
                int rightMostTile = 0;
                int leftMostTile = 0;
                int topMostTile = 0;
                int bottomMostTile = 0;
                int leftMostScreen = 0;
                int topMostScreen = 0;
                for (int i = 0; i < roomMaps.Count; i++)
                {
                    SerializedRoom room = serialized.Rooms[i];
                    int roomRight = (room.RoomWidth + room.RoomLocation.X) * ScreenTilesX;
                    int roomBottom = (1 + room.RoomLocation.Y) * ScreenTilesY;
                    int roomLeft = room.RoomLocation.X * ScreenTilesX;
                    int roomTop = room.RoomLocation.Y * ScreenTilesX;

                    if (roomRight > rightMostTile) rightMostTile = roomRight;
                    if (roomBottom > bottomMostTile) bottomMostTile = roomBottom;
                    if (roomLeft < leftMostTile) leftMostTile = roomLeft;
                    if (roomTop < topMostTile) topMostTile = roomTop;
                }
                maxTilemapWidth = rightMostTile - leftMostTile;
                maxTilemapHeight = bottomMostTile - topMostTile;
                leftMostScreen = leftMostTile / ScreenTilesX;
                topMostScreen = topMostTile / ScreenTilesY;

                //for (int i = 0; i < roomMaps.Count; i++)
                //{
                //    SerializedRoom room = serialized.Rooms[i];
                //    int roomRight = (room.RoomWidth + room.RoomLocation.X) * ScreenTilesX;
                //    int roomBottom = (1 + room.RoomLocation.Y) * ScreenTilesY;
                //    if (roomRight > maxTilemapWidth)
                //        maxTilemapWidth = roomRight;
                //    if (roomBottom > maxTilemapHeight)
                //        maxTilemapHeight = roomBottom;
                //}

                // Prepare serialized level: Configuring the master tilemap from individual room tilemaps
                serialized.Map = new List<List<int>>();
                for (int y = 0; y < maxTilemapHeight; y++)
                {
                    List<int> row = new List<int>();
                    for (int x = 0; x < maxTilemapWidth; x++)
                    {
                        row.Add(-1);
                    }
                    serialized.Map.Add(row);
                }
                for (int i = 0; i < roomMaps.Count; i++)
                {
                    SerializedRoom room = serialized.Rooms[i];

                    // Look at the position of each room, and copy over to cooresponding sub-array in master tilemap
                    int startTileX = (room.RoomLocation.X - leftMostScreen) * ScreenTilesX;
                    int startTileY = (room.RoomLocation.Y - topMostScreen) * ScreenTilesY;

                    for (int y = startTileY; y < startTileY + 1 * ScreenTilesY; y++)
                    {
                        for (int x = startTileX; x < startTileX + room.RoomWidth * ScreenTilesX; x++)
                        {
                            serialized.Map[y][x] = roomMaps[i][x - startTileX, y - startTileY];
                        }
                    }
                }

                // Configuring the master enemy map and obstacle map from individual room maps
                serialized.EnemyMap = new List<List<int>>();
                serialized.ObstacleMap = new List<List<int>>();
                for (int y = 0; y < maxTilemapHeight; y++) // initialize each cell to have no enemy (-1)
                {
                    List<int> rowE = new List<int>();
                    List<int> rowO = new List<int>();
                    for (int x = 0; x < maxTilemapWidth; x++)
                    {
                        rowE.Add(-1);
                        rowO.Add(-1);
                    }
                    serialized.EnemyMap.Add(rowE);
                    serialized.ObstacleMap.Add(rowO);
                }
                for (int i = 0; i < roomEnemies.Count; i++)
                {
                    SerializedRoom room = serialized.Rooms[i];
                    // Look at the position of each room, and copy over to cooresponding sub-array in master enemy map
                    int startTileX = (room.RoomLocation.X - leftMostScreen) * ScreenTilesX;
                    int startTileY = (room.RoomLocation.Y - topMostScreen) * ScreenTilesY;

                    for (int y = startTileY; y < startTileY + 1 * ScreenTilesY; y++)
                    {
                        for (int x = startTileX; x < startTileX + room.RoomWidth * ScreenTilesX; x++)
                        {
                            serialized.ObstacleMap[y][x] = roomObstacles[i][x - startTileX, y - startTileY];
                            serialized.EnemyMap[y][x] = roomEnemies[i][x - startTileX, y - startTileY];
                        }
                    }
                }

                // Shift every room down and right until the top-left most room is 0,0
                for (int i = 0; i < roomMaps.Count; i++)
                {
                    serialized.Rooms[i].RoomLocation.X -= leftMostScreen;
                    serialized.Rooms[i].RoomLocation.Y -= topMostScreen;
                }

                // Save serialized level
                string levelPath = String.Format("Levels");
                string levelFile = System.IO.Path.Combine(levelPath, save.Name + ".xml");
                EditorLevel.Write(levelFile, serialized); // Name doesn't matter yet!
                tbxStatus.Text = String.Format("Saved \"{0}\" to file", save.Name + ".xml");

                // Finished saving
                EditorLevel.UnsavedChanges = false;
            }

            return result;
        }

        private void menSaveLevel_Click(object sender, RoutedEventArgs e)
        {
            SaveLevelDialog();
        }

        private void menuMain_Drop(object sender, DragEventArgs e)
        {
            menuMain.Focus();
        }

        //private void radTileLayer_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (!toolbar.IsEnabled) return;
        //    CurrentLayer = Layers.TileLayer;
        //}

        //private void radEnemyLayer_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (!toolbar.IsEnabled) return;
        //    CurrentLayer = Layers.EnemyLayer;
        //}

        //private void radSpecialLayer_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (!toolbar.IsEnabled) return;
        //    CurrentLayer = Layers.SpecialLayer;
        //}

        private void toggleGrid_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                grdRoomViewer.ShowGridLines = true;
            }
        }

        private void toggleGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                grdRoomViewer.ShowGridLines = false;
            }
        }

        private void toggleScreen_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                foreach (System.Windows.Shapes.Rectangle rect in screenRects)
                {
                    rect.Visibility = Visibility.Visible;
                }
            }
        }

        private void toggleScreen_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                foreach (System.Windows.Shapes.Rectangle rect in screenRects)
                {
                    rect.Visibility = Visibility.Hidden;
                }
            }
        }

        private void menSpawnPoint_Click(object sender, RoutedEventArgs e)
        {
            SetSpawnPointMode = true;
            tbxStatus.Text = "Setting spawn point...";
        }

        private void menPreview_Click(object sender, RoutedEventArgs e)
        {
            wndLevelPreview window = new wndLevelPreview(imgTileset, vgrdTileset.NumRows, vgrdTileset.NumColumns,
                roomMaps, serialized, ScreenTilesX, ScreenTilesY);
            window.ShowDialog();
        }

        private void menTest_Click(object sender, RoutedEventArgs e)
        {
            wndTestGame testGame = new wndTestGame();
            IntPtr handle = testGame.RenderPanel.Handle;
            
            Game1 game;
            new System.Threading.Thread(new System.Threading.ThreadStart(() => { game = new Game1(handle); game.Run(); })).Start();
        }

        #endregion

        /// <summary>
        /// Delete all changes to the current level, and clear all relevant UI elements.
        /// </summary>
        public void UnloadLevel()
        {
            roomMaps.Clear();
            roomEnemies.Clear();
            roomObstacles.Clear();
            lbxRooms.Items.Clear();

            for (int y = 0; y < imgTilesInGrid.GetLength(1); y++)
            {
                for (int x = 0; x < imgTilesInGrid.GetLength(0); x++)
                {
                    imgTilesInGrid[x, y].Source = null;
                    imgObstaclesInGrid[x, y].Source = null;
                    imgEnemiesInGrid[x, y].Source = null;
                }
            }
        }

        /// <summary>
        /// Deserialize a level opened from file, opening it in the editor. 
        /// Assumes that "SerializedLevel" has already been instantiated.
        /// </summary>
        public void LoadLevel()
        {
            int screenWidthTiles = (ViewportWidth / serialized.TileWidth);
            int screenHeightTiles = (ViewportHeight / serialized.TileHeight);

            // Populate primitive roomMaps from master Tilemap
            for (int i = 0; i < serialized.Rooms.Count; i++)
            {
                SerializedRoom room = serialized.Rooms[i];
                
                int roomWidthInTiles = room.RoomWidth * screenWidthTiles;
                int roomHeightInTiles = screenHeightTiles; // Always 1 screen high
                int xPos = room.RoomLocation.X;
                int yPos = room.RoomLocation.Y;

                int[,] roomMap = new int[roomWidthInTiles, roomHeightInTiles];
                int[,] roomObstacleMap = new int[roomWidthInTiles, roomHeightInTiles];
                int[,] roomEnemyMap = new int[roomWidthInTiles, roomHeightInTiles];
                for (int y = 0; y < roomHeightInTiles; y++)
                {
                    for (int x = 0; x < roomWidthInTiles; x++)
                    {
                        // Grab sub-array for room out of entire Tilemap
                        roomMap[x, y] = serialized.Map[y + screenHeightTiles * yPos][x + screenWidthTiles * xPos];
                        roomEnemyMap[x, y] = serialized.EnemyMap[y + screenHeightTiles * yPos][x + screenWidthTiles * xPos];
                        roomObstacleMap[x, y] = (serialized.ObstacleMap.Count == 0) ? -1 : serialized.ObstacleMap[y + screenHeightTiles * yPos][x + screenWidthTiles * xPos];
                    }
                }

                roomMaps.Add(roomMap);
                roomEnemies.Add(roomEnemyMap);
                roomObstacles.Add(roomObstacleMap);
            }

            // TODO: Populate primitive enemyMaps from master Tilemap

            // Load room and exit UI info, display current tilemap
            LoadRoomsAndTileset();
            //DisplayTilemapGrid(); <-- called by lbxRooms selectionchanged event handler in LoadRoomsAndTileset();
            IsLevelLoaded = true;
        }

        /// <summary>
        /// Adds Room listing to the UI, and selects the first room, showing the map.
        /// </summary>
        public void LoadRoomsAndTileset()
        {
            // Enable UI elements
            gbxRoomBrowser.IsEnabled = true;
            tabObjectPalette.IsEnabled = true;
            //gbxSelectedTile.IsEnabled = true;
            statusBar.IsEnabled = true;
            menSaveLevel.IsEnabled = true;
            menHeaderLevel.IsEnabled = true;

            // Load rooms
            for (int i = 0; i < serialized.Rooms.Count; i++)
            {
                lbxRooms.Items.Add(serialized.Rooms[i].RoomName);
            }

            // Grab image source from filename and load into local image
            ImageSourceConverter sourceConverter = new ImageSourceConverter();
            Console.WriteLine(Directory.GetCurrentDirectory());

            object wtf = sourceConverter.ConvertFromString("Content\\" + serialized.TexturePath + ".png");
            ImageSource source = (ImageSource)wtf;
            imgTileset.Source = source;
            imgTileset.Width = source.Width * ScaleTileset;
            imgTileset.Height = source.Height * ScaleTileset;

            // Load tileset image into grid
            vgrdTileset = new VisibleGrid(serialized.TileWidth * ScaleTileset,
                serialized.TileHeight * ScaleTileset,
                (int)(imgTileset.Width / (serialized.TileWidth * ScaleTileset)),
                (int)(imgTileset.Height / (serialized.TileHeight * ScaleTileset)));
            vgrdTileset.HorizontalAlignment = HorizontalAlignment.Left;
            vgrdTileset.VerticalAlignment = VerticalAlignment.Top;
            grdTilesetPalette.Children.RemoveAt(1);
            grdTilesetPalette.Children.Add(vgrdTileset);


            // Select the first one
            lbxRooms.SelectedIndex = 0;
        }

        #region Event Handlers - Exits

        private void lbxExits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedExitIndex == -1) return;

            SerializedRoom room = serialized.Rooms[SelectedRoomIndex];

            // Population Exits groupbox with current Exit info
            tbxExitLocation.Text = room.ExitScreenLocations[SelectedExitIndex].ToString();
            tbxExitDestination.Text = room.ExitDestinationRooms[SelectedExitIndex].ToString();
            cbxDirection.SelectedIndex = room.ExitDirections[SelectedExitIndex];
        }

        private void btnAddExit_Click(object sender, RoutedEventArgs e)
        {
            SerializedRoom thisRoom = serialized.Rooms[SelectedRoomIndex];
            thisRoom.ExitDirections.Add(0);
            thisRoom.ExitScreenLocations.Add(0);
            thisRoom.ExitDestinationRooms.Add(0);

            lbxExits.Items.Add(String.Format("Exit {0}", lbxExits.Items.Count));
            cbxDirection.IsEnabled = true;
            tbxExitLocation.IsEnabled = true;
            tbxExitDestination.IsEnabled = true;
            lbxExits.SelectedIndex = thisRoom.ExitDestinationRooms.Count - 1;
            EditorLevel.UnsavedChanges = true;
        }

        private void tbxExitLocation_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            if (Int32.TryParse(tbxExitLocation.Text, out value) && value <= 64 && value > 0)
            {
                serialized.Rooms[SelectedRoomIndex].ExitScreenLocations[SelectedExitIndex] = value;
            }
            else {
                tbxExitLocation.Text = "0";
                serialized.Rooms[SelectedRoomIndex].ExitScreenLocations[SelectedExitIndex] = 0;
            }
            if (IsLevelLoaded)
            {
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void tbxExitDestination_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            if (Int32.TryParse(tbxExitDestination.Text, out value) && value <= 64 && value > 0)
            {
                serialized.Rooms[SelectedRoomIndex].ExitDestinationRooms[SelectedExitIndex] = value;
            }
            else {
                tbxExitDestination.Text = "0";
                serialized.Rooms[SelectedRoomIndex].ExitDestinationRooms[SelectedExitIndex] = 0;
            }
            if (IsLevelLoaded)
            {
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void cbxDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            serialized.Rooms[SelectedRoomIndex].ExitDirections[SelectedExitIndex] = cbxDirection.SelectedIndex;
            if (IsLevelLoaded)
            {
                EditorLevel.UnsavedChanges = true;
            }
        }

        #endregion

        #region Event Handlers - Tileset

        private void grdTilesetPalette_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsLevelLoaded) return;

            // Highlight the selected tile on grid
            vgrdTileset.OnClick();

            int tileIndex = EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns);

            // Display selected tile info on the right side
            tbkTileCol.Text = vgrdTileset.SelectedColumn.ToString();
            tbkTileRow.Text = vgrdTileset.SelectedRow.ToString();

            // Check appropriate radio button
            collisionButtons[serialized.TilesetCollisionKey[tileIndex]].IsChecked = true;

            // Show proper animation sequence
            tbxFrames.Text = serialized.TilesetAnimationKey[tileIndex].ToString();
            animationFrame = 0;

            // Show cropped tile image
            imgSelectedTile.Source = TilesetImageAt(vgrdTileset.SelectedColumn, vgrdTileset.SelectedRow);
        }

        private void tbxFrames_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLevelLoaded)
                return;

            int value;
            if (Int32.TryParse(tbxFrames.Text, out value) && value <= 16 && value > 0)
            {
                // Assign a new animation
                serialized.TilesetAnimationKey[EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns)] = value;
                //animationKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = value;
            }
            else
            {
                tbxFrames.Text = "1";
                serialized.TilesetAnimationKey[EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns)] = 1;
                //animationKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = 1;
            }

            EditorLevel.UnsavedChanges = true;
        }

        private void radImpassable_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                serialized.TilesetCollisionKey[EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns)] = (int)TileCollision.Impassable;
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void radPassable_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                serialized.TilesetCollisionKey[EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns)] = (int)TileCollision.Passable;
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void radPlatform_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                serialized.TilesetCollisionKey[EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns)] = (int)TileCollision.Platform;
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void radLadder_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                serialized.TilesetCollisionKey[EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns)] = (int)TileCollision.Ladder;
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void radConveyorRight_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                serialized.TilesetCollisionKey[EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns)] = (int)TileCollision.ConveyorRight;
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void radConveyorLeft_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLevelLoaded)
            {
                serialized.TilesetCollisionKey[EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns)] = (int)TileCollision.ConveyorLeft;
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void tabObjectPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentLayer = (Layers)tabObjectPalette.SelectedIndex;

            switch (CurrentLayer)
            {
                case Layers.TileLayer:
                    break;
                case Layers.EnemyLayer:
                    break;
                case Layers.SpecialLayer:
                    break;
            }
        }

        private void lbxObstacleSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!tabObjectPalette.IsEnabled) return;

            string path = "pack://application:,,,/MMEditor;component/Images/Obstacles/";

            if (lbxObstacleSelect.SelectedIndex >= 0)
            {
                path = path + EditorLevel.ObstacleString((ObstacleTypes)lbxObstacleSelect.SelectedIndex) + ".png";
            }
            else
            {
                return;
            }

            // Load image file from content folder, scale it, and apply to image source
            BitmapImage bmp = new BitmapImage(new Uri(path));
            int scalarX = (int)(stackSelectedObstacle.ActualWidth / bmp.PixelWidth);
            int scalarY = (int)(stackSelectedObstacle.ActualHeight / bmp.PixelHeight);
            imgSelectedObstacle.Height = bmp.PixelHeight * scalarX;
            imgSelectedObstacle.Width = bmp.PixelWidth * scalarY;
            imgSelectedObstacle.Source = bmp;
        }

        private void lbxEnemySelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!tabObjectPalette.IsEnabled) return;

            string path = "pack://application:,,,/MMEditor;component/Images/Enemies/";

            if (lbxEnemySelect.SelectedIndex >= 0)
            {
                path = path + EditorLevel.EnemyString((EnemyTypes)lbxEnemySelect.SelectedIndex) + ".png";
            }

            // Load image file from content folder, scale it, and apply to image source
            BitmapImage bmp = new BitmapImage(new Uri(path));
            int scalarX = (int)(stackSelectedEnemy.ActualWidth / bmp.PixelWidth);
            int scalarY = (int)(stackSelectedEnemy.ActualHeight / bmp.PixelHeight);
            imgSelectedEnemy.Height = bmp.PixelHeight * scalarX;
            imgSelectedEnemy.Width = bmp.PixelWidth * scalarY;
            imgSelectedEnemy.Source = bmp;
        }

        public ImageSource TilesetImageAt(int col, int row)
        {
            Int32Rect cropRegion = new Int32Rect(col * serialized.TileWidth,
                                     row * serialized.TileHeight,
                                     serialized.TileWidth,
                                     serialized.TileHeight);
            return new CroppedBitmap((BitmapSource)imgTileset.Source, cropRegion);
        }

        private void AnimateStuff(object sender, EventArgs ev)
        {
            if (!IsLevelLoaded || serialized.TilesetAnimationKey == null) return;

            // Have an animation selected
            int animationIndex = EditorLevel.IndexFromRowCol(vgrdTileset.SelectedRow, vgrdTileset.SelectedColumn, vgrdTileset.NumColumns);
            if (serialized.TilesetAnimationKey.ElementAt(animationIndex) > 1)
            {
                // Navigate to the proper frame
                int gridCol = vgrdTileset.SelectedColumn + animationFrame;
                int gridRow = vgrdTileset.SelectedRow;
                while (gridCol >= vgrdTileset.NumColumns)
                {
                    // Goto next row of tileset if necessary
                    gridCol = gridCol - vgrdTileset.NumColumns;
                    if (++gridRow >= vgrdTileset.NumRows) gridRow = 0;
                }

                //Int32Rect cropRegion = new Int32Rect(gridCol * serialized.TileWidth,
                //                     gridRow * serialized.TileHeight,
                //                     serialized.TileWidth,
                //                     serialized.TileHeight);
                //imgSelectedTile.Source = new CroppedBitmap((BitmapSource)imgTileset.Source, cropRegion);

                imgSelectedTile.Source = TilesetImageAt(gridCol, gridRow);

                // Loop animation
                if (++animationFrame >= serialized.TilesetAnimationKey.ElementAt(animationIndex))
                {
                    animationFrame = 0;
                }
            }
        }

        #endregion

        #region Event Handlers - Tilemap

        private void scrollRoomGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsLevelLoaded) return;

            System.Windows.Point p = MouseUtilities.CorrectGetPosition(grdRoomViewer);
            int cellCol = (int)((p.X) / grdRoomViewer.ColumnDefinitions[0].Width.Value);
            int cellRow = (int)((p.Y) / grdRoomViewer.RowDefinitions[0].Height.Value);

            // Display mouse hover info in status bar
            tbxTilePosHover.Text = String.Format("({0}, {1})", cellCol, cellRow);
            try
            {
                int tilesetIndexHover = roomMaps[SelectedRoomIndex][cellCol, cellRow];
                tbxTileTypeHover.Text = ((TileCollision)serialized.TilesetCollisionKey[tilesetIndexHover]).ToString();
                tbxCurrentScreen.Text = String.Format("{0}", cellCol / 16);
            }
            catch (IndexOutOfRangeException ex)
            {
                tbxTileTypeHover.Text = "Nothing";
                tbxCurrentScreen.Text = "??";
            }
            catch (ArgumentOutOfRangeException ex)
            {
                tbxTileTypeHover.Text = "Nothing";
                tbxCurrentScreen.Text = String.Format("{0}", cellCol / 16);
            }

            // Paint a tile, enemy, or special object
            if (grdRoomViewer.IsMouseOver && scrollRoomGrid.IsFocused && !SetSpawnPointMode)
            {
                switch (CurrentLayer)
                {
                    case Layers.TileLayer:
                        // Draw tiles onto tilemap!
                        if (Mouse.LeftButton == MouseButtonState.Pressed)
                        {
                            PaintTile(cellCol, cellRow);
                        }
                        else if (Mouse.RightButton == MouseButtonState.Pressed)
                        {
                            EraseTile(cellCol, cellRow);
                        }
                        break;

                    case Layers.EnemyLayer:
                        // Draw enemies onto tilemap!
                        if (Mouse.LeftButton == MouseButtonState.Pressed)
                        {
                            PaintEnemy(cellCol, cellRow);
                        }
                        else if (Mouse.RightButton == MouseButtonState.Pressed)
                        {
                            EraseEnemy(cellCol, cellRow);
                        }
                        break;

                    case Layers.SpecialLayer:
                        // Draw obstacles onto tilemap!
                        if (Mouse.LeftButton == MouseButtonState.Pressed)
                        {
                            PaintObstacle(cellCol, cellRow);
                        }
                        else if (Mouse.RightButton == MouseButtonState.Pressed)
                        {
                            EraseObstacle(cellCol, cellRow);
                        }
                        break;

                } // end switch

                EditorLevel.UnsavedChanges = true;

            } // end if able to paint stuff
        }

        private void scrollRoomGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SetSpawnPointMode)
            {
                // Get mouse pos and the coorepsonding cells
                System.Windows.Point mouse = MouseUtilities.CorrectGetPosition(grdRoomViewer);
                int cellCol = (int)((mouse.X) / grdRoomViewer.ColumnDefinitions[0].Width.Value);
                int cellRow = (int)((mouse.Y) / grdRoomViewer.RowDefinitions[0].Height.Value);

                serialized.SpawnTile = new Microsoft.Xna.Framework.Point(cellCol, cellRow);
                serialized.SpawnRoom = SelectedRoomIndex;

                DisplayTilemapGrid();
                SetSpawnPointMode = false;
                tbxStatus.Text = String.Format("Spawn set in room #{0} \"{1}\" at ({2},{3})", SelectedRoomIndex, lbxRooms.SelectedItem, cellCol, cellRow);
            }
            else
            {
                scrollRoomGrid.Focus();
                scrollRoomGrid_PreviewMouseMove(sender, e);
            }
        }

        private void scrollRoomGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            scrollRoomGrid.Focus();
            scrollRoomGrid_PreviewMouseMove(sender, e);
        }

        public void DisplayTilemapGrid()
        {
            // Construct grid
            grdRoomViewer.Children.Clear();
            grdRoomViewer.ColumnDefinitions.Clear();
            grdRoomViewer.RowDefinitions.Clear();
            for (int x = 0; x < roomMaps[SelectedRoomIndex].GetLength(0); x++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(serialized.TileWidth * ScaleTilemap);
                grdRoomViewer.ColumnDefinitions.Add(col);
            }
            for (int y = 0; y < roomMaps[SelectedRoomIndex].GetLength(1); y++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(serialized.TileHeight * ScaleTilemap);
                grdRoomViewer.RowDefinitions.Add(row);
            }
            grdRoomViewer.Width = grdRoomViewer.ColumnDefinitions.Count * serialized.TileWidth * ScaleTilemap;
            grdRoomViewer.Height = grdRoomViewer.RowDefinitions.Count * serialized.TileHeight * ScaleTilemap;

            // Erase old grid
            for (int y = 0; y < imgTilesInGrid.GetLength(1); y++)
            {
                for (int x = 0; x < imgTilesInGrid.GetLength(0); x++)
                {
                    imgTilesInGrid[x, y].Source = null;
                    imgEnemiesInGrid[x, y].Source = null;
                    imgObstaclesInGrid[x, y].Source = null;
                }
            }

            // Place images in each cell of the grid
            imgTilesInGrid = new Image[grdRoomViewer.ColumnDefinitions.Count, grdRoomViewer.RowDefinitions.Count];
            imgEnemiesInGrid = new Image[grdRoomViewer.ColumnDefinitions.Count, grdRoomViewer.RowDefinitions.Count];
            imgObstaclesInGrid = new Image[grdRoomViewer.ColumnDefinitions.Count, grdRoomViewer.RowDefinitions.Count];
            for (int y = 0; y < grdRoomViewer.RowDefinitions.Count; y++)
            {
                for (int x = 0; x < grdRoomViewer.ColumnDefinitions.Count; x++)
                {
                    // Make a new, blank image for tiles
                    Image cellTile = new Image();
                    Grid.SetColumn(cellTile, x);
                    Grid.SetRow(cellTile, y);
                    grdRoomViewer.Children.Add(cellTile);
                    imgTilesInGrid[x, y] = cellTile;
                    RenderOptions.SetBitmapScalingMode(cellTile, BitmapScalingMode.NearestNeighbor);

                    // Make a new, blank image for obstacles
                    Image cellObstacle = new Image();
                    Grid.SetColumn(cellObstacle, x);
                    Grid.SetRow(cellObstacle, y);
                    grdRoomViewer.Children.Add(cellObstacle);
                    imgObstaclesInGrid[x, y] = cellObstacle;

                    // Make a new, blank image for enemies
                    Image cellEnemy = new Image();
                    Grid.SetColumn(cellEnemy, x);
                    Grid.SetRow(cellEnemy, y);
                    grdRoomViewer.Children.Add(cellEnemy);
                    imgEnemiesInGrid[x, y] = cellEnemy;
                    RenderOptions.SetBitmapScalingMode(cellEnemy, BitmapScalingMode.NearestNeighbor);

                    // Set to image source from tileset based on roomMap
                    int tileIndex = roomMaps[SelectedRoomIndex][x, y];
                    if (tileIndex > -1)
                    {
                        Microsoft.Xna.Framework.Point tileLoc = EditorLevel.RowColFromIndex(tileIndex, vgrdTileset.NumRows, vgrdTileset.NumColumns);
                        Int32Rect cropRegion = new Int32Rect(tileLoc.X * serialized.TileWidth,
                                            tileLoc.Y * serialized.TileHeight,
                                            serialized.TileWidth,
                                            serialized.TileHeight);
                        cellTile.Source = new CroppedBitmap((BitmapSource)imgTileset.Source, cropRegion);
                    }
                    else
                    {
                        cellTile.Source = null;
                    }

                    // Set image source of obstacles based on obstacleMap
                    int obstacleIndex = roomObstacles[SelectedRoomIndex][x, y];
                    if (obstacleIndex > -1)
                    {
                        // TODOTDOTDOTODTODOTDTDOTDOTODTOD
                        string path = "pack://application:,,,/MMEditor;component/Images/Obstacles/";

                        path = path + EditorLevel.ObstacleString((ObstacleTypes)obstacleIndex) + ".png";
                        BitmapImage bmp = new BitmapImage(new Uri(path));
                        cellObstacle.Source = bmp;
                    }
                    else
                    {
                        cellObstacle.Source = null;
                    }

                    //  Set image source of enemies based on enemyMap
                    int enemyIndex = roomEnemies[SelectedRoomIndex][x, y];
                    if (enemyIndex > -1)
                    {
                        // TODOTDOTDOTODTODOTDTDOTDOTODTOD
                        string path = "pack://application:,,,/MMEditor;component/Images/Enemies/";

                        path = path + EditorLevel.EnemyString((EnemyTypes)enemyIndex) + ".png";
                        BitmapImage bmp = new BitmapImage(new Uri(path));
                        //cellEnemy.Width = bmp.PixelWidth;
                        //cellEnemy.Height = bmp.PixelHeight;
                        cellEnemy.Source = bmp;
                    }
                    else
                    {
                        cellEnemy.Source = null;
                    }
                }
            }

            // Draw rectangle around each screen across the entire room
            int screenWidthTiles = (ViewportWidth / serialized.TileWidth);
            int screenHeightTiles = (ViewportHeight / serialized.TileHeight);
            screenRects = new List<System.Windows.Shapes.Rectangle>();
            for (int i = 0; i < grdRoomViewer.ColumnDefinitions.Count; i += screenWidthTiles)
            {
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Stroke = new SolidColorBrush(System.Windows.Media.Colors.Red);
                rect.StrokeThickness = 2.0;
                Grid.SetColumn(rect, i);
                Grid.SetRow(rect, 0);
                Grid.SetColumnSpan(rect, screenWidthTiles);
                Grid.SetRowSpan(rect, screenHeightTiles);
                grdRoomViewer.Children.Add(rect);
                screenRects.Add(rect);
                if (toggleScreen.IsChecked == false)
                {
                    rect.Visibility = Visibility.Hidden;
                }
            }

            // Show spawn point, if it is in this room
            if (serialized.SpawnRoom == SelectedRoomIndex)
            {
                System.Windows.Shapes.Ellipse spawnRect = new System.Windows.Shapes.Ellipse();
                SolidColorBrush fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 255));
                SolidColorBrush stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(196, 255, 255, 255));
                spawnRect.Fill = fill;
                spawnRect.Stroke = stroke;
                spawnRect.StrokeThickness = 2;

                Grid.SetColumn(spawnRect, serialized.SpawnTile.X);
                Grid.SetRow(spawnRect, serialized.SpawnTile.Y);
                Grid.SetColumnSpan(spawnRect, 2);
                Grid.SetRowSpan(spawnRect, 2);
                grdRoomViewer.Children.Add(spawnRect);
            }

            // Set a solid empty background so MouseOver events fire on the grid
            grdRoomViewer.Background = new SolidColorBrush(System.Windows.Media.Colors.White);

        }

        private void EraseTile(int cellCol, int cellRow)
        {
            // Nullify image in the given cell
            try
            {
                imgTilesInGrid[cellCol, cellRow].Source = null;

                // Save tile type to serialized tilemap
                roomMaps.ElementAt(SelectedRoomIndex)[cellCol, cellRow] = -1;
            }
            catch (IndexOutOfRangeException ex)
            {
                // Do nothing, probably moved mouse outside the map
            }
        }

        private void PaintTile(int cellCol, int cellRow)
        {
            // Place image in cell, if in bounds
            try
            {
                //imgTilesInGrid[cellCol, cellRow].Source = imgSelectedTile.Source;
                imgTilesInGrid[cellCol, cellRow].Source = TilesetImageAt(vgrdTileset.SelectedColumn, vgrdTileset.SelectedRow);

                // Save tile type to serialized tilemap
                roomMaps.ElementAt(SelectedRoomIndex)[cellCol, cellRow] = (vgrdTileset.SelectedColumn) + (vgrdTileset.SelectedRow * vgrdTileset.NumColumns);
            }
            catch (IndexOutOfRangeException ex)
            {
                // Do nothing, probably moved mouse outside the map
            }
        }

        private void EraseObstacle(int cellCol, int cellRow)
        {
            try
            {
                imgObstaclesInGrid[cellCol, cellRow].Source = null;
                roomObstacles.ElementAt(SelectedRoomIndex)[cellCol, cellRow] = -1;
            }
            catch (IndexOutOfRangeException ex)
            { }
        }

        private void PaintObstacle(int cellCol, int cellRow)
        {
            try
            {
                imgObstaclesInGrid[cellCol, cellRow].Source = imgSelectedObstacle.Source;
                roomObstacles.ElementAt(SelectedRoomIndex)[cellCol, cellRow] = lbxObstacleSelect.SelectedIndex;
            }
            catch (IndexOutOfRangeException ex)
            { }

        }

        private void EraseEnemy(int cellCol, int cellRow)
        {
            // Nullify enemy image in the given cell
            try
            {
                imgEnemiesInGrid[cellCol, cellRow].Source = null;
                roomEnemies.ElementAt(SelectedRoomIndex)[cellCol, cellRow] = -1;
            }
            catch (IndexOutOfRangeException ex)
            { }
        }

        private void PaintEnemy(int cellCol, int cellRow)
        {
            try
            {
                imgEnemiesInGrid[cellCol, cellRow].Source = imgSelectedEnemy.Source;

                // Save enemy type of serialized enemy map
                roomEnemies.ElementAt(SelectedRoomIndex)[cellCol, cellRow] = lbxEnemySelect.SelectedIndex;
            }
            catch (IndexOutOfRangeException ex)
            { }

        }

        #endregion

        #region Event Handlers - Room Browser

        private void lbxRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedRoomIndex == -1) return;

            SerializedRoom room = serialized.Rooms[SelectedRoomIndex];

            IsLevelLoaded = false;

            // Populate Exits groupbox
            lbxExits.Items.Clear();
            for (int i = 0; i < room.ExitScreenLocations.Count; i++)
            {
                lbxExits.Items.Add(String.Format("Exit {0}", i));
            }
            if (lbxExits.Items.Count == 0)
            {
                tbxExitDestination.IsEnabled = false;
                tbxExitLocation.IsEnabled = false;
                cbxDirection.IsEnabled = false;
                lbxExits.SelectedIndex = -1;
            }
            else
            {
                tbxExitDestination.IsEnabled = true;
                tbxExitLocation.IsEnabled = true;
                cbxDirection.IsEnabled = true;
                lbxExits.SelectedIndex = 0;
            }

            // Populate Toom groupbox
            tbxScreenX.Text = room.RoomLocation.X.ToString();
            tbxScreenY.Text = room.RoomLocation.Y.ToString();
            tbxScreenWidth.Text = room.RoomWidth.ToString();
            tbxRoomName.Text = lbxRooms.SelectedItem.ToString();

            IsLevelLoaded = true;

            // Construct grid
            DisplayTilemapGrid();

            // Scroll grid back to the left
            scrollRoomGrid.ScrollToHome();

            // TODO: Populate sub-Tilemap for this room
        }

        private void tbxScreenX_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            if (Int32.TryParse(tbxScreenX.Text, out value) && value < 32 && value >= -32)
            {
            }
            else
            {
                value = 0;
                tbxScreenX.Text = "0";
            }
            serialized.Rooms[SelectedRoomIndex].RoomLocation.X = value;
            if (IsLevelLoaded)
            {
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void tbxScreenY_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            if (Int32.TryParse(tbxScreenY.Text, out value) && value < 32 && value >= -32)
            {
            }
            else
            {
                value = 0;
                tbxScreenY.Text = "0";
            }
            serialized.Rooms[SelectedRoomIndex].RoomLocation.Y = value;
            if (IsLevelLoaded)
            {
                EditorLevel.UnsavedChanges = true;
            }
        }

        private void tbxScreenWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;

            if (!(Int32.TryParse(tbxScreenWidth.Text, out value) && value <= 64 && value > 0))
            {
                tbxScreenWidth.Text = serialized.Rooms[SelectedRoomIndex].RoomWidth.ToString();
                return;
            }

            serialized.Rooms[SelectedRoomIndex].RoomWidth = value;
            int[,] oldMap = roomMaps[SelectedRoomIndex];
            int[,] oldEnemyMap = roomEnemies[SelectedRoomIndex];
            int[,] oldObstacleMap = roomObstacles[SelectedRoomIndex];
            roomMaps[SelectedRoomIndex] = new int[value * ScreenTilesX, ScreenTilesY];
            roomEnemies[SelectedRoomIndex] = new int[value * ScreenTilesX, ScreenTilesY];
            roomObstacles[SelectedRoomIndex] = new int[value * ScreenTilesX, ScreenTilesY];

            for (int y = 0; y < roomMaps[SelectedRoomIndex].GetLength(1); y++)
            {
                for (int x = 0; x < roomMaps[SelectedRoomIndex].GetLength(0); x++)
                {
                    if (x < oldMap.GetLength(0) && y < oldMap.GetLength(1))
                    {
                        roomMaps[SelectedRoomIndex][x, y] = oldMap[x, y];
                        roomEnemies[SelectedRoomIndex][x, y] = oldEnemyMap[x, y];
                        roomObstacles[SelectedRoomIndex][x, y] = oldObstacleMap[x, y];
                    }
                    else
                    {
                        roomMaps[SelectedRoomIndex][x, y] = -1;
                        roomEnemies[SelectedRoomIndex][x, y] = -1;
                        roomObstacles[SelectedRoomIndex][x, y] = -1;
                    }
                }
            }

            if (IsLevelLoaded)
            {
                EditorLevel.UnsavedChanges = true;
            }
            DisplayTilemapGrid();
        }

        private void btnAddRoom_Click(object sender, RoutedEventArgs e)
        {
            wndAddRoom addRoom = new wndAddRoom(String.Format("Room {0}", serialized.Rooms.Count), roomMaps, serialized);
            bool? result = addRoom.ShowDialog();

            if (result == true)
            {
                int screenX = addRoom.ScreenX;
                int screenY = addRoom.ScreenY;
                int screenW = addRoom.ScreenWidth;

                // Set up simple starting tilemap and enemymap in the first room
                int[,] newRoom = new int[16 * screenW, 14];
                int[,] newObstacles = new int[16 * screenW, 14];
                int[,] newEnemies = new int[16 * screenW, 14];
                for (int y = 0; y < 14; y++)
                {
                    for (int x = 0; x < 16 * screenW; x++)
                    {
                        newRoom[x, y] = -1;
                        newEnemies[x, y] = -1;
                        newObstacles[x, y] = -1;
                    }
                }
                roomMaps.Add(newRoom);
                roomObstacles.Add(newObstacles);
                roomEnemies.Add(newEnemies);
                serialized.Rooms.Add(new SerializedRoom(serialized.Rooms.Count, 
                    String.Format("Room {0}", serialized.Rooms.Count), 
                    new Microsoft.Xna.Framework.Point(addRoom.ScreenX + RoomIndexOffset, addRoom.ScreenY + RoomIndexOffset), 
                    addRoom.ScreenWidth, 
                    new List<int>(), 
                    new List<int>(), 
                    new List<int>()));
                lbxRooms.Items.Add(addRoom.RoomName);
                EditorLevel.UnsavedChanges = true;

                // If placed in a negative x or y, shift all other rooms until top-most room is 0,0
                if (addRoom.ScreenX < 0)
                {
                    int leftMost = addRoom.ScreenX;
                    for (int i = 0; i < roomMaps.Count; i++)
                    {
                        serialized.Rooms[i].RoomLocation.X -= leftMost;
                    }
                }
                if (addRoom.ScreenY < 0)
                {
                    int topMost = addRoom.ScreenY;
                    for (int i = 0; i < roomMaps.Count; i++)
                    {
                        serialized.Rooms[i].RoomLocation.Y -= topMost;
                    }
                }
            }


        }

        private void cbxTilemapZoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbxTilemapZoom.SelectedIndex)
            {
                case 0: ScaleTilemap = 1; break;
                case 1: ScaleTilemap = 2; break;
                case 2: ScaleTilemap = 3; break;
                case 3: ScaleTilemap = 4; break;
            }

            if (IsLevelLoaded)
            {
                DisplayTilemapGrid();
            }
        }

        private void tbxRoomName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int a = SelectedRoomIndex;
                lbxRooms.UnselectAll();
                lbxRooms.Items[a] = tbxRoomName.Text;
                lbxRooms.SelectedIndex = a;

                serialized.Rooms[lbxRooms.SelectedIndex].RoomName = tbxRoomName.Text;
                EditorLevel.UnsavedChanges = true;
            }
        }

#endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (EditorLevel.UnsavedChanges)
            {
                UnsavedChanges window = new UnsavedChanges(this);
                switch (window.ShowCustomDialog())
                {
                    case SaveResult.Yes:
                        // Try to save before closing: If user cancels the SaveLevelDialog, this UnsavedChanges dialog is also canceled.
                        bool? saveDialog = SaveLevelDialog();
                        if (saveDialog == false)
                        {
                            e.Cancel = true;
                            return;
                        }
                        break;
                    case SaveResult.No:
                        // User doesn't want to save, proceed with closing and discard changes.
                        break;
                    case SaveResult.Cancel:
                        // User doesn't want to save, but doesn't want the program to close. Cancel.
                        e.Cancel = true;
                        return;
                }
            }

            dt.Stop();
        }



    }

    public enum Layers
    {
        TileLayer,
        EnemyLayer,
        SpecialLayer
    }
}
