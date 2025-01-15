using UnityEngine;

public class TurretShotgun : TurretBase
{
    
    protected override void Fire(bool hasRecoil)
    {
        base.Fire(true);
    }
}
