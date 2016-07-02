using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EntityParticleSystemData
    {
        //---------------------------------------------------------------------
        public FishLordCommonData mFishLordCommonData = new FishLordCommonData();
        public List<EntityGeneratorData> mListBaseGenerator = new List<EntityGeneratorData>();
        public List<EntityAffectorData> mListBaseAffector = new List<EntityAffectorData>();
        public List<GeneratorAffectorkeyValuePair> mGeneratorAffectorMap = new List<GeneratorAffectorkeyValuePair>();

        //---------------------------------------------------------------------
        public BaseFishLordDataJson getJson()
        {
            BaseFishLordDataJson json_data = new BaseFishLordDataJson();

            json_data.mFishLordCommonDataString = BaseJsonSerializer.serialize(mFishLordCommonData);

            foreach (var it in mListBaseGenerator)
            {
                JsonItem json_item = new JsonItem();
                json_item.mTypeName = it.GetType().ToString();
                json_item.mJsonString = BaseJsonSerializer.serialize(it);
                json_data.mListBaseGenerator.Add(json_item);
            }

            foreach (var it in mListBaseAffector)
            {
                JsonItem json_item = new JsonItem();
                json_item.mTypeName = it.GetType().ToString();
                json_item.mJsonString = BaseJsonSerializer.serialize(it);
                json_data.mListBaseAffector.Add(json_item);
            }

            json_data.mGeneratorAffectorMapString = BaseJsonSerializer.serialize(mGeneratorAffectorMap);

            return json_data;
        }

        //---------------------------------------------------------------------
        public int getBaseEntityCount()
        {
            int count = 0;
            foreach (var it in mListBaseGenerator)
            {
                count += it.getBaseEntityCount();
            }
            return count;
        }

        //---------------------------------------------------------------------
        public EntityParticleSystemData clone()
        {
            EntityParticleSystemData data = new EntityParticleSystemData();

            data.mFishLordCommonData = mFishLordCommonData.clone();

            foreach (var it in mListBaseGenerator)
            {
                data.mListBaseGenerator.Add(it.clone());
            }

            foreach (var it in mListBaseAffector)
            {
                data.mListBaseAffector.Add(it.clone());
            }

            foreach (var it in mGeneratorAffectorMap)
            {
                data.mGeneratorAffectorMap.Add(it.clone());
            }

            return data;
        }
    }

    public class FishLordCommonData
    {
        //---------------------------------------------------------------------
        public float mDestroyTime = 0;

        //---------------------------------------------------------------------
        public FishLordCommonData clone()
        {
            FishLordCommonData common_data = new FishLordCommonData();
            common_data.mDestroyTime = mDestroyTime;
            return common_data;
        }
    }

    public class BaseFishLordDataJson
    {
        public string mFishLordCommonDataString = string.Empty;
        public List<JsonItem> mListBaseGenerator = new List<JsonItem>();
        public List<JsonItem> mListBaseAffector = new List<JsonItem>();
        public string mGeneratorAffectorMapString = string.Empty;
    }

    public class JsonItem
    {
        public string mTypeName = string.Empty;
        public string mJsonString = string.Empty;
    }

    public class GeneratorAffectorkeyValuePair
    {
        public int mGeneratorIndex = 0;
        public int mAffectorIndex = 0;

        public GeneratorAffectorkeyValuePair clone()
        {
            GeneratorAffectorkeyValuePair pair = new GeneratorAffectorkeyValuePair();
            pair.mGeneratorIndex = mGeneratorIndex;
            pair.mAffectorIndex = mAffectorIndex;
            return pair;
        }
    }
}
