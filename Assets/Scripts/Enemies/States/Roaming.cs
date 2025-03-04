using UnityEngine;

public class Roaming : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {

        zombie.RagdollModeOffServerRpc();
        zombie.aiPath.canMove = true;
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

        if (zombie.isCrippled)
        {
            zombie.anim.SetBool("IsCrippled", true);
        }
    }

    private void ExitState(ZombieStateManager zombie, ZombieBaseState state)
    {
        zombie.patrol.enabled = false;
        zombie.SwitchState(state);
    }
}
