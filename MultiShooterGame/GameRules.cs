using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiShooterGame
{
    class GameRules
    {
        public enum MapSelectionSetting { RandomGenerated,Shuffle,OneMap}
        public static MapSelectionSetting _mapSelectionSetting;
        public enum GameType {WinRoundsToWin,KillToWin, KillToWinActiveRespawn, CollectToWin}
        public static GameType selectedGameType = GameType.WinRoundsToWin;
        public static int numberToWin = 15;
        public static int[] playerScores;

    }
}
