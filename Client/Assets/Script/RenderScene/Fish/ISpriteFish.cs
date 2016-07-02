using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public interface ISpriteFish
    {
        //-------------------------------------------------------------------------
        GameObject FishGameObject { get; }

        //-------------------------------------------------------------------------
        void create(CRenderScene scene, TbDataFish vib_fish_data);

        //-------------------------------------------------------------------------
        void update(float elapsed_tm);

        //-------------------------------------------------------------------------
        void destroy();

        //-------------------------------------------------------------------------
        void setScale(float scale);

        //-------------------------------------------------------------------------
        void setLayer(float layer);

        //-------------------------------------------------------------------------
        void setPosition(EbVector3 position, float angle);

        //-------------------------------------------------------------------------
        void setDirection(float angle);

        //-------------------------------------------------------------------------
        void playAnimation(string name);

        //-------------------------------------------------------------------------
        void playDieAnimation();

        //-------------------------------------------------------------------------
        void stopAnimation();

        //-------------------------------------------------------------------------
        void setTrigger(bool isTrigger, float size = 1);

        //-------------------------------------------------------------------------
        void setGameobjectName(string name);

        //-------------------------------------------------------------------------
        void setColor(Color color);

        //-------------------------------------------------------------------------
        //void resetColor(float scale);

        //-------------------------------------------------------------------------
        //void setAlpha(float alpha);

        //-------------------------------------------------------------------------
        void setTag(string tag);

        //-------------------------------------------------------------------------
        void setRenderFish(CRenderFish render_fish);

        //-------------------------------------------------------------------------
        void playRotationAnimation();

        //-------------------------------------------------------------------------
        CRenderFish getRenderFish();

        //-------------------------------------------------------------------------
        bool hasFishStillSprite(FishStillSprite fish_still_sprite);
    }

    public class SpriteFishFactory
    {
        //-------------------------------------------------------------------------
        CRenderScene mScene = null;

        //-------------------------------------------------------------------------
        public SpriteFishFactory(CRenderScene scene)
        {
            mScene = scene;
        }

        //-------------------------------------------------------------------------
        //根据vib数据创建鱼的代码全部放在这里，ISpriteFish的实现类不在需要知道TbDataFish的任何事情？//
        public ISpriteFish buildSpriteFish(CRenderFish render_fish, int fish_vib_id)
        {
            TbDataFish fish_data = EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id);
            ISpriteFish sprite_fish = null;

            if (fish_data.Type == TbDataFish.FishType.Custom)
            {
                sprite_fish = new CSpriteCustomGroup();
            }
            else if (fish_data.Type == TbDataFish.FishType.EvenFour)
            {
                sprite_fish = new CSpriteEvenFourFish();
            }
            else if (fish_data.Type == TbDataFish.FishType.EvenFive)
            {
                sprite_fish = new CSpriteEvenFiveFish();
            }
            else
            {
                sprite_fish = new CSpriteFishGroup((int)fish_data.Type);
            }

            sprite_fish.create(mScene, fish_data);
            sprite_fish.setRenderFish(render_fish);
            sprite_fish.setTag("CSpriteFish");
            sprite_fish.setTrigger(true);
            sprite_fish.setLayer(mScene.getLayerAlloter().getFishLayer(fish_vib_id));

#if UNITY_EDITOR
            //ViDebuger.Warning("TkFish_vib_id_ " + fish_vib_id);
            sprite_fish.setGameobjectName("TkFish_vib_id_" + fish_vib_id);
#endif

            return sprite_fish;
        }
    }
}
