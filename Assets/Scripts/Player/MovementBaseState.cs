using UnityEngine;

public abstract class MovementBaseState
{
    public abstract void EnterState(MovementStateManager movementStateManager);

    public abstract void ExitState(MovementStateManager movementStateManager);
}
