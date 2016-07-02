using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderBufferMgr
    {
        //-------------------------------------------------------------------------
        CRenderScene mScene = null;
        CRenderTurret mTurret = null;
        Dictionary<string, CRenderBuffer> mMapBuffer = new Dictionary<string, CRenderBuffer>();
        EbVector3 mFisrtPoint = EbVector3.Zero;
        float mUpAngle = 0;
        float mGap = 65f;
        float mRadius = 10f;
        float mAngleSpeed = 300f;
        float mCurrentAngle = 0;

        //-------------------------------------------------------------------------
        public CRenderBufferMgr(CRenderScene scene, CRenderTurret turret, EbVector3 position, float up_angle)
        {
            mScene = scene;
            mTurret = turret;
            mFisrtPoint = position;
            mUpAngle = up_angle;
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            removeAllBuffer();
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            mCurrentAngle += elapsed_tm * mAngleSpeed;
            EbVector3 right_direction = CLogicUtility.getVector2ByRotateAngle(CLogicUtility.getDirection(mUpAngle), 90).normalized;
            EbVector3 postion = mFisrtPoint + CLogicUtility.getDirection(mCurrentAngle) * mRadius;

            for (int i = 0; i < mMapBuffer.Count; i++)
            {
                var item = mMapBuffer.ElementAt(i);
                item.Value.update(elapsed_tm);
                item.Value.setPosition(postion + right_direction * i * mGap, mUpAngle);
                if (item.Value.canDestroy())
                {
                    item.Value.destroy();
                    mMapBuffer.Remove(item.Key);
                }
            }
        }

        //-------------------------------------------------------------------------
        public void addBuffer(string name, List<object> param = null)
        {
            if (mMapBuffer.ContainsKey(name)) return;

            if (name == "BufLock")
            {
                string path = "Game/Commonent/Effect/suodingPrefab";
                mMapBuffer.Add(name, new CRenderBufferLock(mScene, mTurret, name, param, "buffer_card_lock"));
            }
            else if (name == "BufPower")
            {
                string path = "Game/Commonent/Effect/nengliangpaoPrefab";
                mMapBuffer.Add(name, new CRenderBufferPower(mScene, mTurret, name, param, "buffer_card_power"));
            }
            else if (name == "BufLongpress")
            {
                string path = "";
                mMapBuffer.Add(name, new CRenderBufferLongpress(mScene, mTurret, name, param, path));
            }
            else if (name == "BufRapid")
            {
                string path = "";
                mMapBuffer.Add(name, new CRenderBufferRapid(mScene, mTurret, name, param, path));
            }
            else if (name == "BufFreeze")
            {
                string path = "";
                mMapBuffer.Add(name, new CRenderBufferFreeze(mScene, mTurret, name, param, path));
            }

            _updateBufferPosition();
        }

        //-------------------------------------------------------------------------
        public void removeBuffer(string name)
        {
            if (mMapBuffer.ContainsKey(name))
            {
                CRenderBuffer buf = mMapBuffer[name];
                mMapBuffer.Remove(name);
                buf.destroy();
            }
        }

        //-------------------------------------------------------------------------
        public bool hasBuffer(string name)
        {
            return mMapBuffer.ContainsKey(name);
        }

        //-------------------------------------------------------------------------
        public void removeAllBuffer()
        {
            foreach (var i in mMapBuffer)
            {
                i.Value.destroy();
            }
            mMapBuffer.Clear();
        }

        //-------------------------------------------------------------------------
        public CRenderBuffer getBuffer(string name)
        {
            if (mMapBuffer.ContainsKey(name))
            {
                return mMapBuffer[name];
            }
            else
            {
                return null;
            }
        }

        //-------------------------------------------------------------------------
        public void onFingerTouchBuffer(GameObject buffer_gameobject)
        {
            foreach (var i in mMapBuffer)
            {
                i.Value.onTouch(buffer_gameobject);
            }

            _updateBufferPosition();
        }

        //-------------------------------------------------------------------------
        void _updateBufferPosition()
        {
            EbVector3 right_direction = CLogicUtility.getVector2ByRotateAngle(CLogicUtility.getDirection(mUpAngle), 90).normalized;
            int index = 0;
            foreach (var i in mMapBuffer)
            {
                i.Value.setPosition(mFisrtPoint + right_direction * index * mGap, mUpAngle);
                index++;
            }
        }
    }
}
