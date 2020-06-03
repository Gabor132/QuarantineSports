using UnityEngine;
using UnityEngine.UI;

namespace Menu_Scripts
{
    public class Annotating : MonoBehaviour
    {
        public GameObject inputMenu;
        public GameObject annotationMenu;

        public InputField inputPath;
        public InputField outputPath;

        private void Start()
        {
            // Setup Menus
            inputMenu.SetActive(true);
            annotationMenu.SetActive(false);
        
            // Setup default Paths
            inputPath.text = "C:\\Users\\Bg\\Desktop\\video.mp4";
            outputPath.text = "C:\\Users\\Bg\\Desktop\\Output\\";
        }
    }
}
