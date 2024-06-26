using MinesweeperApiApp.Messages;
using MinesweeperGameLibrary.Minesweeper;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MinesweeperApiApp.Utilities;

internal static class GameStore
{
    private static readonly Dictionary<Guid, Game> mGames = new Dictionary<Guid, Game>();

    public static Ok<GameInfoResponse> ProcessNewGame(NewGameRequest aRequest)
    {
        Guid aGameId = Guid.NewGuid();
        Game aGame = new(aRequest.width, aRequest.height, aRequest.mines_count);
        mGames[aGameId] = aGame;
        var aResponse = new GameInfoResponse(aGame.Width, aGame.Height, aGame.MinesCount, aGameId.ToString(), aGame.IsOver, aGame.Win, GameInfoHelper.GetGameInfoField(aGame));
        return TypedResults.Ok(aResponse);
    }

    public static IResult ProcessGameTurn(GameTurnRequest aRequest)
    {
        if (Guid.TryParse(aRequest.game_id, out var aGameId) && mGames.TryGetValue(aGameId, out var aGame))
        {
            var aTurnTuple = aGame.DoTurn(aRequest.col, aRequest.row, aRequest.mine_mark);
            if (aRequest.finish)
                aGame.Finish();

            if (aRequest.finish || (aTurnTuple.Item2 == ETurnError.None))
            {
                var aResponse = new GameInfoResponse(aGame.Width, aGame.Height, aGame.MinesCount, aGameId.ToString(), aGame.IsOver, aGame.Win, GameInfoHelper.GetGameInfoField(aGame));
                if (aGame.IsOver)
                    mGames.Remove(aGameId);
                return TypedResults.Ok(aResponse);
            }
            else
            {
                if (aGame.IsOver)
                    mGames.Remove(aGameId);
                string aErrorMessage = aTurnTuple.Item2 switch
                {
                    ETurnError.BadCellCoordinates => "Неверные координаты ячейки.",
                    ETurnError.AlreadyOpened => "Уже открытая ячейка.",
                    ETurnError.GameIsOver => "Игра завершена.",
                    _ => "Unknown error"
                };
                var aResponse = new ErrorResponse(aErrorMessage);
                return TypedResults.BadRequest(aResponse);
            }
        }
        else
        {
            var aResponse = new ErrorResponse($"Игра с идентификатором {aGameId.ToString()} не была создана или устарела (неактуальна).");
            return TypedResults.BadRequest(aResponse);
        }
    }
}
