using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class ParticleSystemDataPrototype
    {
        //---------------------------------------------------------------------
        Dictionary<string, EntityParticleSystemData> mDicBaseFishLordDataPrototype = new Dictionary<string, EntityParticleSystemData>();
        ParticleSystemFactory mParticleSystemFactory = null;

        //---------------------------------------------------------------------
        public ParticleSystemDataPrototype(List<JsonPacket> json_packet_list, ParticleSystemFactory factory)
        {
            mParticleSystemFactory = factory;

            foreach (var json_packet in json_packet_list)
            {
                // 根据Json字符串来生成BaseFishLordData
                BaseFishLordDataJson fish_lord_json = BaseJsonSerializer.deserialize<BaseFishLordDataJson>(json_packet.JsonString);
                EntityParticleSystemData fish_lord = new EntityParticleSystemData();

                // 根据Json字符串来生成
                fish_lord.mFishLordCommonData = BaseJsonSerializer.deserialize<FishLordCommonData>
                    (fish_lord_json.mFishLordCommonDataString);

                // 根据Json字符串来生成BaseGeneratorData
                foreach (var it in fish_lord_json.mListBaseGenerator)
                {
                    EntityGeneratorData generator_data = mParticleSystemFactory.buildGeneratorData(it);
                    if (generator_data == null) continue;
                    fish_lord.mListBaseGenerator.Add(generator_data);
                }

                // 根据Json字符串来生成BaseAffectorData
                foreach (var it in fish_lord_json.mListBaseAffector)
                {
                    EntityAffectorData affector_data = mParticleSystemFactory.buildAffectorData(it);
                    if (affector_data == null) continue;
                    fish_lord.mListBaseAffector.Add(affector_data);
                }

                // 读出特效发射器和影响器的映射表。
                fish_lord.mGeneratorAffectorMap = BaseJsonSerializer.deserialize<List<GeneratorAffectorkeyValuePair>>
                    (fish_lord_json.mGeneratorAffectorMapString);

                mDicBaseFishLordDataPrototype.Add(json_packet.FileName.Substring(json_packet.FileName.LastIndexOf("/") + 1), fish_lord);
            }
        }

        //---------------------------------------------------------------------
        public int getCountOfParticleSystemEntity(string file_name)
        {
            if (mDicBaseFishLordDataPrototype.ContainsKey(file_name))
            {
                return mDicBaseFishLordDataPrototype[file_name].getBaseEntityCount();
            }
            return 0;
        }

        //---------------------------------------------------------------------
        public EntityParticleSystemData cloneParticleSystemData(string file_name)
        {
            if (!mDicBaseFishLordDataPrototype.ContainsKey(file_name)) return null;
            return mDicBaseFishLordDataPrototype[file_name].clone();
        }

        //---------------------------------------------------------------------
        public List<string> getPrototypeNameList()
        {
            List<string> name_list = new List<string>();
            foreach (var it in mDicBaseFishLordDataPrototype)
            {
                name_list.Add(it.Key);
            }
            return name_list;
        }
    }

    public class JsonPacket
    {
        string mFileName = string.Empty;
        string mJsonString = string.Empty;

        public string FileName { get { return mFileName; } }
        public string JsonString { get { return mJsonString; } }

        public JsonPacket(string file_name, string json_string)
        {
            mFileName = file_name;
            mJsonString = json_string;
        }
    }
}
