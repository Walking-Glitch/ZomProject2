using UnityEngine;

public abstract class ActionStateBase
{
    public abstract void EnterState(ActionStateManager actionStateManager);

    public abstract void UpdateState(ActionStateManager actionStateManager);
}
