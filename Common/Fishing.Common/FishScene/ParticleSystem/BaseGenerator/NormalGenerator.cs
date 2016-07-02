using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class NormalGenerator : EntityGenerator<NormalGeneratorData>
    {
        //---------------------------------------------------------------------
        public NormalGenerator(NormalGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
            : base(generator_data, server_param, route_object_mgr)
        { }

        //---------------------------------------------------------------------
        public override void create()
        {
            if (Done) return;

            int fish_vib_id = int.Parse(getServerParam()[0]);
            int route_vib_id = int.Parse(getServerParam()[1]);

            TbDataFish fish_data = EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id);
            if (null != fish_data)
            {

                float delay_time = 0;
                if (null != fish_data.ChainFish && fish_data.Type == TbDataFish.FishType.ChainFish)
                {
                    delay_time = (float)(fish_data.ChainFish.FishHeight) / fish_data.ChainFish.getSpeed();

                    for (int i = 0; i < fish_data.ChainFishNumber; i++)
                    {
                        newBaseEntity(fish_data.ChainFish.Id, route_vib_id, delay_time * i);
                    }
                }
                else
                {
                    newBaseEntity(fish_vib_id, route_vib_id, 0);
                }
            }
            setDone();
        }

        //---------------------------------------------------------------------
        BaseEntity newBaseEntity(int fish_vib_id, int route_vib_id, float delay_time)
        {
            BaseEntity entity = buildEntity(fish_vib_id);
            DynamicSystem system = getDynamicSystem(route_vib_id, delay_time);

            if (system == null)
            {
                entity.addRoute(RouteHelper.buildLineRoute(route_vib_id, delay_time));
            }
            else
            {
                entity.addDynamicSystem(system);
            }

            return entity;
        }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
            base.destroy();
        }
    }

    public class NormalGeneratorFactory : EntityGeneratorFactory
    {
        //---------------------------------------------------------------------
        public override string getGeneratorType()
        {
            return typeof(NormalGeneratorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityGenerator buildGenerator(EntityGeneratorData generator_data,
            List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            return new NormalGenerator((NormalGeneratorData)generator_data, server_param, route_object_mgr);
        }

        //---------------------------------------------------------------------
        public override EntityGeneratorData buildGeneratorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<NormalGeneratorData>(json_item.mJsonString);
        }
    }

    public class NormalGeneratorData : EntityGeneratorData
    {
        //---------------------------------------------------------------------
        public override EntityGeneratorData clone()
        {
            NormalGeneratorData generator_data = new NormalGeneratorData();
            return generator_data;
        }

        //---------------------------------------------------------------------
        public override int getBaseEntityCount()
        {
            return 1;
        }
    }
}
