using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GF.Common;
using Ps;

public class StillParticle : MonoBehaviour
{
    //-------------------------------------------------------------------------
    public delegate void onTriggerEnterDelegate(Collider other);
    public onTriggerEnterDelegate onTriggerEnter;
    private ParticleSystem mParticleSystem = null;
    private Transform mTransform = null;
    private Transform mTargetTransform = null;
    MassEntity mMassEntity = null;
    bool mCanDestroy = false;
    public bool CanDestroy
    {
        get
        {
            if (mParticleSystem == null)
            {
                return mBaseParticle.IsFinished;
            }
            return mParticleSystem.isStopped || mCanDestroy || mBaseParticle.IsFinished;
        }
    }
    EbVector3 mOffsetLocation = EbVector3.Zero;
    float mDirection = 0;
    BaseParticle mBaseParticle = null;

    //-------------------------------------------------------------------------
    public void init()
    {
        mTransform = transform;
        mParticleSystem = GetComponent<ParticleSystem>();
        mBaseParticle = gameObject.GetComponent<BaseParticle>();

        if (mBaseParticle == null)
        {
            mBaseParticle = gameObject.AddComponent<NullParticle>();
        }
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (mMassEntity == null) return;
        mMassEntity.update(elapsed_tm);
        setPosition(mMassEntity.Position);
        setDirection(mMassEntity.Angle);
        if (mMassEntity.IsOutScreen || mMassEntity.IsEndRoute)
        {
            mCanDestroy = true;
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        if (mBaseParticle != null)
        {
            mBaseParticle.destroy();
            mBaseParticle = null;
        }

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    //-------------------------------------------------------------------------
    public void create(Vector3 init_position, Vector3 target_position, int coins_count, float layer)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();

        parameters.Add("init_position", init_position);
        parameters.Add("target_position", target_position);
        parameters.Add("coins_count", coins_count);
        parameters.Add("layer", layer);

        mBaseParticle.init(parameters);
    }

    //-------------------------------------------------------------------------
    public void setRoute(IRoute route, float speed)
    {
        if (mMassEntity == null)
        {
            mMassEntity = new MassEntity();
        }
        mMassEntity.setRoute(route);
        mMassEntity.setSpeed(speed);
        update(0);
    }

    //-------------------------------------------------------------------------
    public void setOffsetLocation(EbVector3 offset_location)
    {
        mOffsetLocation = offset_location;
    }

    //-------------------------------------------------------------------------
    public void setLooping(bool is_looping)
    {
        if (mParticleSystem == null) return;
        mParticleSystem.loop = is_looping;
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
        pixel_pos += CLogicUtility.getVector2ByRotateAngle(mOffsetLocation, mDirection);

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
        if (angle == 180) angle -= 0.1f;

        mDirection = angle;

        EbVector3 vec = CLogicUtility.getDirection(angle);
        Vector3 direction = new Vector3(vec.x, vec.y, 0);
        mTransform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
    }

    //-------------------------------------------------------------------------
    public void setTrigger(bool isTrigger, float size = 1)
    {
        Rigidbody rigibody = gameObject.GetComponent<Rigidbody>();
        if (rigibody == null)
        {
            rigibody = gameObject.AddComponent<Rigidbody>();
        }

        if (isTrigger)
        {
            rigibody.useGravity = false;
            rigibody.isKinematic = true;
        }
        else
        {
            rigibody.useGravity = false;
            rigibody.isKinematic = false;
        }

        MeshCollider mesh_collider = gameObject.GetComponent<MeshCollider>();
        if (mesh_collider != null)
        {
            mesh_collider.isTrigger = isTrigger;
            mesh_collider.enabled = isTrigger;
        }

        BoxCollider box_collider = gameObject.GetComponent<BoxCollider>();
        if (box_collider != null)
        {
            box_collider.isTrigger = isTrigger;
            box_collider.size = new Vector3(box_collider.size.x, box_collider.size.y, size);
            box_collider.enabled = isTrigger;
        }
    }

    //-------------------------------------------------------------------------
    public void setTag(string tag)
    {
        gameObject.tag = tag;
    }

    //-------------------------------------------------------------------------
    public void setActive(bool active)
    {
    }

    //-------------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (onTriggerEnter == null) return;
        onTriggerEnter(other);
    }
}