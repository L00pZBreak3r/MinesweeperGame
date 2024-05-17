namespace MinesweeperApiApp.Messages;

internal record NewGameRequest(int width, int height, int mines_count);
