using Assets.Scripts.Player.Actions;
using UnityEngine;

public abstract class ActionStateBase
{
    public abstract void EnterState(ActionStateManager actionStateManager);

    public abstract void UpdateState(ActionStateManager actionStateManager);

    public virtual void OnFire(ActionStateManager actionStateManager)
    {}
    public virtual void OnScroll(ActionStateManager actionStateManager, float scrollDelta)
    {

    }
}
