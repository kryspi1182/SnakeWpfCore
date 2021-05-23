using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakeWpfCore
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //player variables
        Point playerPosition;
        Direction playerDirection;
        int score;

        //snake variables
        List<Point> snakePoints;
        SolidColorBrush snakeColor;
        int snakeSegmentSize;
        int snakeLength;

        //fruit variables
        SolidColorBrush fruitColor;
        Point fruitPosition;

        //game variables
        Random rand;
        DispatcherTimer timer;
        string[] imageLinks =
        {
            "https://cdn.pixabay.com/photo/2014/07/30/19/28/king-cobra-405623__340.jpg",
            "https://cdn.pixabay.com/photo/2017/01/12/10/45/snake-1974382__340.jpg",
            "https://cdn.pixabay.com/photo/2015/02/28/15/25/rattlesnake-653642__340.jpg",
            "https://cdn.pixabay.com/photo/2014/12/25/14/54/snake-579682__340.jpg",
            "https://cdn.pixabay.com/photo/2015/02/28/15/25/snake-653639_1280.jpg"
        };
        public MainWindow()
        {
            InitializeComponent();

            score = 0;
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = new TimeSpan(700000);
            this.KeyDown += new KeyEventHandler(KeyPressed);
            timer.Start();
            rand = new Random();
            CreateSnake();
            CreateFruit();
        }

        private void CreateSnake() //draw snake at the start of the game
        {
            snakePoints = new List<Point>();
            snakeColor = new SolidColorBrush();
            snakeColor.Color = Color.FromRgb(0, 0, 0);
            snakeSegmentSize = 10;
            snakeLength = 4;
            playerPosition = new Point();
            playerPosition.X = 100;
            playerPosition.Y = 100;
            for (int i = 0; i < snakeLength; i++)
            {
                playerPosition.Y += i * snakeSegmentSize;
                PaintSnake();
            }
            playerDirection = Direction.Up;
        }

        private void CreateFruit() //draw fruit at the start of the game
        {
            fruitColor = new SolidColorBrush();
            fruitColor.Color = Color.FromRgb(255, 0, 0);
            fruitPosition = new Point();
            fruitPosition.X = 200;
            fruitPosition.Y = 200;
            PaintFruit();
        }

        private void CutTail()
        {
            /*
             *we assume that there is always one fruit on the map present while moving,
             *if player eats the fruit, the game adds a snake segment without cutting the tail
             */
            int segmentCount = GameCanvas.Children.Count - 1;
            if (segmentCount > snakeLength)
            {
                GameCanvas.Children.RemoveAt(1);
                snakePoints.RemoveAt(0);
            }


        }

        private void PaintSnake() //draw snake
        {
            Rectangle bodySegment = new Rectangle();
            bodySegment.Fill = snakeColor;
            bodySegment.Width = snakeSegmentSize;
            bodySegment.Height = snakeSegmentSize;

            Canvas.SetTop(bodySegment, playerPosition.Y);
            Canvas.SetLeft(bodySegment, playerPosition.X);

            GameCanvas.Children.Add(bodySegment);
            snakePoints.Add(playerPosition);
            CutTail();

        }

        private void PaintFruit() //draw fruit
        {
            Ellipse fruit = new Ellipse();
            fruit.Fill = fruitColor;
            fruit.Width = snakeSegmentSize; //fruit is simillar sized as a snake fragment
            fruit.Height = snakeSegmentSize;

            Canvas.SetTop(fruit, fruitPosition.Y);
            Canvas.SetLeft(fruit, fruitPosition.X);
            GameCanvas.Children.Insert(0, fruit);

        }

        private void TimerTick(object sender, EventArgs e) //mechanics checked every interval
        {
            //set snake movement according to the direction chosen by player
            switch (playerDirection)
            {
                case Direction.Up:
                    playerPosition.Y -= snakeSegmentSize;
                    PaintSnake();
                    break;

                case Direction.Down:
                    playerPosition.Y += snakeSegmentSize;
                    PaintSnake();
                    break;

                case Direction.Left:
                    playerPosition.X -= snakeSegmentSize;
                    PaintSnake();
                    break;

                case Direction.Right:
                    playerPosition.X += snakeSegmentSize;
                    PaintSnake();
                    break;
            }

            //check if snake is eating fruit
            if ((Math.Abs(fruitPosition.X - playerPosition.X) < snakeSegmentSize) && (Math.Abs(fruitPosition.Y - playerPosition.Y) < snakeSegmentSize))
            {
                DrawBackground();
                snakeLength++;
                score += 100;
                GameCanvas.Children.RemoveAt(0);
                GenerateFruitPoint();
                PaintFruit();
            }

            //check if snake is touching walls
            if (playerPosition.X < snakeSegmentSize || playerPosition.X > GameCanvas.Width - snakeSegmentSize || playerPosition.Y < snakeSegmentSize || playerPosition.Y > GameCanvas.Height - snakeSegmentSize)
            {
                GameOver();
            }

            //check if snake head is touching any segment apart from it's neighbour
            for (int i = 0; i < snakePoints.Count - 1; i++)
            {
                if ((Math.Abs(playerPosition.X - snakePoints[i].X) < snakeSegmentSize) && (Math.Abs(playerPosition.Y - snakePoints[i].Y) < snakeSegmentSize))
                {
                    GameOver();
                }
            }


        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (playerDirection != Direction.Down)
                        playerDirection = Direction.Up;
                    break;

                case Key.Down:
                    if (playerDirection != Direction.Up)
                        playerDirection = Direction.Down;
                    break;

                case Key.Left:
                    if (playerDirection != Direction.Right)
                        playerDirection = Direction.Left;
                    break;

                case Key.Right:
                    if (playerDirection != Direction.Left)
                        playerDirection = Direction.Right;
                    break;
            }
        }

        private void GameOver()
        {
            timer.Stop();
            GameCanvas.Children.Clear();
            TextBlock text = new TextBlock();
            text.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            text.FontSize = 64;
            text.Text = "Game over! \n Your score: " + score;
            Canvas.SetTop(text, GameCanvas.Height/3);
            Canvas.SetLeft(text, GameCanvas.Width/3);
            GameCanvas.Children.Add(text);
        }

        private void DrawBackground() //download image as background
        {
            int imageIndex = rand.Next(0, 5);
            Uri imageUri = new Uri(imageLinks[imageIndex], UriKind.Absolute);
            BitmapImage bitmap = new BitmapImage(imageUri);
            BackgroundImage.ImageSource = bitmap;
        }

        private void GenerateFruitPoint() //choose new fruit point that is not in the same place as the snake
        {
            Point point = new Point();
            bool generatedX = false;
            bool generatedY = false;

            while(!generatedX)
            {
                point.X = rand.Next(snakeSegmentSize*2, (int)(GameCanvas.Width - snakeSegmentSize*2));
                foreach(Point snakePoint in snakePoints)
                {
                    if (Math.Abs(snakePoint.X - point.X) < snakeSegmentSize)
                    {
                        generatedX = false;
                        break;
                    }
                    generatedX = true;   
                }
            }

            while (!generatedY)
            {
                point.Y = rand.Next(snakeSegmentSize*2, (int)(GameCanvas.Height - snakeSegmentSize*2));
                foreach (Point snakePoint in snakePoints)
                {
                    if (Math.Abs(snakePoint.Y - point.Y) < snakeSegmentSize)
                    {
                        generatedY = false;
                        break;
                    }
                    generatedY = true;
                }
            }

            fruitPosition.X = point.X;
            fruitPosition.Y = point.Y;
        }
    }
}
