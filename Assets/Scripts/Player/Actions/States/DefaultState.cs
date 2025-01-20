using Assets.Scripts.Player.Actions;
using UnityEngine;

public class DefaultState : ActionStateBase
{
    public override void EnterState(ActionStateManager actionStateManager)
    {
        actionStateManager.gameManager.WeaponLaser.ChangeLazerColorDefault();
        actionStateManager.WeaponManager.AdjustWeaponParentedHand();
    }

    public override void UpdateState(ActionStateManager actionStateManager)
    {
       
    }
}

