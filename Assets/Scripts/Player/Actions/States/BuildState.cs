using Assets.Scripts.Player.Actions;
using UnityEngine;

public class BuildState : ActionStateBase
{
    public override void EnterState(ActionStateManager actionStateManager)
    {
        actionStateManager.gameManager.WeaponLaser.ChangeLazerColorInventory();
    }

    public override void UpdateState(ActionStateManager actionStateManager)
    {
        //if (actionStateManager.gameManager.BuildManager.isPlacing)
        //{
            actionStateManager.gameManager.BuildManager.CheckIfOnValidPlacement();
        //}
      

       if (actionStateManager.AimStateManager.CurrentState == actionStateManager.AimStateManager.AimIdleState)
        {
             
            actionStateManager.SwitchState(actionStateManager.Default);
        }
    }

    public override void OnFire(ActionStateManager actionStateManager)
    {
        actionStateManager.gameManager.BuildManager.PlacePrefab();
    }

    public override void OnScroll(ActionStateManager actionStateManager, float scrollDelta)
    {
        actionStateManager.gameManager.BuildManager.SwitchSelectedPrefab(scrollDelta);  
    }

   


}


