using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TurretDataTable
    {
        //-------------------------------------------------------------------------
        Dictionary<TbDataTurret.TurretType, Dictionary<int, TbDataTurret>> mDictTurretData = new Dictionary<TbDataTurret.TurretType, Dictionary<int, TbDataTurret>>();

        //-------------------------------------------------------------------------
        public TurretDataTable()
        {
            TbDataTurret.TurretType turret_type;
            Dictionary<int, EbData> mapData = EbDataMgr.Instance.getMapData<TbDataTurret>();
            Dictionary<int, TbDataTurret> mapTurrets = new Dictionary<int, TbDataTurret>();
            foreach (var it in mapData)
            {
                turret_type = ((TbDataTurret)it.Value).mTurretType;

                if (turret_type == TbDataTurret.TurretType.None) continue;
                if (!mDictTurretData.ContainsKey(turret_type))
                {
                    mDictTurretData.Add(turret_type, new Dictionary<int, TbDataTurret>());
                }
                if (!mDictTurretData[turret_type].ContainsKey(((TbDataTurret)it.Value).TurretRate))
                {
                    mDictTurretData[turret_type].Add(((TbDataTurret)it.Value).TurretRate, (TbDataTurret)it.Value);
                }
                else
                {
                    EbLog.Warning("TbDataTurret configure has same value " + it.Value.Id);
                }
            }
        }

        //-------------------------------------------------------------------------
        public TbDataTurret getTurretData(TbDataTurret.TurretType turret_type, int rate)
        {
            Dictionary<int, TbDataTurret> dic_turretdata;
            if (mDictTurretData.ContainsKey(turret_type))
            {
                dic_turretdata = mDictTurretData[turret_type];
            }
            else
            {
                dic_turretdata = mDictTurretData[TbDataTurret.TurretType.NormalTurret];
            }

            TbDataTurret max_turretdata = null;
            foreach (var it in dic_turretdata)
            {
                max_turretdata = it.Value;
                break;
            }

            foreach (var it in dic_turretdata)
            {
                if (max_turretdata.TurretRate < it.Value.TurretRate)
                {
                    max_turretdata = it.Value;
                }
            }

            foreach (var it in dic_turretdata)
            {
                if (rate <= it.Value.TurretRate)
                {
                    if (max_turretdata.TurretRate > it.Value.TurretRate)
                    {
                        max_turretdata = it.Value;
                    }
                }
            }

            return max_turretdata;
        }
    }
}
