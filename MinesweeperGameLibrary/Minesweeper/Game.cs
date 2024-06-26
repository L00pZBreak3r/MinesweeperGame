using System;
using System.Linq;
using System.Collections.Generic;

namespace MinesweeperGameLibrary.Minesweeper;

/// <summary>
/// Minesweeper game
/// </summary>
public class Game
{
    public const int MaxWidth = 256;
    public const int MaxHeight = 256;
    public const int MinEmptyCells = 1;

    public const byte CellMine = 0x1F;
    private const byte CellNumberMask = 0x1F;
    private const byte CellIsOpenedMask = 0x20;
    private const byte CellIsMineMarkMask = 0x40;
    private const byte CellIsCorrectMask = 0x80;

    private readonly byte[] mCells;

    /// <summary>
    /// Minesweeper game
    /// </summary>
    /// <param name="aWidth"></param>
    /// <param name="aHeight"></param>
    /// <param name="aMinesCount"></param>
    public Game(int aWidth, int aHeight, int aMinesCount)
    {
        if (aWidth <= 0)
            aWidth = 1;
        if (aHeight <= 0)
            aHeight = 1;
        if (aMinesCount < 0)
            aMinesCount = 0;

        Width = aWidth;
        Height = aHeight;            
        MinesCount = GetMineCount(aWidth, aHeight, aMinesCount);
        mCells = BuildCells();
        IsOver = MinesCount <= 0;
    }

    /// <summary>
    /// GetMineCount
    /// </summary>
    /// <param name="aWidth"></param>
    /// <param name="aHeight"></param>
    /// <param name="aMinesCount"></param>
    public static int GetMineCount(int aWidth, int aHeight, int aMinesCount)
    {
        if (aWidth <= 0)
            aWidth = 1;
        if (aHeight <= 0)
            aHeight = 1;
        if (aMinesCount < 0)
            aMinesCount = 0;

        int aCellCount = aWidth * aHeight;
        int aMinEmptyCells = MinEmptyCells;
        if (aMinEmptyCells > aCellCount)
            aMinEmptyCells = aCellCount;

        int aMaxMinesCount = aCellCount - aMinEmptyCells;
        if (aMinesCount > aMaxMinesCount)
            aMinesCount = aMaxMinesCount;
            
        return aMinesCount;
    }

    private static void SetAdjacentCells(byte[] aCells, int aWidth, int aHeight, int aMineCellIndex)
    {
        int aRow = aMineCellIndex / aWidth;
        int aCol = aMineCellIndex % aWidth;

        for (int aCurrentCol = aCol - 1; aCurrentCol <= aCol + 1; aCurrentCol++)
        {
            int aCurrentRow = aRow;
            if ((aCurrentCol >= 0) && (aCurrentCol < aWidth))
            {
                int aCurrentCellIndex = aCurrentRow * aWidth + aCurrentCol;
                if (aCells[aCurrentCellIndex] != CellMine)
                    aCells[aCurrentCellIndex]++;
            }

            if (aRow > 0)
            {
                aCurrentRow = aRow - 1;
                if ((aCurrentCol >= 0) && (aCurrentCol < aWidth))
                {
                    int aCurrentCellIndex = aCurrentRow * aWidth + aCurrentCol;
                    if (aCells[aCurrentCellIndex] != CellMine)
                        aCells[aCurrentCellIndex]++;
                }
            }
            if (aRow < aHeight - 1)
            {
                aCurrentRow = aRow + 1;
                if ((aCurrentCol >= 0) && (aCurrentCol < aWidth))
                {
                    int aCurrentCellIndex = aCurrentRow * aWidth + aCurrentCol;
                    if (aCells[aCurrentCellIndex] != CellMine)
                        aCells[aCurrentCellIndex]++;
                }
            }
        }
    }

    private byte[] BuildCells()
    {
        int aCellCount = Width * Height;
        byte[] aResult = new byte[aCellCount];
        List<int> aCellIndexes = Enumerable.Range(0, aCellCount).ToList();

        Random aRandom = new Random();
        for (int i = 0; i < MinesCount; i++)
        {
            int aIndex = aRandom.Next(aCellIndexes.Count);
            int aCellIndex = aCellIndexes[aIndex];
            aCellIndexes.RemoveAt(aIndex);
            aResult[aCellIndex] = CellMine;
            SetAdjacentCells(aResult, Width, Height, aCellIndex);
        }

        return aResult;
    }

    /// <summary>
    /// Width
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Height
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// MinesCount
    /// </summary>
    public int MinesCount { get; }

    /// <summary>
    /// Is game over.
    /// </summary>
    public bool IsOver { get; private set; }

    /// <summary>
    /// Player wins.
    /// </summary>
    public bool Win { get; private set; }

    private Tuple<int, bool, ECellState> GetCellByIndex(int aCellIndex)
    {
        bool aIsMine = false;

        byte aCell = mCells[aCellIndex];
        int aNumber = aCell & CellNumberMask;
        if (aNumber == CellMine)
        {
            aNumber = 0;
            aIsMine = true;
        }

        ECellState aCellState = ((aCell & CellIsOpenedMask) != 0) ? ECellState.Opened : (((aCell & CellIsMineMarkMask) != 0) ? ECellState.MineMark : ECellState.None);
        if (IsOver)
        {
            if ((aCell & CellIsCorrectMask) != 0)
                aCellState = ECellState.Correct;
            else if (((aCell & CellIsMineMarkMask) != 0) && !aIsMine)
                aCellState = ECellState.WrongMine;
        }

        return Tuple.Create(aNumber, aIsMine, aCellState);
    }

    /// <summary>
    /// DoTurn
    /// </summary>
    /// <param name="aCol"></param>
    /// <param name="aRow"></param>
    /// <returns>(number of adjacent mines, is mine, ECellState)</returns>
    public Tuple<int, bool, ECellState> GetCell(int aCol, int aRow)
    {
        if ((aCol >= 0) && (aCol < Width) && (aRow >= 0) && (aRow < Height))
        {
            return GetCellByIndex(aRow * Width + aCol);
        }

        return Tuple.Create(-1, false, ECellState.None);
    }

    private static bool IsCorrectCell(byte aCell)
    {
        bool aIsCorrect;
        int aNumber = aCell & CellNumberMask;
        if (aNumber == CellMine)
        {
            aIsCorrect = ((aCell & CellIsMineMarkMask) != 0) || ((aCell & CellIsOpenedMask) == 0);
        }
        else
        {
            aIsCorrect = ((aCell & CellIsMineMarkMask) == 0) && ((aCell & CellIsOpenedMask) != 0);
        }
        return aIsCorrect;
    }

    /// <summary>
    /// Finishes the game
    /// </summary>
    public void Finish()
    {
        IsOver = true;
        bool aAllCorrect = true;
        int aCellCount = Width * Height;
        for (int i = 0; i < aCellCount; i++)
        {
            byte aCell = mCells[i];
            bool aIsCorrect = IsCorrectCell(aCell);
            mCells[i] |= CellIsOpenedMask;
            if (aIsCorrect)
                mCells[i] |= CellIsCorrectMask;
            else
                aAllCorrect = false;
        }

        Win = aAllCorrect;
    }

    private void OpenAdjacentZeroCells(int aCellIndex)
    {
        var aCellTuple = GetCellByIndex(aCellIndex);
        if (!aCellTuple.Item2)
        {
            mCells[aCellIndex] |= CellIsOpenedMask;

            if (aCellTuple.Item1 <= 0)
            {
                int aRow = aCellIndex / Width;
                int aCol = aCellIndex % Width;
                List<int> aAdjacentCells = new();

                for (int aCurrentCol = aCol - 1; aCurrentCol <= aCol + 1; aCurrentCol++)
                {
                    int aCurrentRow = aRow;
                    if ((aCurrentCol >= 0) && (aCurrentCol < Width))
                    {
                        int aCurrentCellIndex = aCurrentRow * Width + aCurrentCol;
                        var aCurrentCellTuple = GetCellByIndex(aCurrentCellIndex);
                        if (!aCurrentCellTuple.Item2 && (aCurrentCellTuple.Item3 == ECellState.None))
                            aAdjacentCells.Add(aCurrentCellIndex);
                    }

                    if (aRow > 0)
                    {
                        aCurrentRow = aRow - 1;
                        if ((aCurrentCol >= 0) && (aCurrentCol < Width))
                        {
                            int aCurrentCellIndex = aCurrentRow * Width + aCurrentCol;
                            var aCurrentCellTuple = GetCellByIndex(aCurrentCellIndex);
                            if (!aCurrentCellTuple.Item2 && (aCurrentCellTuple.Item3 == ECellState.None))
                                aAdjacentCells.Add(aCurrentCellIndex);
                        }
                    }
                    if (aRow < Height - 1)
                    {
                        aCurrentRow = aRow + 1;
                        if ((aCurrentCol >= 0) && (aCurrentCol < Width))
                        {
                            int aCurrentCellIndex = aCurrentRow * Width + aCurrentCol;
                            var aCurrentCellTuple = GetCellByIndex(aCurrentCellIndex);
                            if (!aCurrentCellTuple.Item2 && (aCurrentCellTuple.Item3 == ECellState.None))
                                aAdjacentCells.Add(aCurrentCellIndex);
                        }
                    }
                }

                foreach (int aAdjCellIndex in aAdjacentCells)
                    OpenAdjacentZeroCells(aAdjCellIndex);
            }
        }
    }

    private void OpenSuitable(int aCellIndex)
    {
        OpenAdjacentZeroCells(aCellIndex);
        if (mCells.All(IsCorrectCell))
            Finish();
    }

    /// <summary>
    /// DoTurn
    /// </summary>
    /// <param name="aCol"></param>
    /// <param name="aRow"></param>
    /// <param name="aToggleMineMark"></param>
    /// <returns>((number of adjacent mines, is mine, ECellState), ETurnError)</returns>
    public Tuple<Tuple<int, bool, ECellState>, ETurnError> DoTurn(int aCol, int aRow, bool aToggleMineMark)
    {
        Tuple<int, bool, ECellState> aCellTuple = Tuple.Create(-1, false, ECellState.None);
        ETurnError aTurnError = ETurnError.None;
        bool aIsGameOver = IsOver;

        if (aIsGameOver)
            aTurnError = ETurnError.GameIsOver;
        else
        {
            if ((aCol >= 0) && (aCol < Width) && (aRow >= 0) && (aRow < Height))
            {
                aCellTuple = GetCell(aCol, aRow);
                if (aCellTuple.Item3 == ECellState.Opened)
                    aTurnError = ETurnError.AlreadyOpened;
                else
                {
                    int aCellIndex = aRow * Width + aCol;
                    if (aToggleMineMark)
                    {
                        if (aCellTuple.Item3 == ECellState.MineMark)
                            mCells[aCellIndex] = (byte)(mCells[aCellIndex] & ~CellIsMineMarkMask);
                        else
                            mCells[aCellIndex] |= CellIsMineMarkMask;
                    }
                    else
                    {
                        if (aCellTuple.Item3 == ECellState.None)
                        {
                            aIsGameOver = aCellTuple.Item2;
                            mCells[aCellIndex] |= CellIsOpenedMask;
                            if (aIsGameOver)
                                Finish();
                            else
                                OpenSuitable(aCellIndex);
                        }
                    }

                    aCellTuple = GetCell(aCol, aRow);
                }
            }
            else
                aTurnError = ETurnError.BadCellCoordinates;
        }

        return Tuple.Create(aCellTuple, aTurnError);
    }
}
