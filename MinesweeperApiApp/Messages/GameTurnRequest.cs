namespace MinesweeperApiApp.Messages;

internal record GameTurnRequest(string game_id, int col, int row, bool mine_mark, bool finish);
