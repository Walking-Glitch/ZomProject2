using Assets.Scripts.Player.Actions;
using UnityEngine;

public class BuildState : ActionStateBase
{
    public override void EnterState(ActionStateManager actionStateManager)
    {
        actionStateManager.gameManager.WeaponLaser.ChangeLazerColorInventory();
        actionStateManager.gameManager.BuildManager.isPlacing = false;
    }

    public override void UpdateState(ActionStateManager actionStateManager)
    { 
       actionStateManager.gameManager.BuildManager.CheckIfOnValidPlacement();
  
       if (actionStateManager.AimStateManager.CurrentState == actionStateManager.AimStateManager.AimIdleState)
        {
            actionStateManager.gameManager.BuildManager.DestroyPreview();
            actionStateManager.SwitchState(actionStateManager.Default);
        }
    }

    public override void OnFire(ActionStateManager actionStateManager)
    {
        actionStateManager.gameManager.BuildManager.PlacePrefab();
    }

    public override void OnScroll(ActionStateManager actionStateManager, float scrollDelta)
    {
        actionStateManager.gameManager.BuildManager.SwitchSelectedPrefab(scrollDelta, actionStateManager.gameManager.BuildManager.AlternateBetweenFakeLists());  
    }

    public override void OnInventory(ActionStateManager actionStateManager)
    {
        actionStateManager.gameManager.BuildManager.DestroyPreview();
        actionStateManager.SwitchState(actionStateManager.Default);
        //Debug.Log("ASDFADADADA");
    }




}


