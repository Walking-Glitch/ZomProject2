using UnityEngine;

public abstract class AimStateBase
{
    public abstract void EnterState(AimStateManager aimStateManager);
    public abstract void UpdateState(AimStateManager aimStateManager);
}
