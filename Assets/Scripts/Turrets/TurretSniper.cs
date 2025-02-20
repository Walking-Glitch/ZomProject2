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
   
}
