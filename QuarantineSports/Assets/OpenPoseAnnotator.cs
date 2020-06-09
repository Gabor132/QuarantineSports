
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.DatasetObjects;
using Newtonsoft.Json;
using OpenPose;
//using OpenPose.Modules.Scripts;
using UnityEngine;
using UnityEngine.UI;
using Video_Player_Scripts;
using Formatting = Newtonsoft.Json.Formatting;
using RenderMode = OpenPose.RenderMode;
using ScaleMode = OpenPose.ScaleMode;

namespace Core.OpenPoseHandling
{
    /**
     * This class is made to extract the OpenPose Keypoints locations upon extracing of images for the dataset and
     * create the dataset needed for training the classifier
     */
    public class OpenPoseAnnotator : MonoBehaviour{

        // UI elements
        public Text stateText;

        // OpenPose Datum
        private OPDatum _datum;

        private bool _running = false;

        // Image Output
        public ImageRenderer image;

        // Video Specific Input
        public string videoInputPath;
        public List<VideoPlayerScript.Sequence> sequences;
        public string outputPath;
        public bool exportPosePicture = false;

        // Auxiliar Variables
        private string _videoName;
       

        // Frames that will be exported in JSON
        private List<Frame> _output = new List<Frame>();

        // OpenPose variables
        public int maxPeople = 1;
        public float renderThreshold = 0.05f;

        public Vector2Int
            netResolution = new Vector2Int(-1, 80),
            handResolution = new Vector2Int(368, 368),
            faceResolution = new Vector2Int(368, 368);



        public void ApplyChanges()
        {
            // Restart OpenPose
            StartCoroutine(UserRebootOpenPoseCoroutine());
        }


        private void Start()
        {
            Debug.Log("Starting Openpose");
            _output = new List<Frame>();

            // Extract Video name for exporting
            string[] splitResult = videoInputPath.Split('\\');
            splitResult = splitResult[splitResult.Length - 1].Split('.');
            _videoName = splitResult[0];




            // Register callbacks
            OPWrapper.OPRegisterCallbacks();
            // Enable OpenPose log to unity (default true)
            OPWrapper.OPEnableDebug(true);
            // Enable OpenPose output to unity (default true)
            OPWrapper.OPEnableOutput(true);
            // Enable receiving image (default false)
            OPWrapper.OPEnableImageOutput(false);

            UserConfigureOpenPose();

            // Start OpenPose

            if (OPWrapper.state != OPState.Ready)
            {
                StartCoroutine(UserRebootOpenPoseCoroutine());
                Debug.Log("Started Coroutine as OP was not ready");
                return;
            }
            OPWrapper.OPRun();
            _running = true;


            // Configure OpenPose with default value, or using specific configuration for each
            /* OPWrapper.OPConfigureAllInDefault(); */
            Debug.Log("OP Started");
        }

        // Parameters can be set here
        private void UserConfigureOpenPose()
        {

            // Configuring OpenPose to handle images
            OPWrapper.OPConfigurePose(
                /* poseMode */ PoseMode.Enabled,
                /* netInputSize */ netResolution,
                /* outputSize */ null,
                /* keypointScaleMode */ ScaleMode.ZeroToOne,
                /* gpuNumber */ -1,
                /* gpuNumberStart */ 0,
                /* scalesNumber */ 1,
                /* scaleGap */ 0.25f,
                /* renderMode */ RenderMode.Auto,
                /* poseModel */ PoseModel.BODY_25,
                /* blendOriginalFrame */ true,
                /* alphaKeypoint */ 0.6f,
                /* alphaHeatMap */ 0.7f,
                /* defaultPartToRender */ 0,
                /* modelFolder */ null,
                /* heatMapTypes */ HeatMapType.None,
                /* heatMapScaleMode */ ScaleMode.ZeroToOne,
                /* addPartCandidates */ false,
                /* renderThreshold */ renderThreshold,
                /* numberPeopleMax */ maxPeople,
                /* maximizePositives */ false,
                /* fpsMax fps_max */ -1.0,
                /* protoTxtPath */ "",
                /* caffeModelPath */ "",
                /* upsamplingRatio */ 0f);

            // Configure OpenPose to not handle hands

            OPWrapper.OPConfigureHand(
                /* enable */ false,
                /* detector */ Detector.Body,
                /* netInputSize */ handResolution,
                /* scalesNumber */ 1,
                /* scaleRange */ 0.4f,
                /* renderMode */ RenderMode.Auto,
                /* alphaKeypoint */ 0.6f,
                /* alphaHeatMap */ 0.7f,
                /* renderThreshold */ 0.2f);

            //Configure OpenPose to not handle faces
            OPWrapper.OPConfigureFace(
                /* enable */ false,
                /* detector */ Detector.Body,
                /* netInputSize */ faceResolution,
                /* renderMode */ RenderMode.Auto,
                /* alphaKeypoint */ 0.6f,
                /* alphaHeatMap */ 0.7f,
                /* renderThreshold */ 0.4f);

            // Configure if OpenPose should do 3D reconstruction
            OPWrapper.OPConfigureExtra(
                /* reconstruct3d */ false,
                /* minViews3d */ -1,
                /* identification */ false,
                /* tracking */ -1,
                /* ikThreads */ 0);

            // Configure OpenPose input type
            OPWrapper.OPConfigureInput(
                /* producerType */ ProducerType.Video,
                /* producerString */ videoInputPath,
                /* frameFirst */ 0,
                /* frameStep */ 1,
                /* frameLast */ ulong.MaxValue,
                /* realTimeProcessing */ false,
                /* frameFlip */ false,
                /* frameRotate */ 0,
                /* framesRepeat */ false,
                /* cameraResolution */ null,
                /* cameraParameterPath */ null,
                /* undistortImage */ false,
                /* numberViews */ -1);

            // Configure OpenPose output type
            OPWrapper.OPConfigureOutput(
                /* verbose */ -1.0,
                /* writeKeypoint */ "",
                /* writeKeypointFormat */ DataFormat.Json,
                /* writeJson */  "",
                /* writeCocoJson */ "",
                /* writeCocoJsonVariants */ 1,
                /* writeCocoJsonVariant */ 1,
                /* writeImages */ "",
                /* writeImagesFormat */ "png",
                /* writeVideo */ exportPosePicture ? System.IO.Path.GetTempPath() + _videoName + ".avi" : "",
                /* writeVideoFps */ -1.0,
                /* writeVideoWithAudio */ false,
                /* writeHeatMaps */ "",
                /* writeHeatMapsFormat */ "png",
                /* writeVideo3D */ "",
                /* writeVideoAdam */ "",
                /* writeBvh */ "",
                /* udpHost */ "",
                /* udpPort */ "8051");

            OPWrapper.OPConfigureGui(
                /* displayMode */ DisplayMode.NoDisplay,
                /* guiVerbose */ false,
                /* fullScreen */ false);

            OPWrapper.OPConfigureDebugging(
                /* loggingLevel */ Priority.High,
                /* disableMultiThread */ false,
                /* profileSpeed */ 1000);
        }

        private IEnumerator UserRebootOpenPoseCoroutine()
        {
            if (OPWrapper.state == OPState.None) yield break;
            // Shutdown if running
            if (OPWrapper.state == OPState.Running)
            {
                OPWrapper.OPShutdown();
            }
            // Wait until fully stopped
            yield return new WaitUntil(() => { return OPWrapper.state == OPState.Ready; });
            // Configure and start
            UserConfigureOpenPose();
            OPWrapper.OPRun();
            _running = true;

        }

        

        private void Update()
        {
            // Update state in UI
            stateText.text = OPWrapper.state.ToString();
            if (_running)
            {
                //Debug.Log(OPWrapper.state.ToString());
                if (OPWrapper.OPGetOutput(out _datum))
                { // true: has new frame data
                    Debug.Log("newdatum");
                    long currentFrame = (long)_datum.frameNumber;
                    Debug.Log($"Current frame: {currentFrame}");


                    image.UpdateImage(_datum.cvInputData);
                    int category = -1;

                    _output.Add(new Frame(_datum.poseKeypoints, category));
                    if (_datum.poseKeypoints != null)
                    {
                        if (!dimensions.ContainsKey(_datum.poseKeypoints.Count))
                        {
                            dimensions.Add(_datum.poseKeypoints.Count, currentFrame);
                        }
                    }
                }
                
            }
            // Try getting new frame
           
        }

        public Dictionary<int, long> dimensions = new Dictionary<int, long>();

        public void SaveOutput()
        {
            Debug.Log("Saving output as JSON");
            string output = JsonConvert.SerializeObject(_output, Formatting.Indented);
            
            string temp = System.IO.Path.Combine(System.IO.Path.GetTempPath(),_videoName);
            File.WriteAllText(temp + ".json", output);
            foreach (int i in dimensions.Keys)
            {
                Debug.Log($"Dimension: {i} -> Frame {dimensions[i]}");
            }
        }

        public void Stop()
        {
            _running = false;
            SaveOutput();
            OPWrapper.OPShutdown();
        }
    }
}
