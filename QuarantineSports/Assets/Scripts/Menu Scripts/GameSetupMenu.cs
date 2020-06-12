using System;
using Core.OpenPoseHandling;
using UnityEngine;
using UnityEngine.UI;

namespace Menu_Scripts
{
    public class GameSetupMenu : MonoBehaviour
    {

        /**
         * Unity Components
         */
        public Text timerText;

        private TimeSpan _timer;

        private string defaultTimerText = "Timer {0}";

        public OpenPoseOnnxHandler openPoseOnnxScene;

        private void _setTimerText()
        {
            timerText.text = string.Format(defaultTimerText, _timer.ToString());
        }
        // Start is called before the first frame update
        void Start()
        {
            _timer = TimeSpan.Zero;
            _timer = _timer.Add(TimeSpan.FromSeconds(30));
            _setTimerText();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void AddSeconds()
        {
            _timer = _timer.Add(TimeSpan.FromSeconds(10));
            _setTimerText();
        }
        
        
        public void SubstractSeconds()
        {
            _timer = _timer.Subtract(TimeSpan.FromSeconds(10));
            _setTimerText();
        }

        public void SetTimer()
        {
            openPoseOnnxScene.timeLeft = new TimeSpan(_timer.Hours, _timer.Minutes, _timer.Seconds);
        }
    }
}
