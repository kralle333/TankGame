using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame
{
    class GameRules
    {
        public enum GameType { KillToWin, WinRoundsToWin, CollectToWin }
        public static GameType selectedGameType;
        public static int numberToWin = 0;
        public static int[] playerScores;

    }
}
