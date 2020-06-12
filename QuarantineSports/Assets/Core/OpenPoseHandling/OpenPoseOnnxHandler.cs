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

        public WebcamOpenPoseHandler openPoseHandler;
        public OnnxHandler onnxHandler;
        
        public Text timeLeftText;
        public TimeSpan timeLeft;

        private float _passedTime = 0.0f;
        private readonly List<Frame> _frames = new List<Frame>();

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
        }
        
        void Update()
        {
            if (! timeLeft.Equals(TimeSpan.Zero) && onnxHandler.isStarted && openPoseHandler.IsStarted())
            {
                _passedTime += Time.deltaTime;
                if (_passedTime >= 1)
                {
                    _passedTime = 0.0f;
                    timeLeft = timeLeft.Subtract(new TimeSpan(0, 0, 0, 1));
                }
                _setTimeLeftText();
            }
            else if(onnxHandler.isStarted && ! HasFrames())
            {
                Debug.Log("Timer finished");
                openPoseHandler.StopOpenPose();
                onnxHandler.StopOnnx();
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
    }
}
