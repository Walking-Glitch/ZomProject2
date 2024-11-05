using UnityEngine;

public abstract class ZombieBaseState
{
    public abstract void EnterState(ZombieStateManager zombie);

    public abstract void UpdateState(ZombieStateManager zombie);
}
