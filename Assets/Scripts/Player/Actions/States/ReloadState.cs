using Assets.Scripts.Player.Actions;
using UnityEngine;

public class ReloadState : ActionStateBase
{
    public override void EnterState(ActionStateManager actionStateManager)
    { 
        actionStateManager.anim.SetTrigger("Reload");
        actionStateManager.WeaponManager.laser.DisableLaser();
    }

    public override void UpdateState(ActionStateManager actionStateManager)
    {
       
    }
}
