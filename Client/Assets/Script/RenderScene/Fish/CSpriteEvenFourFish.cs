using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;

namespace Ps
{
    public class CSpriteEvenFourFish : CSpriteEvenFish
    {
        //-------------------------------------------------------------------------
        public override void create(CRenderScene scene, TbDataFish vib_fish_data)
        {
            base.create(scene, vib_fish_data);

            if (mVibFishData.Type == TbDataFish.FishType.EvenFour)
            {
                //_initSprite(91f, 91f, 80f);
                _initSprite(128.69f, 80f);
            }
            _setBackgroundSize(0.7f);
        }

        //-------------------------------------------------------------------------
        void _initSprite(float position_offset, float angle_speed)
        {
            mBackground.Add(new SpriteAndOffset(_loadFishStillSprite("bluebig", 0.7f), EbVector3.Zero, -angle_speed, mFishCycleGap * 2));

            _newSpriteAndOffset("bluesmall", "bluemetal", mVibFishData.FishEvenFour.ListFishIdAndScale[0], new EbVector3(position_offset, 0, 0), 0, angle_speed);
            _newSpriteAndOffset("bluesmall", "bluemetal", mVibFishData.FishEvenFour.ListFishIdAndScale[1], new EbVector3(0, -position_offset, 0), 0, angle_speed);
            _newSpriteAndOffset("bluesmall", "bluemetal", mVibFishData.FishEvenFour.ListFishIdAndScale[2], new EbVector3(-position_offset, 0, 0), 0, angle_speed);
            _newSpriteAndOffset("bluesmall", "bluemetal", mVibFishData.FishEvenFour.ListFishIdAndScale[3], new EbVector3(0, position_offset, 0), 0, angle_speed);

            //_newSpriteAndOffset("bluesmall", "bluemetal", mVibFishData.FishEvenFour.Data.NoOneFish, new EbVector3(position_offset_x, position_offset_y, 0), 0, angle_speed);
            //_newSpriteAndOffset("bluesmall", "bluemetal", mVibFishData.FishEvenFour.Data.NoTwoFish, new EbVector3(-position_offset_x, position_offset_y, 0), 0, angle_speed);
            //_newSpriteAndOffset("bluesmall", "bluemetal", mVibFishData.FishEvenFour.Data.NoThreeFish, new EbVector3(position_offset_x, -position_offset_y, 0), 0, angle_speed);
            //_newSpriteAndOffset("bluesmall", "bluemetal", mVibFishData.FishEvenFour.Data.NoFourFish, new EbVector3(-position_offset_x, -position_offset_y, 0), 0, angle_speed);
        }
    }
}
