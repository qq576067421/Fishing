//using System;
//using System.Collections.Generic;

//    public class DestroyAffector : BaseAffector<DestroyAffectorData>
//    {
//        //---------------------------------------------------------------------
//        bool mHasDivergence = false;

//        //---------------------------------------------------------------------
//        public DestroyAffector(DestroyAffectorData affector_data) : base(affector_data) { }

//        //---------------------------------------------------------------------
//        public override void update(float elapsed_tm)
//        {
//            if (mHasDivergence) return;

//            base.update(elapsed_tm);

//            if (getAffectorData().mDestroyTime <= getPassedTime())
//            {
//                getBaseFishLord()/
//            }
//        }

//        //---------------------------------------------------------------------
//        public override void destroy()
//        {
//        }
//    }

//    public class DestroyAffectorFactory : BaseAffectorFactory
//    {
//        //---------------------------------------------------------------------
//        public override string getAffectorType()
//        {
//            return typeof(DestroyAffector).ToString();
//        }

//        //---------------------------------------------------------------------
//        public override BaseAffector buildAffector(BaseAffectorData affector_data)
//        {
//            return new DestroyAffector((DestroyAffectorData)affector_data);
//        }

//        public override BaseAffectorData buildAffectorData(JsonItem json_item)
//        {
//            return BaseJsonSerializer.deserialize<DestroyAffectorData>(json_item.mJsonString);
//        }
//    }

//    public class DestroyAffectorData : BaseAffectorData
//    {
//        public float mDestroyTime = 0;

//        public override BaseAffectorData clone()
//        {
//            DestroyAffectorData affector_data = new DestroyAffectorData();

//            affector_data.mDestroyTime = mDestroyTime;

//            return affector_data;
//        }
//    }
