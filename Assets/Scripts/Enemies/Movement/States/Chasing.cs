using UnityEngine;

public class Chasing : ZombieBaseState
{
    private float elapsed;
    private float timeToChange = 2f;
    public override void EnterState(ZombieStateManager zombie)
    {
        elapsed = 0;
        zombie.destinationSetter.enabled = true;
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        if (!zombie.IsPlayerInDetectionArea())
        {
            Debug.Log(elapsed);
            elapsed += Time.deltaTime;

            if (elapsed >= timeToChange)
            {
                ExitState(zombie, zombie.roaming);
            }
        }
        else
        {
            elapsed = 0; 
        }

       
    }

    private void ExitState(ZombieStateManager zombie, ZombieBaseState state)
    {
        zombie.destinationSetter.enabled = false;
        zombie.SwitchState(state);
    }
}
