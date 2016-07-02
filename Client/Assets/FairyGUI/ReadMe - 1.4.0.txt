--------------------
     FairyGUI
Copyright © 2014-2015 FairyGUI.com
Version 1.4.0
support@fairygui.com
--------------------
FairyGUI is a flexible UI framework for Unity, working with the professional FREE Game UI Editor: FairyGUI Editor. 
Download the editor from here: http://www.fairygui.com/download

--------------------
Get Started
--------------------
FairyGUI requires DOTween. Get it from Asset Store or http://dotween.demigiant.com/
Run demos in Assets/FairyGUI/Examples/Scenes.
The UI project is in Examples-UIProject.zip, unzip it anywhere. Use FairyGUI Editor to open it.

Using FairyGUI in Unity:
* Place a UIPanel in scene by using editor menu GameObject/FairyGUI/UIPanel.
* Or using UIPackage.CreateObject to create UI component dynamically.

-----------------
 Version History
-----------------
1.4.0
- NEW: Add GObject.BlendMode.
- NEW: Add GObject.Filter.
- NEW: Add GObject.Skew.

1.3.3
- NEW: Add gesture support

1.3.2
- NEW: Add free mask support.

1.3.1
- NEW: Add Tween delay to gears.
- FIX: GTextInput bug.
- IMPROVE: Remove white space in shader file name.

1.3.0
- NEW: New UIPainter component. Curve UI now supported.
- NEW: New list feature: page mode.
- NEW: New render order feature for GComponent: GComponent.childrenRenderOrder.
- NEW: Html parsing enhanced.
- NEW: Add Window.bringToFontOnClick and UIConfig.bringWindowToFrontOnClick.
- NEW: Add GTextInput.keyboardType
- REMOVE: GObject.SetPivotByRatio. Use SetPivot instead. 

1.2.3
- NEW: Add UIConfig.allowSoftnessOnTopOrLeftSide
- IMPROVE: UI Shaders.
- IMPROVE: Add GImage/GLoader/GMovieClip.shader/material

1.2.2
- NEW: Add "FairyGUI/UI Config" Component
- FIX: Pixel perfect issues.
- FIX: Change GMovieClip to keep its speed even time scale changed.

1.2.1
- NEW: Sort UIPanel by z order is supported. See Stage.SortWorldSpacePanelsByZOrder.
- IMPROVE: Image FillMethod implementation. FillImage shader is removed.
- IMPROVE: Shader optimization.
- 
1.2.0
- NEW: Added editor extensions: UIPanel/UIScaler/StageCamera.
- NEW: Added FillMethod to image/loader.
- IMPROVE: Refactor shaders.
- IMPROVE: Refactor some classes.
- REMOVE: StageNode. Use UIPanel instead.
- REMOVE: Stage prefab is removed, you dont need to put this into scene now.

1.1.0
- NEW: Added virtual list and loop list support. See GList.SetVirutal/GList.SetVirtualAndLoop
- NEW: Added StageNode. For displaying UI in perspetive camera.
- NEW: Added GObject.SetPivotByRatio.
- FIX: GTextField.maxLength now working correctly.
- REMOVE: MiniStage. Use StageNode instead.
- REMOVE: GComponent.onScroll. Use GComponent.scrollPane.onScroll instead.

1.0.3
- NEW: Now you can have different scrollbar styles for each component.
- NEW: Added fixed grip size option for scrollbar.
- NEW: Added tween value behavior for progressbar.
- NEW: Added Transition.playReverse.
- FIX: Scroll event bug.
- FIX: Large moveclip performance.

1.0.2
- NEW: Added Controller.autoRadioGroupDepth (UI Editor function)
- NEW: Added GTextInput.promptText (UI Editor function)
- FIX: Change to create MobileInputAdapter statically to avoid il2cpp problem.

1.0.1
- NEW: Added EventBrige to improve Event system.
- NEW: Added Container.touchChildren. 
- FIX: RenderingOrder will now handle properly when display list changing frequently.
- FIX: Use GRoot.scaleX/Y for content scaling, removed the redundant code.