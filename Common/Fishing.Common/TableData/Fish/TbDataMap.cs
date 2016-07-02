using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataMap : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        //-------------------------------------------------------------------------
        public struct BackgroundSpriteDataKeyStruct
        {
            public TbDataBackgroundSprite SpriteData;
            public int PositionX;
            public int PositionY;
        }

        public struct ParticleStruct
        {
            public TbDataParticle TbDataParticle;
            public int PositionX;
            public int PositionY;
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public string MapName { get; private set; }
        public List<BackgroundSpriteDataKeyStruct> Sprites { get; private set; }
        public string AudioName { get; private set; }
        public List<ParticleStruct> SeaStarParticle { get; private set; }
        public TbDataParticle LevelRippleParticle { get; private set; }
        public string SwitchLevelAudioName { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            MapName = prop_set.getPropString("T_MapName").get();

            Sprites = new List<BackgroundSpriteDataKeyStruct>();
            for (int i = 1; i <= 20; ++i)
            {
                string strSprites = prop_set.getPropString("T_Sprites" + i.ToString()).get();
                string[] arraySprites = strSprites.Split(';');

                BackgroundSpriteDataKeyStruct backgroundSpriteDataKeyStruct = new BackgroundSpriteDataKeyStruct();
                backgroundSpriteDataKeyStruct.SpriteData = EbDataMgr.Instance.getData<TbDataBackgroundSprite>(int.Parse(arraySprites[0]));
                backgroundSpriteDataKeyStruct.PositionX = int.Parse(arraySprites[1]);
                backgroundSpriteDataKeyStruct.PositionY = int.Parse(arraySprites[2]);
                Sprites.Add(backgroundSpriteDataKeyStruct);
            }

            AudioName = prop_set.getPropString("T_AudioName").get();

            SeaStarParticle = new List<ParticleStruct>();
            for (int i = 1; i <= 10; ++i)
            {
                string strSeaStarParticles = prop_set.getPropString("T_SeaStarParticle" + i.ToString()).get();
                string[] arraySeaStarParticles = strSeaStarParticles.Split(';');
                ParticleStruct particleStruct = new ParticleStruct();
                particleStruct.TbDataParticle = EbDataMgr.Instance.getData<TbDataParticle>(int.Parse(arraySeaStarParticles[0]));
                particleStruct.PositionX = int.Parse(arraySeaStarParticles[1]);
                particleStruct.PositionY = int.Parse(arraySeaStarParticles[2]);
                SeaStarParticle.Add(particleStruct);
            }

            LevelRippleParticle = EbDataMgr.Instance.getData<TbDataParticle>(prop_set.getPropInt("I_LevelRippleParticle").get());
            SwitchLevelAudioName = prop_set.getPropString("T_SwitchLevelAudioName").get();
        }
    }
}
