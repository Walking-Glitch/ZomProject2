using UnityEngine;

public class Roaming : ZombieMovementBaseState
{
    public override void EnterState(ZombieMovementStateManager zombieMovement)
    {
        zombieMovement.patrol.enabled = true; 
    }

    public override void UpdateState(ZombieMovementStateManager zombieMovement)
    {
        if (zombieMovement.currentSpeed < 0.1f && !zombieMovement.IsPlayerInDetectionArea())
        {
            zombieMovement.SwitchState(zombieMovement.idle);
        }

        if (zombieMovement.IsPlayerInDetectionArea())
        {
            ExitState(zombieMovement, zombieMovement.chasing);
        }
    }

    private void ExitState(ZombieMovementStateManager zombieMovement, ZombieMovementBaseState state)
    {
        zombieMovement.patrol.enabled = false;


        zombieMovement.SwitchState(state);
    }
}
