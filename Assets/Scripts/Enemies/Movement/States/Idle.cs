using UnityEngine;

public class Idle : ZombieMovementBaseState
{
    public override void EnterState(ZombieMovementStateManager zombieMovement)
    {
        
    }

    public override void UpdateState(ZombieMovementStateManager zombieMovement)
    {
        if (zombieMovement.currentSpeed > 0.1f)
        {
            zombieMovement.SwitchState(zombieMovement.roaming);
        }

    }
}
