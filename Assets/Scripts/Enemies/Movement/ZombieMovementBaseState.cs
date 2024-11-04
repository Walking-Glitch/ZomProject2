using UnityEngine;

public abstract class ZombieMovementBaseState
{
    public abstract void EnterState(ZombieMovementStateManager zombieMovement);

    public abstract void UpdateState(ZombieMovementStateManager zombieMovement);
}
