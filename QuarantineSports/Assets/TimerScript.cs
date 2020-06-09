using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using System.Linq;
using Camera_Scripts;
using Core.OpenPoseHandling;
using Video_Player_Scripts;

public class TimerScript : MonoBehaviour
{
    
    public float countDownLength = 5f;
    public Text countdownText;

    public float timerLength = 30.0f;
    public Text timerText;
    public Text setupTimerText;
    public CameraScript cameraScript;
    public OpenPoseAnnotator openPoseAnnotator;
    public GameObject openPoseObject;
    public GameObject exportMenu;
    

    private float currentTimer;
    private bool countdown = false;
    private bool timer = false;

   

    VideoCapture m_VideoCapture = null;
    float m_stopRecordingTimer = float.MaxValue;
    

    // Start is called before the first frame update
    void Start()
    {
        countDownLength = countDownLength + 0.49f;
        timerText.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

        
        if (countdown)
        {
            countDownLength -= 1 * Time.deltaTime;
            if (countDownLength < 0.5f)
            {
                countdown = false;
                countdownText.text = "GO!";
          
                StartTimer();
             
                //TODO invoke game start
                return;
            }
            countdownText.text = countDownLength.ToString("0");
        }


        if (timer)
        {
            
            currentTimer -= 1 * Time.deltaTime;
            if(timerLength - currentTimer < 1.0f)
            {
                countdownText.enabled = false;
            }

            if(currentTimer < 0.5f)
            {
                timer = false;
                cameraScript.StopCamera();
                
                
                
                //StartCoroutine(openPoseAnnotator.startOpenPose());
                timerText.text = "Finish!";
                //OpenPoseAnnotator handler = openPoseObject.GetComponent<OpenPoseAnnotator>();
                //handler.videoInputPath = cameraScript.getFilepath();
                //Debug.Log(cameraScript.getFilepath());
                //openPoseObject.SetActive(true);
                //if (timerText.text.Equals(OpenPose.OPState.Ready.ToString()))
                //{
                //    handler.ApplyChanges();
                //}
                //gameObject.SetActive(false);
                //start Video Recording


               
                VideoAnnotationOpenPoseHandler handler = exportMenu.GetComponent<VideoAnnotationOpenPoseHandler>();
                string _videoPath = cameraScript.getFilepath();
                long _lengthOfVideoInFrames = 200;
                string _outputPath = System.IO.Path.GetTempPath();
                handler.videoInputPath = _videoPath;
                Debug.Log(_videoPath);
                handler.sequences = new List<VideoPlayerScript.Sequence>();
                handler.lengthOfVideoInFrames = _lengthOfVideoInFrames;
                handler.outputPath = _outputPath;
                handler.exportPosePicture = true;
                handler.isAnnotated = false;
                exportMenu.SetActive(true);
                if (handler.stateText.text.Equals(OpenPose.OPState.Ready.ToString()))
                {
                    Debug.Log("apply changes called");
                    handler.ApplyChanges();
                }
                gameObject.SetActive(false);



                return;
            }
            timerText.text = "Time left: "+ currentTimer.ToString("0");

        }

      
    }



    public void StartCountdown()
    {
        countdown = true;
    }

    public void StartTimer()
    {
        cameraScript.StartRecording();
        timerText.enabled = true;
        timer = true;
        currentTimer = timerLength;

    }

    public void IncreaseTimer()
    {
        timerLength += 30.0f;
        setupTimerText.text = string.Format("Timer: {0}", timerLength);

    }

    public void DecreaseTimer()
    {
        if (timerLength > 30.1f)
        {
            timerLength -= 30.0f;
            setupTimerText.text = string.Format("Timer: {0}", timerLength);
        }    
    }

}
