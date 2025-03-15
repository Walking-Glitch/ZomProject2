using Assets.Scripts.Game_Manager;
using Unity.Netcode;
using UnityEngine;

public class TurretSniper : TurretBase
{
   
    protected override void Start()
    {
        base.Start();
    }

    
    protected override void Fire(bool hasRecoil)
    {
        base.Fire(true);
    }


    protected override void SetAimDispersion()
    {
        minSpreadX = -0.2f;
        maxSpreadX = 0.2f;
        minSpreadY = 0.5f;
        maxSpreadY = 1.5f;
    }
}
