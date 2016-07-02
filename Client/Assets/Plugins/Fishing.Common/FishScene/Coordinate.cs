using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GF.Common;

namespace Ps
{
    public class CCoordinate
    {
        //---------------------------------------------------------------------
        public static float LogicSceneLength { get { return 960.0f; } }
        public static float LogicSceneWidth { get { return 640.0f; } }
        static EbVector3 mGameAreaOrigin = EbVector3.Zero;
        static EbVector3 mGameAreaSize;
        static EbVector3 mScreenSize;
        static float mLogic2PixelX = 1.0f;
        static float mLogic2PixelY = 1.0f;

        //---------------------------------------------------------------------
        public static void setCoordinate(EbVector3 screen_size, EbVector3 area_origin, EbVector3 area_size)
        {
            // 屏幕坐标原点为屏幕的左下角为屏幕原点计算
            mScreenSize = screen_size;
            mScreenSize.z = 0;
            mGameAreaOrigin = area_origin;
            mGameAreaSize = area_size;

            initConversionOfCoordinates();
            //EbLog.Warning("setCoordinate screen_size" + screen_size + "area_origin" + area_origin + "area_size" + area_size + "mLogic2PixelX" + mLogic2PixelX + "mLogic2PixelY" + mLogic2PixelY);
        }

        //---------------------------------------------------------------------
        // 根据屏幕大小，游戏区域，和逻辑场景大小来设置转换变量
        static void initConversionOfCoordinates()
        {
            mLogic2PixelX = LogicSceneLength / mGameAreaSize.x;
            mLogic2PixelY = LogicSceneWidth / mGameAreaSize.y;
        }

        //---------------------------------------------------------------------
        static EbVector3 screen2game_area_pos(EbVector3 screen_pos)
        {
            return screen_pos - mGameAreaOrigin;
        }

        //---------------------------------------------------------------------
        static EbVector3 game_area2logic_pos(EbVector3 game_area_pos)
        {
            EbVector3 logic_pos = EbVector3.Zero;

            logic_pos.x = game_area_pos.x * mLogic2PixelX - LogicSceneLength / 2;// 逻辑坐标系原点约定在中间
            logic_pos.y = game_area_pos.y * mLogic2PixelY - LogicSceneWidth / 2;// 逻辑坐标系原点约定在中间

            return logic_pos;
        }

        //---------------------------------------------------------------------
        static EbVector3 logic2game_area_pos(EbVector3 logic_pos)
        {
            EbVector3 game_area_pos = EbVector3.Zero;

            game_area_pos.x = (logic_pos.x + LogicSceneLength / 2) / mLogic2PixelX;
            game_area_pos.y = (logic_pos.y + LogicSceneWidth / 2) / mLogic2PixelY;

            return game_area_pos;
        }

        //---------------------------------------------------------------------
        static EbVector3 game_area2screen_pos(EbVector3 game_area_pos)
        {
            return game_area_pos + mGameAreaOrigin;
        }

        //---------------------------------------------------------------------
        // conversion of coordinates
        public static EbVector3 logic2pixelPos(EbVector3 logic_pos)
        {
            return game_area2screen_pos(logic2game_area_pos(logic_pos));
        }

        //---------------------------------------------------------------------
        public static EbVector3 pixel2logicPos(EbVector3 pixel_pos)
        {
            //逻辑位置在正中心为（0，0），以米为单位
            //屏幕坐标原点为屏幕的左下角，屏幕的宽度对应逻辑长度960米
            EbVector3 logic_postion = pixel_pos;

            logic_postion.x *= mLogic2PixelX;
            logic_postion.y *= mLogic2PixelY;

            logic_postion.x -= LogicSceneLength / 2f;
            logic_postion.y -= LogicSceneWidth / 2f;

            return logic_postion;
        }

        //---------------------------------------------------------------------
        public static float logic2pixelAngle(float logic_angle)
        {
            return logic_angle;
        }

        //---------------------------------------------------------------------
        public static float pixel2logicAngle(float pixel_angle)
        {
            return pixel_angle;
        }

        //---------------------------------------------------------------------
        // 将逻辑坐标系转换为2D toolkit对应的坐标系
        public static EbVector3 logic2toolkitPos(EbVector3 logic_pos)
        {
            EbVector3 toolkit_pos = EbVector3.Zero;

            toolkit_pos.x = (logic_pos.x + LogicSceneLength / 2);
            toolkit_pos.y = (logic_pos.y + LogicSceneWidth / 2);

            return toolkit_pos;
        }
    }
}
