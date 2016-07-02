using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataBullet : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public int Level { get; private set; }
        public float Speed { get; private set; }
        public string Bullet0Animation { get; private set; }
        public string Bullet1Animation { get; private set; }
        public string Bullet2Animation { get; private set; }
        public string Bullet3Animation { get; private set; }
        public string Bullet4Animation { get; private set; }
        public string Bullet5Animation { get; private set; }
        public int BulletPixelHeight { get; private set; }
        public int BulletHeight { get; private set; }
        public TbDataEffectCompose EffectCompose { get; private set; }
        public string ColorBulletAnimation { get; private set; }
        public string Bullet6Animation { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            Level = prop_set.getPropInt("I_Level").get();
            Speed = prop_set.getPropFloat("R_Speed").get();
            Bullet0Animation = prop_set.getPropString("T_Bullet0Animation").get();
            Bullet1Animation = prop_set.getPropString("T_Bullet1Animation").get();
            Bullet2Animation = prop_set.getPropString("T_Bullet2Animation").get();
            Bullet3Animation = prop_set.getPropString("T_Bullet3Animation").get();
            Bullet4Animation = prop_set.getPropString("T_Bullet4Animation").get();
            Bullet5Animation = prop_set.getPropString("T_Bullet5Animation").get();
            BulletPixelHeight = prop_set.getPropInt("I_BulletPixelHeight").get();
            BulletHeight = prop_set.getPropInt("I_BulletHeight").get();
            EffectCompose = EbDataMgr.Instance.getData<TbDataEffectCompose>(prop_set.getPropInt("I_EffectCompose").get());
            ColorBulletAnimation = prop_set.getPropString("T_ColorBulletAnimation").get();
            Bullet6Animation = prop_set.getPropString("T_Bullet6Animation").get();
        }
    }
}