using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.DatasetObjects;
using Core.OpenPoseHandling;
using OpenPose;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Video_Player_Scripts
{
    /**
     * Class Used for annotating the input videos
     */
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
        public Button endSequenceButton;
        public Button discardSequenceButton;
        public Slider videoSlider;
        public InputField currentVideoTime;
        public InputField totalVideoTime;
        
        public Dropdown sequencesDropdown;
        public Text sequenceCurrentText;
        public Toggle exportOutputAsContent;
        public Dropdown sequenceCategoryDropdown;
        
        /*
         * Auxiliar Variables
         */
        private Sequence _currentSequence;
        private string _defaultSequenceTextFormat = "Current Sequence: {0}: {1} -> {2}: {3}";
        private string _defaultSequenceTextEmpty = "No current sequence";
        private double _lengthOfVideoInFrames;

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

        public void GoToNextFrame()
        {
            PausePlaying();
            long newFrame = _videoPlayer.frame + 1L;
            _videoPlayer.frame = newFrame;
            currentVideoTime.text = newFrame.ToString();
            Debug.Log($"Setting the video at frame: {newFrame}");
            UpdateCurrentSequence(newFrame);
            UpdateTexture();
            StartPlaying();
            PausePlaying();
        }

        public void GoToPreviousFrame()
        {
            PausePlaying();
            long newFrame = _videoPlayer.frame - 1L;
            _videoPlayer.frame = newFrame;
            currentVideoTime.text = newFrame.ToString();
            Debug.Log($"Setting the video at frame: {newFrame}");
            UpdateCurrentSequence(newFrame);
            UpdateTexture();
            StartPlaying();
            PausePlaying();
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
                _lengthOfVideoInFrames = Math.Floor((float) _videoPlayer.length * _videoPlayer.frameRate);
                Debug.Log("Length of video: " + _lengthOfVideoInFrames);
                videoSlider.wholeNumbers = true;
                videoSlider.maxValue = (float) _lengthOfVideoInFrames;
                videoSlider.minValue = 0f;

                // Setup CurrentVideoTime
                currentVideoTime.text = "0";
                
                
                // Setup TotalVideoTime
                totalVideoTime.text = _lengthOfVideoInFrames.ToString();
                
                // Setup Sequence buttons
                startSequenceButton.interactable = true;
                endSequenceButton.interactable = false;
                discardSequenceButton.interactable = false;
                // Annotation category dropdown
                sequenceCategoryDropdown.interactable = false;

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
                    _currentSequence.EndFrame, _currentSequence.Category);
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
            discardSequenceButton.interactable = true;
            
            // Setup Dropdown
            sequencesDropdown.interactable = false;
            sequenceCategoryDropdown.interactable = true;
            
            UpdateCurrentSequence(newFrame);
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
            discardSequenceButton.interactable = false;
            
            // Setup Dropdown
            sequencesDropdown.interactable = true;
            sequenceCategoryDropdown.interactable = false;
            
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
            discardSequenceButton.interactable = false;
            
            // Setup Dropdown
            sequencesDropdown.interactable = true;
            sequenceCategoryDropdown.interactable = false;
            
            // Set dropdown to None
            sequencesDropdown.value = 0;
            
            UpdateCurrentSequence(_videoPlayer.frame);
        }
        public void UpdateSequenceCategory()
        {
            if (_currentSequence != null)
            {
                Dropdown.OptionData option = sequenceCategoryDropdown.options[sequenceCategoryDropdown.value]; 
                Category desiredCategory;
                if (Enum.TryParse(option.text, out desiredCategory))
                {
                    SetSequenceCategory(desiredCategory);
                }
            }
        }

        public void SetSequenceCategory(Category category)
        {
            Debug.Log("Setting individual frame sequence to " + category);
            if (_currentSequence == null)
            {
                Debug.Log("Must first start a sequence");
                return;
            }
            _currentSequence.Category = category;
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
                videoSlider.value = _currentSequence.EndFrame;
                currentVideoTime.text = _currentSequence.EndFrame.ToString();
                
                // Setup buttons
                startSequenceButton.interactable = false;
                endSequenceButton.interactable = true;
                discardSequenceButton.interactable = true;
                sequenceCategoryDropdown.interactable = true;
                //
                // Set IndividualFrameDropdown existing value
                //
                Dropdown.OptionData categoryOption =
                    sequenceCategoryDropdown.options.Find(data =>
                        data.text.Equals(_currentSequence.Category.ToString()));
                if (categoryOption == null)
                {
                    Debug.LogError("Unknown category!");
                }
                else
                {
                    sequenceCategoryDropdown.value =
                        sequenceCategoryDropdown.options.IndexOf(categoryOption);
                }
                
                // Set Current Sequence Text
                sequenceCurrentText.text = string.Format(_defaultSequenceTextFormat, _currentSequence.Id, _currentSequence.StartFrame,
                    _currentSequence.EndFrame, _currentSequence.Category);
            }
            else
            {
                _currentSequence = null;
                
                // Set Current Sequence Text
                sequenceCurrentText.text = _defaultSequenceTextEmpty;
                
                // Setup buttons
                startSequenceButton.interactable = true;
                endSequenceButton.interactable = false;
                discardSequenceButton.interactable = false;
                sequenceCategoryDropdown.interactable = false;
            }
            
            UpdateTexture();
        }
        public class Sequence
        {
            public long StartFrame { get; set; }
            public long EndFrame { get; set; }
            public Category Category { get; set; }

            public int Id { get; set; }

            public static int nextId = 0;

            public Sequence()
            {
                StartFrame = 0;
                EndFrame = 0;
                Id = nextId;
                nextId++;
            }
            
            public Sequence(long startFrame)
            {
                StartFrame = startFrame;
                EndFrame = startFrame;
                Id = nextId;
                nextId++;
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
            _ExportSequences();
        }
        private void _ExportSequences()
        {
            VideoAnnotationOpenPoseHandler handler = exportMenu.GetComponent<VideoAnnotationOpenPoseHandler>();
            handler.videoInputPath = _videoPath;
            Debug.Log(_videoPath);
            handler.sequences = new List<Sequence>(_sequences);
            handler.lengthOfVideoInFrames = (long) _lengthOfVideoInFrames;
            handler.outputPath = _outputPath;
            handler.exportPosePicture = exportOutputAsContent.isOn;
            exportMenu.SetActive(true);
            if (handler.stateText.text.Equals(OPState.Ready.ToString()))
            {
                Debug.Log("apply changes called");
                handler.ApplyChanges();
            }
            gameObject.SetActive(false);
        }
        
    }
}