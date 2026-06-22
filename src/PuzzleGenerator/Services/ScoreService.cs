using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PuzzleGenerator.Services
{
    public class ScoreService
    {
        private const int MaxTopTimes = 5;

        public ScoreService(string topTimesFilePath)
        {
            TopTimesFilePath = topTimesFilePath;
        }

        public string TopTimesFilePath { get; }

        public Dictionary<string, List<int>> LoadTopTimes()
        {
            if (!File.Exists(TopTimesFilePath))
            {
                return new Dictionary<string, List<int>>();
            }

            string json = File.ReadAllText(TopTimesFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(json)
                ?? new Dictionary<string, List<int>>();
        }

        public void SaveTopTimes(Dictionary<string, List<int>> topTimes)
        {
            string directoryPath = Path.GetDirectoryName(TopTimesFilePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonConvert.SerializeObject(topTimes, Formatting.Indented);
            File.WriteAllText(TopTimesFilePath, json);
        }

        public (bool IsTopTime, int Position) RecordCompletionTime(
            Dictionary<string, List<int>> topTimes,
            string puzzleSizeKey,
            int elapsedTimeInSeconds)
        {
            if (!topTimes.ContainsKey(puzzleSizeKey))
            {
                topTimes[puzzleSizeKey] = new List<int>();
            }

            List<int> timesList = topTimes[puzzleSizeKey];
            bool isTopTime = false;
            int position = -1;

            if (timesList.Count < MaxTopTimes || elapsedTimeInSeconds < timesList[timesList.Count - 1])
            {
                timesList.Add(elapsedTimeInSeconds);
                timesList.Sort();

                if (timesList.Count > MaxTopTimes)
                {
                    timesList.RemoveAt(timesList.Count - 1);
                }

                position = timesList.IndexOf(elapsedTimeInSeconds) + 1;
                isTopTime = true;
            }

            return (isTopTime, position);
        }
    }
}
