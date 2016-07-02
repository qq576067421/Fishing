using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class ClientEquipMirror<TDef> : Component<TDef> where TDef : DefEquipMirror, new()
    {
        //-------------------------------------------------------------------------
        public ClientActorMirror<DefActorMirror> CoActorMirror { get; private set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            CoActorMirror = Entity.getComponent<ClientActorMirror<DefActorMirror>>();

            //int weapon_itemid = Def.mPropWeaponItemId.get();
            //if (weapon_itemid != 0)
            //{
            //    // 穿武器            
            //}
            //CoActorMirror.ActorLoadedCallBack(_changeWeapon);
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public void changeWeapon(int weapon_itemid)
        {
            //if (CoActorMirror.SceneActorObj != null)
            //{
            //    string weapon_name = "";
            //    if (weapon_itemid != 0)
            //    {
            //        TbDataItem item = EbDataMgr.Instance.getData<TbDataItem>(weapon_itemid);
            //        if (item.UnitType == "Equip")
            //        {
            //            TbDataUnitEquip equip = EbDataMgr.Instance.getData<TbDataUnitEquip>(weapon_itemid);
            //            weapon_name = equip.SpinePrefabName;
            //        }
            //        else
            //        {
            //            EbLog.Note("该物品不是装备！");
            //        }                
            //    }
            //    CoActorMirror.SceneActorObj.setWeapon(weapon_name);
            //}
        }

        //-------------------------------------------------------------------------
        void _changeWeapon()
        {
            changeWeapon(Def.mPropWeaponItemId.get());
        }
    }
}
