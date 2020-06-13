using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu_Scripts
{
    /**
     * Main menu script for scene changes
     */
    public class MainMenu : MonoBehaviour
    {
        public void SelectPlay()
        {
            SceneManager.LoadScene((int) SceneIndexes.PlayScene);
        }
        
        public void SelectBackMenu()
        {
            SceneManager.LoadScene((int) SceneIndexes.MainMenuScene);
        }

        public void SelectExit()
        {
            Debug.Log("Quit");
            Application.Quit();
        }
        
        public void SelectCheckCamera()
        {
            SceneManager.LoadScene((int) SceneIndexes.CheckCameraScene);
        }

        public void SelectExportDataset()
        {
            SceneManager.LoadScene((int) SceneIndexes.ExportDataset);
        }
    }
}
