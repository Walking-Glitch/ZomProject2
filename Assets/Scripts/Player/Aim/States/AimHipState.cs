using UnityEngine;

public class AimHipState : AimStateBase
{
    public override void EnterState(AimStateManager aimStateManager)
    { 
          aimStateManager.anim.SetBool("IsAiming", false);
          // a shot queued while aiming would otherwise play the firing animation from the hip
          aimStateManager.anim.ResetTrigger("Firing");
          if(aimStateManager.WeaponManager.laser != null) aimStateManager.WeaponManager.laser.DisableLaser();

    }

    public override void UpdateState(AimStateManager aimStateManager)
    {
        
    }
}
