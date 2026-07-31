using Tetris;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.CursorVisible = false;

try
{
    do
    {
        var game = new TetrisGame();
        game.Run();

        if (game.ExitRequested)
            break;

        Console.SetCursorPosition(0, GameField.Height + 5);
        Console.Write("Игра окончена! Нажмите R, чтобы начать заново, или Esc для выхода. ");

        ConsoleKey key;
        do
        {
            key = Console.ReadKey(true).Key;
        }
        while (key is not (ConsoleKey.R or ConsoleKey.Escape));

        if (key == ConsoleKey.Escape)
            break;
    }
    while (true);
}
finally
{
    Console.CursorVisible = true;
    Console.Clear();
}
