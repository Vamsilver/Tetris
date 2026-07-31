namespace Tetris;

/// <summary>Падающая фигура, представленная матрицей из нулей и единиц.</summary>
public sealed class Tetromino
{
    private static readonly int[][,] Shapes =
    {
        new int[,] { { 1, 1, 1, 1 } },                     // I
        new int[,] { { 1, 1 }, { 1, 1 } },                 // O
        new int[,] { { 0, 1, 0 }, { 1, 1, 1 } },           // T
        new int[,] { { 0, 1, 1 }, { 1, 1, 0 } },           // S
        new int[,] { { 1, 1, 0 }, { 0, 1, 1 } },           // Z
        new int[,] { { 1, 0, 0 }, { 1, 1, 1 } },           // J
        new int[,] { { 0, 0, 1 }, { 1, 1, 1 } }            // L
    };

    public int[,] Blocks { get; private set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width => Blocks.GetLength(1);
    public int Height => Blocks.GetLength(0);

    public Tetromino(Random random)
    {
        Blocks = (int[,])Shapes[random.Next(Shapes.Length)].Clone();
        X = (GameField.Width - Width) / 2;
        Y = 0;
    }

    /// <summary>Возвращает новую матрицу, повернутую на 90 градусов вправо.</summary>
    public int[,] GetRotatedBlocks()
    {
        var rotated = new int[Width, Height];
        for (var row = 0; row < Height; row++)
        for (var column = 0; column < Width; column++)
            rotated[column, Height - 1 - row] = Blocks[row, column];

        return rotated;
    }

    public void ApplyRotation(int[,] rotatedBlocks) => Blocks = rotatedBlocks;
}
