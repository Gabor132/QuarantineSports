using System.Collections.Generic;
using Core.DatasetObjects;
using Core.OpenPoseHandling;
using Onnx_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Scripts
{
    public class TimerScript : MonoBehaviour
    {
    
        public float countDownLength = 5f;
        public Text countdownText;

        public float timerLength = 30.0f;
        public Text timerText;
        public Text setupTimerText;


        private float _currentTimer;
        private bool _countdown = false;
        private bool _timer = false;

        public Text finalNumberOfPushUps;

        public OnnxHandler onnxHandler;
        public WebcamOpenPoseHandler webcamOpenPoseHandler;

        public GameObject setupMenu;
        public GameObject gameStatusTexts;
    
        private readonly List<Frame> _frames = new List<Frame>();
    
        // Start is called before the first frame update
        void Start()
        {
            countDownLength = countDownLength + 0.49f;
            timerText.enabled = true;
            ResetFrames();
        }

        // Update is called once per frame
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



        public void StartCountdown()
        {
            _countdown = true;
            PrepareProcessing();
        }

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

        public void StartProcessing()
        {
            setupMenu.gameObject.SetActive(false);
            gameStatusTexts.gameObject.SetActive(true);
            webcamOpenPoseHandler.StartProcessing();
            onnxHandler.StartProcessing();
        }

        public void StopProcessing()
        {
            webcamOpenPoseHandler.StopProcessing();
            webcamOpenPoseHandler.gameObject.SetActive(false);
            onnxHandler.StopProcessing();
            SetNumberPushUpText(onnxHandler.GetNrOfPushups());
            setupMenu.gameObject.SetActive(true);
            gameStatusTexts.gameObject.SetActive(false);
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


        private void SetNumberPushUpText(int count)
        {
            finalNumberOfPushUps.gameObject.SetActive(true);
            finalNumberOfPushUps.text = string.Format("Number of Pushups: {0}", count);
        }

    }
}
