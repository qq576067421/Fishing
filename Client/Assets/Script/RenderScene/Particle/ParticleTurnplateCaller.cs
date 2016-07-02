using System;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class ParticleTurnplateCaller
{
    //-------------------------------------------------------------------------
    public bool Done { get { return mDone; } }
    bool mDone = false;

    CRenderScene mScene = null;
    uint mPlayerId = 0;
    List<StillParticle> mParticleList = null;
    int mFishVibId = -1;
    int mTotalScore = 0;

    //-------------------------------------------------------------------------
    public ParticleTurnplateCaller(CRenderScene scene, uint et_player_rpcid, 
        int fish_vib_id, int total_score, List<StillParticle> particle_list)
    {
        mScene = scene;
        mPlayerId = et_player_rpcid;
        mFishVibId = fish_vib_id;
        mTotalScore = total_score;
        mParticleList = particle_list;
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (mDone) return;

        bool need_display_turnplate = true;

        foreach (var it in mParticleList)
        {
            if (it == null) continue;
            if (it.CanDestroy) continue;
            need_display_turnplate = false;
        }

        if (need_display_turnplate)
        {
            ss();
            mDone = true;
        }
    }

    void ss()
    {
        CRenderTurret turret = mScene.getTurret(mPlayerId);
        if (turret == null) return;
        TbDataFish fish_data = EbDataMgr.Instance.getData<TbDataFish>(mFishVibId);
        turret.displayScoreTurnplate(mTotalScore, fish_data.TurnplateParticle);
    }
}
