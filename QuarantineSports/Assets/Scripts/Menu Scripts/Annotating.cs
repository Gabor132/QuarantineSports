﻿using UnityEngine;
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
            inputPath.text = "C:\\Users\\Paso\\Documents\\Uni\\QuarantineAnnotations\\EDITED-24_geschnitten - 1of3.mp4";

            outputPath.text = "C:\\Users\\Paso\\Documents\\Uni\\QuarantineAnnotations\\annotatedJson\\";
        }
    }
}
