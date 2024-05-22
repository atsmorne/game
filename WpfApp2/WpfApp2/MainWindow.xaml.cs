using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace WpfApp2
{
    public interface IGraphicBase
    {
        void DrawLine(Point a, Point b);
    }

    class DrawAssistant : IGraphicBase
    {
        public DrawingGroup drawing = new();
        public Color BackgroundBrush { get; set; }
        public Color BorderBrush { get; set; }
        public double BorderThickness { get; set; }
        public void DrawLine(Point a, Point b)
        {
            drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(BackgroundBrush),
                                                     new Pen(new SolidColorBrush(BorderBrush), BorderThickness),
                                                     new LineGeometry(a, b)));
        }

        public void DrawRectangle(Point TopRight, Point TopLeft, Point BottomLeft, Point BottomRight)
        {
            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext geometryContext = streamGeometry.Open())
            {
                // Начинаем фигуру с верхнего правого угла 
                geometryContext.BeginFigure(TopRight, true, true);

                // Определяем линии против часовой стрелки
                geometryContext.LineTo(TopLeft, true, false);
                geometryContext.LineTo(BottomLeft, true, false);
                geometryContext.LineTo(BottomRight, true, false);

                // Завершаем фигуру
                geometryContext.Close();
            }
            drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(BackgroundBrush),
                                                     new Pen(new SolidColorBrush(BorderBrush), BorderThickness),
                                                     streamGeometry));
        }
    }

    public class Vector
    {
        public double x { get; set; }
        public double y { get; set; }
        public Vector()
        {
        }
        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Game
    {
        public bool InGame = true;
        public Vector init_playerPosition = new Vector(2.5, 1.5);
        public Vector init_playerDirection = new Vector(1, 0);
        public Vector init_cameraPlane = new Vector(0, -1);
        public double init_moveSpeed = 0.1;
        public double init_rotSpeed = 0.1;
        public int[,] worldMap = new int[,]
        {
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2},
            {1,-1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,-1,2},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,0,1,1,1,1},
            {1,0,1,0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,1,1,0,1,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,1,0,1,0,1,1,1,0,1,1,0,1,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,1,0,1,0,1,0,1,0,1,1,0,1,1,1,1},
            {1,0,1,0,0,0,0,0,0,0,1,0,1,0,0,0,1,0,1,1,0,0,1,1,1},
            {1,0,1,0,0,0,0,0,0,0,1,0,1,1,1,1,1,0,1,1,1,0,0,1,1},
            {1,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,1,1,0,0,1},
            {1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1},
            {1,0,1,-1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,1,0,0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0,1,1,0,0,1},
            {1,0,1,0,0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0,1,1,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        };
        // YYYY
        // X
        // X
        // X
        // X
        public void Restart()
        {
            player.ResetPlayer(new Vector(2.5, 1.5), new Vector(1, 0), 0.1, 0.1);
            InGame = true;
        }

        public Player player;
        public bool CheckWin()
        {
            if (worldMap[(int)(player.playerPosition.x), (int)(player.playerPosition.y)] == -1) return true;
            else return false;
        }

        public Game()
        {
            player = new Player(worldMap, init_playerPosition, init_playerDirection, init_moveSpeed, init_rotSpeed);
        }
    }

    public class Player
    {
        public Vector playerPosition;
        public Vector playerDirection;
        public double moveSpeed = 0.1;
        public double rotSpeed = 0.1;
        readonly int[,] worldMap;

        public bool forward = false;
        public bool back = false;
        public bool left = false;
        public bool right = false;

        public Player(int[,] _worldMap, Vector _playerPosition, Vector _playerDirection, double _moveSpeed, double _rotSpeed)
        {
            this.worldMap = _worldMap;
            this.playerPosition = _playerPosition;
            this.playerDirection = _playerDirection;
            this.moveSpeed = _moveSpeed;
            this.rotSpeed = _rotSpeed;
            RayCaster.UpdateCameraPlane(this);
        }
        public void ResetPlayer(Vector _playerPosition, Vector _playerDirection, double _moveSpeed, double _rotSpeed)
        {
            this.playerPosition = _playerPosition;
            this.playerDirection = _playerDirection;
            this.moveSpeed = _moveSpeed;
            this.rotSpeed = _rotSpeed;
            RayCaster.UpdateCameraPlane(this);
        }
        public void MovePlayer()
        {
            if (this.forward)
            {
                this.Move(true);
            }

            if (this.back)
            {
                this.Move(false);
            }

            if (this.left)
            {
                this.Turn(false);
            }

            if (this.right)
            {
                this.Turn(true);
            }
        }

        public void Move(bool forwards)
        {
            Debug.WriteLine("x");
            Debug.WriteLine(playerPosition.x);
            Debug.WriteLine("y");
            Debug.WriteLine(playerPosition.y);
            Debug.WriteLine("worldMap");
            Debug.WriteLine(worldMap[(int)playerPosition.x, (int)playerPosition.y]);
            Debug.WriteLine("");
            if (forwards)
            {
                // First we check that moving wont put us in a wall
                if (worldMap[(int)(playerPosition.x + playerDirection.x * moveSpeed), (int)(playerPosition.y)] <= 0)
                {
                    // If it doesnt put us in a wall, we can move forwards (or backwards).
                    playerPosition.x += playerDirection.x * moveSpeed;
                }
                if (worldMap[(int)(playerPosition.x), (int)(playerPosition.y + playerDirection.y * moveSpeed)] <= 0)
                {
                    playerPosition.y += playerDirection.y * moveSpeed;
                }
            }
            else
            {
                if (worldMap[(int)(playerPosition.x - playerDirection.x * moveSpeed), (int)(playerPosition.y)] <= 0)
                {
                    playerPosition.x -= playerDirection.x * moveSpeed;
                }
                if (worldMap[(int)(playerPosition.x), (int)(playerPosition.y - playerDirection.y * moveSpeed)] <= 0)
                {
                    playerPosition.y -= playerDirection.y * moveSpeed;
                }
            }
        }

        public void Turn(bool turnRight)
        {
            if (!turnRight)
            {
                // We use a rotation matrix to rotate the plane and direction vectors.
                // First we keep track of the old direction, so that the transformation on X first
                // doesn't affect the Y transformation.
                Vector oldDirection = new Vector(playerDirection.x, playerDirection.y);
                playerDirection.x = (playerDirection.x * Math.Cos(rotSpeed) - playerDirection.y * Math.Sin(rotSpeed));
                playerDirection.y = (oldDirection.x * Math.Sin(rotSpeed) + playerDirection.y * Math.Cos(rotSpeed));
                RayCaster.CameraPlane_Correction(turnRight, rotSpeed);
            }
            else
            {
                Vector oldDirection = new Vector(playerDirection.x, playerDirection.y);
                playerDirection.x = (playerDirection.x * Math.Cos(-rotSpeed) - playerDirection.y * Math.Sin(-rotSpeed));
                playerDirection.y = (oldDirection.x * Math.Sin(-rotSpeed) + playerDirection.y * Math.Cos(-rotSpeed));
                RayCaster.CameraPlane_Correction(turnRight, rotSpeed);
            }
        }
    }

    public static class RayCaster
    {
        public static Vector cameraPlane = new Vector(0, -1);

        public static void CameraPlane_Correction(bool turnRight, double RotSpeed)
        {
            if (!turnRight)
            {
                // We use a rotation matrix to rotate the plane and direction vectors.
                // First we keep track of the old direction, so that the transformation on X first
                // doesn't affect the Y transformation.
                Vector oldPlane = new Vector(cameraPlane.x, cameraPlane.y);
                cameraPlane.x = (cameraPlane.x * Math.Cos(RotSpeed) - cameraPlane.y * Math.Sin(RotSpeed));
                cameraPlane.y = (oldPlane.x * Math.Sin(RotSpeed) + cameraPlane.y * Math.Cos(RotSpeed));
            }
            else
            {
                Vector oldPlane = new Vector(cameraPlane.x, cameraPlane.y);
                cameraPlane.x = (cameraPlane.x * Math.Cos(-RotSpeed) - cameraPlane.y * Math.Sin(-RotSpeed));
                cameraPlane.y = (oldPlane.x * Math.Sin(-RotSpeed) + cameraPlane.y * Math.Cos(-RotSpeed));
            }
        }

        public static DrawingImage draw(int[,] worldMap, Vector playerPosition, Vector playerDirection)
        {
            int width = 640;
            int height = 480;

            var drawing = new DrawAssistant();
            drawing.BackgroundBrush = Color.FromArgb(255, 200, 200, 200);
            drawing.BorderBrush = Color.FromArgb(255, 200, 200, 200);
            drawing.BorderThickness = 1;
            drawing.DrawRectangle(new Point(640, 0),
                                  new Point(0, 0),
                                  new Point(0, 480),
                                  new Point(640, 480));

            for (int i = 0; i < width; i++)
            {
                // This var tracks the relative position of the ray on the camera plane, from -1 to 1, with 0 being screen centre
                // so that we can use it to muliply the half-length of the camera plane to get the right direction of the ray.
                double cameraX = 2 * (i / Convert.ToDouble(width)) - 1;
                // This vector holds the direction the current ray is pointing.
                Vector rayDir = new Vector(
                    playerDirection.x + cameraPlane.x * cameraX,
                    playerDirection.y + cameraPlane.y * cameraX);

                // This holds the absolute SQUARE of the map the ray is in, regardless of position
                // within that square.
                int mapX = (int)playerPosition.x;
                int mapY = (int)playerPosition.y;
                // These two variables track the distance to the next side of a map square from the player, 
                // e.g where the ray touches the horizontal side of a square, the distance is sideDistX and vertical square sideDistY.
                double sideDistX;
                double sideDistY;
                // These two variables are the distance between map square side intersections
                double deltaDistX = Math.Abs(1 / rayDir.x);
                double deltaDistY = Math.Abs(1 / rayDir.y);
                // This var is for the overall length of the ray calculations
                double perpWallDist;

                // Each time we check the next square we step either 1 in the x or 1 in the y, they will be 1 or -1 depending on whether 
                // the character is facing towards the origin or away.
                int stepX;
                int stepY;

                // Finally, these two track whether a wall was hit, and the side tracks which side, horizontal or vertical was hit.
                // A horizontal side givess 0 and a vertical side is 1.
                bool hit = false;
                int side = 0;

                // Now we calculate the way we will step based upon the direction the character is facing
                // And the initial sideDist based upon this direction, and the deltaDist
                if (rayDir.x < 0)
                {
                    stepX = -1;
                    sideDistX = (playerPosition.x - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0 - playerPosition.x) * deltaDistX;
                }
                if (rayDir.y < 0)
                {
                    stepY = -1;
                    sideDistY = (playerPosition.y - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0 - playerPosition.y) * deltaDistY;
                }

                // Now we loop steping until we hit a wall
                while (!hit)
                {
                    // Here we check which distance is closer to us, x or y, and increment the lesser
                    if (sideDistX < sideDistY)
                    {
                        // Increase the distance we've travelled.
                        sideDistX += deltaDistX;
                        // Set the ray's mapX to the new square we've reached.
                        mapX += stepX;
                        // Set it so the side we're currently on is an X side.
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }
                    // Check if the ray is not on the side of a square that is a wall.
                    if (worldMap[mapX, mapY] > 0)
                    {
                        hit = true;
                    }
                }

                // Now we've found where the next wall is, we have to find the actual distance.
                if (side == 0)
                {
                    perpWallDist = ((mapX - playerPosition.x + ((1 - stepX) / 2)) / rayDir.x);
                }
                else
                {
                    perpWallDist = ((mapY - playerPosition.y + ((1 - stepY)) / 2)) / rayDir.y;
                }

                // Here we'll start drawing the column of pixels, now we know what, and how far away.
                // First we find the height of the wall, e.g how much of the screen it should take up
                int columnHeight = (int)(height / perpWallDist);
                // Next we need to find where to start drawing the column and where to stop, since the walls
                // will be in the centre of the screen, finding the start and end is quite simple.
                int drawStart = ((height / 2) + (columnHeight / 2));
                // If we are going to be drawing off-screen, then draw just on screen.
                if (drawStart >= height)
                {
                    drawStart = height - 1;
                }
                int drawEnd = ((height / 2) - (columnHeight / 2));
                if (drawEnd < 0)
                {
                    drawEnd = 0;
                }

                //Debug.WriteLine(columnHeight);
                double colorfactror = (double)Math.Abs(drawEnd - drawStart) / 480;
                if (colorfactror > 1)
                    colorfactror = 1;
                if (colorfactror < 0) 
                    colorfactror = 0;
                colorfactror *= colorfactror * colorfactror;
                //Debug.WriteLine(columnHeight);

                // Now we pick the colour to draw the line in, this is based upon the colour of the wall
                // and is then made darker if the wall is x aligned or y aligned.

                Random rand = new Random();

                switch (worldMap[mapX, mapY])
                {

                    case 1:
                        drawing.BorderBrush = Color.FromArgb((byte)(255 * colorfactror), 0, 102, 102);
                        break;
                        
                    case 2:
                        drawing.BorderBrush = Color.FromArgb((byte)(255 * colorfactror), 0, 255, 0);
                        break;
                }
                drawing.DrawLine(new Point(i, drawStart), new Point(i, drawEnd));
            }

            return new DrawingImage(drawing.drawing);
        }

        public static void UpdateCameraPlane(Player player, double scale = 1.0)
        {
            cameraPlane.x = player.playerDirection.y * scale;
            cameraPlane.y = -player.playerDirection.x * scale;
        }

        public static BitmapImage ReturnWinImage()
        {
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string imagePath = System.IO.Path.Combine(projectRoot, "img", "win.png");
            if (System.IO.File.Exists(imagePath))
            {
                // Создание BitmapImage и загрузка изображения
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();
            
                // Назначение изображения элементу Image
                return bitmap;
            }
            else
            {
                MessageBox.Show("Изображение не найдено по указанному пути: " + imagePath);
            }
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri("win.png", UriKind.RelativeOrAbsolute);
            myBitmapImage.DecodePixelWidth = 200;
            myBitmapImage.EndInit();
            return myBitmapImage;
        }
    }

    public partial class MainWindow : Window
    {
        Game game = new Game();

        public bool restart = false;
        public MainWindow()
        {
            InitializeComponent();

            UpdateImage(RayCaster.draw(game.worldMap, game.player.playerPosition, game.player.playerDirection));
            this.KeyDown += MainWindow_KeyDown;
            StartBackgroundTask();
            // Запускаем таймер для обработки движения персонажа
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(20); // Установка интервала обновления
            timer.Tick += Timer_Tick;
            timer.Start();

        }

        private async Task StartBackgroundTask()
        {
            while (true) 
            { 
                await Task.Delay(5);
                boardcheck();
            }
            
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, какая клавиша была нажата
            if (e.Key == Key.Up)
            {
                game.player.forward = true;
            }
            if (e.Key == Key.Down)
            {
                game.player.back = true;
            }
            if (e.Key == Key.Left)
            {
                game.player.left = true;
            }
            if (e.Key == Key.Right)
            {
                game.player.right = true;
            }
            if ((e.Key == Key.R) && (!game.InGame))
            {
                restart = true;
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (game.InGame)
            {
                game.player.MovePlayer();
                UpdateImage(RayCaster.draw(game.worldMap, game.player.playerPosition, game.player.playerDirection));
            }
            else
            {
                UpdateImage(RayCaster.ReturnWinImage());
                if (restart)
                {
                    game.Restart();
                    restart = false;
                    //restart = false;
                }
            }
            if (game.CheckWin())
            {
                game.InGame = false;
            }
        }

        void boardcheck()
        {
            if (Keyboard.IsKeyUp(Key.Up))
                game.player.forward = false;
            if (Keyboard.IsKeyUp(Key.Down))
                game.player.back = false;
            if (Keyboard.IsKeyUp(Key.Left))
                game.player.left = false;
            if (Keyboard.IsKeyUp(Key.Right))
                game.player.right = false;
        }

        private void UpdateImage(DrawingImage drawing)
        {
            Screen.Source = drawing;
        }
        private void UpdateImage(BitmapImage drawing)
        {
            Screen.Source = drawing;
        }
    }
}
