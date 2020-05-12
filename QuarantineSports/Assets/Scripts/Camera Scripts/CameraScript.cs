using UnityEngine;
using UnityEngine.UI;

namespace Camera_Scripts
{
    public class CameraScript : MonoBehaviour
    {
        private bool _camAvailable;
        private static WebCamTexture _frontCam;
        private Texture _defaultBackground;

        public RawImage background;
        public AspectRatioFitter fit;

        private void Start()
        {
            _defaultBackground = background.texture;
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.Log("No camera could be found");
                _camAvailable = false;
                return;
            }
            for(int i = 0; i < devices.Length; i++)
            {
                if (devices[i].isFrontFacing)
                {
                    _frontCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
                }
            }

            if (_frontCam == null)
            {
                Debug.Log("No front camera found");
                return;
            }
            _frontCam.Play();
            background.texture = _frontCam;
            Debug.Log("Camera is setup");
            _camAvailable = true;
        }

        private void Update()
        {
            if (!_camAvailable)
                return;
            float ration = (float) _frontCam.width / (float) _frontCam.height;
            fit.aspectRatio = ration;

            float scaleY = _frontCam.videoVerticallyMirrored ? -1f : 1f;
            background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

            int orient = -_frontCam.videoRotationAngle;
            background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
        }

        public void StopCamera()
        {
            if (_camAvailable)
            {
                _camAvailable = false;
                if (_frontCam.isPlaying)
                {
                    _frontCam.Stop();
                    background.texture = _defaultBackground;
                }
            }
            Debug.Log("Letting go of camera");
        }

    }
}
