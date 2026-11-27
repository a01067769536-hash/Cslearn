using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace TetrisSpecialBlocks
{
    public enum BlinkDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    // 기본 블록 클래스 (모든 블록은 정확히 2칸 가로폭 ASCII 문자열을 사용하여 폰트 깨짐 방지)
    class 블록
    {
        public string shape = "[]";

        public 블록(string shape)
        {
            this.shape = shape;
        }
    }

    // 시간 제한이 있어서 일정 시간 후에 파괴되는 타이머 블록
    class TimerBlock : 블록
    {
        public int deathTimer = 3;
        private double elapsedAccumulator = 0.0;

        public TimerBlock(string shape) : base(shape)
        {
            UpdateShape();
        }

        public TimerBlock(int seconds = 3) : base("[3]")
        {
            this.deathTimer = seconds;
            UpdateShape();
        }

        private void UpdateShape()
        {
            // 숫자만 2칸 폭으로 표기 (예: 3s, 2s, 1s)
            this.shape = $"{Math.Max(0, deathTimer)}s";
        }

        // 시간이 지났을 때 없어지는 기능 (파괴되면 true 반환)
        public bool TimeDeath(double deltaTime)
        {
            elapsedAccumulator += deltaTime;
            if (elapsedAccumulator >= 1.0)
            {
                int ticks = (int)elapsedAccumulator;
                elapsedAccumulator -= ticks;
                deathTimer -= ticks;
                UpdateShape();
            }

            return deathTimer <= 0;
        }
    }

    // 다 떨어졌을 때 화살표 방향으로 숫자만큼 순간이동하는 점멸 블록
    class BlinkBlock : 블록
    {
        public BlinkDirection direction;
        public int distance;
        public bool hasBlinked = false;

        public BlinkBlock(BlinkDirection direction, int distance) : base("<2")
        {
            this.direction = direction;
            this.distance = Math.Clamp(distance, 1, 9);
            UpdateShape();
        }

        // 어떤 콘솔 폰트에서도 밀리지 않는 순수 ASCII 2글자 조합 (예: <2, >3, ^2, v1)
        public void UpdateShape()
        {
            char arrow = direction switch
            {
                BlinkDirection.Left  => '<',
                BlinkDirection.Right => '>',
                BlinkDirection.Up    => '^',
                BlinkDirection.Down  => 'v',
                _ => '*'
            };

            this.shape = $"{arrow}{distance}";
        }

        // 착지 시 화살표 방향으로 distance만큼 장애물을 통과해 점멸
        public (int newX, int newY) Blink(블록[,] board, int currentX, int currentY, int width, int height)
        {
            if (hasBlinked) return (currentX, currentY);
            hasBlinked = true;

            int dx = direction switch
            {
                BlinkDirection.Left  => -1,
                BlinkDirection.Right => 1,
                _ => 0
            };

            int dy = direction switch
            {
                BlinkDirection.Up   => -1,
                BlinkDirection.Down => 1,
                _ => 0
            };

            int targetX = Math.Clamp(currentX + (dx * distance), 0, width - 1);
            int targetY = Math.Clamp(currentY + (dy * distance), 0, height - 1);

            // 목표 지점이 비어있으면 중간 블록을 무시하고 즉시 이동
            if (board[targetY, targetX] == null)
            {
                board[currentY, currentX] = null;
                board[targetY, targetX] = this;
                this.shape = "[]";
                return (targetX, targetY);
            }

            // 목표 지점이 막혀 있다면 가장 가까운 빈칸 탐색
            for (int step = distance - 1; step >= 1; step--)
            {
                int fallbackX = Math.Clamp(currentX + (dx * step), 0, width - 1);
                int fallbackY = Math.Clamp(currentY + (dy * step), 0, height - 1);

                if (board[fallbackY, fallbackX] == null)
                {
                    board[currentY, currentX] = null;
                    board[fallbackY, fallbackX] = this;
                    this.shape = "[]";
                    return (fallbackX, fallbackY);
                }
            }

            this.shape = "[]";
            return (currentX, currentY);
        }
    }

    // 입력, 갱신, 렌더링을 모두 포함하는 단일 게임 클래스
    class TetrisGame
    {
        private const int BOARD_WIDTH = 10;
        private const int BOARD_HEIGHT = 20;

        private 블록[,] board = new 블록[BOARD_HEIGHT, BOARD_WIDTH];
        private 블록[,] currentPieceBlocks = new 블록[4, 4];
        private int currentX = 3;
        private int currentY = 0;

        private double dropTimer = 0.0;
        private double dropInterval = 0.6;
        private int score = 0;
        private bool isGameOver = false;
        private Random rand = new Random();

        private static readonly int[,,] TetrominoShapes = new int[,,]
        {
            // I
            { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} },
            // O
            { {0,1,1,0}, {0,1,1,0}, {0,0,0,0}, {0,0,0,0} },
            // T
            { {0,1,0,0}, {1,1,1,0}, {0,0,0,0}, {0,0,0,0} },
            // S
            { {0,1,1,0}, {1,1,0,0}, {0,0,0,0}, {0,0,0,0} },
            // Z
            { {1,1,0,0}, {0,1,1,0}, {0,0,0,0}, {0,0,0,0} },
            // J
            { {1,0,0,0}, {1,1,1,0}, {0,0,0,0}, {0,0,0,0} },
            // L
            { {0,0,1,0}, {1,1,1,0}, {0,0,0,0}, {0,0,0,0} }
        };

        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.Clear();

            TetrisGame game = new TetrisGame();
            game.Run();
        }

        public void Run()
        {
            SpawnPiece();
            Stopwatch stopwatch = Stopwatch.StartNew();
            double lastTime = stopwatch.Elapsed.TotalSeconds;

            while (!isGameOver)
            {
                double currentTime = stopwatch.Elapsed.TotalSeconds;
                double deltaTime = currentTime - lastTime;
                lastTime = currentTime;

                ProcessInput();
                Update(deltaTime);
                Render();

                Thread.Sleep(30);
            }

            Console.SetCursorPosition(0, BOARD_HEIGHT + 5);
            Console.WriteLine($"GAME OVER! Final Score: {score}");
            Console.CursorVisible = true;
        }

        private 블록 CreateRandomBlock()
        {
            int roll = rand.Next(100);
            if (roll < 20)
            {
                return new TimerBlock(3);
            }
            else if (roll < 40)
            {
                BlinkDirection dir = (BlinkDirection)rand.Next(4);
                int dist = rand.Next(1, 4);
                return new BlinkBlock(dir, dist);
            }
            return new 블록("[]");
        }

        private void SpawnPiece()
        {
            int shapeIndex = rand.Next(TetrominoShapes.GetLength(0));
            currentPieceBlocks = new 블록[4, 4];

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (TetrominoShapes[shapeIndex, r, c] == 1)
                    {
                        currentPieceBlocks[r, c] = CreateRandomBlock();
                    }
                }
            }

            currentX = 3;
            currentY = 0;

            if (!IsValidPosition(currentPieceBlocks, currentX, currentY))
            {
                isGameOver = true;
            }
        }

        // 1. 입력 (Input)
        private void ProcessInput()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (IsValidPosition(currentPieceBlocks, currentX - 1, currentY))
                            currentX--;
                        break;
                    case ConsoleKey.RightArrow:
                        if (IsValidPosition(currentPieceBlocks, currentX + 1, currentY))
                            currentX++;
                        break;
                    case ConsoleKey.DownArrow:
                        if (IsValidPosition(currentPieceBlocks, currentX, currentY + 1))
                        {
                            currentY++;
                            dropTimer = 0;
                        }
                        break;
                    case ConsoleKey.UpArrow:
                        RotatePiece();
                        break;
                    case ConsoleKey.Spacebar:
                        while (IsValidPosition(currentPieceBlocks, currentX, currentY + 1))
                        {
                            currentY++;
                        }
                        LockPiece();
                        SpawnPiece();
                        dropTimer = 0;
                        break;
                }
            }
        }

        private void RotatePiece()
        {
            블록[,] rotated = new 블록[4, 4];
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    rotated[c, 3 - r] = currentPieceBlocks[r, c];
                }
            }

            if (IsValidPosition(rotated, currentX, currentY))
            {
                currentPieceBlocks = rotated;
            }
        }

        // 2. 갱신 (Update)
        private void Update(double deltaTime)
        {
            dropTimer += deltaTime;
            if (dropTimer >= dropInterval)
            {
                dropTimer = 0;
                if (IsValidPosition(currentPieceBlocks, currentX, currentY + 1))
                {
                    currentY++;
                }
                else
                {
                    LockPiece();
                    SpawnPiece();
                }
            }

            // 보드에 고정된 TimerBlock 시간 체크 및 파괴 처리
            bool anyBlockDestroyed = false;
            for (int r = 0; r < BOARD_HEIGHT; r++)
            {
                for (int c = 0; c < BOARD_WIDTH; c++)
                {
                    if (board[r, c] is TimerBlock timerBlock)
                    {
                        if (timerBlock.TimeDeath(deltaTime))
                        {
                            board[r, c] = null;
                            anyBlockDestroyed = true;
                        }
                    }
                }
            }

            // 블록이 파괴되었으면 위에 있는 블록을 아래로 내림
            if (anyBlockDestroyed)
            {
                ApplyGravityToBoard();
                ClearFullLines();
            }
        }

        private void LockPiece()
        {
            var placedBlinkBlocks = new System.Collections.Generic.List<(BlinkBlock block, int x, int y)>();

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (currentPieceBlocks[r, c] != null)
                    {
                        int boardY = currentY + r;
                        int boardX = currentX + c;
                        if (boardY >= 0 && boardY < BOARD_HEIGHT && boardX >= 0 && boardX < BOARD_WIDTH)
                        {
                            블록 b = currentPieceBlocks[r, c];
                            board[boardY, boardX] = b;

                            if (b is BlinkBlock blink)
                            {
                                placedBlinkBlocks.Add((blink, boardX, boardY));
                            }
                        }
                    }
                }
            }

            // 착지한 점멸 블록들을 화살표 방향으로 순간이동
            bool blinked = false;
            foreach (var item in placedBlinkBlocks)
            {
                if (board[item.y, item.x] == item.block)
                {
                    item.block.Blink(board, item.x, item.y, BOARD_WIDTH, BOARD_HEIGHT);
                    blinked = true;
                }
            }

            if (blinked)
            {
                ApplyGravityToBoard();
            }

            ClearFullLines();
        }

        // 파괴되거나 점멸해서 빈 공간이 생기면 위쪽 블록을 아래로 자동 하강
        private void ApplyGravityToBoard()
        {
            for (int c = 0; c < BOARD_WIDTH; c++)
            {
                int writeRow = BOARD_HEIGHT - 1;
                for (int r = BOARD_HEIGHT - 1; r >= 0; r--)
                {
                    if (board[r, c] != null)
                    {
                        블록 temp = board[r, c];
                        board[r, c] = null;
                        board[writeRow, c] = temp;
                        writeRow--;
                    }
                }
            }
        }

        private void ClearFullLines()
        {
            int linesCleared = 0;
            for (int r = BOARD_HEIGHT - 1; r >= 0; r--)
            {
                bool isFull = true;
                for (int c = 0; c < BOARD_WIDTH; c++)
                {
                    if (board[r, c] == null)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull)
                {
                    linesCleared++;
                    for (int moveRow = r; moveRow > 0; moveRow--)
                    {
                        for (int c = 0; c < BOARD_WIDTH; c++)
                        {
                            board[moveRow, c] = board[moveRow - 1, c];
                        }
                    }
                    for (int c = 0; c < BOARD_WIDTH; c++)
                    {
                        board[0, c] = null;
                    }
                    r++;
                }
            }

            if (linesCleared > 0)
            {
                score += linesCleared * 100;
            }
        }

        private bool IsValidPosition(블록[,] piece, int targetX, int targetY)
        {
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (piece[r, c] != null)
                    {
                        int boardX = targetX + c;
                        int boardY = targetY + r;

                        if (boardX < 0 || boardX >= BOARD_WIDTH || boardY < 0 || boardY >= BOARD_HEIGHT)
                            return false;

                        if (board[boardY, boardX] != null)
                            return false;
                    }
                }
            }
            return true;
        }

        // 3. 렌더링 (Render) - 모든 셀이 정확히 2칸 반각 문자이므로 폰트와 무관하게 정렬 유지
        private void Render()
        {
            Console.SetCursorPosition(0, 0);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("=== TETRIS (ASCII SAFE MODE) ===");
            sb.AppendLine($"Score: {score,-10}");
            sb.AppendLine("+" + new string('-', BOARD_WIDTH * 2) + "+");

            for (int r = 0; r < BOARD_HEIGHT; r++)
            {
                sb.Append("|");
                for (int c = 0; c < BOARD_WIDTH; c++)
                {
                    블록 cellBlock = board[r, c];

                    int pieceR = r - currentY;
                    int pieceC = c - currentX;
                    if (pieceR >= 0 && pieceR < 4 && pieceC >= 0 && pieceC < 4)
                    {
                        if (currentPieceBlocks[pieceR, pieceC] != null)
                        {
                            cellBlock = currentPieceBlocks[pieceR, pieceC];
                        }
                    }

                    if (cellBlock != null)
                    {
                        sb.Append(cellBlock.shape);
                    }
                    else
                    {
                        sb.Append(" .");
                    }
                }
                sb.AppendLine("|");
            }

            sb.AppendLine("+" + new string('-', BOARD_WIDTH * 2) + "+");
            sb.AppendLine("Move: Left/Right | Rot: Up | Drop: Space");
            sb.AppendLine("[] : Normal | 3s : Timer | <2 >3 ^1 v2 : Blink");

            Console.Write(sb.ToString());
        }
    }
}
