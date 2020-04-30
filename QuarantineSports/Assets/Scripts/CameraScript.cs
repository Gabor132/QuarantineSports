using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

public class CameraScript : MonoBehaviour
{
    private bool camAvailable;
    private static WebCamTexture frontCam;
    private Texture defaultBackground;

    public RawImage background;
    public AspectRatioFitter fit;

    private void Start()
    {
        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("No camera could be found");
            camAvailable = false;
            return;
        }
        for(int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing)
            {
                frontCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }

        if (frontCam == null)
        {
            Debug.Log("No front camera found");
            return;
        }
        frontCam.Play();
        background.texture = frontCam;
        Debug.Log("Camera is setup");
        camAvailable = true;
    }

    private void Update()
    {
        if (!camAvailable)
            return;
        float ration = (float) frontCam.width / (float) frontCam.height;
        fit.aspectRatio = ration;

        float scaleY = frontCam.videoVerticallyMirrored ? -1f : 1f;
        background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -frontCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }
    
}
