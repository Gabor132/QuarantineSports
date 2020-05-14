using System.IO;
using UnityEngine;

namespace Core.DatasetObjects
{
    public class Data
    {
        private string _outputPath;
        private string _datasetName;
        private int _sequenceIndex;
        
        private byte[][] Frames { get; set; }
        public Category Category { get; set; }

        public Data(byte[][] frames,Category category, string outputPath, string datasetName, int sequenceIndex)
        {
            Frames = frames;
            Category = category;
            _outputPath = outputPath;
            _datasetName = datasetName;
            _sequenceIndex = sequenceIndex;
        }

        internal void SaveData()
        {
            string sequenceFolderPath = $"{_outputPath}\\{_datasetName}\\{_sequenceIndex}";
            Debug.Log($"Create directory for sequence at {sequenceFolderPath}");
            Directory.CreateDirectory(sequenceFolderPath);
            string pathCategoryFile = sequenceFolderPath + "\\category.txt";
            File.WriteAllText(pathCategoryFile, Category.ToString());
            for (int frameIndex = 0; frameIndex < Frames.Length; frameIndex++)
            {
                string savePath =
                    $"{sequenceFolderPath}\\{_datasetName}_sequence{_sequenceIndex}_frame{frameIndex}.png";
                File.WriteAllBytes(savePath, Frames[frameIndex]);
                Debug.Log(Frames[frameIndex].Length/1024 + "Kb saved as: " + savePath);
            }
        }

        internal string GetPath()
        {
            string sequenceFolderPath = $"{_outputPath}\\{_datasetName}\\{_sequenceIndex}";
            return sequenceFolderPath;
        }
    }

    public enum Category
    {
        Wrong = 0,
        Correct = 1
    }
}