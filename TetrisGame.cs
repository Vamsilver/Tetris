using System.Diagnostics;

namespace Tetris;

public sealed class TetrisGame
{
    private const int FallIntervalMilliseconds = 500;
    private readonly GameField _field = new();
    private readonly Random _random = new();
    private Tetromino _current;
    private int _score;
    private bool _gameOver;

    public TetrisGame() => _current = new Tetromino(_random);

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

            _field.Draw(_current, _score);
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
                    TryMove(-1, 0);
                    break;
                case ConsoleKey.RightArrow:
                    TryMove(1, 0);
                    break;
                case ConsoleKey.DownArrow:
                    MoveDown();
                    break;
                case ConsoleKey.UpArrow:
                    TryRotate();
                    break;
                case ConsoleKey.Escape:
                    _gameOver = true;
                    break;
            }
        }
    }

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

    private void MoveDown()
    {
        if (TryMove(0, 1))
            return;

        _field.Lock(_current);
        var lines = _field.ClearFullLines();
        _score += lines * lines * 100;
        _current = new Tetromino(_random);

        // Новая фигура не помещается сверху — поле заполнено, игра окончена.
        if (!_field.CanPlace(_current, _current.X, _current.Y))
            _gameOver = true;
    }

    private void TryRotate()
    {
        var rotated = _current.GetRotatedBlocks();
        if (_field.CanPlace(_current, _current.X, _current.Y, rotated))
            _current.ApplyRotation(rotated);
    }
}
