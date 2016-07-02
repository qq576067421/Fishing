using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;

namespace Ps
{
    public class CSpriteEvenFiveFish : CSpriteEvenFish
    {
        //-------------------------------------------------------------------------
        public override void create(CRenderScene scene, TbDataFish vib_fish_data)
        {
            base.create(scene, vib_fish_data);
            if (mVibFishData.Type == TbDataFish.FishType.EvenFive)
            {
                //_initSprite(154f, 80f);
                _initSprite(108.89f, 108.89f, 80f);
            }
            _setBackgroundSize(0.7f);
        }

        //-------------------------------------------------------------------------
        void _initSprite(float position_offset_x, float position_offset_y, float angle_speed)
        {
            _newSpriteAndOffset("goldbig", "goldmetal", mVibFishData.FishEvenFive.ListFishIdAndScale[0], EbVector3.Zero, mFishCycleGap * 2, angle_speed);
            _newSpriteAndOffset("goldsmall", "goldmetal", mVibFishData.FishEvenFive.ListFishIdAndScale[1], new EbVector3(position_offset_x, position_offset_y, 0), 0, angle_speed);
            _newSpriteAndOffset("goldsmall", "goldmetal", mVibFishData.FishEvenFive.ListFishIdAndScale[2], new EbVector3(-position_offset_x, position_offset_y, 0), 0, angle_speed);
            _newSpriteAndOffset("goldsmall", "goldmetal", mVibFishData.FishEvenFive.ListFishIdAndScale[3], new EbVector3(position_offset_x, -position_offset_y, 0), 0, angle_speed);
            _newSpriteAndOffset("goldsmall", "goldmetal", mVibFishData.FishEvenFive.ListFishIdAndScale[4], new EbVector3(-position_offset_x, -position_offset_y, 0), 0, angle_speed);

            //_newSpriteAndOffset("goldbig", "goldmetal", mVibFishData.FishEvenFive.Data.NoOneFish, EbVector3.Zero, mFishCycleGap * 2, angle_speed);
            //_newSpriteAndOffset("goldsmall", "goldmetal", mVibFishData.FishEvenFive.Data.NoTwoFish, new EbVector3(position_offset, 0, 0), 0, angle_speed);
            //_newSpriteAndOffset("goldsmall", "goldmetal", mVibFishData.FishEvenFive.Data.NoThreeFish, new EbVector3(0, -position_offset, 0), 0, angle_speed);
            //_newSpriteAndOffset("goldsmall", "goldmetal", mVibFishData.FishEvenFive.Data.NoFourFish, new EbVector3(-position_offset, 0, 0), 0, angle_speed);
            //_newSpriteAndOffset("goldsmall", "goldmetal", mVibFishData.FishEvenFive.Data.NoFiveFish, new EbVector3(0, position_offset, 0), 0, angle_speed);
        }
    }
}
