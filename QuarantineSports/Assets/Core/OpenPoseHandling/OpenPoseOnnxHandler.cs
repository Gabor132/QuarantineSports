using System;
using System.Collections;
using System.Collections.Generic;
using Core.DatasetObjects;
using Onnx_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Core.OpenPoseHandling
{
    public class OpenPoseOnnxHandler : MonoBehaviour
    {

        private List<Frame> _frames = new List<Frame>();
        public WebcamOpenPoseHandler openPoseHandler;
        public OnnxHandler onnxHandler;

        public TimeSpan timeLeft;

        public Text timeLeftText;

        private float _passedTime = 0.0f;
        private float _startOfTimer = 0.0f;

        private void _setTimeLeftText()
        {
            timeLeftText.text = string.Format("Time left: {0}", timeLeft.ToString());
        }
        
        // Start is called before the first frame update
        private void Start()
        {
            if (_frames != null)
            {
                lock (_frames)
                {
                    _frames.Clear();
                }   
            }
            _setTimeLeftText();
            openPoseHandler.gameObject.SetActive(true);
            onnxHandler.gameObject.SetActive(true);
            onnxHandler.StartOnnx();
            _startOfTimer = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (! timeLeft.Equals(TimeSpan.Zero))
            {
                _passedTime += Time.deltaTime - _startOfTimer;
                if (_passedTime >= 1)
                {
                    _passedTime = 0.0f;
                    timeLeft = timeLeft.Subtract(new TimeSpan(0, 0, 0, 1));
                }
                _setTimeLeftText();
            }
        }

        public void Stop()
        {
            openPoseHandler.StopOpenPose();
            openPoseHandler.gameObject.SetActive(false);
            onnxHandler.StopOnnx();
        }

        /**
         * 
         */
        private void StartKeras()
        {
            StartCoroutine(OpenPoseKerasIntegration());
        }

        /**
         * 
         */
        public void WriteFrame(Frame newFrame)
        {
            lock (_frames)
            {
                _frames.Add(newFrame);
                if (_frames.Count >= 15)
                {
                    StartKeras();
                }
            }
        }
        
        /**
         * 
         */
        public void WriteFrames(List<Frame> newFrames)
        {
            lock (_frames)
            {
                _frames.AddRange(newFrames);
                if (_frames.Count >= 15)
                {
                    StartKeras();
                }
            }
        }

        /**
         * 
         */
        public List<Frame> ReadFrame(int count)
        {
            List<Frame> readFrames = new List<Frame>();
            lock (_frames)
            {
                readFrames.AddRange(_frames.GetRange(0, count));
            }

            return readFrames;
        }
        
        /**
         * 
         */
        IEnumerator OpenPoseKerasIntegration()
        {
            if (! timeLeft.Equals(TimeSpan.Zero))
            {
                yield return null;
            }

            lock (_frames)
            {
                onnxHandler.StartOnnx();
            }
        }
    }
}
