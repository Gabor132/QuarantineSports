using System;
using System.Linq;
using Core.OpenPoseHandling;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;

namespace Onnx_Scripts
{
    
    public class OnnxHandler : MonoBehaviour
    {
        
        public OpenPoseOnnxHandler openPoseOnnxHandler;
        public NNModel loadedModel;
        public bool isStarted = false;
        public Text outputText;

        private string _defaultOutputTextFormat = "Frame was: {0}/ Value: {1}";
        
        private Model _runtimeModel;
        private IWorker _worker;
        private Tensor _input;
        private Tensor _output;

        // Start is called before the first frame update
        private void Start()
        {
            Debug.Log("Starting ONNX");
            _runtimeModel = ModelLoader.Load(loadedModel);
            _worker = WorkerFactory.CreateComputeWorker( _runtimeModel);
            isStarted = true;
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
            isStarted = false;
            Debug.Log("Stopping ONNX");
            _worker?.Dispose();
            _input?.Dispose();
            _output?.Dispose();
            Debug.Log("Stopped ONNX");
        }

        private void Update()
        {
            if (isStarted && openPoseOnnxHandler.HasFrames())
            {
                ProcessNextFrame();
            }
        }

        private void ProcessNextFrame()
        {
            var frames = openPoseOnnxHandler.ReadFrame(1);
            foreach (var frame in frames.Where(frame => frame.Keypoints != null && frame.Keypoints.Count != 0))
            {
                _input = frame.GetAsTensor();
                try
                {
                    _worker.SetInput(_input);
                    _worker.Execute();
                    _output = _worker.CopyOutput();
                    float[] classification = GetClassification(_output);
                    SetOutputText(classification);
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
    }
}
