namespace Tetris;

/// <summary>Игровое поле: 0 означает пустую клетку, 1 — занятую.</summary>
public sealed class GameField
{
    public const int Width = 10;
    public const int Height = 20;

    private readonly int[,] _cells = new int[Height, Width];

    public bool CanPlace(Tetromino figure, int newX, int newY, int[,]? blocks = null)
    {
        blocks ??= figure.Blocks;

        for (var row = 0; row < blocks.GetLength(0); row++)
        for (var column = 0; column < blocks.GetLength(1); column++)
        {
            if (blocks[row, column] == 0)
                continue;

            var fieldX = newX + column;
            var fieldY = newY + row;

            if (fieldX < 0 || fieldX >= Width || fieldY < 0 || fieldY >= Height)
                return false;

            if (_cells[fieldY, fieldX] == 1)
                return false;
        }

        return true;
    }

    public void Lock(Tetromino figure)
    {
        for (var row = 0; row < figure.Height; row++)
        for (var column = 0; column < figure.Width; column++)
            if (figure.Blocks[row, column] == 1)
                _cells[figure.Y + row, figure.X + column] = 1;
    }

    public int ClearFullLines()
    {
        var cleared = 0;

        for (var row = Height - 1; row >= 0; row--)
        {
            var isFull = true;
            for (var column = 0; column < Width; column++)
                isFull &= _cells[row, column] == 1;

            if (!isFull)
                continue;

            cleared++;
            for (var sourceRow = row; sourceRow > 0; sourceRow--)
            for (var column = 0; column < Width; column++)
                _cells[sourceRow, column] = _cells[sourceRow - 1, column];

            for (var column = 0; column < Width; column++)
                _cells[0, column] = 0;

            row++; // Проверяем эту строку ещё раз после сдвига.
        }

        return cleared;
    }

    /// <summary>Отрисовывает поле вместе с текущей падающей фигурой.</summary>
    public void Draw(Tetromino figure, Tetromino nextFigure, int score)
    {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine("ТЕТРИС".PadRight(48));
        Console.WriteLine($"Счёт: {score}".PadRight(48));
        Console.WriteLine("┌" + new string('─', Width * 2) + "┐");

        for (var row = 0; row < Height; row++)
        {
            Console.Write('│');
            for (var column = 0; column < Width; column++)
            {
                var occupied = _cells[row, column] == 1 || IsFigureBlock(figure, column, row);
                Console.Write(occupied ? "██" : "  ");
            }
            Console.Write('│');
            Console.Write("   ");
            Console.WriteLine(GetPreviewLine(nextFigure, row).PadRight(24));
        }

        Console.WriteLine("└" + new string('─', Width * 2) + "┘");
        Console.WriteLine("← → — движение | ↑ — поворот | ↓ — вниз | Esc — выход".PadRight(64));
    }

    private static string GetPreviewLine(Tetromino figure, int fieldRow)
    {
        const int previewSize = 4;

        if (fieldRow == 0)
            return "Следующая фигура:";
        if (fieldRow == 1)
            return "┌" + new string('─', previewSize * 2) + "┐";
        if (fieldRow == previewSize + 2)
            return "└" + new string('─', previewSize * 2) + "┘";
        if (fieldRow < 2 || fieldRow > previewSize + 1)
            return string.Empty;

        var previewRow = fieldRow - 2;
        var offsetX = (previewSize - figure.Width) / 2;
        var offsetY = (previewSize - figure.Height) / 2;
        var line = "│";

        for (var column = 0; column < previewSize; column++)
        {
            var figureRow = previewRow - offsetY;
            var figureColumn = column - offsetX;
            var occupied = figureRow >= 0 && figureRow < figure.Height &&
                           figureColumn >= 0 && figureColumn < figure.Width &&
                           figure.Blocks[figureRow, figureColumn] == 1;
            line += occupied ? "██" : "  ";
        }

        return line + "│";
    }

    private static bool IsFigureBlock(Tetromino figure, int x, int y)
    {
        var localX = x - figure.X;
        var localY = y - figure.Y;
        return localX >= 0 && localX < figure.Width &&
               localY >= 0 && localY < figure.Height &&
               figure.Blocks[localY, localX] == 1;
    }
}
