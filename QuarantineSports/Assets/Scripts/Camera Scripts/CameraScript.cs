using UnityEditor.Media;
using UnityEngine;
using UnityEngine.UI;

namespace Camera_Scripts
{
    /**Note: deprecated
     * This class is used to access the webcam. 
     * Grabs webcam frames and stores them onto WebcamTexture.
     * Once all frames are handled, Mediaencoder is used to persist an mp4 video containing the webcam frames.
     */
    public class CameraScript : MonoBehaviour
    {
        private bool _camAvailable;
        private static WebCamTexture _frontCam;
        private Texture _defaultBackground;
        Color32[] data;
        MediaEncoder _mediaEncoder;
        private MediaTime _mediaTime;
        private string _filename;
        private string _filepath;
        private bool recording = false;
        private VideoTrackAttributes _videoTrackAttributes;
        Texture2D _currentTexture;
        public RawImage background;
        public AspectRatioFitter fit;

  

        private void Start()
        {
            
            _defaultBackground = background.texture;
            WebCamDevice[] devices = WebCamTexture.devices;

            //Loop Over Cameras and use the last webcam, if it is front facing.
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

            //Startup Webcam and save texture onto the RawImage GameObject
            _frontCam.Play();
            background.texture = _frontCam;


            //Path for the finished mp4 file
            _filename = string.Format("TestVideo_{0}.mp4", System.DateTime.Now.ToFileTime());
            _filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), _filename);
            _filepath = _filepath.Replace("/", @"\");


            //Setup the Video Attributes for the Media Encoder
            Debug.Log("setting up");
            _videoTrackAttributes = new VideoTrackAttributes();
            _videoTrackAttributes.width = (uint)_frontCam.width;
            _videoTrackAttributes.height = (uint)_frontCam.height;
            _currentTexture = new Texture2D(_frontCam.width, _frontCam.height);
            _videoTrackAttributes.frameRate = new MediaRational(30);
            _videoTrackAttributes.includeAlpha = false;
            _mediaEncoder = new MediaEncoder(_filepath, _videoTrackAttributes);
            
            
            
            Debug.Log("Camera is setup");
            _camAvailable = true;
        }


        /**
         * Update the webcam texture.
         * Aspect Ratio is calculated, to fit the texture onto the canvas
             */

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
            
            if (recording)
            {
                _currentTexture.SetPixels(_frontCam.GetPixels());
                //_mediaEncoder.AddFrame(_currentTexture,Time.deltaTime);
                _mediaEncoder.AddFrame(_currentTexture);

            }

        }

        /**
         * Stop the Camera and dispose the video file, to persist it onto the harddrive
         */

        public void StopCamera()
        {
            recording = false;
            _mediaEncoder.Dispose();
            
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

        public void StartRecording()
        {
            recording = true;
            //_mediaTime.count = 0;
        }

        public string getFilepath()
        {
            return _filepath;
        }

    }
}
