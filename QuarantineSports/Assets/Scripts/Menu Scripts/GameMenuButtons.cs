using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu_Scripts
{
    public class GameMenuButtons : MonoBehaviour
    {
        public void SelectBackMenu()
        {
            SceneManager.LoadScene(0);
        }
    }
}
