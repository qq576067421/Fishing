using System;
using System.IO;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class ParticleSystemFactory
    {
        //---------------------------------------------------------------------
        Dictionary<string, EntityGeneratorFactory> mDicGeneratorFactory = new Dictionary<string, EntityGeneratorFactory>();
        Dictionary<string, EntityAffectorFactory> mDicAffectorFactory = new Dictionary<string, EntityAffectorFactory>();

        //---------------------------------------------------------------------
        public void regGeneratorFactory(EntityGeneratorFactory generator_factory)
        {
            mDicGeneratorFactory.Add(generator_factory.getGeneratorType(), generator_factory);
        }

        //---------------------------------------------------------------------
        public void regAffectorFactory(EntityAffectorFactory affector_factory)
        {
            mDicAffectorFactory.Add(affector_factory.getAffectorType(), affector_factory);
        }

        //---------------------------------------------------------------------
        public EntityGeneratorData buildGeneratorData(JsonItem json_item)
        {
            if (mDicGeneratorFactory.ContainsKey(json_item.mTypeName))
            {
                return mDicGeneratorFactory[json_item.mTypeName].buildGeneratorData(json_item);
            }
            else
            {
                EbLog.Error(@"BaseFishLordMgr::buildGeneratorData::error::there are no " + json_item.mTypeName + " factory");
                return null;
            }
        }

        //---------------------------------------------------------------------
        public EntityAffectorData buildAffectorData(JsonItem json_item)
        {
            if (mDicAffectorFactory.ContainsKey(json_item.mTypeName))
            {
                return mDicAffectorFactory[json_item.mTypeName].buildAffectorData(json_item);
            }
            else
            {
                EbLog.Error(@"BaseFishLordMgr::buildAffectorData::error::there are no " + json_item.mTypeName + " factory");
                return null;
            }
        }

        //---------------------------------------------------------------------
        public EntityGenerator buildGenerator(EntityGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            string type_name = generator_data.GetType().ToString();
            if (mDicGeneratorFactory.ContainsKey(type_name))
            {
                return mDicGeneratorFactory[type_name].buildGenerator(generator_data, server_param, route_object_mgr);
            }
            else
            {
                EbLog.Error(@"BaseFishLordMgr::buildGenerator::error::there are no " + type_name + " factory");
                return null;
            }
        }

        //---------------------------------------------------------------------
        public EntityAffector buildAffector(EntityAffectorData affector_data)
        {
            string type_name = affector_data.GetType().ToString();
            if (mDicAffectorFactory.ContainsKey(type_name))
            {
                return mDicAffectorFactory[type_name].buildAffector(affector_data);
            }
            else
            {
                EbLog.Error(@"BaseFishLordMgr::buildAffector::error::there are no " + type_name + " factory");
                return null;
            }
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            mDicGeneratorFactory.Clear();
            mDicAffectorFactory.Clear();
        }
    }
}
