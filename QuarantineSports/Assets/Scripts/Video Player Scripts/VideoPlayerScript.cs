using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.DatasetObjects;
using Core.OpenPoseHandling;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Video_Player_Scripts
{
    public class VideoPlayerScript : MonoBehaviour
    {
        /*
         * Raw Image to Show Video Images [Assign from the Editor]
         */
        public RawImage image;

        /*
         * Input Path Field
         */
        public InputField inputPathField;
        
        /*
         * Output Path Field
         */
        public InputField outputPathField;

        /*
         * Number of frames to extract per second
         */
        public InputField numberOfFramesField;

        /*
         * The Menu shown while annotating the data using OpenPose
         */
        public GameObject exportMenu;

        private string _videoPath;
        private string _outputPath;

        /*
         * Unity Components
         */
        // Unity Video Player
        private VideoPlayer _videoPlayer;
        
        // Others
        public Text videoTitle;
        public GameObject playButton;
        public GameObject pauseButton;
        public Button startSequenceButton;
        public Button correctSequenceButton;
        public Button wrongSequenceButton;
        public Button endSequenceButton;
        public Button discardSequenceButton;
        public Slider videoSlider;
        public InputField currentVideoTime;
        public InputField totalVideoTime;
        
        public Dropdown sequencesDropdown;
        public Text sequenceCurrentText;
        
        /*
         * Auxiliar Variables
         */
        private Sequence _currentSequence;
        private string _defaultSequenceTextFormat = "Current Sequence: {0}: {1} -> {2}: {3}";
        private string _defaultSequenceTextEmpty = "No current sequence";

        /*
         * Annotation Sequences
         */
        private readonly List<Sequence> _sequences = new List<Sequence>();
        
        /*
         * Initialization
         */
        void Start()
        {
            Application.runInBackground = true;
            
            // Initialize Video Player Components
            if (inputPathField == null || string.IsNullOrEmpty(inputPathField.text))
            {
                Debug.Log("Input Path was not set");
                return;
            }
            _videoPath = inputPathField.text;
            _outputPath = outputPathField.text;
            
            // Start Preparation Coroutine
            StartCoroutine(PrepareVideo());

        }

        /*
         * Video Controls
         */
        public void StartPlaying()
        {
            playButton.SetActive(false);
            pauseButton.SetActive(true);
            _videoPlayer.Play();
        }
        public void PausePlaying()
        {
            playButton.SetActive(true);
            pauseButton.SetActive(false);
            _videoPlayer.Pause();
        }

        public void StopPlaying()
        {
            playButton.SetActive(true);
            pauseButton.SetActive(false);
            _videoPlayer.Stop();
        }
        public void ApplySliderChange(bool fromUi = false)
        {
            if (fromUi)
            {
                bool videoWasPlaying = _videoPlayer.isPlaying;
                PausePlaying();
                float sliderValue = videoSlider.value;
                _videoPlayer.frame = (long) sliderValue;
                currentVideoTime.text = sliderValue.ToString();
                UpdateCurrentSequence((long) sliderValue);
                UpdateTexture();
                if (videoWasPlaying)
                {
                    StartPlaying();
                }
            }
        }
        public void ApplyInputFieldChange(bool fromUi = false)
        {
            if (fromUi)
            {
                bool videoWasPlaying = _videoPlayer.isPlaying;
                PausePlaying();
                int newFrame = int.Parse(currentVideoTime.text);
                Debug.Log($"Setting the video at frame: {newFrame}");
                _videoPlayer.frame = newFrame;
                videoSlider.value = newFrame;
                UpdateCurrentSequence(newFrame);
                UpdateTexture();
                if (videoWasPlaying)
                {
                    StartPlaying();
                }
            }
        }

        /*
         * Video Player Preparations
         */
        private IEnumerator PrepareVideo()
        {
            // Setup video player
            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            _videoPlayer.playOnAwake = false;
            _videoPlayer.url = _videoPath;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

            // Prepare the video
            Debug.Log("Preparing Video");
            _videoPlayer.Prepare();
            while (! _videoPlayer.isPrepared)
            {
                yield return null;
            }
            Debug.Log("Done Preparing Video");
            
            PrepareComponents();

        }
        private void PrepareComponents()
        {
            if (_videoPlayer.isPrepared)
            {
                // Set the title of the video
                videoTitle.text = "Video: " + _videoPath;

                // Setup Play/Stop Buttons
            
                playButton.SetActive(true);
                pauseButton.SetActive(false);
            
                // Setup Slider
                double lengthOfVideoInFrames = Math.Floor((float) _videoPlayer.length * _videoPlayer.frameRate);
                Debug.Log("Length of video: " + lengthOfVideoInFrames);
                videoSlider.wholeNumbers = true;
                videoSlider.maxValue = (float) lengthOfVideoInFrames;
                videoSlider.minValue = 0f;

                // Setup CurrentVideoTime
                currentVideoTime.text = "0";
                
                
                // Setup TotalVideoTime
                totalVideoTime.text = lengthOfVideoInFrames.ToString();
                
                // Setup Sequence buttons
                startSequenceButton.interactable = true;

                // Setup Sequence Text
                sequenceCurrentText.text = _defaultSequenceTextEmpty;
                
                // Setup Frame per second input with video default
                //_videoPlayer.frameRate
                numberOfFramesField.text = ((int) Math.Floor(1f)).ToString();
            }
            else
            {
                Debug.LogWarning("Components can only be prepared when the video player is prepared!");
            }

        }


        /*
         * Update Logic
         */
        void Update()
        {
            // Assign the Texture from Video to RawImage to be displayed
            if (_videoPlayer.isPrepared && _videoPlayer.isPlaying)
            {
                UpdateTexture();
                long newFrame = _videoPlayer.frame;
                videoSlider.value = newFrame;
                UpdateCurrentSequence(newFrame);
                currentVideoTime.text = _videoPlayer.frame.ToString();
            }
        }
        void UpdateTexture()
        {
            image.texture = _videoPlayer.texture;
        }
        void UpdateCurrentSequence(long newFrame)
        {
            if (_currentSequence != null && _currentSequence.StartFrame <= newFrame)
            {
                _currentSequence.EndFrame = newFrame;
                sequenceCurrentText.text = string.Format(_defaultSequenceTextFormat, _currentSequence.Id, _currentSequence.StartFrame,
                    _currentSequence.EndFrame, _currentSequence.IsCorrect ? "Correct" : "Wrong");
            }
            else
            {
                sequenceCurrentText.text = _defaultSequenceTextEmpty;
            }
        }
        
        /*
         * Sequence Logic & Class
         */
        public void CreateSequence()
        {
            Debug.Log("Adding new Sequence");
            if (_currentSequence != null)
            {
                Debug.Log("Must first finish the current sequence");
                return;
            }

            long newFrame = _videoPlayer.frame;
            Sequence newSequence = new Sequence(newFrame);
            _currentSequence = newSequence;
            
            // Setup buttons
            startSequenceButton.interactable = false;
            endSequenceButton.interactable = true;
            correctSequenceButton.interactable = true;
            wrongSequenceButton.interactable = false;
            discardSequenceButton.interactable = true;
            
            // Setup Dropdown
            sequencesDropdown.interactable = false;
            
            UpdateCurrentSequence(newFrame);
        }
        public void SetSequence(bool isCorrect)
        {
            Debug.Log("Setting sequence to " + isCorrect);
            if (_currentSequence == null)
            {
                Debug.Log("Must first start a sequence");
                return;
            }
            _currentSequence.IsCorrect = isCorrect;
            
            // Setup buttons
            correctSequenceButton.interactable = ! isCorrect;
            wrongSequenceButton.interactable = isCorrect;
            
            UpdateCurrentSequence(_videoPlayer.frame);
        }
        public void SaveSequence()
        {
            Debug.Log("Ending Sequence");
            if (_currentSequence == null)
            {
                Debug.Log("Must first start a sequence");
                return;
            }
            
            // Check if the sequence was already saved
            Dropdown.OptionData alreadyExistingOption =
                sequencesDropdown.options.Find(data => data.text.Equals(_currentSequence.GetSequenceText()));
            if (alreadyExistingOption == null)
            {
                // The sequence is brand new
                _sequences.Add(_currentSequence);
                Dropdown.OptionData newOptionData = new Dropdown.OptionData();
                newOptionData.text = _currentSequence.GetSequenceText();
                sequencesDropdown.options.Add(newOptionData);
            }
            
            // Reset everything else
            _currentSequence = null;
            
            // Setup buttons
            startSequenceButton.interactable = true;
            endSequenceButton.interactable = false;
            correctSequenceButton.interactable = false;
            wrongSequenceButton.interactable = false;
            discardSequenceButton.interactable = false;
            
            // Setup Dropdown
            sequencesDropdown.interactable = true;
            
            // Set dropdown to None
            sequencesDropdown.value = 0;
            
            UpdateCurrentSequence(_videoPlayer.frame);
        }
        public void DeleteSequence()
        {
            // Check if sequence was already saved
            Dropdown.OptionData alreadyExistingOption =
                sequencesDropdown.options.Find(data => data.text.Equals(_currentSequence.GetSequenceText()));
            if (alreadyExistingOption != null)
            {
                // The sequence was already saved, proceed to delete if from dropdown and array
                _sequences.Remove(_currentSequence);
                sequencesDropdown.options.Remove(alreadyExistingOption);
            }
            
            // Reset everything else
            _currentSequence = null;
            // Setup buttons
            startSequenceButton.interactable = true;
            endSequenceButton.interactable = false;
            correctSequenceButton.interactable = false;
            wrongSequenceButton.interactable = false;
            discardSequenceButton.interactable = false;
            
            // Setup Dropdown
            sequencesDropdown.interactable = true;
            
            // Set dropdown to None
            sequencesDropdown.value = 0;
            
            UpdateCurrentSequence(_videoPlayer.frame);
        }
        public void SelectSequence()
        {
            PausePlaying();
            Dropdown.OptionData option = sequencesDropdown.options[sequencesDropdown.value];
            Debug.Log("Selecting option: " + option.text);
            if (_currentSequence != null && !option.text.Equals(_currentSequence.GetSequenceText()))
            {
                Debug.Log("Ending current sequence");
                // Check if the sequence was already saved
                Dropdown.OptionData alreadyExistingOption =
                    sequencesDropdown.options.Find(data => data.text.Equals(_currentSequence.GetSequenceText()));
                if (alreadyExistingOption == null)
                {
                    // The sequence is brand new
                    _sequences.Add(_currentSequence);
                    Dropdown.OptionData newOptionData = new Dropdown.OptionData();
                    newOptionData.text = _currentSequence.GetSequenceText();
                    sequencesDropdown.options.Add(newOptionData);
                }
            }
            if(! option.text.Equals("None"))
            {
                // Retrieve sequence from list
                _currentSequence = _sequences.Find(sequence => sequence.GetSequenceText().Equals(option.text));
                if (_currentSequence == null)
                {
                    Debug.Log("Failed to find sequence with text: " + option.text);
                    return;
                }
                
                // Set Video player to ending of sequence
                _videoPlayer.frame = _currentSequence.EndFrame;
                videoSlider.value = (float) _currentSequence.EndFrame;
                currentVideoTime.text = _currentSequence.EndFrame.ToString();
                
                // Setup buttons
                startSequenceButton.interactable = false;
                endSequenceButton.interactable = true;
                correctSequenceButton.interactable = ! _currentSequence.IsCorrect;
                wrongSequenceButton.interactable = _currentSequence.IsCorrect;
                discardSequenceButton.interactable = true;
                
                // Set Current Sequence Text
                sequenceCurrentText.text = string.Format(_defaultSequenceTextFormat, _currentSequence.Id, _currentSequence.StartFrame,
                    _currentSequence.EndFrame, _currentSequence.IsCorrect ? "Correct" : "Wrong");
            }
            else
            {
                _currentSequence = null;
                
                // Set Current Sequence Text
                sequenceCurrentText.text = _defaultSequenceTextEmpty;
                
                // Setup buttons
                startSequenceButton.interactable = true;
                endSequenceButton.interactable = false;
                correctSequenceButton.interactable = false;
                wrongSequenceButton.interactable = false;
                discardSequenceButton.interactable = false;
                
            }
            
            UpdateTexture();
        }
        public class Sequence
        {
            public long StartFrame { get; set; }
            public long EndFrame { get; set; }
            public bool IsCorrect { get; set; }

            public int Id { get; set; }

            public static int NextId = 0;

            public Sequence()
            {
                StartFrame = 0;
                EndFrame = 0;
                Id = NextId;
                NextId++;
            }
            
            public Sequence(long startFrame)
            {
                StartFrame = startFrame;
                EndFrame = startFrame;
                Id = NextId;
                NextId++;
            }

            public string GetSequenceText()
            {
                return string.Format("Sequence {0}", Id);
            }
        }
        
        /*
         * Export Frames with categories
         */
        public void ExportSequences()
        {
            Debug.Log("Exporting sequences");
            if (string.IsNullOrEmpty(_outputPath))
            {
                Debug.Log("Output path is not set");
                return;
            }

            StartCoroutine(ExportSequencesCoroutine());
        }

        IEnumerator ExportSequencesCoroutine()
        {
            Dataset dataset = new Dataset(_videoPath, _outputPath);
            
            float frameRate = _videoPlayer.frameRate;
            
            int framesPerSecond = int.Parse(numberOfFramesField.text);

            float frameIncrease = (frameRate / framesPerSecond);

            foreach(Sequence sequence in _sequences)
            {
                List<byte[]> frames = new List<byte[]>();
                long startFrame = sequence.StartFrame;
                long endFrame = sequence.EndFrame;
                Debug.Log("Extracting textures between: " 
                          + startFrame
                          + " and " 
                          + endFrame);
                while (startFrame <= endFrame)
                {
                    // Move Video Player to desired frame
                    _videoPlayer.frame = startFrame;
                    _videoPlayer.Prepare();
                    yield return new WaitForSeconds(1);
                    
                    Debug.Log("Getting texture at frame " + _videoPlayer.frame + " / time " + TimeSpan.FromSeconds(_videoPlayer.time).ToString(@"mm\:ss"));
                    
                    // Extract the image as bytes
                    RenderTexture renderTexture = (RenderTexture) _videoPlayer.texture;
                    Texture2D textureToSave = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                    RenderTexture.active = renderTexture;
                    textureToSave.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    textureToSave.Apply();
                    byte[] bytes = textureToSave.EncodeToPNG();
                    
                    // Add the image in the list of frames
                    frames.Add(bytes);
                    
                    // Update the UI to visually see the progress
                    image.texture = _videoPlayer.texture;
                    startFrame += (long) Math.Floor(frameIncrease);
                }
                dataset.AddData(frames.ToArray(), sequence.IsCorrect ? Category.Correct : Category.Wrong);
            }
            
            dataset.SaveDataset();
            LightWeightOpenPoseHandler handler = exportMenu.GetComponent<LightWeightOpenPoseHandler>();
            handler.inputDataset = dataset;
            exportMenu.SetActive(true);
            gameObject.SetActive(false);

        }
        
    }
}