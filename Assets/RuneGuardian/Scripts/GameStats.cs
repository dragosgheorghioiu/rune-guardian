using UnityEngine;

namespace RuneGuardian
{
    /// <summary>
    /// Tracks game statistics for display on the finish panel
    /// </summary>
    public class GameStats
    {
        private static int gesturesMissed = 0;
        private static int toysDelivered = 0;
        private static int wrongSpellsUsed = 0;

        /// <summary>
        /// Reset all statistics (call when starting a new game)
        /// </summary>
        public static void Reset()
        {
            gesturesMissed = 0;
            toysDelivered = 0;
            wrongSpellsUsed = 0;
            Debug.Log("GameStats: Statistics reset");
        }

        /// <summary>
        /// Record a gesture attempt with its recognition score
        /// </summary>
        /// <param name="score">Recognition score (0-1)</param>
        /// <param name="wasRecognized">Whether the gesture was successfully recognized</param>
        public static void RecordGesture(float score, bool wasRecognized)
        {
            if (!wasRecognized)
            {
                gesturesMissed++;
            }

            Debug.Log($"GameStats: Gesture recorded - Score: {score:F2}, Recognized: {wasRecognized}");
        }

        /// <summary>
        /// Record a successfully delivered toy
        /// </summary>
        public static void RecordToyDelivered()
        {
            toysDelivered++;
            Debug.Log($"GameStats: Toy delivered! Total: {toysDelivered}");
        }

        /// <summary>
        /// Record when a player uses the wrong spell on a toy
        /// </summary>
        public static void RecordWrongSpell()
        {
            wrongSpellsUsed++;
            Debug.Log($"GameStats: Wrong spell used! Total: {wrongSpellsUsed}");
        }

        /// <summary>
        /// Get total number of drawings that were missed/failed
        /// </summary>
        public static int GetDrawingsMissed()
        {
            return gesturesMissed;
        }

        /// <summary>
        /// Get total number of toys successfully delivered
        /// </summary>
        public static int GetToysDelivered()
        {
            return toysDelivered;
        }

        /// <summary>
        /// Get total number of wrong spells used on toys
        /// </summary>
        public static int GetWrongSpellsUsed()
        {
            return wrongSpellsUsed;
        }

        /// <summary>
        /// Get formatted statistics string for display
        /// </summary>
        public static string GetFormattedStats()
        {
            return $"Jucarii livrate: {toysDelivered}\n" +
                   $"Vraji ratate: {gesturesMissed}\n" +
                   $"Vraji gresite: {wrongSpellsUsed}\n";
        }
    }
}
