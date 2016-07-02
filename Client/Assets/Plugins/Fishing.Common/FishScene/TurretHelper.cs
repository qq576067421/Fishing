using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public static class CBulletConstant
    {
        public static float ManualSpeed = 400f;
        public static float AutoLongpressSpeed = 600f;
        public static float AutoRapidSpeed = 1000f;
    }

    public class CTurretHelper
    {
        public class Color
        {
            public float r;
            public float g;
            public float b;
            public Color(float r, float g, float b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
            }
        }

        //-------------------------------------------------------------------------
        EbVector3[] mBasePositions = new EbVector3[7];// 0-5是联机玩家的炮台，6号炮台为单机炮台
        float[] mBaseAngles = new float[7];
        Color[] mBaseColor = new Color[7];

        //-------------------------------------------------------------------------
        public CTurretHelper()
        {
            float scene_length = CCoordinate.LogicSceneLength;
            float scene_width = CCoordinate.LogicSceneWidth;

            mBasePositions[0] = new EbVector3(-(scene_length / 4), scene_width / 2, 0);
            mBasePositions[1] = new EbVector3(scene_length / 4, scene_width / 2, 0);
            mBasePositions[2] = new EbVector3(scene_length / 2, 0, 0);

            mBasePositions[3] = new EbVector3(scene_length / 4, -(scene_width / 2), 0);
            mBasePositions[4] = new EbVector3(-(scene_length / 4), -(scene_width / 2), 0);
            mBasePositions[5] = new EbVector3(-(scene_length / 2), 0, 0);
            mBasePositions[6] = new EbVector3(0, -(scene_width / 2), 0);

            mBaseAngles[0] = 180f;
            mBaseAngles[1] = 180f;
            mBaseAngles[2] = -90f;
            mBaseAngles[3] = 0f;
            mBaseAngles[4] = 0f;
            mBaseAngles[5] = 90f;
            mBaseAngles[6] = 0f;

            mBaseColor[0] = new Color(240 / 255f, 255f / 255f, 240f / 255f);
            mBaseColor[1] = new Color(255f / 255f, 255f / 255f, 255 / 255f);
            mBaseColor[2] = new Color(240f / 255f, 255f / 255f, 255 / 255f);

            mBaseColor[3] = new Color(255 / 255f, 240 / 255f, 245f / 255f);
            mBaseColor[4] = new Color(255 / 255f, 240 / 255f, 180 / 245f);
            mBaseColor[5] = new Color(255 / 255f, 255 / 255f, 224 / 255f);

            mBaseColor[6] = new Color(255 / 255f, 240 / 255f, 245f / 255f);
        }

        //-------------------------------------------------------------------------
        public EbVector3 getPositionByOffset(int turret_id, EbVector3 offset)
        {
            return mBasePositions[turret_id] + CLogicUtility.getVector2ByRotateAngle(offset, mBaseAngles[turret_id]);
        }

        //-------------------------------------------------------------------------
        public float getBaseAngleByTurretId(int turret_id)
        {
            return mBaseAngles[turret_id];
        }

        //-------------------------------------------------------------------------
        public Color getBaseColorByTurretId(int turret_id)
        {
            return mBaseColor[turret_id];
        }
    }
}
