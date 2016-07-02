using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    class LogicFishFactory : BaseEntityFactory
    {
        //---------------------------------------------------------------------
        CLogicScene mScene = null;

        //---------------------------------------------------------------------
        public LogicFishFactory(CLogicScene scene)
        {
            mScene = scene;
        }

        //---------------------------------------------------------------------
        public string getFactoryName()
        {
            return "logic_fish";
        }

        //---------------------------------------------------------------------
        public BaseEntity buildBaseEntity(int vib_id, int fish_id)
        {
            CLogicFish fish = new CLogicFish(mScene);
            fish.create(vib_id, fish_id);
            return fish;
        }

        //---------------------------------------------------------------------a
        public void destroyBaseEntity(BaseEntity entity)
        {
            entity.destroy();
        }
    }
}
