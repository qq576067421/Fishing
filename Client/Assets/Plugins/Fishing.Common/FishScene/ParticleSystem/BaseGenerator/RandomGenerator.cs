//using System;
//using System.Collections.Generic;
//using GF.Common;

//    public class RandomGenerator : EntityGenerator<RandomGeneratorData>
//    {
//        //---------------------------------------------------------------------
//        public RandomGenerator(RandomGeneratorData generator_data)
//            : base(generator_data)
//        {

//        }

//        void randomFish()
//        {
//            int random_fish_vib_id = getRandoNumber(getGeneratorData().mFishVibStartID, getGeneratorData().mFishVibEndID);

//            BaseEntity entity = buildEntity(random_fish_vib_id, EbVector3.Zero, 0);
//            entity.addRoute(RouteHelper.buildLineRoute(1));

//            setDone();
//        }

//        //---------------------------------------------------------------------
//        public override void update(float elapsed_tm)
//        {
//        }

//        //---------------------------------------------------------------------
//        public override void destroy()
//        {
//            base.destroy();
//        }
//    }

//    public class RandomGeneratorFactory : BaseGeneratorFactory
//    {
//        //---------------------------------------------------------------------
//        public override string getGeneratorType()
//        {
//            return typeof(RandomGeneratorData).ToString();
//        }

//        //---------------------------------------------------------------------
//        public override EntityGenerator buildGenerator(BaseGeneratorData generator_data)
//        {
//            return new RandomGenerator((RandomGeneratorData)generator_data);
//        }

//        public override BaseGeneratorData buildGeneratorData(JsonItem json_item)
//        {
//            return BaseJsonSerializer.deserialize<RandomGeneratorData>(json_item.mJsonString);
//        }
//    }

//    public class RandomGeneratorData : BaseGeneratorData
//    {
//        public int mFishVibStartID = 0;
//        public int mFishVibEndID = 0;

//        public override BaseGeneratorData clone()
//        {
//            RandomGeneratorData generator_data = new RandomGeneratorData();

//            generator_data.mFishVibStartID = mFishVibStartID;
//            generator_data.mFishVibEndID = mFishVibEndID;

//            return generator_data;
//        }

//        public override int getBaseEntityCount()
//        {
//            return 1;
//        }
//    }