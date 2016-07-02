using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class RedFishGenerator : EntityGenerator<RedFishGeneratorData>
    {
        //---------------------------------------------------------------------
        float mOutFishDelay = 0.1f;

        //---------------------------------------------------------------------
        public RedFishGenerator(RedFishGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
            : base(generator_data, server_param, route_object_mgr)
        {
        }

        //---------------------------------------------------------------------
        public override void create()
        {

        }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            mOutFishDelay -= elapsed_tm;
            if (mOutFishDelay >= 0) return;
            if (Done) return;

            var list_serverparam = getServerParam();
            int index = -1;
            int normal_fish_vib_id = int.Parse(list_serverparam[++index]);
            int red_fish_vib_id = int.Parse(list_serverparam[++index]);
            int red_fish_index = int.Parse(list_serverparam[++index]);
            int fish_count = int.Parse(list_serverparam[++index]);

            float position_x = float.Parse(list_serverparam[++index]);
            float position_y = float.Parse(list_serverparam[++index]);

            int red_fish_obj_id = int.Parse(list_serverparam[++index]);

            if (index + 2 == (int)getServerParam().Count)
            {
                position_x = float.Parse(list_serverparam[++index]);
                position_y = float.Parse(list_serverparam[++index]);
            }

            EbVector3 position = new EbVector3(position_x, position_y, 0);
            float angle = 360.0f / fish_count;

            for (int i = 0; i < fish_count; i++)
            {
                float direction = angle * i;
                int current_fish_vib_id = normal_fish_vib_id;
                if (i == red_fish_index)
                {
                    current_fish_vib_id = red_fish_vib_id;
                }

                BaseEntity entity = buildEntity(current_fish_vib_id);
                entity.addRoute(RouteHelper.buildLineRoute(position, CLogicUtility.getDirection(direction), 2000));
            }

            setDone();
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
            base.destroy();
        }
    }

    public class RedFishGeneratorFactory : EntityGeneratorFactory
    {
        //---------------------------------------------------------------------
        public override string getGeneratorType()
        {
            return typeof(RedFishGeneratorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityGenerator buildGenerator(EntityGeneratorData generator_data,
            List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            return new RedFishGenerator((RedFishGeneratorData)generator_data, server_param, route_object_mgr);
        }

        //---------------------------------------------------------------------
        public override EntityGeneratorData buildGeneratorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<RedFishGeneratorData>(json_item.mJsonString);
        }
    }

    public class RedFishGeneratorData : EntityGeneratorData
    {
        //---------------------------------------------------------------------
        public override EntityGeneratorData clone()
        {
            RedFishGeneratorData generator_data = new RedFishGeneratorData();
            return generator_data;
        }

        //---------------------------------------------------------------------
        public override int getBaseEntityCount()
        {
            return 1;
        }
    }
}
