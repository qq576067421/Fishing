using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class CSceneBox
    {
        //---------------------------------------------------------------------
        public float straightLineEquationByX(EbVector3 one_point, EbVector3 direction, float x)
        {
            return (x - one_point.x) * direction.y / direction.x + one_point.y;
        }

        //---------------------------------------------------------------------
        public float straightLineEquationByY(EbVector3 one_point, EbVector3 direction, float y)
        {
            return (y - one_point.y) * direction.x / direction.y + one_point.x;
        }

        //---------------------------------------------------------------------
        public bool check(ref EbVector3 position, ref float direction)
        {
            EbVector3 dir_vector = CLogicUtility.getDirection(direction);

            if (position.x < -CCoordinate.LogicSceneLength / 2)
            {
                if (dir_vector.x == 0) return false;
                EbVector3 return_postion = new EbVector3();
                return_postion.x = -CCoordinate.LogicSceneLength / 2;
                return_postion.y = straightLineEquationByX(position, dir_vector, return_postion.x);
                return_postion.z = position.z;
                position = return_postion;
                dir_vector.x = -dir_vector.x;
                direction = CLogicUtility.getAngle(dir_vector);
                return true;
            }

            if (position.x > CCoordinate.LogicSceneLength / 2)
            {
                if (dir_vector.x == 0) return false;
                EbVector3 return_postion = new EbVector3();
                return_postion.x = CCoordinate.LogicSceneLength / 2;
                return_postion.y = straightLineEquationByX(position, dir_vector, return_postion.x);
                return_postion.z = position.z;
                position = return_postion;
                dir_vector.x = -dir_vector.x;
                direction = CLogicUtility.getAngle(dir_vector);
                return true;
            }

            if (position.y < -CCoordinate.LogicSceneWidth / 2)
            {
                if (dir_vector.y == 0) return false;
                EbVector3 return_postion = new EbVector3();
                return_postion.y = -CCoordinate.LogicSceneWidth / 2;
                return_postion.x = straightLineEquationByY(position, dir_vector, return_postion.y);
                return_postion.z = position.z;
                position = return_postion;
                dir_vector.y = -dir_vector.y;
                direction = CLogicUtility.getAngle(dir_vector);
                return true;
            }

            if (position.y > CCoordinate.LogicSceneWidth / 2)
            {
                if (dir_vector.y == 0) return false;
                EbVector3 return_postion = new EbVector3();
                return_postion.y = CCoordinate.LogicSceneWidth / 2;
                return_postion.x = straightLineEquationByY(position, dir_vector, return_postion.y);
                return_postion.z = position.z;
                position = return_postion;
                dir_vector.y = -dir_vector.y;
                direction = CLogicUtility.getAngle(dir_vector);
                return true;
            }

            return false;
        }
    }
}
