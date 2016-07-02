using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class InputController
{
    //-------------------------------------------------------------------------
    public delegate void FingerTouchDelegate(Vector2 fire_goal_position);
    public FingerTouchDelegate onFingerTouch;
    public delegate void FingerLongPressDelegate(Vector2 fire_goal_position);
    public FingerLongPressDelegate onFingerLongPress;
    public delegate void FingerUpDelegate();
    public FingerUpDelegate onFingerUp;
    public delegate void FingerDragMoveDelegate(Vector2 fire_goal_position);
    public FingerDragMoveDelegate onFingerDragMove;
    public delegate void FingerTouchTurretDelegate(int turret_id);
    public FingerTouchTurretDelegate onFingerTouchTurret;
    public delegate void FingerTouchFishDelegate(List<FishStillSprite> fishs);
    public FingerTouchFishDelegate onFingerTouchFish;
    public delegate void FingerTouchBufferDelegate(GameObject buffer);
    public FingerTouchBufferDelegate onFingerTouchBuffer;

    static InputController mInputController = null;
    bool mActiveInput = false;
    Camera mGameCamera = null;
    Camera mUICamera = null;
    GameObject RenderFingerGesturesGameObject = null;
    ClientFingerGestures mClientFingerGestures = null;
    public Vector2 CurrentMousePosition { get { return mCurrentMousePosition; } }
    Vector2 mCurrentMousePosition = Vector2.zero;
    public bool MouseDown { get { return mMouseDown; } }
    bool mMouseDown = false;

    //-------------------------------------------------------------------------
    public static InputController Instance
    {
        get
        {
            if (mInputController == null)
            {
                mInputController = new InputController();
            }
            return mInputController;
        }
    }

    //-------------------------------------------------------------------------
    public bool ActiveInput
    {
        get { return mActiveInput; }
        set { mActiveInput = value; }
    }

    //-------------------------------------------------------------------------
    InputController()
    {
        RenderFingerGesturesGameObject = GameObject.Find("Main Object");
        if (RenderFingerGesturesGameObject == null)
        {
            Debug.LogError("InputController init error -- Cannot found Main Object GameObject");
        }
        mClientFingerGestures = RenderFingerGesturesGameObject.GetComponent<ClientFingerGestures>();

        mClientFingerGestures.onFingerLongPress += (Vector2 fingerPos) =>
        {
            if (mActiveInput && onFingerLongPress != null)
            {
                mCurrentMousePosition = pixel2logicPos(fingerPos);
                onFingerLongPress(pixel2logicPos(fingerPos));
            }
        };

        mClientFingerGestures.onFingerUp += (Vector2 fingerPos) =>
        {
            if (mActiveInput && onFingerUp != null)
            {
                mMouseDown = false;
                mCurrentMousePosition = pixel2logicPos(fingerPos);
                onFingerUp();
            }
        };

        mClientFingerGestures.onFingerFingerSwipe += OnFingerSwipe;

        mClientFingerGestures.onFingerDown += onFingerDown;

        mClientFingerGestures.onFingerDragMove += (Vector2 fingerPos) =>
        {
            if (!mActiveInput) return;

            mCurrentMousePosition = pixel2logicPos(fingerPos);

            if (mActiveInput && onFingerDragMove != null)
            {
                onFingerDragMove(pixel2logicPos(fingerPos));
            }
        };

        mGameCamera = GameObject.Find("tk2dCameraObject").GetComponent<Camera>();
        mUICamera = GameObject.Find("UI Camera").GetComponent<Camera>();
    }

    //-------------------------------------------------------------------------
    void onFingerDown(Vector2 fingerPos)
    {
        if (!mActiveInput) return;

        GameObject picked_obj = pickObjectByUICamera(fingerPos);
        if (picked_obj != null && picked_obj.layer == LayerMask.NameToLayer("UI")) { return; }

        mMouseDown = true;
        mCurrentMousePosition = pixel2logicPos(fingerPos);

        picked_obj = pickObjectByGameCamera(fingerPos);
        if (picked_obj != null)
        {
            mCurrentMousePosition = pixel2logicPos(fingerPos);
            switch (picked_obj.tag)
            {
                case "CSpriteTurret0":
                    onFingerTouchTurret(0);
                    break;
                case "CSpriteTurret1":
                    onFingerTouchTurret(1);
                    break;
                case "CSpriteTurret2":
                    onFingerTouchTurret(2);
                    break;
                case "CSpriteTurret3":
                    onFingerTouchTurret(3);
                    break;
                case "CSpriteTurret4":
                    onFingerTouchTurret(4);
                    break;
                case "CSpriteTurret5":
                    onFingerTouchTurret(5);
                    break;
                case "CSpriteTurret6":
                    onFingerTouchTurret(6);
                    break;
                case "CSpriteBuffer0":
                    onFingerTouchBuffer(picked_obj);
                    break;
                case "CSpriteBuffer1":
                    onFingerTouchBuffer(picked_obj);
                    break;
                case "CSpriteBuffer2":
                    onFingerTouchBuffer(picked_obj);
                    break;
                case "CSpriteBuffer3":
                    onFingerTouchBuffer(picked_obj);
                    break;
                case "CSpriteBuffer4":
                    onFingerTouchBuffer(picked_obj);
                    break;
                case "CSpriteBuffer5":
                    onFingerTouchBuffer(picked_obj);
                    break;
                case "CSpriteBuffer6":
                    onFingerTouchBuffer(picked_obj);
                    break;
                default:
                    {
                        if (onFingerTouch != null) onFingerTouch(pixel2logicPos(fingerPos));
                    }
                    break;
            }
        }
        else if (onFingerTouch != null)
        {
            onFingerTouch(pixel2logicPos(fingerPos));
        }
    }

    //-------------------------------------------------------------------------
    GameObject pickObjectByUICamera(Vector2 screen_pos)
    {
        return pickObjectByCamre(mUICamera, screen_pos);
    }

    //-------------------------------------------------------------------------
    GameObject pickObjectByGameCamera(Vector2 screen_pos)
    {
        return pickObjectByCamre(mGameCamera, screen_pos);
    }

    //-------------------------------------------------------------------------
    public List<GameObject> pickAllObjectByCamre(Camera camera, Vector2 screen_pos, string tag)
    {
        if (camera == null) return null;

        Ray ray = camera.ScreenPointToRay(screen_pos);

        RaycastHit[] hits = Physics.RaycastAll(ray, 100000);

        if (hits == null || hits.Length <= 0)
        {
            return null;
        }
        List<GameObject> object_list = new List<GameObject>();
        foreach (var it in hits)
        {
            if (it.collider.gameObject.tag != tag) continue;
            object_list.Add(it.collider.gameObject);
        }
        return object_list;
    }

    public GameObject pickObjectByCamre(Camera camera, Vector2 screen_pos)
    {
        if (camera == null) return null;

        Ray ray = camera.ScreenPointToRay(screen_pos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject;
        }
        else return null;
    }

    //-------------------------------------------------------------------------
    public void OnFingerSwipe(FingerGestures.SwipeDirection direction, Vector2 finger_start_position, Vector2 finger_end_position)
    {
        if (!mActiveInput || onFingerTouchFish == null) return;

        mCurrentMousePosition = pixel2logicPos(finger_end_position);

        List<FishStillSprite> fish_list = new List<FishStillSprite>();
        float gap = 5f;
        float distance = Vector2.Distance(finger_start_position, finger_end_position);
        int index = 0;
        int max_index = (int)(distance / gap) + 1;
        Vector2 unit_move_vector = (finger_end_position - finger_start_position).normalized;
        List<FishStillSprite> fishs = null;
        do
        {
            fishs = getAllFishOnOnePoint(finger_start_position + unit_move_vector * gap * index);
            if (fishs != null)
            {
                foreach (var it in fishs)
                {
                    if (it != null && notInList(it, fish_list))
                    {
                        fish_list.Add(it);
                    }
                }
            }
            index++;
        } while (index < max_index);

        if (fish_list.Count > 0)
        {
            onFingerTouchFish(fish_list);
        }
    }

    //-------------------------------------------------------------------------
    bool notInList(StillSprite fish, List<FishStillSprite> fish_list)
    {
        foreach (var it in fish_list)
        {
            if (it == fish) return false;
        }
        return true;
    }

    //-------------------------------------------------------------------------
    FishStillSprite getFishOnOnePoint(Vector2 position)
    {
        GameObject picked_obj = pickObjectByUICamera(position);
        if (picked_obj != null && picked_obj.layer == LayerMask.NameToLayer("UI")) { return null; }

        picked_obj = pickObjectByGameCamera(position);
        if (picked_obj != null && picked_obj.tag == "CSpriteFish")
        {
            FishStillSprite fish = picked_obj.GetComponent<FishStillSprite>();
            return fish;
        }
        return null;
    }

    //-------------------------------------------------------------------------
    List<FishStillSprite> getAllFishOnOnePoint(Vector2 position)
    {
        GameObject picked_obj = pickObjectByUICamera(position);
        if (picked_obj != null && picked_obj.layer == LayerMask.NameToLayer("UI")) { return null; }

        List<GameObject> obj_list = null;
        obj_list = pickAllObjectByCamre(mGameCamera, position, "CSpriteFish");
        if (obj_list == null) return null;
        List<FishStillSprite> fish_list = new List<FishStillSprite>();
        foreach (var it in obj_list)
        {
            fish_list.Add(it.GetComponent<FishStillSprite>());
        }
        return fish_list;
    }

    //-------------------------------------------------------------------------
    Vector2 pixel2logicPos(Vector2 origin)
    {
        EbVector3 vv = CCoordinate.pixel2logicPos(new EbVector3(origin.x, origin.y, 0));
        return new Vector2(vv.x, vv.y);
    }
}
