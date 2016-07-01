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
using System.Windows.Shapes;

using RobotMaster.TileEngine;

namespace MMEditor
{
    /// <summary>
    /// Interaction logic for wndAddRoom.xaml
    /// </summary>
    public partial class wndAddRoom : Window
    {
        public int ScreenX = 0;
        public int ScreenY = 0;
        public int ScreenWidth = 0;
        public string RoomName = "";

        private Brush _selectionColor = Brushes.LightGreen;
        private Brush _selectionBorder = Brushes.DarkGreen;
        private float _selectionThickness = 3.0f;
        private Brush _roomColor = Brushes.Blue;
        private Brush _roomBorder = Brushes.Navy;
        private float _roomThickness = 3.0f;
        private int _gridPixelSize = 24;

        private List<int[,]> roomMaps;
        private SerializedLevel serialized;
        private Rectangle _selectionRect;

        private bool _hasSelectedRoom = false;
        private int _roomPadding = 2;

        int[,] map;

        public wndAddRoom(string name, List<int[,]> roomMaps, SerializedLevel serialized)
        {
            InitializeComponent();

            this.RoomName = name;
            this.ScreenX = 0;
            this.ScreenY = 0;
            this.ScreenWidth = 1;

            tbxName.Text = name;
            tbxScreenX.Text = 0.ToString();
            tbxScreenY.Text = 0.ToString();
            tbxScreenWidth.Text = 1.ToString();

            this.roomMaps = roomMaps;
            this.serialized = serialized;

            _selectionRect = new Rectangle();
            _selectionRect.Fill = _selectionColor;
            _selectionRect.Stroke = _selectionBorder;
            _selectionRect.StrokeThickness = _selectionThickness;

            grdLevelMap.Children.Add(_selectionRect);

            DrawScreenMap(roomMaps, serialized);

        }

        private void DrawScreenMap(List<int[,]> roomMaps, SerializedLevel serialized)
        {
            // find the bounds for the entire level
            int mapHeight = 1;
            int mapWidth = 1;
            for (int i = 0; i < roomMaps.Count; i++)
            {
                SerializedRoom room = serialized.Rooms[i];
                int roomRight = room.RoomWidth + room.RoomLocation.X;
                int roomBottom = 1 + room.RoomLocation.Y;// + room.RoomHeight
                if (roomRight > mapWidth)
                    mapWidth = roomRight;
                if (roomBottom > mapHeight)
                    mapHeight = roomBottom;
            }

            // prepare level screen array
            map = new int[mapWidth, mapHeight];
            for (int i = 0; i < roomMaps.Count; i++)
            {
                SerializedRoom room = serialized.Rooms[i];

                // Look at the position of each room, and copy over to cooresponding sub-array in master tilemap
                //for (int h = 0; h < room.RoomHeight; h++)
                //{
                //    for (int w = 0; w < room.RoomWidth; w++)
                //    {
                //        map[w + room.RoomLocation.X - 1, h + room.RoomLocation.Y - 1] = 1;
                //    }
                //}
                for (int w = 0; w < room.RoomWidth; w++)
                {
                    map[w + room.RoomLocation.X, room.RoomLocation.Y] = 1;
                }
            }

            // Construct Grid
            grdLevelMap.Children.Clear();
            grdLevelMap.ColumnDefinitions.Clear();
            grdLevelMap.RowDefinitions.Clear();
            for (int x = 0; x < mapWidth + (_roomPadding * 2); x++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(_gridPixelSize);
                grdLevelMap.ColumnDefinitions.Add(col);
            }
            for (int y = 0; y < mapHeight + (_roomPadding * 2); y++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(_gridPixelSize);
                grdLevelMap.RowDefinitions.Add(row);
            }
            grdLevelMap.Width = grdLevelMap.ColumnDefinitions.Count * _gridPixelSize;
            grdLevelMap.Height = grdLevelMap.RowDefinitions.Count * _gridPixelSize;


            for (int i = 0; i < roomMaps.Count; i++)
            {
                SerializedRoom room = serialized.Rooms[i];
                Rectangle rect = new Rectangle();

                rect.Stroke = _roomBorder;
                rect.Fill = _roomColor;
                rect.StrokeThickness = _roomThickness;
                Grid.SetColumn(rect, room.RoomLocation.X + _roomPadding);
                Grid.SetRow(rect, room.RoomLocation.Y + _roomPadding);
                //Grid.SetRowSpan(rect, room.RoomHeight);
                Grid.SetColumnSpan(rect, room.RoomWidth);
                grdLevelMap.Children.Add(rect);
            }

            //for (int y = 2; y < grdLevelMap.RowDefinitions.Count - 2; y++)
            //{
            //    for (int x = 2; x < grdLevelMap.ColumnDefinitions.Count - 2; x++)
            //    {
            //        Rectangle rect = new Rectangle();
            //        if (map[x - 2, y - 2] != 0)
            //        {
            //            rect.Stroke = Brushes.Navy;
            //            rect.Fill = Brushes.Blue;
            //            rect.StrokeThickness = 2.0f;
            //        }

            //        Grid.SetColumn(rect, x);
            //        Grid.SetRow(rect, y);
            //        grdLevelMap.Children.Add(rect);
            //    }
            //}

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void tbxScreenX_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            if ((Int32.TryParse(tbxScreenX.Text, out value) && value < 32 && value >= -32))
            {
                ScreenX = value;
            }
            else
            {
                if (tbxScreenX.Text != "-")
                    tbxScreenX.Text = ScreenX.ToString();
            }
        }

        private void tbxScreenY_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            if ((Int32.TryParse(tbxScreenY.Text, out value) && value < 32 && value >= -32))
            {
                ScreenY = value;
            }
            else
            {
                if (tbxScreenY.Text != "-")
                    tbxScreenY.Text = ScreenY.ToString();
            }
        }

        private void tbxScreenWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            if ((Int32.TryParse(tbxScreenWidth.Text, out value) && value <= 64 && value > 0))
            {
                ScreenWidth = value;
                if (value > _roomPadding)
                {
                    _roomPadding = value;
                    DrawScreenMap(roomMaps, serialized);
                }
            }
            else
            {
                tbxScreenWidth.Text = ScreenWidth.ToString();
            }
        }

        private void tbxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            RoomName = tbxName.Text;
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e){ grdLevelMap.ShowGridLines = true; }
        private void checkBox1_Unchecked(object sender, RoutedEventArgs e){ grdLevelMap.ShowGridLines = false; }

        private void grdLevelMap_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_hasSelectedRoom || e.LeftButton == MouseButtonState.Pressed)
            {
                DrawSelectionBox();
            }
        }

        private void grdLevelMap_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DrawSelectionBox();
            _hasSelectedRoom = true;
        }

        private void DrawSelectionBox()
        {
            //System.Windows.Point p = MouseUtilities.CorrectGetPosition(this);
            Point p = Mouse.GetPosition(grdLevelMap);
            Thickness t = grdLevelMap.Margin;

            int cellCol = (int)((p.X) / grdLevelMap.ColumnDefinitions[0].Width.Value);
            int cellRow = (int)((p.Y) / grdLevelMap.RowDefinitions[0].Height.Value);

            int width;
            if (!Int32.TryParse(tbxScreenWidth.Text, out width))
            {
                width = 1;
            }
            
            tbxScreenX.Text = String.Format("{0}", cellCol - _roomPadding);
            tbxScreenY.Text = String.Format("{0}", cellRow - _roomPadding);

            grdLevelMap.Children.Remove(_selectionRect);
            Grid.SetColumn(_selectionRect, cellCol);
            Grid.SetRow(_selectionRect, cellRow);
            //Grid.SetRowSpan(rect, room.RoomHeight);
            Grid.SetColumnSpan(_selectionRect, width);
            grdLevelMap.Children.Add(_selectionRect);
        }



    }
}
