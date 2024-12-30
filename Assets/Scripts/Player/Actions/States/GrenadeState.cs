using Assets.Scripts.Player.Actions;
using UnityEngine;

public class GrenadeState : ActionStateBase
{
    //private float elapsed;
    //private float timeToexplode = 2f;
    public override void EnterState(ActionStateManager actionStateManager)
    {
        //elapsed = 0;
        actionStateManager.WeaponManager.AdjustWeaponParentedHand();
        actionStateManager.anim.SetTrigger("Grenade");
        actionStateManager.WeaponManager.laser.DisableLaser();
      
    }

    public override void UpdateState(ActionStateManager actionStateManager)
    {

        //if (elapsed < timeToexplode)
        //{
        //    elapsed += Time.deltaTime;
        //    Debug.Log(elapsed);
        //}

        //else
        //{
        //    actionStateManager.SwitchState(actionStateManager.Default);
        //}
    }
}
