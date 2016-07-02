using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;
using Ps;

public class FishParticleMgr : IDisposable
{
    //-------------------------------------------------------------------------
    TbDataFish mFishData = null;
    ParticleManager mParticlemanager = null;
    Dictionary<TbDataFish.ParticleProduceTimeEnum, List<TbDataFish.ParticleDataStruct>> mDicParticleData =
        new Dictionary<TbDataFish.ParticleProduceTimeEnum, List<TbDataFish.ParticleDataStruct>>();
    List<StillParticle> mListParticle = new List<StillParticle>();//伴随鱼游动的特效
    CRenderScene mScene = null;
    CRenderFish mCRenderFish = null;
    ISpriteFish mISpriteFish = null;
    System.Random mRandom = new System.Random(unchecked((int)System.DateTime.Now.Ticks));

    //-------------------------------------------------------------------------
    public FishParticleMgr(CRenderScene render_scene, CRenderFish fish, int fish_vib_id, ISpriteFish sprite_fish)
    {
        mScene = render_scene;
        mCRenderFish = fish;
        mParticlemanager = mScene.getParticlemanager();
        mISpriteFish = sprite_fish;

        mFishData = EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id);

        //解析鱼带的特效数据并保存下来,等待鱼指定播放特效
        foreach (var it in mFishData.ParticleArray)
        {
            if (null == it.TbDataParticle || it.TbDataParticle.Id <= 0) continue;

            TbDataFish.ParticleProduceTimeEnum time_enum = (TbDataFish.ParticleProduceTimeEnum)it.ParticleProduceTime;
            if (!mDicParticleData.ContainsKey(time_enum))
            {
                mDicParticleData[time_enum] = new List<TbDataFish.ParticleDataStruct>();
            }
            mDicParticleData[time_enum].Add(it);
        }
    }

    //-------------------------------------------------------------------------
    public void fishBorn()
    {
        _loopBuildParticle(TbDataFish.ParticleProduceTimeEnum.FishBorn);
    }

    //-------------------------------------------------------------------------
    public void fishMoving()
    {
        _loopBuildParticle(TbDataFish.ParticleProduceTimeEnum.FishMoving);

        if (mISpriteFish == null) return;
        foreach (var it in mListParticle)
        {
            it.transform.parent = mISpriteFish.FishGameObject.transform;
            it.transform.localPosition = Vector3.zero;
        }
    }

    //-------------------------------------------------------------------------
    public void fishDie()
    {
        List<StillParticle> particle_list = _loopBuildParticle(TbDataFish.ParticleProduceTimeEnum.FishDie);
        float angle = mCRenderFish.getFishAngle();
        mISpriteFish.setDirection(0);
        foreach (var it in particle_list)
        {
            it.transform.parent = mISpriteFish.FishGameObject.transform;
            it.transform.localPosition = Vector3.zero;
        }
        mISpriteFish.setDirection(angle);
    }

    //-------------------------------------------------------------------------
    public void fishDestroy(uint et_player_rpcid, int fish_vib_id, int total_score)
    {
        List<StillParticle> particle_list = _loopBuildParticle(TbDataFish.ParticleProduceTimeEnum.FishDestroy, et_player_rpcid);

        foreach (var it in particle_list)
        {
            it.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.Particle));
        }

        TbDataFish fish_data = EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id);
        if (fish_data.FishDisplayScoreType == TbDataFish._eDisplayScoreType.Turnplate
            || fish_data.FishDisplayScoreType == TbDataFish._eDisplayScoreType.ChipsAndTurnplate)
        {
            mScene.addParticleTurnplateCaller(new ParticleTurnplateCaller(mScene, et_player_rpcid, fish_vib_id, total_score, particle_list));
        }
    }

    //-------------------------------------------------------------------------
    public void fishCoins(uint et_player_rpcid)
    {
        if(null == mFishData.mCoinParticle.CointParticleData)
        {
            return;
        }
        if (mFishData.mCoinParticle.CointParticleData.Id <= 0) return;

        CRenderTurret turret = mScene.getTurret(et_player_rpcid);
        if (turret == null) return;

        string coin_particle_name = mFishData.mCoinParticle.CointParticleData.ParticlePrefabName;
        int radius = mFishData.mCoinParticle.Radius;
        int coin_count = mFishData.mCoinParticle.CointCount;

        if (radius <= 0) radius = 30;
        if (coin_count <= 0) coin_count = 1;

        float layer = mScene.getLayerAlloter().getLayer(_eLevelLayer.Coin);

        StillParticle still_particle = mParticlemanager.newParticle(coin_particle_name);
        still_particle.setLayer(layer);
        still_particle.create(mCRenderFish.Position.logic2pixel(), turret.getTurretPos().logic2pixel(), coin_count, layer);
    }

    //-------------------------------------------------------------------------
    public void Dispose()
    {
        destroy();
        GC.SuppressFinalize(this);
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        foreach (var it in mListParticle)
        {
            mParticlemanager.freeParticle(it);
        }
        mListParticle.Clear();
    }

    //-------------------------------------------------------------------------
    List<StillParticle> _loopBuildParticle(TbDataFish.ParticleProduceTimeEnum time_enum, uint et_player_rpcid = 0)
    {
        if (!mDicParticleData.ContainsKey(time_enum)) return new List<StillParticle>();

        List<StillParticle> particle_list = new List<StillParticle>();

        foreach (var it in mDicParticleData[time_enum])
        {
            particle_list.Add(_buildParticle(it, et_player_rpcid));
        }

        return particle_list;
    }

    //-------------------------------------------------------------------------
    StillParticle _buildParticle(TbDataFish.ParticleDataStruct particle_data, uint et_player_rpcid)
    {
        StillParticle still_particle = mParticlemanager.newParticle(particle_data.TbDataParticle.ParticlePrefabName);

        if (particle_data.TargetPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.None ||
            particle_data.TargetPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.Default)
        {
            _particleNoneTarget(still_particle, particle_data, et_player_rpcid);
        }
        else if (particle_data.TargetPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.Turret)
        {
            _particleTurretTarget(still_particle, particle_data, et_player_rpcid);
        }
        else if (particle_data.TargetPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.Fixed)
        {
            _particleTurretTarget(still_particle, particle_data, et_player_rpcid);
        }

        still_particle.setLayer(mScene.getLayerAlloter().getFishLayer(mFishData.Id));

        return still_particle;
    }

    //-------------------------------------------------------------------------
    void _setOffsetLocation(StillParticle still_particle, float x, float y)
    {
        still_particle.setOffsetLocation(new EbVector3(x, y, 0));
    }

    //-------------------------------------------------------------------------
    void _particleNoneTarget(StillParticle still_particle, TbDataFish.ParticleDataStruct particle_data, uint et_player_rpcid)
    {
        if (particle_data.StartPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.Fish)
        {
            if (particle_data.ParticleProduceTime == TbDataFish.ParticleProduceTimeEnum.FishMoving)
            {
                mListParticle.Add(still_particle);//跟随鱼运动
            }
            else
            {
                still_particle.setPosition(mCRenderFish.Position);
                still_particle.setLooping(false);
            }
        }
        else if (particle_data.StartPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.Fixed)
        {
            still_particle.setPosition(new EbVector3(particle_data.StartPoint.x, particle_data.StartPoint.y, 0));
        }

        _setOffsetLocation(still_particle, particle_data.StartPoint.x, particle_data.StartPoint.y);
    }

    //-------------------------------------------------------------------------
    void _particleTurretTarget(StillParticle still_particle, TbDataFish.ParticleDataStruct particle_data, uint et_player_rpcid)
    {
        CRenderTurret turret = mScene.getTurret(et_player_rpcid);
        if (turret == null) return;
        EbVector3 start_position = EbVector3.Zero;

        if (particle_data.StartPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.Fish)
        {
            start_position = mCRenderFish.Position;
        }
        else if (particle_data.StartPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.Fixed)
        {
            start_position = new EbVector3(particle_data.StartPoint.x, particle_data.StartPoint.y, 0);
        }

        still_particle.setRoute(RouteHelper.buildLineRoute(
            start_position + new EbVector3(particle_data.StartPoint.x, particle_data.StartPoint.y, 0),
            turret.getTurretPos() + new EbVector3(particle_data.TargetPoint.x, particle_data.TargetPoint.y, 0)), 200);
    }

    //-------------------------------------------------------------------------
    void _particleFixedTarget(StillParticle still_particle, TbDataFish.ParticleDataStruct particle_data, uint et_player_rpcid)
    {
        CRenderTurret turret = mScene.getTurret(et_player_rpcid);
        EbVector3 start_position = EbVector3.Zero;

        if (particle_data.StartPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.Fish)
        {
            start_position = mCRenderFish.Position;
        }
        else if (particle_data.StartPoint.ParticlePointType == TbDataFish.ParticlePointStruct.ParticlePointTypeEnum.Fixed)
        {
            start_position = new EbVector3(particle_data.StartPoint.x, particle_data.StartPoint.y, 0);
        }

        still_particle.setRoute(RouteHelper.buildLineRoute(
            start_position, new EbVector3(particle_data.TargetPoint.x, particle_data.TargetPoint.y, 0)), 200);
    }
}