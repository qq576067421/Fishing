using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class LogicParticleSystemOuter
    {
        //---------------------------------------------------------------------
        enum OutFishState
        {
            None,
            Normal,
            WaitingFormation,
            Formation
        }

        //---------------------------------------------------------------------
        CLogicScene mScene = null;// 场景指针
        System.Random mRandom = new System.Random(unchecked((int)System.DateTime.Now.Ticks));
        const int miMinFishCrowdCount = 35;// 同一关卡中最小鱼群数量
        const int miMaxFishCrowdCount = 55;// 同一关卡中最大鱼群数量
        float mfCheckAndCreateFishCrowdCountSecond = 1.0f;// 每隔一秒检查一次鱼群数量
        List<int> mListRouteIdSaver = new List<int>();//没次出多条鱼时候保存已经随机的路径
        List<int> mListRoute = new List<int>();// 用于随机选取的所有vib路径列表
        float mOutFishSpeed = 3f;
        int mFishObjIdGenerator = 1;
        FishGroupRouteAlloter mFishGroupRouteAlloter = null;
        FishRouteMap mFishRouteMap = null;
        BaseFishLordMgr mBaseFishLordMgr = null;
        string mNormalFishParticleFileName = "NormalFish.lord";
        string mRedFishParticleFileName = "RedFish.lord";
        float mOutFamtionDelay = 2f;
        OutFishState mOutFishState = OutFishState.None;
        string mParticleSystemName = "";
        List<string> mFormationNameList = null;

        //---------------------------------------------------------------------
        public void create(CLogicScene scene, BaseFishLordMgr base_fish_lord_mgr)
        {
            mScene = scene;
            mBaseFishLordMgr = base_fish_lord_mgr;

            mFishGroupRouteAlloter = new FishGroupRouteAlloter();
            mFishRouteMap = new FishRouteMap();
            Dictionary<int, EbData> mapData = EbDataMgr.Instance.getMapData<TbDataRoute>();
            foreach (var it in mapData)
            {
                if (TbDataRoute.DataState.ACTIVE == ((TbDataRoute)it.Value).State)
                {
                    mListRoute.Add(it.Key);
                }
            }

            mFormationNameList = mBaseFishLordMgr.getPrototypeNameList();
            mFormationNameList.Remove(mNormalFishParticleFileName);
            mFormationNameList.Remove(mRedFishParticleFileName);

            mOutFishState = OutFishState.Normal;
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            switch (mOutFishState)
            {
                case OutFishState.None:
                    break;
                case OutFishState.Normal:
                    _checkAndCreateFishCrowd(elapsed_tm);
                    break;
                case OutFishState.WaitingFormation:
                    mOutFamtionDelay -= elapsed_tm;
                    if (mOutFamtionDelay < 0)
                    {
                        generateFormation(mParticleSystemName);
                        mOutFishState = OutFishState.Formation;
                    }
                    break;
                case OutFishState.Formation:
                    if (mBaseFishLordMgr.getParticleSystemCount() <= 0)
                    {
                        mOutFishState = OutFishState.Normal;
                    }
                    break;
            }
        }

        //---------------------------------------------------------------------
        public void outFormation()
        {
            mOutFishState = OutFishState.WaitingFormation;
            mOutFamtionDelay = 2f;
            mParticleSystemName = mFormationNameList[getRandoNumber(0, 1000) % mFormationNameList.Count];
        }

        //---------------------------------------------------------------------
        void generateFormation(string particle_system_name)
        {
            int fish_begin_obj_id = getNextFishObjId(mBaseFishLordMgr.getCountOfParticleSystemEntity(particle_system_name));
            mBaseFishLordMgr.addParticleSystem(particle_system_name, new List<string>(), fish_begin_obj_id);
            mScene.getProtocol().s2allcCreateFishLord(particle_system_name, new List<string>(), fish_begin_obj_id);
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            mBaseFishLordMgr = null;
        }

        //---------------------------------------------------------------------
        public bool isFormation()
        {
            return mOutFishState == OutFishState.Formation;
        }

        //---------------------------------------------------------------------
        public void outRedFish(int normal_fish_vib_id, int red_fish_vib_id, int fish_count, EbVector3 position, int red_fish_obj_id)
        {
            createRedEntityParticleSystem(normal_fish_vib_id, red_fish_vib_id, fish_count, position, red_fish_obj_id);
        }

        //---------------------------------------------------------------------
        int getNextFishObjId(int fish_count)
        {
            int next_obj_id = mFishObjIdGenerator;
            mFishObjIdGenerator += fish_count;
            return next_obj_id;
        }

        //---------------------------------------------------------------------
        public void clearAllFish()
        {
        }

        //---------------------------------------------------------------------
        void createRedEntityParticleSystem(int normal_fish_vib_id, int red_fish_vib_id, int fish_count, EbVector3 position, int red_fish_obj_id)
        {
            int fish_begin_obj_id = getNextFishObjId(fish_count);

            List<string> server_param = new List<string>();
            server_param.Add(normal_fish_vib_id.ToString());
            server_param.Add(red_fish_vib_id.ToString());
            server_param.Add(getRedIndex(red_fish_vib_id > 0, position, fish_count).ToString());
            server_param.Add(fish_count.ToString());
            server_param.Add(position.x.ToString());
            server_param.Add(position.y.ToString());
            server_param.Add(red_fish_obj_id.ToString());

            mBaseFishLordMgr.addParticleSystem(mRedFishParticleFileName, server_param, fish_begin_obj_id);
            mScene.getProtocol().s2allcCreateFishLord(mRedFishParticleFileName, server_param, fish_begin_obj_id);
        }

        //---------------------------------------------------------------------
        int getRedIndex(bool has_red_fish, EbVector3 position, int count)
        {
            int red_fish_index = -1;

            if (has_red_fish)
            {
                float scale = 0;
                float range = 180f;

                if (position.y >= -range && position.y < range)
                {
                    if (position.x < 0)
                    {
                        scale = 0.25f;
                    }
                    else
                    {
                        scale = 0.75f;
                    }
                }
                else if (position.x >= 0 && position.y >= 0)
                {
                    scale = 0.625f;
                }
                else if (position.x < 0 && position.y >= 0)
                {
                    scale = 0.375f;
                }
                else if (position.x < 0 && position.y < 0)
                {
                    scale = 0.125f;
                }
                else
                {
                    scale = 0.875f;
                }

                red_fish_index = (int)((float)count * scale);
            }

            return red_fish_index;
        }

        //---------------------------------------------------------------------
        public int getRandoNumber(int min, int max)// 闭区间，即包括最小和最大值
        {
            return mRandom.Next(min, max + 1);
        }

        //---------------------------------------------------------------------
        // 定时1秒，根据创建鱼群规则决定是否需要创建新鱼群
        void _checkAndCreateFishCrowd(float elapsed_tm)
        {
            mfCheckAndCreateFishCrowdCountSecond += elapsed_tm;
            if (mfCheckAndCreateFishCrowdCountSecond > 1.0f)
            {
                mfCheckAndCreateFishCrowdCountSecond = 0.0f;
            }
            else return;

            int cur_fishcrowd_count = mBaseFishLordMgr.getAllEntityCount();
            int cur_need_fishcrowd_count = getRandoNumber(miMinFishCrowdCount, miMaxFishCrowdCount) - cur_fishcrowd_count;

            if (cur_need_fishcrowd_count <= 0) return;

            mListRouteIdSaver.Clear();// 清除上波出鱼记下来的随机路径id

            if (cur_need_fishcrowd_count > mOutFishSpeed) { cur_need_fishcrowd_count = (int)mOutFishSpeed; }

            int random_max_count = cur_need_fishcrowd_count * 2;// 防止没有鱼可以出的时候发生死循环
            while (cur_need_fishcrowd_count > 0 && random_max_count > 0)
            {
                if (_createFishCrowd())
                {
                    --cur_need_fishcrowd_count;
                }
                --random_max_count;
            }
            return;
        }

        //---------------------------------------------------------------------
        bool _createFishCrowd()
        {
            int fish_vib_id = mFishGroupRouteAlloter.getRandomRoute();
            if (null == EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id))
            {
                return false;
            }
            int fish_self_max_count = EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id).OutFishUpperBound;
            int fish_group_max_count = EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id).dataOutFishGroup.FishCount;

            int current_fish_self_count = mBaseFishLordMgr.getEntityCount(fish_vib_id);
            int current_fish_group_count = _getOutGroupFishCount(fish_vib_id);

            bool can_out_fish = false;

            if (fish_self_max_count == 0 && fish_group_max_count == 0)
            {
                can_out_fish = true;
            }
            else if (fish_self_max_count == 0)
            {
                if (fish_group_max_count > current_fish_group_count)
                {
                    can_out_fish = true;
                }
            }
            else if (fish_group_max_count == 0)
            {
                if (fish_self_max_count > current_fish_self_count)
                {
                    can_out_fish = true;
                }
            }
            else
            {
                if (fish_self_max_count > current_fish_self_count
                    && fish_group_max_count > current_fish_group_count)
                {
                    can_out_fish = true;
                }
            }

            if (can_out_fish)
            {
                createEntityParticleSystem(fish_vib_id);
            }

            return can_out_fish;
        }

        //---------------------------------------------------------------------
        int _getOutGroupFishCount(int fish_vib_id)
        {
            return mBaseFishLordMgr.getCountByGroup(EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id).dataOutFishGroup);
        }

        //---------------------------------------------------------------------
        void createEntityParticleSystem(int fish_vib_id)
        {
            int fish_count = 1;

            TbDataFish fish_data = EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id);
            if (fish_data.Type == TbDataFish.FishType.ChainFish)
            {
                fish_count = fish_data.ChainFishNumber;
            }

            int fish_begin_obj_id = getNextFishObjId(fish_count);
            int route_vib_id = _getRandomRouteId((TbDataFish.FishRouteCategory)EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id).fishRouteCategory);

            List<string> server_param = new List<string>();
            server_param.Add(fish_vib_id.ToString());
            server_param.Add(route_vib_id.ToString());

            mBaseFishLordMgr.addParticleSystem(mNormalFishParticleFileName, server_param, fish_begin_obj_id);
            mScene.getProtocol().s2allcCreateFishLord(mNormalFishParticleFileName, server_param, fish_begin_obj_id);
        }

        //---------------------------------------------------------------------
        int _getRandomRouteId(TbDataFish.FishRouteCategory category)
        {
            List<int> group_fish_id = mFishRouteMap.getGroupFishId(category);
            int random_id = getRandoNumber(0, group_fish_id.Count - 1);
            int route_vibid = 0;

            for (int i = 0; i + random_id < group_fish_id.Count; i++)
            {
                route_vibid = group_fish_id[random_id + i];
                if (!mListRouteIdSaver.Contains(route_vibid))
                {
                    mListRouteIdSaver.Add(route_vibid);
                    return route_vibid;
                }
            }

            for (int i = 1; random_id - i > 0; i++)
            {
                route_vibid = group_fish_id[random_id - i];
                if (!mListRouteIdSaver.Contains(route_vibid))
                {
                    mListRouteIdSaver.Add(route_vibid);
                    return route_vibid;
                }
            }

            return 1;
        }
    }
}
