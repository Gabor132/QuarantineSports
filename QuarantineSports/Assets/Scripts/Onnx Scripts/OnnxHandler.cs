﻿using System;
using System.Linq;
using Core.OpenPoseHandling;
using Game_Scripts;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;

namespace Onnx_Scripts
{
    
    public class OnnxHandler : MonoBehaviour
    {
        
        public TimerScript timerScript;
        public NNModel loadedModel;
        public Text outputText;
        public Text counterText;
        public float probabiltyThreshold = 0.8f;

        private int _lastCategory = -1;
        private int _nrOfPushups = 0;

        private string _defaultOutputTextFormat = "Frame was: {0}/ Value: {1}";
        private string _defaultCountetTextFormat = "Counter: {0}";
        
        private Model _runtimeModel;
        private IWorker _worker;
        private Tensor _input;
        private Tensor _output;
        private bool _isProcessing = false;

        // Start is called before the first frame update
        private void Start()
        {
            Debug.Log("Starting ONNX");
            _runtimeModel = ModelLoader.Load(loadedModel);
            _worker = WorkerFactory.CreateComputeWorker( _runtimeModel);
            Debug.Log("ONNX Model Started");
            foreach (var layer in _runtimeModel.layers)
            {
                Debug.Log(layer);
            }
        }

        public void StopOnnx()
        {
            OnDestroy();
        }

        private void OnDestroy()
        {
            _isProcessing = false;
            Debug.Log("Stopping ONNX");
            _worker?.Dispose();
            _input?.Dispose();
            _output?.Dispose();
            Debug.Log("Stopped ONNX");
        }

        public void StartProcessing()
        {
            _nrOfPushups = 0;
            _isProcessing = true;
        }

        public void StopProcessing()
        {
            _isProcessing = true;
        }

        private void Update()
        {
            if (_isProcessing && timerScript.HasFrames())
            {
                ProcessNextFrame();
            }
        }

        private void SetCounterText()
        {
            counterText.text = string.Format(_defaultCountetTextFormat, _nrOfPushups);
        }

        private void CountPushups(float[] classification)
        {
            int newClass = (int) classification[0];
            float probabilty = classification[1];
            if (probabilty < 0.8)
            {
                return;
            }

            if (newClass == 2 && _lastCategory == 3 || newClass == 3 && _lastCategory == 2)
            {
                _nrOfPushups++;
                SetCounterText();
                _lastCategory = newClass;
                Debug.Log("Counted a new pushup");
            }

            if (newClass == 3 || newClass == 2)
            {
                _lastCategory = newClass;
            }
            
        }

        private void ProcessNextFrame()
        {
            var frames = timerScript.ReadFrame(1);
            foreach (var frame in frames.Where(frame => frame.Keypoints != null && frame.Keypoints.Count != 0))
            {
                _input = frame.GetAsTensor(new TensorShape(_runtimeModel.inputs[0].shape));
                try
                {
                    _worker.SetInput(_input);
                    _worker.Execute();
                    _output = _worker.CopyOutput();
                    float[] classification = GetClassification(_output);
                    SetOutputText(classification);
                    CountPushups(classification);
                    _output.Dispose();
                }
                finally
                {
                    _input.Dispose();
                }
            }
        }
        
        public static float[] GetClassification(Tensor outputT)
        {
            float[] max = new float[2];
            max[0] = -1;
            max[1] = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                if (outputT[i] > max[1])
                {
                    max[1] = outputT[i];
                    max[0] = i;
                    
                }
            }
            return max;
        }

        public void SetOutputText(float[] classification)
        {
            outputText.text = string.Format(_defaultOutputTextFormat, classification[0].ToString(), classification[1].ToString());
        }

        public int GetNrOfPushups()
        {
            return _nrOfPushups;
        }
    }
}
