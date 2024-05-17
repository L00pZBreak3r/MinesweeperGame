namespace MinesweeperApiApp.Messages;

internal record GameInfoResponse(int width, int height, int mines_count, string game_id, bool completed, bool win, string[][] field)
    : NewGameRequest(width, height, mines_count);
