using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GF.Common;
using Ps;

public class StillSprite : MonoBehaviour
{
    //-------------------------------------------------------------------------
    private tk2dSprite mSprite = null;
    int mNeedPlayAnimation = 0;
    string mAniName = "";
    private tk2dSpriteAnimator mSpriteAnimator = null;
    private Transform mTransform = null;
    public delegate void onTriggerEnterDelegate(Collider other);
    public onTriggerEnterDelegate onTriggerEnter;
    bool mAnimationIsLoop = true;
    MeshCollider mMeshCollider = null;
    BoxCollider mBoxCollider = null;
    Rigidbody mRigidbody = null;
    CRenderScene mScene = null;
    bool mIsRunForeground = true;

    //-------------------------------------------------------------------------
    public void init(CRenderScene scene)
    {
        mScene = scene;
        mSprite = gameObject.GetComponent<tk2dSprite>();
        mSpriteAnimator = gameObject.GetComponent<tk2dSpriteAnimator>();
        mTransform = transform;

        if (mSpriteAnimator == null) return;
        setKinematic();
    }

    //-------------------------------------------------------------------------
    void Update()
    {
        if (mScene == null) return;

        if (mIsRunForeground != mScene.IsRunForeground)
        {
            mIsRunForeground = mScene.IsRunForeground;
            setRenderer(mIsRunForeground);
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    //-------------------------------------------------------------------------
    public void setRenderer(bool isRender)
    {
        if (GetComponent<Renderer>() == null) return;
        GetComponent<Renderer>().enabled = false;
    }

    //-------------------------------------------------------------------------
    public void setScale(float scale)
    {
        setScale(scale, scale);
    }

    //-------------------------------------------------------------------------
    public void setScale(float x_scale, float y_scale)
    {
        if (x_scale <= 0 || y_scale <= 0) return;

        Vector3 s;
        s.x = x_scale;
        s.y = y_scale;
        s.z = mTransform.localScale.z;
        mTransform.localScale = s;
    }

    //-------------------------------------------------------------------------
    public void setLayer(float layer)
    {
        Vector3 cur_pos = mTransform.position;
        cur_pos.z = layer;
        mTransform.position = cur_pos;
    }

    //-------------------------------------------------------------------------
    public void setPosition(EbVector3 pos)
    {
        EbVector3 pixel_pos = CCoordinate.logic2toolkitPos(pos);
        Vector3 cur_pos;
        cur_pos.x = pixel_pos.x;
        cur_pos.y = pixel_pos.y;
        cur_pos.z = mTransform.position.z;
        mTransform.position = cur_pos;
    }

    //-------------------------------------------------------------------------
    public Vector3 getPosition()
    {
        return mTransform.position;
    }

    //-------------------------------------------------------------------------
    public void setDirection(float angle)
    {
        EbVector3 vec = CLogicUtility.getDirection(angle);
        Vector3 direction = new Vector3(vec.x, vec.y, 0);
        mTransform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
    }

    //-------------------------------------------------------------------------
    public void playAnimation(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        mSpriteAnimator.Stop();

        tk2dSpriteAnimationClip clip = mSpriteAnimator.GetClipByName(name);
        if (clip == null)
        {
            Debug.LogError("sprite animation clip not exist! clip_name=" + name);
        }

        mSpriteAnimator.Play(name);
    }

    //-------------------------------------------------------------------------
    public void stopAnimation()
    {
        if (mSpriteAnimator != null)
        {
            mSpriteAnimator.Stop();
        }
    }

    //-------------------------------------------------------------------------
    public float getAnimationTime()
    {
        return mSpriteAnimator.ClipTimeSeconds;
    }

    //-------------------------------------------------------------------------
    public void setTrigger(bool isTrigger, float size = 1)
    {
        mMeshCollider = gameObject.GetComponent<MeshCollider>();
        if (mMeshCollider != null)
        {
            mMeshCollider.isTrigger = isTrigger;
            mMeshCollider.enabled = isTrigger;
        }

        mBoxCollider = gameObject.GetComponent<BoxCollider>();
        if (mBoxCollider != null)
        {
            mBoxCollider.isTrigger = isTrigger;
            mBoxCollider.size = new Vector3(mBoxCollider.size.x, mBoxCollider.size.y, size);
            mBoxCollider.enabled = isTrigger;
        }
    }

    //-------------------------------------------------------------------------
    public Vector3 getBoxColliderSize()
    {
        if (mBoxCollider != null)
        {
            return mBoxCollider.size;
        }
        return Vector3.zero;
    }

    //-------------------------------------------------------------------------
    public void removeTrigger()
    {
        if (mMeshCollider != null) GameObject.Destroy(mMeshCollider);
        if (mBoxCollider != null) GameObject.Destroy(mBoxCollider);
        if (mRigidbody != null) GameObject.Destroy(mRigidbody);
    }

    //-------------------------------------------------------------------------
    public void setColor(Color c)
    {
        if (mSprite == null) return;
        mSprite.color = c;
    }

    //-------------------------------------------------------------------------
    public void setAlpha(float alpha)
    {
        Color c = mSprite.color;
        c.a = alpha;
        mSprite.color = c;
    }

    //-------------------------------------------------------------------------
    public void setTag(string tag)
    {
        gameObject.tag = tag;
    }

    //-------------------------------------------------------------------------
    public void setActive(bool active)
    {
        gameObject.SetActive(active);
    }

    //-------------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (onTriggerEnter == null) return;
        onTriggerEnter(other);
    }

    //-------------------------------------------------------------------------
    void setKinematic()
    {
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        if (mRigidbody == null)
        {
            mRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        mRigidbody.useGravity = false;
        mRigidbody.isKinematic = true;
    }
}
