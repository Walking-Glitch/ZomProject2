using UnityEngine;

public class Chasing : ZombieMovementBaseState
{
    private float elapsed;
    private float timeToChange = 2f;
    public override void EnterState(ZombieMovementStateManager zombieMovement)
    {
        elapsed = 0;
        zombieMovement.destinationSetter.enabled = true;
    }

    public override void UpdateState(ZombieMovementStateManager zombieMovement)
    {
        if (!zombieMovement.IsPlayerInDetectionArea())
        {
            Debug.Log(elapsed);
            elapsed += Time.deltaTime;

            if (elapsed >= timeToChange)
            {
                ExitState(zombieMovement, zombieMovement.roaming);
            }
        }
        else
        {
            elapsed = 0; 
        }

       
    }

    private void ExitState(ZombieMovementStateManager zombieMovement, ZombieMovementBaseState state)
    {
        zombieMovement.destinationSetter.enabled = false;
        elapsed = 0;
        zombieMovement.SwitchState(state);
    }
}
