using System.Data;
using System.Net.Quic;
using System.Security.Cryptography;
using RNG = System.Security.Cryptography.RandomNumberGenerator;

namespace M2 {
    internal class Program {

        static int[,] field;
        static int xSize, ySize;
        static int playerX, playerY, playerMovingToX, playerMovingToY;
        static int goalX, goalY;
        static int lv = 0;
        static int enemyCount;
        static int[] enemyX, enemyY, enemyMovingToX, enemyMovingToY, enemyType;
        static int gameState = GAMESTATE_NORMAL;
        static int lostTo = 0;

        const int FLOOR = 0;
        const int WALL = 1;

        const ConsoleColor GOOD_COLOUR = ConsoleColor.Green;
        const ConsoleColor BAD_COLOUR = ConsoleColor.DarkRed;

        const ConsoleColor FLOOR_COLOUR = ConsoleColor.Black;
        const ConsoleColor WALL_COLOUR = ConsoleColor.DarkGray;

        const string ENEMY_SYMBOLS = "0i";

        const int FOE_TO_PLAYER = 0;
        const int FOE_RANDOM_TOGGLE_WALL = 1;
        const int FOE_TYPES_COUNT = 2;

        const int GAMESTATE_NORMAL = 0;
        const int GAMESTATE_LOSE = 1;
        const int GAMESTATE_WON = 2;

        static int[] foesAffectedByWalls = [FOE_TO_PLAYER];

        static void Main(string[] args) {
            Console.Clear();
            Console.CursorVisible = false;


            createLevel();


            while (true) {
                if (gameState == GAMESTATE_NORMAL) {
                    ConsoleKeyInfo k = Console.ReadKey(true);
                    switch (k.Key) {
                        case ConsoleKey.UpArrow:
                            playerMovingToX = playerX;
                            playerMovingToY = playerY - 1;
                            break;
                        case ConsoleKey.RightArrow:
                            playerMovingToX = playerX + 1;
                            playerMovingToY = playerY;
                            break;
                        case ConsoleKey.DownArrow:
                            playerMovingToX = playerX;
                            playerMovingToY = playerY + 1;
                            break;
                        case ConsoleKey.LeftArrow:
                            playerMovingToX = playerX - 1;
                            playerMovingToY = playerY;
                            break;
                        case ConsoleKey.C:
                            throw new Exception();
                    }

                    clear();
                    update();
                    drawMap();
                } else if (gameState == GAMESTATE_LOSE) {
                    fillBoard(ENEMY_SYMBOLS[lostTo], false);
                } else {
                    fillBoard('O', true);
                }
            }
        }

        private static void createLevel() {
            gameState = GAMESTATE_NORMAL;
            xSize = Console.WindowWidth;
            ySize = Console.WindowHeight;
            field = new int[xSize, ySize];

            Console.BackgroundColor = WALL_COLOUR;
            for (int x = 0; x < xSize; x++) {
                for (int y = 0; y < ySize; y++) {
                    if (RNG.GetInt32(100) < 10 + (2 * lv)) {
                        field[x, y] = WALL;
                        Console.CursorLeft = x;
                        Console.CursorTop = y;
                        Console.Write(' ');
                    }
                }
            }
            Console.ResetColor();


            do {
                playerX = RNG.GetInt32(xSize);
                playerY = RNG.GetInt32(ySize);
            } while (field[playerX, playerY] == WALL);
            Console.ForegroundColor = GOOD_COLOUR;
            Console.CursorLeft = playerX;
            Console.CursorTop = playerY;
            Console.Write('T');

            do {
                goalX = RNG.GetInt32(xSize);
                goalY = RNG.GetInt32(ySize);
            } while ((field[goalX, goalY] == WALL) || (goalX == playerX && goalY == playerY));
            Console.CursorLeft = goalX;
            Console.CursorTop = goalY;
            Console.Write('O');


            enemyCount = (2 * lv) + 5;
            enemyX = new int[enemyCount];
            enemyY = new int[enemyCount];
            enemyMovingToX = new int[enemyCount];
            enemyMovingToY = new int[enemyCount];
            enemyType = new int[enemyCount];

            Console.ForegroundColor = BAD_COLOUR;
            for (int i = 0; i < enemyCount; i++) {
                enemyType[i] = RNG.GetInt32(FOE_TYPES_COUNT);
                do {
                    enemyX[i] = RNG.GetInt32(xSize);
                    enemyY[i] = RNG.GetInt32(ySize);
                } while ((enemyX[i] == playerX && enemyY[i] == playerY) || field[enemyX[i], enemyY[i]] == WALL);
                Console.CursorLeft = enemyX[i];
                Console.CursorTop = enemyY[i];
                Console.Write(ENEMY_SYMBOLS[enemyType[i]]);
            }


            Console.ResetColor();
        }

        private static void clear() {
            Console.CursorLeft = playerX;
            Console.CursorTop = playerY;
            Console.Write(' ');
            Console.CursorLeft = goalX;
            Console.CursorTop = goalY;
            Console.Write(' ');
            for (int i = 0; i < enemyCount; i++) {
                Console.CursorLeft = enemyX[i];
                Console.CursorTop = enemyY[i];
                Console.ForegroundColor = field[enemyX[i], enemyY[i]] == WALL ? WALL_COLOUR : FLOOR_COLOUR;
                Console.Write(' ');
            }
            Console.ResetColor();
        }

        private static void update() {
            if (playerMovingToX > -1 && playerMovingToY > -1 && playerMovingToX < xSize && playerMovingToY < ySize) {
                if (field[playerMovingToX, playerMovingToY] != WALL) {
                    playerX = playerMovingToX;
                    playerY = playerMovingToY;
                }
            }
            field[goalX, goalY] = FLOOR;
            for (int i = 0; i < enemyCount; i++) {
                switch (enemyType[i]) {
                    case FOE_TO_PLAYER:
                        bool xDiff = enemyX[i] != playerX, yDiff = enemyY[i] != playerY;
                        if (xDiff && yDiff)
                            if ((RNG.GetInt32(100) % 2) == 0)
                                xDiff = false;
                            else
                                yDiff = false;
                        if (xDiff) {
                            if (playerX > enemyX[i])
                                enemyMovingToX[i] = enemyX[i] + 1;
                            else
                                enemyMovingToX[i] = enemyX[i] - 1;
                            enemyMovingToY[i] = enemyY[i];
                        }
                        if (yDiff) {
                            if (playerY > enemyY[i])
                                enemyMovingToY[i] = enemyY[i] + 1;
                            else
                                enemyMovingToY[i] = enemyY[i] - 1;
                            enemyMovingToX[i] = enemyX[i];
                        }
                        break;
                    case FOE_RANDOM_TOGGLE_WALL:
                        enemyMovingToX[i] = RNG.GetInt32(xSize);
                        enemyMovingToY[i] = RNG.GetInt32(ySize);
                        if (field[enemyMovingToX[i], enemyMovingToY[i]] == WALL)
                            field[enemyMovingToX[i], enemyMovingToY[i]] = FLOOR;
                        else
                            field[enemyMovingToX[i], enemyMovingToY[i]] = WALL;
                        break;
                }

                if (enemyMovingToX[i] > -1 && enemyMovingToY[i] > -1 && enemyMovingToX[i] < xSize && enemyMovingToY[i] < ySize) {
                    if (!(field[enemyMovingToX[i], enemyMovingToY[i]] == WALL && foesAffectedByWalls.Contains(enemyType[i]))) {
                        enemyX[i] = enemyMovingToX[i];
                        enemyY[i] = enemyMovingToY[i];
                    }
                    if (enemyX[i] == playerX && enemyY[i] == playerY) {
                        gameState = GAMESTATE_LOSE;
                        lostTo = enemyType[i];
                    }
                }
            }

            if (playerY == goalY && playerX == goalX) {
                gameState = GAMESTATE_WON;
            }
        }

        private static void drawMap() {
            Console.BackgroundColor = WALL_COLOUR;
            for (int x = 0; x < xSize; x++) {
                for (int y = 0; y < ySize; y++) {
                    if (field[x, y] == WALL) {
                        Console.CursorLeft = x;
                        Console.CursorTop = y;
                        Console.Write(' ');
                    }
                }
            }
            Console.ResetColor();

            Console.ForegroundColor = GOOD_COLOUR;
            Console.CursorLeft = playerX;
            Console.CursorTop = playerY;
            Console.Write('T');
            Console.CursorLeft = goalX;
            Console.CursorTop = goalY;
            Console.Write('O');


            for (int i = 0; i < enemyCount; i++) {
                Console.BackgroundColor = field[enemyX[i], enemyY[i]] == WALL ? WALL_COLOUR : FLOOR_COLOUR;
                Console.ForegroundColor = field[enemyX[i], enemyY[i]] == WALL ? FLOOR_COLOUR : BAD_COLOUR;
                Console.CursorLeft = enemyX[i];
                Console.CursorTop = enemyY[i];
                Console.Write(ENEMY_SYMBOLS[enemyType[i]]);
            }

            Console.ResetColor();
        }
        private static void fillBoard(char spam, bool isWin) {
            for (int y = 0; y < ySize; y++)
                for (int x = 0; x < xSize; x++) {
                    if (isWin) {
                        int fg = RNG.GetInt32(16);
                        int bg = RNG.GetInt32(16);
                        while (bg == fg) {
                            bg = RNG.GetInt32(16);
                        }
                        Console.ForegroundColor = getConsoleColour(fg);
                        Console.BackgroundColor = getConsoleColour(bg);
                    } else {
                        Console.ForegroundColor = BAD_COLOUR;
                        Console.BackgroundColor = FLOOR_COLOUR;
                    }
                    Console.CursorLeft = x;
                    Console.CursorTop = y;
                    Console.Write(spam);
                }
            Console.ResetColor();

            Console.ReadKey(true);
            Console.Clear();
            if (isWin)
                lv++;
            createLevel();
        }

        private static ConsoleColor getConsoleColour(int i) {
            return (i % 16) switch {
                0 => ConsoleColor.Black,
                1 => ConsoleColor.Blue,
                2 => ConsoleColor.Cyan,
                3 => ConsoleColor.DarkBlue,
                4 => ConsoleColor.DarkCyan,
                5 => ConsoleColor.DarkGray,
                6 => ConsoleColor.DarkGreen,
                7 => ConsoleColor.DarkMagenta,
                8 => ConsoleColor.DarkRed,
                9 => ConsoleColor.DarkYellow,
                10 => ConsoleColor.Gray,
                11 => ConsoleColor.Green,
                12 => ConsoleColor.Magenta,
                13 => ConsoleColor.Red,
                14 => ConsoleColor.White,
                _ => ConsoleColor.Yellow,
            };
        }
    }
}
