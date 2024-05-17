namespace MinesweeperGameLibrary.Minesweeper;

/// <summary>
/// ETurnError
/// </summary>
public enum ETurnError : byte
{
    None,
    BadCellCoordinates,
    AlreadyOpened,
    GameIsOver
}
