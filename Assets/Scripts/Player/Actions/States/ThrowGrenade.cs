using Assets.Scripts.Player.Actions;
using UnityEngine;

public class ThrowGrenade : ActionStateBase
{
    private float elapsed;
    private float timeToexplode = 5f;
    public override void EnterState(ActionStateManager actionStateManager)
    {
        elapsed = 0;
        actionStateManager.TossGrenade();
    }

    public override void UpdateState(ActionStateManager actionStateManager)
    {

        if (elapsed < timeToexplode)
        {
            elapsed += Time.deltaTime;
            Debug.Log(elapsed);
        }

        else
        {
            actionStateManager.SwitchState(actionStateManager.Default);
        }
    }
}
