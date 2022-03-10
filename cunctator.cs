using System;
using TenCrowns.AppCore;
using TenCrowns.GameCore;
using UnityEngine;
using UnityEngine.UI;


namespace cunctator
{
    public class cunctator : ModEntryPointAdapter
    {
        public override void Initialize(ModSettings modSettings)
        {
            base.Initialize(modSettings);
            Debug.Log("dickbuttt");
            modSettings.Factory = (GameFactory) new CunctatorFactory();
            
        }
    }
}
