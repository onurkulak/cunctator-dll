using System;
using TenCrowns.AppCore;
using TenCrowns.GameCore;
using Mohawk.SystemCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TenCrowns.GameCore.Text;
using static TenCrowns.GameCore.Text.TextExtensions;

namespace cunctator
{
    internal class CunctatorPlayer : Player
    {
        public CunctatorPlayer()
        {
            return;
        }
        public override int getUnitBuildCost(UnitType eUnit, City pCity)
        {
            Debug.Log("why is this causing problem");
            int iModifier = 0;

            if (pCity != null)
            {
                Debug.Log("getunitbuildcost yo");
                iModifier += pCity.getUnitTrainModifier(eUnit);
                // if the unit is a military unit, its cost increase as the # units of the family increases
                if (infos().Helpers.canDamage(eUnit) && pCity.hasFamily()){
                    iModifier += (infos().Globals.SPECIALIST_COST_PRODUCED_MODIFIER * countFamilyMilitary(pCity.getFamily()));
                    Debug.Log("imdoifier:\t" + iModifier);
                    Debug.Log("fmaily unit cunt:\t" + countFamilyMilitary(pCity.getFamily()));
                }
                
                if (infos().unit(eUnit).mbReligious)
                {
                    iModifier += pCity.getDiscipleTrainTimeModifier();
                }

                foreach (UnitTraitType eLoopUnitTrait in infos().unit(eUnit).maeUnitTrait)
                {
                    iModifier += pCity.getUnitTraitTrainModifier(eLoopUnitTrait);
                }
            }

            int iYieldCost = infos().unit(eUnit).miProduction;

            if (iModifier != 0)
            {
                iYieldCost = infos().utils().modify(iYieldCost, iModifier);
            }

            return iYieldCost;
        }

    }
}
