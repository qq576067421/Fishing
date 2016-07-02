using UnityEngine;
using System.Collections;
using GF.Common;

namespace Ps
{
    public class CRenderFishFactory : BaseEntityFactory
    {
        //-------------------------------------------------------------------------
        CRenderScene mScene = null;

        //-------------------------------------------------------------------------
        public CRenderFishFactory(CRenderScene scene)
        {
            mScene = scene;
        }

        //-------------------------------------------------------------------------
        public BaseEntity buildBaseEntity(int vib_id, int fish_id)
        {
            CRenderFish fish = new CRenderFish(mScene);
            fish.create(vib_id, fish_id);
            return fish;
        }

        //-------------------------------------------------------------------------
        public void destroyBaseEntity(BaseEntity entity)
        {
            entity.destroy();
        }

        //-------------------------------------------------------------------------
        public string getFactoryName()
        {
            return "RenderFish";
        }
    }
}
