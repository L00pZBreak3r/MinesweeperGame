using MinesweeperGameLibrary.Minesweeper;

namespace MinesweeperApiApp.Utilities;

internal static class GameInfoHelper
{
    public static string[][] GetGameInfoField(Game aGame)
    {
        int aHeight = aGame.Height;
        int aWidth = aGame.Width;
        bool aIsOver = aGame.IsOver;
        bool aWin = aGame.Win;
        string[][] aResult = new string[aHeight][];
        for (int r = 0; r < aHeight; r++)
        {
            aResult[r] = new string[aWidth];
            for (int c = 0; c < aWidth; c++)
            {
                string aCellContent = " ";
                var aCellTuple = aGame.GetCell(c, r);
                if (aCellTuple.Item3 != ECellState.None)
                {
                    aCellContent = (aCellTuple.Item3 == ECellState.Opened) ? (aCellTuple.Item2 ? "X" : aCellTuple.Item1.ToString()) : ((aCellTuple.Item3 == ECellState.MineMark) ? "m" : " ");
                }
                if (aIsOver)
                {
                    aCellContent = ((aCellTuple.Item3 == ECellState.Correct) && aWin) ? (aCellTuple.Item2 ? "M" : aCellTuple.Item1.ToString()) : ((aCellTuple.Item3 == ECellState.WrongMine) ? "E" : (aCellTuple.Item2 ? "X" : aCellTuple.Item1.ToString()));
                }
                aResult[r][c] = aCellContent;
            }
        }

        return aResult;
    }
}
