namespace MinesweeperGameLibrary.Minesweeper;

/// <summary>
/// ECellState
/// </summary>
public enum ECellState : byte
{
    None,
    Opened,
    MineMark,
    Correct,
    WrongMine
}
