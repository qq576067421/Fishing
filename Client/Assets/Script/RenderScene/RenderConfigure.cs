using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class RenderConfigureStruct
    {
        //-------------------------------------------------------------------------
        public float mCoinScale = 1f;
        public float mChipSacle = 1f;
        public float mChipHSpace = 1f;
        public float mChipVSpace = 1f;
        public float mChipNumberOffset = 1f;
        public Dictionary<CSpriteNumber._eNumberSize, float> mScoreNumberScale = new Dictionary<CSpriteNumber._eNumberSize, float>();
        public Dictionary<CSpriteNumber._eNumberSize, float> mPanelNumberScale = new Dictionary<CSpriteNumber._eNumberSize, float>();
        public Point mTurretOffset = new Point();
        public Point mChipsOffset = new Point();
        public Point mTurretTurnplateOffset = new Point();
        public Point mTurretRateOffset = new Point();
        public Point mTurretPanelScoreBgOffset = new Point();
        public Point mTurretPanelScoreOffset = new Point();
        public Point mTurretBufferOffset = new Point();
        public Point mTurretAvatarOffset = new Point();
        public float mUpTurretPanelScoreOffset = 0f;
        public int mChipsMaxCount;
        public string mScoreGap = "";

        public class Point
        {
            //-------------------------------------------------------------------------
            public float x = 0;
            public float y = 0;

            //-------------------------------------------------------------------------
            public EbVector3 toViVector3()
            {
                return new EbVector3(x, y, 0);
            }
        }
    }
    public class RenderConfigure
    {
        //-------------------------------------------------------------------------
        RenderConfigureStruct mRenderConfigureStruct = null;

        public float CoinScale { get { return mRenderConfigureStruct.mCoinScale; } }
        public float ChipSacle { get { return mRenderConfigureStruct.mChipSacle; } }
        public float ChipHSpace { get { return mRenderConfigureStruct.mChipHSpace; } }
        public float ChipVSpace { get { return mRenderConfigureStruct.mChipVSpace; } }
        public float ChipNumberOffset { get { return mRenderConfigureStruct.mChipNumberOffset; } }
        public float getScoreNumberScale(CSpriteNumber._eNumberSize size)
        {
            return mRenderConfigureStruct.mScoreNumberScale[size];
        }
        public float getPanelNumberScale(CSpriteNumber._eNumberSize size)
        {
            return mRenderConfigureStruct.mPanelNumberScale[size];
        }
        public EbVector3 TurretOffset { get { return mRenderConfigureStruct.mTurretOffset.toViVector3(); } }
        public EbVector3 ChipsOffset { get { return mRenderConfigureStruct.mChipsOffset.toViVector3(); } }
        public EbVector3 TurretTurnplateOffset { get { return mRenderConfigureStruct.mTurretTurnplateOffset.toViVector3(); } }
        public EbVector3 TurretRateOffset { get { return mRenderConfigureStruct.mTurretRateOffset.toViVector3(); } }
        public EbVector3 TurretPanelScoreBgOffset { get { return mRenderConfigureStruct.mTurretPanelScoreBgOffset.toViVector3(); } }
        public EbVector3 TurretPanelScoreOffset { get { return mRenderConfigureStruct.mTurretPanelScoreOffset.toViVector3(); } }
        public EbVector3 TurretBufferOffset { get { return mRenderConfigureStruct.mTurretBufferOffset.toViVector3(); } }
        public EbVector3 TurretAvatarOffset { get { return mRenderConfigureStruct.mTurretAvatarOffset.toViVector3(); } }
        public float UpTurretPanelScoreOffset { get { return mRenderConfigureStruct.mUpTurretPanelScoreOffset; } }
        public float ChipsMaxCount { get { return mRenderConfigureStruct.mChipsMaxCount; } }
        public string ScoreGap { get { return mRenderConfigureStruct.mScoreGap; } }

        //-------------------------------------------------------------------------
        public RenderConfigure(string file_name)
        {
            try
            {
                deserializeFile(file_name);
            }
            catch (Exception e)
            {
                readFileException(e, file_name);
            }
        }

        //-------------------------------------------------------------------------
        void deserializeFile(string file_name)
        {
            mRenderConfigureStruct = BaseJsonSerializer.deserialize<RenderConfigureStruct>(new RenderFileReader(file_name).ReadToEnd());
        }

        //-------------------------------------------------------------------------
        void readFileException(Exception e, string file_name)
        {
            throw new Exception("RenderConfigure::Read file error:: " + file_name, e);
        }
    }
}
