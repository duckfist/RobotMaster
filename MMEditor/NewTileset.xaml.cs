using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using System.Windows.Threading;
using RobotMaster.TileEngine;
using Microsoft.Xna.Framework;

namespace MMEditor
{
    public partial class NewTileset : Window
    {
        VisibleGrid visibleGrid;
        int tileWidth = 16;
        int tileHeight = 16;

        int[,] collisionKey;

        // This signifies which tiles belong to an animation.  Every tile is defaulted to "1", having no animation.
        // Any tile with a key greater than one is the first frame of animation for a Tile's animation sequence -
        // Subsequent tiles a part of that animation will be numbered "0" and have the same collision key as the first.
        // e.g. the tile at (1,0) in the tileset has the value "4", to have 4 frames of animation. The following 3
        // tiles, (2,0) (3,0) and (4,0), will have they key value "0". Every other tile has 1 frame, keyed "1".
        int[,] animationKey;
        int animationFrame = 0;
        DispatcherTimer dt;

        SerializedLevel newLevel;

        List<RadioButton> collisionButtons = new List<RadioButton>();

        public int NumTilesWide
        {
            get
            {
                return (int)(imgTileset.Width / TileWidth);
            }
        }

        public int NumTilesHigh
        {
            get
            {
                return (int)(imgTileset.Height / TileHeight);
            }
        }

        public int TileWidth
        {
            get { return tileWidth; }
            set
            {
                tileWidth = value;
                tbxTileWidth.Text = value.ToString();
            }
        }

        public int TileHeight
        {
            get { return tileHeight; }
            set
            {
                tileHeight = value;
                tbxTileHeight.Text = value.ToString();
            }
        }

        public NewTileset(SerializedLevel level)
        {
            InitializeComponent();
            this.newLevel = level;

            // make custom control visible grid
            visibleGrid = new VisibleGrid(1, 1, 1, 1);
            GridTileset.Children.Add(visibleGrid);

            // save radio buttons to convenient list
            collisionButtons.Add(radPassable);
            collisionButtons.Add(radImpassable);
            collisionButtons.Add(radPlatform);
            collisionButtons.Add(radLadder);

            // Start UI Thread
            dt = new DispatcherTimer();
            dt.Tick += new EventHandler(AnimateStuff);
            dt.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dt.Start();
        }

        private void Populate()
        {
            // Make VisibleGrid according to image dimensions
            visibleGrid = new VisibleGrid(TileWidth, TileHeight, (int)(imgTileset.Width / TileWidth), (int)(imgTileset.Height / TileHeight));
            visibleGrid.HorizontalAlignment = HorizontalAlignment.Left;
            visibleGrid.VerticalAlignment = VerticalAlignment.Top;
            GridTileset.Children.RemoveAt(1);
            GridTileset.Children.Add(visibleGrid);

            // Update some textboxes
            txtDimensions.Text = String.Format("{0}x{1}", NumTilesHigh, NumTilesWide);
            txtTileCount.Text = String.Format("{0}", NumTilesHigh * NumTilesWide);
            collisionKey = new int[NumTilesHigh, NumTilesWide]; // default all to 0
            animationKey = new int[NumTilesHigh, NumTilesWide]; // default all to 1
            for (int i = 0; i < animationKey.GetLength(0); i++)
            {
                for (int j = 0; j < animationKey.GetLength(1); j++)
                {
                    animationKey[i, j] = 1;
                }
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Open file browser to select a tileset image
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.CurrentDirectory + "\\Content\\Tiles";
            dlg.FileName = "tileset";
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Image Files (.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Grab image source from filename and load into local image
                ImageSourceConverter sourceConverter = new ImageSourceConverter();
                ImageSource source = (ImageSource)sourceConverter.ConvertFromString(dlg.FileName);
                imgTileset.Source = source;
                imgTileset.Width = source.Width;
                imgTileset.Height = source.Height;

                // Make VisibleGrid represent new tileset image
                Populate();

                // Save image path
                newLevel.TexturePath = dlg.FileName;    // TODO: copy the image to a local location!
                tbxPath.Text = dlg.FileName;
                tbxPath.CaretIndex = tbxPath.Text.Length;
                var rect = tbxPath.GetRectFromCharacterIndex(tbxPath.CaretIndex);
                tbxPath.ScrollToHorizontalOffset(rect.Right);

                // Enable relevant UI Elements
                btnOk.IsEnabled = true;
                
                gbxSelectedTile.IsEnabled = true;
                tbxTileHeight.IsEnabled = true;
                tbxTileWidth.IsEnabled = true;
                chkShowGrid.IsEnabled = true;
            }
        }

        private void tbxTileWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            int width;
            if (Int32.TryParse(tbxTileWidth.Text, out width) && width <= 1024 && width > 0)
            {
                TileWidth = width;
            }
            else
            {
                TileWidth = 16;
            }

            Populate();
        }

        private void tbxTileHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            int height;
            if (Int32.TryParse(tbxTileHeight.Text, out height) && height <= 1024 && height > 0)
            {
                TileHeight = height;
            }
            else
            {
                TileHeight = 16;
            }

            Populate();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                visibleGrid.Visibility = Visibility.Visible;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                visibleGrid.Visibility = Visibility.Hidden;
            }
        }

        private void GridTileset_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Highlight the selected tile on grid
            visibleGrid.OnClick();

            // Display selected tile info on the right side
            tbkTileCol.Text = visibleGrid.SelectedColumn.ToString();
            tbkTileRow.Text = visibleGrid.SelectedRow.ToString();

            // Check appropriate radio button
            collisionButtons[collisionKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn]].IsChecked = true;

            // Show proper animation sequence
            tbxFrames.Text = animationKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn].ToString();
            animationFrame = 0;

            // Show cropped tile image
            Int32Rect cropRegion = new Int32Rect(visibleGrid.SelectedColumn * TileWidth,
                                                 visibleGrid.SelectedRow * TileHeight,
                                                 TileWidth,
                                                 TileHeight);
            imgSelectedTile.Source = new CroppedBitmap((BitmapSource)imgTileset.Source, cropRegion);
            
        }

        private void radPassable_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                collisionKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = (int)TileCollision.Passable;
            }
        }

        private void radImpassable_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                collisionKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = (int)TileCollision.Impassable;
            }
        }

        private void radPlatform_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                collisionKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = (int)TileCollision.Platform;
            }
        }

        private void radLadder_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                collisionKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = (int)TileCollision.Ladder;
            }
        }

        private void radConveyorLeft_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                collisionKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = (int)TileCollision.ConveyorLeft;
            }
        }

        private void radConveyorRight_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                collisionKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = (int)TileCollision.ConveyorRight;
            }
        }

        private void tbxFrames_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            int value;
            if (Int32.TryParse(tbxFrames.Text, out value) && value <= 16 && value > 0)
            {
                // Assign a new animation
                animationKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = value;
            }
            else
            {
                tbxFrames.Text = "1";
                animationKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] = 1;
            }

        }

        private void AnimateStuff(object sender, EventArgs ev)
        {
            if (!IsLoaded || animationKey == null) return;

            // Have an animation selected
            if (animationKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn] > 1)
            {
                // Navigate to the proper frame
                int gridCol = visibleGrid.SelectedColumn + animationFrame;
                int gridRow = visibleGrid.SelectedRow;
                while (gridCol >= visibleGrid.NumColumns)
                {
                    // Goto next row of tileset if necessary
                    gridCol = gridCol  - visibleGrid.NumColumns;
                    if (++gridRow >= visibleGrid.NumRows) gridRow = 0;
                }

                Int32Rect cropRegion = new Int32Rect(gridCol * TileWidth,
                                     gridRow * TileHeight,
                                     TileWidth,
                                     TileHeight);
                imgSelectedTile.Source = new CroppedBitmap((BitmapSource)imgTileset.Source, cropRegion);

                // Loop animation
                if (++animationFrame >= animationKey[visibleGrid.SelectedRow, visibleGrid.SelectedColumn])
                {
                    animationFrame = 0;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dt.Stop();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            // Set up a simple level with this tileset
            newLevel.TileWidth = TileWidth;
            newLevel.TileHeight = TileHeight;
            newLevel.TilesWide = (int)(imgTileset.Width / TileWidth);
            newLevel.TilesHigh = (int)(imgTileset.Height / TileHeight);
            //newLevel.TexturePath = // Already configured
            newLevel.TilesetCollisionKey = new List<int>();
            newLevel.TilesetAnimationKey = new List<int>();
            for (int i = 0; i < animationKey.GetLength(0); i++)
            {
                for (int j = 0; j < animationKey.GetLength(1); j++)
                {
                    newLevel.TilesetAnimationKey.Add(animationKey[i, j]);
                    newLevel.TilesetCollisionKey.Add(collisionKey[i, j]);
                }
            }

            // Set up simple map
            newLevel.Map = new List<List<int>>();

            // Set up one room
            newLevel.Rooms = new List<SerializedRoom>();
            SerializedRoom aRoom = new SerializedRoom(
                0,
                "Room 0",
                new Microsoft.Xna.Framework.Point(MainWindow.RoomIndexOffset, MainWindow.RoomIndexOffset),
                1,
                new List<int>(),
                new List<int>(),
                new List<int>()
                );
            newLevel.Rooms.Add(aRoom);

            // Place default spawn point in the center of that map
            newLevel.SpawnRoom = 0;
            newLevel.SpawnTile = new Microsoft.Xna.Framework.Point(7, 6);

            this.DialogResult = true;
            this.Close();
        }






    }
}
