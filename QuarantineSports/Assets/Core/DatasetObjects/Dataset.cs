using System.Collections.Generic;
using UnityEngine;

namespace Core.DatasetObjects
{
    public class Dataset
    {
        private string OutputPath { get; set; }
        private string VideoName { get; set; }
        public List<Data> Data { get; set; }
        public Dataset(string inputPath, string outputPath)
        {
            OutputPath = outputPath;
            string[] pathParts = inputPath.Split('\\');
            VideoName = pathParts[pathParts.Length - 1].Split('.')[0];
            Data = new List<Data>();
        }

        public List<string> GetPaths()
        {
            List<string> outputPaths = new List<string>();
            for (int index = 0; index < Data.Count; index++)
            {
                Data toSaveData = Data[index]; 
                string outputPath = toSaveData.GetPath();
                outputPaths.Add(outputPath);
            }
            return outputPaths;
        }
        public void SaveDataset()
        {
            // Check if dataset directory exists
            string folderPath = $"{OutputPath}{VideoName}";
            if (System.IO.Directory.Exists(folderPath))
            {
                Debug.Log($"Folder {folderPath} already exists, deleting...");
                System.IO.Directory.Delete(folderPath, true);
            }
            
            Debug.Log($"Creating folder {folderPath}");
            System.IO.Directory.CreateDirectory(OutputPath);

            Debug.Log(string.Format("Saving dataset {0} to: {1}", VideoName, OutputPath+VideoName));
            for (int index = 0; index < Data.Count; index++)
            {
                Data toSaveData = Data[index];
                toSaveData.SaveData();
            }
        }

        public void AddData(byte[][] frames, Category category)
        {
            int sequenceIndex = Data.Count;
            Data data = new Data(frames, category, OutputPath, VideoName, sequenceIndex);
            Data.Add(data);
        }
        
    }
}
