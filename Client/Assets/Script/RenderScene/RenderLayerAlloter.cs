using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;

namespace Ps
{
    public class RenderLayerAlloter
    {
        //-------------------------------------------------------------------------
        public float EachFishGap { get { return mEachTypeFishLayerGap; } }
        int mFishKind = 0;
        float mLayerOffset = 1000f;

        float mOverUILayerEnd = -2000f;//ui上面的图层不会超过的值
        float mOverUILayerBegin = -3000;//ui的图层开始的值

        float mFishLayerEnd = 80f;//鱼的图层不会超过的值
        float mFishLayerBegin = 30f;//鱼的图层开始的值
        float mFishUpLayerEnd = -200f;//鱼上面的图层不会超过的值
        float mFishUpLayerBegin = -800f;//鱼上面的图层开始的值
        float mFishDownLayerEnd = 100f;//鱼下面的图层不会超过的值
        float mFishDownLayerBegin = 80f;//鱼下面的图层开始的值
        float mEachTypeFishLayerGap = 1f;//每种鱼图层分配的区间大小的值
        Dictionary<int, LayerAlloter> mDicFishLayer = new Dictionary<int, LayerAlloter>();
        Dictionary<_eLevelLayer, LayerAlloter> mDicLayer = new Dictionary<_eLevelLayer, LayerAlloter>();
        Dictionary<int, LayerPair> mFishIdLayerMap = new Dictionary<int, LayerPair>();

        //-------------------------------------------------------------------------
        public RenderLayerAlloter()
        {
            _initFishLayer();
            _initNormalLayer();
        }

        //-------------------------------------------------------------------------
        public float getFishLayer(int fish_vib_id)
        {
            if (!mDicFishLayer.ContainsKey(fish_vib_id))
            {
                mDicFishLayer.Add(fish_vib_id, new LayerAlloter(_getFishInitLayer(fish_vib_id), mEachTypeFishLayerGap));
            }
            return mDicFishLayer[fish_vib_id].getLayer();
        }

        //-------------------------------------------------------------------------
        public float getLayer(_eLevelLayer layer)
        {
            return mDicLayer[layer].getLayer();
        }

        //-------------------------------------------------------------------------
        float _getFishInitLayer(int fish_vib_id)
        {
            try
            {
                return layerAddOffset(mFishLayerBegin) + mFishIdLayerMap[fish_vib_id].Layer * mEachTypeFishLayerGap;//这个函数以后可以修改，以后可能根据vib配置的数值来初始化。
            }
            catch (Exception e)
            {
                EbLog.Error("getFishInitLayer error with fish_vib_id " + fish_vib_id);
                throw (e);
            }
        }

        //-------------------------------------------------------------------------
        void _initFishLayer()
        {
            mFishKind = EbDataMgr.Instance.getMapData<TbDataFish>().Count;
            if (mFishKind == 0)
            {
                EbLog.Error("RenderLayerAlloter init error.");
            }
            mEachTypeFishLayerGap = (layerAddOffset(mFishLayerEnd) - layerAddOffset(mFishLayerBegin)) / mFishKind;

            List<LayerPair> layer_pair_list = new List<LayerPair>();
            foreach (var it in EbDataMgr.Instance.getMapData<TbDataFish>())
            {
                LayerPair layer_pair = new LayerPair(((TbDataFish)it.Value).FishDepth);
                layer_pair_list.Add(layer_pair);
                mFishIdLayerMap.Add(it.Value.Id, layer_pair);
            }

            layer_pair_list.Sort();
            float layer_begin = 0;
            foreach (var it in layer_pair_list)
            {

                it.setLayer(layer_begin++);
            }
        }

        //-------------------------------------------------------------------------
        float layerAddOffset(float layer)
        {
            return mLayerOffset + layer;
        }

        //-------------------------------------------------------------------------
        void _initNormalLayer()
        {
            int ui_layer_count = 0;
            int up_layer_count = 0;
            int down_layer_count = 0;

            foreach (_eLevelLayer type in Enum.GetValues(typeof(_eLevelLayer)))
            {
                if ((int)type <= (int)_eLevelLayer.UI)
                {
                    ui_layer_count++;
                }
                else if ((int)type < (int)_eLevelLayer.Bullet)
                {
                    up_layer_count++;
                }
                else if ((int)type > (int)_eLevelLayer.Fish)
                {
                    down_layer_count++;
                }
            }


            float ui_layer_gap = (layerAddOffset(mOverUILayerEnd) - layerAddOffset(mOverUILayerBegin)) / ui_layer_count;
            float up_layer_gap = (layerAddOffset(mFishUpLayerEnd) - layerAddOffset(mFishUpLayerBegin)) / up_layer_count;
            float down_layer_gap = (layerAddOffset(mFishDownLayerEnd) - layerAddOffset(mFishDownLayerBegin)) / down_layer_count;

            foreach (_eLevelLayer type in Enum.GetValues(typeof(_eLevelLayer)))
            {
                if ((int)type <= (int)_eLevelLayer.UI)
                {
                    mDicLayer.Add(type, new LayerAlloter(layerAddOffset(mOverUILayerBegin) + ui_layer_gap * (float)type, ui_layer_gap));
                }
                else if ((int)type < (int)_eLevelLayer.Bullet)
                {
                    mDicLayer.Add(type, new LayerAlloter(layerAddOffset(mFishUpLayerBegin) + up_layer_gap * (float)type, up_layer_gap));
                }
                else if ((int)type > (int)_eLevelLayer.Fish)
                {
                    mDicLayer.Add(type, new LayerAlloter(layerAddOffset(mFishDownLayerBegin) + down_layer_gap * (float)type, down_layer_gap));
                }
            }

            mDicLayer.Add(_eLevelLayer.Bullet, new LayerAlloter(layerAddOffset(mFishLayerBegin) - 5, 5));
        }

        //-------------------------------------------------------------------------
        class LayerAlloter
        {
            float mLayerEnd;
            float mLayerInit;
            float mLayerCurrent;
            float mLayerGap = 0.001f;

            //-------------------------------------------------------------------------
            public LayerAlloter(float init_layer, float layer_length)
            {
                mLayerInit = init_layer;
                mLayerEnd = init_layer + layer_length;
                mLayerCurrent = mLayerEnd - mLayerGap;
            }

            //-------------------------------------------------------------------------
            public float getLayer()
            {
                mLayerCurrent -= mLayerGap;
                if (mLayerCurrent < mLayerInit)
                {
                    mLayerCurrent = mLayerEnd - mLayerGap;
                }
                return mLayerCurrent;
            }
        }

        //-------------------------------------------------------------------------
        class LayerPair : IComparable
        {
            //-------------------------------------------------------------------------
            float mLayer = -1;
            public float Layer { get { return mLayer; } }
            int mVibLayer = 0;
            public int VibLayer { get { return mVibLayer; } }

            //-------------------------------------------------------------------------
            public LayerPair(int vib_layer)
            {
                mVibLayer = vib_layer;
            }

            //-------------------------------------------------------------------------
            public void setLayer(float layer)
            {
                mLayer = layer;
            }

            //-------------------------------------------------------------------------
            public int CompareTo(object obj)
            {
                LayerPair layer_pair = obj as LayerPair;
                return mVibLayer.CompareTo(layer_pair.mVibLayer);
            }
        }
    }

    //-------------------------------------------------------------------------
    //不要给枚举类型赋值，RenderLayerAlloter分配的时候依赖默认从0开始的默认值。
    public enum _eLevelLayer
    {
        BufferLockFish = 0,
        Buffer,

        UI,

        TurretRate,
        TurretTopCover,
        TurretBarrel,
        TurretBase,
        TurretBlaze,

        TurretScore,
        TurretScoreBg,
        TurretChips,

        Ripple,
        PreBackground,
        FishScore,
        Particle,
        Coin,

        LinkLockedFish,

        Wave,
        FishNet,
        FishHitParticle,
        FishSwim,
        Bullet,
        Fish,//这个值留给鱼图层的，总体图层分为鱼的上面的，鱼的和鱼的下面的层。
        StarFish,
        ShockBackground,
        Background
    }
}
