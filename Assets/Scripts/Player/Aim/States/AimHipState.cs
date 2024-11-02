using UnityEngine;

public class AimHipState : AimStateBase
{
    public override void EnterState(AimStateManager aimStateManager)
    { 
          aimStateManager.anim.SetBool("IsAiming", false);
          if(aimStateManager.WeaponManager.laser != null) aimStateManager.WeaponManager.laser.DisableLaser();

    }

    public override void UpdateState(AimStateManager aimStateManager)
    {
        //if (aimStateManager.actionStateManager.CurrentState != aimStateManager.actionStateManager.Reload)
        //{
        //    aimStateManager.AdjustConstraintWeight();//
        //}

     
    }
}
