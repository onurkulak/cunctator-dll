using System;
using TenCrowns.AppCore;
using TenCrowns.GameCore;
using UnityEngine;
using UnityEngine.UI;


namespace cunctator
{
    internal class CunctatorFactory : GameFactory
    {
        public CunctatorFactory() : base(){
            return;
        }
        public override Unit CreateUnit()
        {
            return (Unit) new CunctatorUnit();
        }

        /*public override Player CreatePlayer()
        {
            return (Player) new CunctatorPlayer();
        }*/
    }
}
