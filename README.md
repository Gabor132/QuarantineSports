# QuarantineSports
Visual Analysis of Human Motion Project


# Setup

1. Clone the project.</br>
2. The project won't work yet, don't panic.</br>
3. Clone OpenPose Unity Project:</br>
  a. Use the link <a href="https://github.com/CMU-Perceptual-Computing-Lab/openpose_unity_plugin">here</a></br>
  b. Run getPlugins.bat</br>
  c. Run getModels.bat</br>
  d. Check if the Demo.unity scene works</br>
  e. Export the unity project as a package</br>
4. Import the OpenPose package into the project, only the following directories are necessare:</br>
  a. StreamingAssets/\*</br>
  b. OpenPose/Modules/Scripts/\*</br>
  c. OpenPose/Plugins/\*</br>
5. Download NuGet .unitypackage from <a href="https://github.com/GlitchEnzo/NuGetForUnity/releases">Link</a> and import the package in the project
6. Restart the Unity Editor
7. Use NuGet (it will have a tab now in the editor) to import Newtonsoft.Json
8. Add the "Runtime File Browser" Asset into the project from this <a href="https://github.com/yasirkula/UnitySimpleFileBrowser/releases/tag/v1.2.0">link</a></br>
9. Go to Unity Package Manager and install Barracuda 1.0.0 then make sure that the model int Our_Models is assigned to the instance of OnnxHandler within the game scene.
10. Everything should work now.
11. If compiler error CS0234: "The type or namespace name 'Modules' does not exist" appears, uncomment line 5 in WebcamOpenPoseHandler.cs and line 7 in VideoAnnotationOpenPoseHandler.cs. (using OpenPose.Modules.Scripts;)
