using UnityEngine;

public class Roaming : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {
        zombie.patrol.enabled = true; 
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        if (zombie.currentSpeed < 0.1f && !zombie.IsPlayerInDetectionArea())
        {
            zombie.SwitchState(zombie.idle);
        }

        if (zombie.IsPlayerInDetectionArea())
        {
            ExitState(zombie, zombie.chasing);
        }
    }

    private void ExitState(ZombieStateManager zombie, ZombieBaseState state)
    {
        zombie.patrol.enabled = false;


        zombie.SwitchState(state);
    }
}
