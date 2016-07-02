using UnityEngine;
using System.Collections.Generic;

public class FGTouchInputProvider : FGInputProvider 
{
    public int maxTouches = 5;

    #region Android Bug Workaround

    // not necessary with Unity 4.1+
    public bool fixAndroidTouchIdBug = true;    
    int touchIdOffset = 0;
    
    #endregion

    void Start()
    {
        finger2touchMap = new int[maxTouches];
    }
    
    void Update()
    {
        UpdateFingerTouchMap();
    }
    
    #region Touch > Finger mapping

    UnityEngine.Touch nullTouch = new UnityEngine.Touch();
    int[] finger2touchMap;  // finger.index -> touch index map

    void UpdateFingerTouchMap()
    {
        for( int i = 0; i < finger2touchMap.Length; ++i )
            finger2touchMap[i] = -1;

        // Android: work around strange Touch.fingerId values after resuming application. 
        // Not sure yet if this is a Unity bug or OS/Hardware issue with some android devices
        // e.g. the first touch on the screen can return a fingerId greater than 0 (4, 5... even 32 has been seen!)
        // NOTE: this bug should be fixed in Unity 4.1+
#if UNITY_ANDROID
        if( fixAndroidTouchIdBug )
        {
            if( Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began )
                touchIdOffset = Input.touches[0].fingerId;
        }
#endif

        for( int i = 0; i < Input.touchCount; ++i )
        {
            int fingerIndex = Input.touches[i].fingerId - touchIdOffset;

            if( fingerIndex < finger2touchMap.Length )
                finger2touchMap[fingerIndex] = i;
        }
    }

    bool HasValidTouch( int fingerIndex )
    {
        return finger2touchMap[fingerIndex] != -1;
    }

    UnityEngine.Touch GetTouch( int fingerIndex )
    {
        int touchIndex = finger2touchMap[fingerIndex];

        if( touchIndex == -1 )
            return nullTouch;

        return Input.touches[touchIndex];
    }

    #endregion
    
    #region FGInputProvider Implementation

    public override int MaxSimultaneousFingers
    {
        get { return maxTouches; }
    }

    public override void GetInputState( int fingerIndex, out bool down, out Vector2 position )
    {
        down = false;
        position = Vector2.zero;

        if( HasValidTouch( fingerIndex ) )
        {
            UnityEngine.Touch touch = GetTouch( fingerIndex );

            if( touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled )
                down = false;
            else
            {
                down = true;
                position = touch.position;
            }
        }
    }

    #endregion
}
