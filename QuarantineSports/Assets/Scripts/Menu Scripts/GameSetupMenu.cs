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

        private TimeSpan timer;

        private string defaultTimerText = "Timer {0}";

        public OpenPoseOnnxHandler openPoseOnnxScene;

        private void _setTimerText()
        {
            timerText.text = string.Format(defaultTimerText, timer.ToString());
        }
        // Start is called before the first frame update
        void Start()
        {
            timer = TimeSpan.Zero;
            timer = timer.Add(TimeSpan.FromMinutes(1));
            _setTimerText();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void AddSeconds()
        {
            timer = timer.Add(TimeSpan.FromSeconds(30));
            _setTimerText();
        }
        
        
        public void SubstractSeconds()
        {
            timer = timer.Subtract(TimeSpan.FromSeconds(30));
            _setTimerText();
        }

        public void StartTimer()
        {
            openPoseOnnxScene.timeLeft = new TimeSpan(timer.Hours, timer.Minutes, timer.Seconds);
        }
    }
}
