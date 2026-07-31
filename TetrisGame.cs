using System.Diagnostics;

namespace Tetris;

public sealed class TetrisGame
{
    private const int InitialFallIntervalMilliseconds = 500;
    private const int MinimumFallIntervalMilliseconds = 100;
    private const int SpeedIncreasePerLevelMilliseconds = 40;
    private const int LinesPerLevel = 10;
    private const int HardDropPointsPerRow = 2;
    private const int FrameDelayMilliseconds = 20;
    private static readonly (int X, int Y)[] WallKickOffsets =
    {
        (0, 0), (-1, 0), (1, 0), (-2, 0), (2, 0), (0, -1)
    };

    private readonly GameField _field = new();
    private readonly Random _random = new();
    private Tetromino _current;
    private Tetromino _next;
    private int _score;
    private int _clearedLines;
    private bool _gameOver;

    public bool ExitRequested { get; private set; }
    private int Level => _clearedLines / LinesPerLevel + 1;
    private int FallIntervalMilliseconds =>
        Math.Max(
            MinimumFallIntervalMilliseconds,
            InitialFallIntervalMilliseconds - (Level - 1) * SpeedIncreasePerLevelMilliseconds);

    public TetrisGame()
    {
        _current = new Tetromino(_random);
        _next = new Tetromino(_random);
    }

    public void Run()
    {
        Console.Clear();
        var fallTimer = Stopwatch.StartNew();

        while (!_gameOver)
        {
            HandleInput();
            if (_gameOver)
                break;

            if (fallTimer.ElapsedMilliseconds >= FallIntervalMilliseconds)
            {
                MoveDown();
                fallTimer.Restart();
            }

            _field.Draw(_current, _next, _score, Level, _clearedLines);
            Thread.Sleep(FrameDelayMilliseconds);
        }
    }

    private void HandleInput()
    {
        while (Console.KeyAvailable)
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.LeftArrow:
                    MoveLeft();
                    break;
                case ConsoleKey.RightArrow:
                    MoveRight();
                    break;
                case ConsoleKey.DownArrow:
                    SoftDrop();
                    break;
                case ConsoleKey.UpArrow:
                    Rotate();
                    break;
                case ConsoleKey.Spacebar:
                    HardDrop();
                    return;
                case ConsoleKey.Escape:
                    ExitRequested = true;
                    _gameOver = true;
                    return;
            }
        }
    }

    /// <summary>Перемещает текущую фигуру влево, если клетка свободна.</summary>
    private void MoveLeft() => TryMove(-1, 0);

    /// <summary>Перемещает текущую фигуру вправо, если клетка свободна.</summary>
    private void MoveRight() => TryMove(1, 0);

    /// <summary>Ускоряет падение фигуры на одну строку.</summary>
    private void SoftDrop() => MoveDown();

    /// <summary>Проверяет возможность перемещения и изменяет координаты фигуры.</summary>
    private bool TryMove(int deltaX, int deltaY)
    {
        var newX = _current.X + deltaX;
        var newY = _current.Y + deltaY;
        if (!_field.CanPlace(_current, newX, newY))
            return false;

        _current.X = newX;
        _current.Y = newY;
        return true;
    }

    /// <summary>Опускает фигуру или фиксирует её при достижении препятствия.</summary>
    private void MoveDown()
    {
        if (TryMove(0, 1))
            return;

        LockCurrentFigure();
    }

    private void LockCurrentFigure()
    {
        _field.Lock(_current);
        UpdateScore(_field.ClearFullLines());
        SpawnNextFigure();

        // Новая фигура не помещается сверху — поле заполнено, игра окончена.
        _gameOver = !_field.CanPlace(_current, _current.X, _current.Y);
    }

    private void SpawnNextFigure()
    {
        _current = _next;
        _current.X = (GameField.Width - _current.Width) / 2;
        _current.Y = 0;
        _next = new Tetromino(_random);
    }

    private void UpdateScore(int clearedLines)
    {
        _score += CalculateLineScore(clearedLines) * Level;
        _clearedLines += clearedLines;
    }

    /// <summary>Поворачивает фигуру на 90 градусов с поправкой возле стен и пола.</summary>
    private void Rotate()
    {
        var rotated = _current.GetRotatedBlocks();

        foreach (var (offsetX, offsetY) in WallKickOffsets)
        {
            var newX = _current.X + offsetX;
            var newY = _current.Y + offsetY;
            if (!_field.CanPlace(_current, newX, newY, rotated))
                continue;

            _current.X = newX;
            _current.Y = newY;
            _current.ApplyRotation(rotated);
            return;
        }
    }

    private void HardDrop()
    {
        var droppedRows = 0;
        while (TryMove(0, 1))
            droppedRows++;

        _score += droppedRows * HardDropPointsPerRow;
        MoveDown();
    }

    private static int CalculateLineScore(int lines) => lines switch
    {
        1 => 100,
        2 => 300,
        3 => 500,
        4 => 800,
        _ => 0
    };
}
