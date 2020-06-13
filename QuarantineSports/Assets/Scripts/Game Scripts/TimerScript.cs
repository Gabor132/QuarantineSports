using System.Collections.Generic;
using Core.DatasetObjects;
using Core.OpenPoseHandling;
using Onnx_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Scripts
{
    /**
     * Main Game Script class
     * Stores all generated Frames from the WebcamOpenPoseHandler which are then used by the OnnxHandler
     * Turns on the WebcamOpenPoseHandler and OnnxHandler to start processing the Frames
     * Handles some UI settings
     */
    public class TimerScript : MonoBehaviour
    {
        //UI elements
        public Text timerText;
        public Text setupTimerText;
        public Text countdownText;
        public Text finalNumberOfPushUps;

        //Length of the Countdown
        public float countDownLength = 5f;
        

        //Length of the Timer, can be increased and decreased
        public float timerLength = 30.0f;
        

        //Remaining length of the Timer
        private float _currentTimer;

        //boolean flag to indicate if countdown has started
        private bool _countdown = false;
        //boolean flag to indicate if timer has started
        private bool _timer = false;

        
        //Unity References
        public OnnxHandler onnxHandler;
        public WebcamOpenPoseHandler webcamOpenPoseHandler;
        public GameObject setupMenu;
        public GameObject gameStatusTexts;
    
        private readonly List<Frame> _frames = new List<Frame>();
        
        void Start()
        {
            countDownLength = countDownLength + 0.49f;
            timerText.enabled = true;
            ResetFrames();
        }

        /**
         * Updates timer and countdown if they are active
         */
        void Update()
        {
            if (_countdown)
            {
                countDownLength -= 1 * Time.deltaTime;
                if (countDownLength < 0.5f)
                {
                    _countdown = false;
                    countdownText.text = "GO!";
                    timerText.enabled = true;
                    StartTimer();
                    ResetFrames();
                    StartProcessing();
                    setupMenu.gameObject.SetActive(false);
                    return;
                }
                countdownText.text = countDownLength.ToString("0");
            }
            if (_timer)
            {
                _currentTimer -= 1 * Time.deltaTime;
                if(timerLength - _currentTimer < 1.0f)
                {
                    countdownText.enabled = false;
                }
                if(_currentTimer < 0.5f)
                {
                    _timer = false;
                    timerText.enabled = false;
                    StopProcessing();
                    setupMenu.gameObject.SetActive(true);
                    return;
                }
                timerText.text = "Time left: "+ _currentTimer.ToString("0");
            }
        }


        //Start Countdown
        public void StartCountdown()
        {
            _countdown = true;
            PrepareProcessing();
        }

        //Start Timer
        public void StartTimer()
        {
            timerText.enabled = true;
            _timer = true;
            _currentTimer = timerLength;
        }


        public void PrepareProcessing()
        {
            webcamOpenPoseHandler.gameObject.SetActive(true);
        }

        /**
          * Start processing the video.
          * Gets called once the timer starts.
          */
        public void StartProcessing()
        {
            setupMenu.gameObject.SetActive(false);
            gameStatusTexts.gameObject.SetActive(true);
            webcamOpenPoseHandler.StartProcessing();
            onnxHandler.StartProcessing();
        }

        /**
          * Start processing the video.
          * Gets called when the timer ends.
          */
        public void StopProcessing()
        {
            webcamOpenPoseHandler.StopProcessing();
            webcamOpenPoseHandler.gameObject.SetActive(false);
            onnxHandler.StopProcessing();
            SetNumberPushUpText(onnxHandler.GetNrOfPushups());
            setupMenu.gameObject.SetActive(true);
            gameStatusTexts.gameObject.SetActive(false);
        }

        //method to increase timer length
        public void IncreaseTimer()
        {
            timerLength += 10.0f;
            setupTimerText.text = string.Format("Timer: {0}", timerLength);

        }

        //method to decrease timer length
        public void DecreaseTimer()
        {
            if (timerLength > 10.1f)
            {
                timerLength -= 10.0f;
                setupTimerText.text = string.Format("Timer: {0}", timerLength);
            }    
        }


        /**
         * Methods to handle the dataset frames.
         */
        public void WriteFrame(Frame newFrame)
        {
            lock (_frames)
            {
                _frames.Add(newFrame);
            }
        }
        
        public void WriteFrames(List<Frame> newFrames)
        {
            lock (_frames)
            {
                _frames.AddRange(newFrames);
            }
        }

        public List<Frame> ReadFrame(int count)
        {
            List<Frame> readFrames = new List<Frame>();
            lock (_frames)
            {
                readFrames.AddRange(_frames.GetRange(0, count));
                _frames.RemoveRange(0,count);
            }
            return readFrames;
        }

        public bool HasFrames()
        {
            lock (_frames)
            {
                return _frames.Count > 0;
            }
        }

        private void ResetFrames()
        {
            if (_frames != null)
            {
                lock (_frames)
                {
                    _frames.Clear();
                }   
            }
        }


        //Method to display the number of counted pushups
        private void SetNumberPushUpText(int count)
        {
            finalNumberOfPushUps.gameObject.SetActive(true);
            finalNumberOfPushUps.text = string.Format("Number of Pushups: {0}", count);
        }

    }
}
