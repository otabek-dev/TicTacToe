using Microsoft.AspNetCore.SignalR;

namespace TicTacToe
{
    public class GameHub : Hub
    {
        private static List<string> players = new List<string>();
        private static Dictionary<string, string[]> playerMoves = new Dictionary<string, string[]>();

        public override Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            if (players.Count < 2)
            {
                players.Add(connectionId);
                playerMoves.Add(connectionId, new string[9]);
                Clients.Caller.SendAsync("ReceivePlayer", GetPlayer(connectionId));

                if (players.Count == 2)
                {
                    Clients.Clients(players).SendAsync("GameStart");
                }
            }

            return base.OnConnectedAsync();
        }
        
        public string JoinGame()
        {
            string connectionId = Context.ConnectionId;
            if (players.Count < 2)
            {
                players.Add(connectionId);

                if (players.Count == 2)
                {
                    Clients.Clients(players).SendAsync("GameStart");
                }
                return GetPlayer(connectionId);
            }

            return string.Empty;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            players.Remove(connectionId);

            if (players.Count < 2)
            {
                playerMoves.Clear();
            }

            return base.OnDisconnectedAsync(exception);
        }

        public void MakeMove(int index, string player)
        {
            string connectionId = Context.ConnectionId;

            if (players.Count == 2 && player == GetPlayer(connectionId))
            {
                playerMoves[connectionId][index] = player;
                Clients.Clients(players).SendAsync("ReceiveMove", index, player);

                if (CheckWinCondition(player))
                {
                    Clients.Clients(players).SendAsync("GameResult", $"Win player {player}!");
                    playerMoves.Clear();
                }
                else
                {
                    var calculateDraw = 0;
                    foreach (var item in playerMoves.Values)
                    {
                        if (item != null)
                        {
                            calculateDraw += item.Where(x => x != null).ToList().Count;
                            if (calculateDraw == 9)
                            {
                                Clients.Clients(players).SendAsync("GameResult", "It's draw!");
                                playerMoves.Clear();
                            }
                        }
                    }
                }
            }
        }

        public string GetPlayer(string connectionId)
        {
            if (players[0] == connectionId)
            {
                return "X";
            }
            else if (players[1] == connectionId)
            {
                return "O";
            }

            return string.Empty;
        }

        private bool CheckWinCondition(string player)
        {
            string[] squares = new string[9];

            squares = player == "X" ? playerMoves[players[0]] : playerMoves[players[1]];
            
            int[][] lines = new int[][]
            {
                new int[] { 0, 1, 2 },
                new int[] { 3, 4, 5 },
                new int[] { 6, 7, 8 },
                new int[] { 0, 3, 6 },
                new int[] { 1, 4, 7 },
                new int[] { 2, 5, 8 },
                new int[] { 0, 4, 8 },
                new int[] { 2, 4, 6 }
            };

            foreach (var line in lines)
            {
                int a = line[0];
                int b = line[1];
                int c = line[2];

                if (squares[a] != null && squares[a] == squares[b] && squares[a] == squares[c])
                {
                    return true;
                }
            }

            return false;
        }

    }
}
