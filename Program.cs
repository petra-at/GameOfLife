using System;
using System.Threading;

/*
 * RULES:
 * If cell is alive and neighbours >= 4 cell = dead
 * If cell is alive and neighbours <= 1 cell = dead
 * - - - - - - - - - - - - - - - - - - - - - - - -
 * If cell is dead and neighbours == 3 cell = alive
 *
 *
 *  #
 *   #
 * ###
 */

namespace GOL
{
    class Program
    {
        static void Main(string[] args)
        {

            int width = 40, height = 35, borderwidth = 1;
            char livingCell = '.', deadCell = ' ';

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine("Manual settings [Y / N]");
            if (Console.ReadKey(true).Key == ConsoleKey.Y)
            {
                Console.CursorTop = 0;
                Console.WriteLine("Width        :           ");
                Console.WriteLine("Height       :           ");
                Console.WriteLine("Border Width :           ");
                Console.WriteLine("Living Cell  :           ");
                Console.WriteLine("Dead Cell    :           ");

                Console.CursorTop = 0;
                Console.SetCursorPosition(15, Console.CursorTop);
                width = Int32.Parse(Console.ReadLine());
                Console.SetCursorPosition(15, Console.CursorTop);
                height = Int32.Parse(Console.ReadLine());
                Console.SetCursorPosition(15, Console.CursorTop);
                borderwidth = Int32.Parse(Console.ReadLine());
                Console.SetCursorPosition(15, Console.CursorTop);
                livingCell = Char.Parse(Console.ReadLine());
                Console.SetCursorPosition(15, Console.CursorTop);
                deadCell = Char.Parse(Console.ReadLine());
            }

            GameOfLife.CreateInstance(width, height, borderwidth, ConsoleColor.White, ConsoleColor.Black, ConsoleColor.Green, livingCell, deadCell, 2);
            GameOfLife.SelectionColor = ConsoleColor.Green;
            GameOfLife.SelectionColor2 = ConsoleColor.DarkGreen;
            /* You can call either
             * GameOfLife.RandomFill();
             * or
             * GameOfLife.GetDrawing();
             * or
             * GameOfLife.RandomFill();
             * GameOfLife.GetDrawing();
             */
            GameOfLife.RandomFill();
            GameOfLife.GetDrawing();
            GameOfLife.Start();
        }
    }
    static class GameOfLife
    {
        #region Fields and Properties
        public static int Width { get; set; } // Width of console and array
        public static int Height { get; set; } // Height of console and array
        public static int BorderWidth { get; set; } // Width of the border (from top, bottom, left and right)
        public static ConsoleColor BorderColor { get; private set; } // Color of the border
        public static ConsoleColor BackColor { get; private set; } // Background color of the game
        public static ConsoleColor LivingCellColor { get; private set; } // Color of a living cell
        public static ConsoleColor DeadCellColor { get; private set; } // Color of a dead cell

        // If the selection point is on a dead cell use this color:
        public static ConsoleColor SelectionColor { get; set; }
        // If the selection point is on a living cell use this color:
        public static ConsoleColor SelectionColor2 { get; set; }
        // Selecteion character
        public static char SelectionCell { get; set; }

        public static int RandomCellDensity { get; set; } // How offten the cell is going to be set
        public static int Delay { get; set; } // Sets delay with time

        public static char LivingCell { get; set; } // Living cell character
        public static char DeadCell { get; set; } // Dead cell character

        static char[,] current; // Current state of the game
        static char[,] comming; // The state that is going to be on the next generation

        static Random random = new Random(); // For random values
        #endregion

        /* Create instance with most necessary parameters */
        public static void CreateInstance(int width, int height, int borderWidth,
            ConsoleColor borderColor, ConsoleColor backColor, ConsoleColor cellColor,
            char livingCell, char deadCell, int randomCellDensity)
        {
            Width = width;
            Height = height;
            BorderWidth = borderWidth;
            BorderColor = borderColor;
            BackColor = backColor;
            LivingCellColor = cellColor;
            LivingCell = livingCell;
            DeadCell = deadCell;
            RandomCellDensity = randomCellDensity;

            /* Setting defaults */
            SelectionColor = cellColor;
            SelectionColor2 = backColor;
            SelectionCell = 'X';
            DeadCellColor = LivingCellColor;

            /* Setup Console */
            Console.CursorVisible = false;
            Console.SetWindowSize(Width, Height);
            Console.SetBufferSize(Width, Height);

            /* Setup arrays */
            current = new char[Width, Height];
            comming = new char[Width, Height];

            /* In order to color the border I use a need trick:
             * I set the background to the border color and then
             * I clear the screen, and the Console.Clear() method
             * clears the screen by drawing empty spaces with the set
             * background color.
             */
            Console.BackgroundColor = BorderColor;
            Console.Clear();

            /* And then set the defaults for the game */
            Console.ForegroundColor = LivingCellColor;
            Console.BackgroundColor = BackColor;

            /* Fill array with dead cells so it doesn't get exceptions and other problems*/
            for (int x = BorderWidth; x < Width - BorderWidth; x++)
            {
                for (int y = BorderWidth; y < Height - BorderWidth; y++)
                {
                    current[x, y] = comming[x, y] = DeadCell;
                    /* Here I draw all because else the border color will be seen
                     * If you don't get it,
                     * comment these two lines of code and than look at it
                     */
                    Console.SetCursorPosition(x, y);
                    Console.Write(current[x, y]);
                }
            }
        }

        // Starts the game
        public static void Start()
        {
            int foundAliveCells = 0;
            int generation = 0;
            int safeDraw = 0; // safe draw is when you set the border with to 0

            // Setting colors
            Console.ForegroundColor = LivingCellColor;
            Console.BackgroundColor = BackColor;

            if (BorderWidth == 0)
                safeDraw = 1;

            while (true)
            {
                Array.Copy(current, comming, Width * Height);
                for (int y = BorderWidth; y < Height - BorderWidth - safeDraw; y++)
                {
                    for (int x = BorderWidth; x < Width - BorderWidth; x++)
                    {
                        /* Here I draw the CURRENT state of the game*/
                        Console.SetCursorPosition(x, y);
                        Console.Write(current[x, y]);

                        // And there go the calculations
                        /* Here it's counting the neighbours */
                        /* It goes in a squere around the cell but not checking the cell itself*/
                        for (int i = -1; i < 2; i++)
                            for (int j = -1; j < 2; j++)
                            {
                                if (!(i == 0 && j == 0) && // We only need the neighbours so here it checks it
                                    ((x + i >= BorderWidth && x + i < Width - BorderWidth) && // For exception handeling
                                    (y + j >= BorderWidth && y + j < Height - BorderWidth)))
                                    if (current[x + i, y + j] == LivingCell) // if the cell is alive foundAliveCells increments by 1
                                        foundAliveCells++;
                            }

                        // Checking rules
                        if (current[x, y] == LivingCell && (foundAliveCells >= 4 || foundAliveCells <= 1))
                            comming[x, y] = DeadCell;
                        else if (current[x, y] == DeadCell && foundAliveCells == 3)
                            comming[x, y] = LivingCell;

                        // reseting
                        foundAliveCells = 0;
                    }
                }
                /* copies the edited 'comming array' (the state of the game in the next generation)
                 * to the 'current' array (which is going to be drawn)
                 */
                Array.Copy(comming, current, Width * Height);

                Console.Title = "Generation: " + generation;
                generation++; // generation incrementing

                Thread.Sleep(Delay); // Delay...
            }
        }

        // Simply fill randomly the map
        public static void RandomFill()
        {
            Console.ForegroundColor = LivingCellColor;

            for (int x = BorderWidth; x < Width - BorderWidth; x++)
            {
                for (int y = BorderWidth; y < Height - BorderWidth; y++)
                {
                    if (random.Next(RandomCellDensity) == 0)
                        current[x, y] = comming[x, y] = LivingCell;
                    else
                        current[x, y] = comming[x, y] = DeadCell;

                    Console.SetCursorPosition(x, y);
                    Console.Write(current[x, y]);
                }
            }

        }

        // The user sets up everything
        public static void GetDrawing()
        {
            ConsoleKeyInfo input = new ConsoleKeyInfo();
            int x = Width / 2, y = Height / 2; // Center coordinates

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = SelectionColor;
            if (current[x, y] == LivingCell)
            {
                Console.ForegroundColor = SelectionColor2;
                Console.BackgroundColor = LivingCellColor;
            }
            Console.Write(SelectionCell);

            while (input.Key != ConsoleKey.Enter) // as long as the user doesn't press enter the loop goes on
            {
                // Getting the keypress
                input = Console.ReadKey(true);

                // Drawing empty space where the selection previously was
                Console.ForegroundColor = LivingCellColor;
                Console.BackgroundColor = BackColor;
                Console.SetCursorPosition(x, y);
                Console.Write(current[x, y]);

                // Simple input checking
                switch (input.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (y > BorderWidth)
                            y -= 1;
                        break;

                    case ConsoleKey.DownArrow:
                        if (y + 1 < Height - BorderWidth)
                            y += 1;
                        break;

                    case ConsoleKey.RightArrow:
                        if (x + 1 < Width - BorderWidth)
                            x += 1;
                        break;

                    case ConsoleKey.LeftArrow:
                        if (x > BorderWidth)
                            x -= 1;
                        break;

                    case ConsoleKey.Spacebar: // Sets the cell to dead if it's alive and alive if it's dead
                        current[x, y] = current[x, y] == DeadCell ? LivingCell : DeadCell;
                        break;
                }

                // Drawing the selection
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = SelectionColor;
                if (current[x, y] == LivingCell)
                {
                    Console.ForegroundColor = SelectionColor2;
                    Console.BackgroundColor = LivingCellColor;
                }
                Console.Write(SelectionCell);
            }

            // If the user pressed enter he got out of the while loop
            // which means he finished and so the game starts:
            Start();
        }
    }
}