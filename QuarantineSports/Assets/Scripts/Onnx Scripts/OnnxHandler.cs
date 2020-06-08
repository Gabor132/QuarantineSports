using System;
using System.Collections.Generic;
using System.ComponentModel;
using Core.DatasetObjects;
using Core.OpenPoseHandling;
using Unity.Barracuda;
using UnityEngine;
using EnumsNET;

namespace Onnx_Scripts
{
    
    public class OnnxHandler : MonoBehaviour
    {
        
        public OpenPoseOnnxHandler openPoseOnnxHandler;
        public NNModel loadedModel;
        public Boolean isProcessing = false;
        
        private Model _runtimeModel;
        private IWorker _worker;

        // Start is called before the first frame update
        public void StartOnnx()
        {
            LoadModel();
        }
        
        public void StopOnnx(){}

        public void LoadModel()
        {
            Debug.Log("Loading ONNX Model");
            _runtimeModel = ModelLoader.Load(loadedModel);
            _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _runtimeModel);
            Debug.Log("Finished Loading ONNX Model");
        }

        public void ProcessInput(List<Frame> frames)
        {
            isProcessing = true;
            foreach (Frame frame in frames)
            {
                /*
                 * TODO: Must transform frame to proper Tensor shape input
                 */
            }
            
            
        }

        public void StartPrediction()
        {
            Debug.Log("Started ONNX Prediction");
        }

        void Update()
        {
            if (loadedModel != null)
            {
                
            }
        }
    }
}
