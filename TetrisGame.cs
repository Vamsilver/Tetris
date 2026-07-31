using System.Diagnostics;

namespace Tetris;

public sealed class TetrisGame
{
    private const int InitialFallIntervalMilliseconds = 500;
    private const int MinimumFallIntervalMilliseconds = 100;
    private readonly GameField _field = new();
    private readonly Random _random = new();
    private Tetromino _current;
    private Tetromino _next;
    private int _score;
    private int _clearedLines;
    private bool _gameOver;

    public bool ExitRequested { get; private set; }
    private int Level => _clearedLines / 10 + 1;
    private int FallIntervalMilliseconds =>
        Math.Max(MinimumFallIntervalMilliseconds, InitialFallIntervalMilliseconds - (Level - 1) * 40);

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

            if (fallTimer.ElapsedMilliseconds >= FallIntervalMilliseconds)
            {
                MoveDown();
                fallTimer.Restart();
            }

            _field.Draw(_current, _next, _score, Level, _clearedLines);
            Thread.Sleep(20);
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

        _field.Lock(_current);
        var lines = _field.ClearFullLines();
        _score += CalculateLineScore(lines) * Level;
        _clearedLines += lines;
        _current = _next;
        _current.X = (GameField.Width - _current.Width) / 2;
        _current.Y = 0;
        _next = new Tetromino(_random);

        // Новая фигура не помещается сверху — поле заполнено, игра окончена.
        if (!_field.CanPlace(_current, _current.X, _current.Y))
            _gameOver = true;
    }

    /// <summary>Поворачивает фигуру на 90 градусов с поправкой возле стен и пола.</summary>
    private void Rotate()
    {
        var rotated = _current.GetRotatedBlocks();
        (int X, int Y)[] kickOffsets = { (0, 0), (-1, 0), (1, 0), (-2, 0), (2, 0), (0, -1) };

        foreach (var (offsetX, offsetY) in kickOffsets)
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

        _score += droppedRows * 2;
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
