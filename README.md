# QuarantineSports
Visual Analysis of Human Motion Project


# Current Setup (not the best)

1. Clone the project.
2. The project won't work yet, don't panic.
3. Clone OpenPose Unity Project:
  a. Use the link here
  b. Run getPlugins.bat
  c. Run getModels.bat
  d. Check if the Demo.unity scene works
  e. Export the unity project as a package
4. Import the OpenPose package into the project, only the following directories are necessare:
  a. StreamingAssets/*
  b. Examples/Scripts/ImageRenderer
  c. Modules/Scripts/*
  d. Plugins/*
5. TextMesh Pro should be automatically imported by Unity, it will ask you.
6. Download NuGet .unitypackage from <a href="https://github.com/GlitchEnzo/NuGetForUnity/releases">Link</a> and import the package in the project
7. Restart the Unity Editor
8. Use NuGet (it will have a tab now in the editor) to import Newtonsoft.Json
9. Everything should work now.
10. If not, oh god.