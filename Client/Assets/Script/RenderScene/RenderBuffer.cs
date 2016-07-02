using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderBuffer
    {
        //-------------------------------------------------------------------------
        protected CRenderScene mScene = null;
        protected CRenderTurret mTurret = null;
        protected StillSprite mSprite = null;
        protected bool mCanClick = false;
        protected string mName = "";
        protected List<object> mParam = null;
        public string Name { get { return mName; } }
        public List<object> Param { get { return mParam; } }
        protected string mAnimationName = "";
        private bool mbCanDestroy = false;

        //-------------------------------------------------------------------------
        public CRenderBuffer(CRenderScene scene, CRenderTurret turret, string name, List<object> param, string animation_name)
        {
            mScene = scene;
            mTurret = turret;
            mName = name;
            mParam = param;
            mAnimationName = animation_name;

            _initCSprite(mName, "CSpriteBuffer" + mTurret.getTurretId().ToString());
        }

        //-------------------------------------------------------------------------
        public virtual void destroy()
        {
            if (mSprite != null)
            {
                mScene.getRenderObjectPool().freeStillSprite(mSprite);
                mSprite = null;
            }
        }

        //-------------------------------------------------------------------------
        public virtual void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public virtual string getName()
        {
            return "";
        }

        //-------------------------------------------------------------------------
        public virtual void onTouch(GameObject buffer)
        {
        }

        //-------------------------------------------------------------------------
        public virtual void setPosition(EbVector3 position, float angle)
        {
            if (mSprite != null)
            {
                mSprite.setPosition(position);
                mSprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.Buffer));
                mSprite.setDirection(angle);
            }
        }

        //-------------------------------------------------------------------------
        public bool canDestroy()
        {
            return mbCanDestroy;
        }

        //-------------------------------------------------------------------------
        public void signDestroy()
        {
            mbCanDestroy = true;
        }

        //-------------------------------------------------------------------------
        void _initCSprite(string name, string tag)
        {
            if (mAnimationName == "") return;

            mSprite = mScene.getRenderObjectPool().newStillSprite();
            mSprite.setTag(tag);
            mSprite.playAnimation(mAnimationName);
            mSprite.setScale(1f);
            mSprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.Buffer));
            //if (name == "BufLock")
            //{
            //    mSprite.setScale(0.47f);
            //}
            //else
            //{
            //    mSprite.setScale(0.38f * 2.7f);
            //}

#if UNITY_EDITOR
            mSprite.gameObject.name = "TkSpriteBuffer_" + name;
#endif
        }
    }
}
