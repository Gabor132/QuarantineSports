using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Core.DatasetObjects;
using Newtonsoft.Json;
using OpenPose;
using OpenPose.Example;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Formatting = Newtonsoft.Json.Formatting;
using RenderMode = OpenPose.RenderMode;
using ScaleMode = OpenPose.ScaleMode;

namespace Core.OpenPoseHandling
{
    /**
     * This class is made to extract the OpenPose Keypoints locations upon extracing of images for the dataset and
     * create the dataset needed for training the classifier
     */
    public class LightWeightOpenPoseHandler : MonoBehaviour
    {

        // UI elements
        public Text stateText;

        // Output
        private OPDatum _datum;

        private List<OutputKeypoints> _output = new List<OutputKeypoints>();

        public ImageRenderer image;

        private JsonSerializer _jsonSerializer = JsonSerializer.CreateDefault();

        public Dataset inputDataset;
        
        private Data _currentData;
        
        public int maxPeople = -1;
        public float renderThreshold = 0.05f;
        public Vector2Int
            netResolution = new Vector2Int(-1, 368),
            handResolution = new Vector2Int(368, 368),
            faceResolution = new Vector2Int(368, 368);

        
        
        public void ApplyChanges(){
            // Restart OpenPose
            StartCoroutine(UserRebootOpenPoseCoroutine());
        }
        
        
        public void StartOpenPose() {

            _output = new List<OutputKeypoints>();
            
            // Register callbacks
            OPWrapper.OPRegisterCallbacks();
            // Enable OpenPose log to unity (default true)
            OPWrapper.OPEnableDebug(true);
            // Enable OpenPose output to unity (default true)
            OPWrapper.OPEnableOutput(true);
            // Enable receiving image (default false)
            OPWrapper.OPEnableImageOutput(true);

            // Configure OpenPose with default value, or using specific configuration for each
            /* OPWrapper.OPConfigureAllInDefault(); */
            UserConfigureOpenPose();

            // Start OpenPose
            OPWrapper.OPRun();
        }

        // Parameters can be set here
        private void UserConfigureOpenPose(){
            
            // Configuring OpenPose to handle images
            OPWrapper.OPConfigurePose(
                /* poseMode */ PoseMode.Enabled, /* netInputSize */ netResolution, /* outputSize */ null,
                /* keypointScaleMode */ ScaleMode.InputResolution,
                /* gpuNumber */ -1, /* gpuNumberStart */ 0, /* scalesNumber */ 1, /* scaleGap */ 0.25f,
                /* renderMode */ RenderMode.Auto, /* poseModel */ PoseModel.BODY_25,
                /* blendOriginalFrame */ true, /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f,
                /* defaultPartToRender */ 0, /* modelFolder */ null,
                /* heatMapTypes */ HeatMapType.None, /* heatMapScaleMode */ ScaleMode.ZeroToOne,
                /* addPartCandidates */ false, /* renderThreshold */ renderThreshold, /* numberPeopleMax */ maxPeople,
                /* maximizePositives */ false, /* fpsMax fps_max */ -1.0,
                /* protoTxtPath */ "", /* caffeModelPath */ "", /* upsamplingRatio */ 0f);

            // Configure OpenPose to not handle hands
            OPWrapper.OPConfigureHand(
                /* enable */ false, /* detector */ Detector.Body, /* netInputSize */ handResolution,
                /* scalesNumber */ 1, /* scaleRange */ 0.4f, /* renderMode */ RenderMode.Auto,
                /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f, /* renderThreshold */ 0.2f);

            // Configure OpenPose to not handle faces
            OPWrapper.OPConfigureFace(
                /* enable */ false, /* detector */ Detector.Body, 
                /* netInputSize */ faceResolution, /* renderMode */ RenderMode.Auto,
                /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f, /* renderThreshold */ 0.4f);

            // Configure if OpenPose should do 3D reconstruction
            OPWrapper.OPConfigureExtra(
                /* reconstruct3d */ false, /* minViews3d */ -1, /* identification */ false, /* tracking */ -1,
                /* ikThreads */ 0);

            if (inputDataset != null)
            {
                if (inputDataset.Data.Count > 0)
                {
                    _currentData = inputDataset.Data[0];
                    inputDataset.Data.Remove(_currentData);
                    Debug.Log($"Input paths that have been found: {_currentData.GetPath()}");
                }
                else
                {
                    Debug.Log("No actual input path found");
                }
            }
            // Configure OpenPose input type
            OPWrapper.OPConfigureInput(
                /* producerType */ ProducerType.ImageDirectory, /* producerString */ _currentData.GetPath(),
                /* frameFirst */ 0, /* frameStep */ 1, /* frameLast */ ulong.MaxValue,
                /* realTimeProcessing */ false, /* frameFlip */ false,
                /* frameRotate */ 0, /* framesRepeat */ false,
                /* cameraResolution */ null, /* cameraParameterPath */ null,
                /* undistortImage */ false, /* numberViews */ -1);

            // Configure OpenPose output type
            OPWrapper.OPConfigureOutput(
                /* verbose */ -1.0, /* writeKeypoint */ "", /* writeKeypointFormat */ DataFormat.Json,
                /* writeJson */ "", /* writeCocoJson */ "", /* writeCocoJsonVariants */ 1,
                /* writeCocoJsonVariant */ 1, /* writeImages */ "", /* writeImagesFormat */ "png",
                /* writeVideo */ "", /* writeVideoFps */ -1.0, /* writeVideoWithAudio */ false,
                /* writeHeatMaps */ "", /* writeHeatMapsFormat */ "png", /* writeVideo3D */ "",
                /* writeVideoAdam */ "", /* writeBvh */ "", /* udpHost */ "", /* udpPort */ "8051");

            OPWrapper.OPConfigureGui(
                /* displayMode */ DisplayMode.NoDisplay, /* guiVerbose */ false, /* fullScreen */ false);
            
            OPWrapper.OPConfigureDebugging(
                /* loggingLevel */ Priority.High, /* disableMultiThread */ false, /* profileSpeed */ 1000);
        }

        private IEnumerator UserRebootOpenPoseCoroutine() {
            if (OPWrapper.state == OPState.None) yield break;
            // Shutdown if running
            if (OPWrapper.state == OPState.Running)
            {
                OPWrapper.OPShutdown();
            }
            // Wait until fully stopped
            yield return new WaitUntil( ()=>{ return OPWrapper.state == OPState.Ready; } );
            // Configure and start
            UserConfigureOpenPose();
            OPWrapper.OPRun();
        }

        private void Update() {
            // Update state in UI
            stateText.text = OPWrapper.state.ToString();

            // Try getting new frame
            if (OPWrapper.OPGetOutput(out _datum)){ // true: has new frame data
                
                image.UpdateImage(_datum.cvInputData);

                OutputKeypoints keypoints = new OutputKeypoints(_datum, _currentData.Category);
                _output.Add(keypoints);
                
            }

            if (OPWrapper.state == OPState.Ready && _output.Count > 0)
            {
                SaveOutput();
            }
        }

        private void SaveOutput()
        {
            string path = _currentData.GetPath() + "\\data.json";
            Debug.Log($"Writing OpenPose Keypoints to json file at {path}");
            using (FileStream file = new FileStream(path, FileMode.OpenOrCreate))
            {
                JsonWriter writer = new JsonTextWriter(new StreamWriter(file));
                writer.Formatting = Formatting.Indented;
                string jsonOutput = JsonConvert.SerializeObject(_output);
                Debug.Log($"JSON output is {jsonOutput}");
                _jsonSerializer.Serialize(writer, jsonOutput);
            }
            _output.Clear();
            if (inputDataset.Data.Count > 0)
            {
                ApplyChanges();
            }
        }

    }
}
