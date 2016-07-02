=============================================================================
 FingerGestures v3.1 (December 3rd 2013)
  http://fingergestures.fatalfrog.com 

 copyright (c) William Ravaine / Fatal Frog Software
=============================================================================

FingerGestures is a script package that lets you easily detect and react to 
input gestures performed with either a mouse or a touch screen device.

To get started with FingerGestures, please consult the online manual 
at http://fingergestures.fatalfrog.com/docs which includes setup instructions 
and several short tutorials.

For more information, please visit the official FingerGestures website 
at http://fingergestures.fatalfrog.com

=======================
Basic Use Instructions
=======================

[Setup]
Add a single FingerGestures component to an object in your scene

[Detecting Standard Gestures]
- Add any of the gesture recognizer scripts from the "Plugins\FingerGestures\Gesture Recognizers" folder to an object in your scene
- To allow the gesture to interact with colliders in the scene, add a "Screen Raycaster" component to it and set it up accordingly
- If the "Use Send Message" property on the gesture recognizer is enabled, it will broadcast an event called "Message Name" to 
  the "Message Target" object (current object by default). The gesture recognizer can also optionally send the same message to the scene object
  it's interacting with (GestureRecognizer.Selection).
- Each gesture recognizer also exposes a standard .NET event (delegate-based) that you can subscribe to by code in order to get fastest performance

[Detecting Finger Events]
- Add any of the FingerEventDetector component from the "Plugins\FingerGestures\Finger Event Detectors" folder to an object in your scene
- To allow the detector to interact with colliders in the scene, add a "Screen Raycaster" component to it and set it up accordingly
- The FingerEventDetectors can also fire events either via SendMessage() or standard .net delegate-based events

=====================
Sample Scenes Setup
=====================
The samples are included in the "Samples.unitypackage" sub-package included in this release. To add the samples to your project, simply double 
click that package file, then click the "Import" button. All the samples content will be copied to your "Assets\FingerGestures Samples" folder.

Due to limitations with Unity's package system, the sample scenes cannot be automatically added to the build settings. 
In order to use the sample browser, you must do this setup manually:

1. Open your project's build settings via File > Build Settings. 
2. Locate the various scene files in your "Assets\FingerGestures Samples" folders
3. Drag & drop all the scenes files you find to the Build Settings window's "Scenes in Build" list
4. Ensure that the "Start" scene is the first item in the list
5. Make a standalone or web-browser build and run it to try out the sample browser

=====================
Contact & Support
=====================
If you need technical support, please visit http://fingergestures.fatalfrog.com/support. 
For more general inquiries or feedback, please use the forum at http://forum.fatalfrog.com. 
You may also contact me directly at fingergestures@fatalfrog.com if all else fails :)

Thank you for using FingerGestures!
William Ravaine
