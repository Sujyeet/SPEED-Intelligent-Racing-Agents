using UnityEngine;

namespace KartGame.GameFlow
{
    public static class GameModeManager
    {
        /// <summary>
        /// True if the user selected Single Player.
        /// </summary>
        public static bool IsSinglePlayer { get; set; } = false;

        /// <summary>
        /// True if ML Agents should be kept in the game.
        /// Always true for Single Player, optional for Multiplayer.
        /// </summary>
        public static bool IncludeMLAgents { get; set; } = true;

        /// <summary>
        /// Action invoked when an agent finishes the race in single-player mode.
        /// </summary>
        public static System.Action<Component> OnAgentFinishedRace;
    }
}











