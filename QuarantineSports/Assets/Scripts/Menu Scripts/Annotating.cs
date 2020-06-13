using UnityEngine;
using UnityEngine.UI;

namespace Menu_Scripts
{
    /**
     * Script used to initialize the Annotation Scene
     */
    public class Annotating : MonoBehaviour
    {
        public GameObject inputMenu;
        public GameObject annotationMenu;

        //Input Path for the video to be annotated
        public InputField inputPath;
        //Output Path for the rendered video
        public InputField outputPath;

        private void Start()
        {
            // Setup Menus
            inputMenu.SetActive(true);
            annotationMenu.SetActive(false);

            // Setup default Paths
            inputPath.text = "";

            outputPath.text = "";
        }
    }
}
