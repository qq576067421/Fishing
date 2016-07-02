using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class BoxColliderTester
    {
        //---------------------------------------------------------------------
        public void test1()
        {
            CBoxCollider box1 = new CBoxCollider(0, 0, 20, 20);
            CBoxCollider box2 = new CBoxCollider(0, 0, 20, 20);

            box1.setPosition(EbVector3.Zero);
            box2.setPosition(new EbVector3(10, 10, 0));
            assert(box1.isIn(box2), "error", "ok");

            box1.setPosition(EbVector3.Zero);
            box2.setPosition(new EbVector3(20, 10, 0));
            assert(box1.isIn(box2), "error", "ok");

            box1.setPosition(EbVector3.Zero);
            box2.setPosition(new EbVector3(212, 10, 0));
            assert(box1.isIn(box2), "error", "ok");
        }

        //---------------------------------------------------------------------
        public void test2()
        {
            CBoxCollider box1 = new CBoxCollider(0, 0, 20, 20);
            //assert(box1.isIn(box2), "error", "ok");
        }

        //---------------------------------------------------------------------
        int test_count = 0;
        void assert(bool is_ok, string error, string ok)
        {
            EbLog.Warning((test_count++) + " :: " + (is_ok ? ok : error));
        }
    }
}
