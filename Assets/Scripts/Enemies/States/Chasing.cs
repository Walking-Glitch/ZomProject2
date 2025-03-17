using Pathfinding;
using UnityEngine;

public class Chasing : ZombieBaseState
{
    float cooldown = 0.5f;
    float elapsed;
    public override void EnterState(ZombieStateManager zombie)
    {
        elapsed = 0;
        zombie.SetIsAlerted(true);
        zombie.destinationSetter.enabled = true;
        zombie.UpdateTarget();
        zombie.SetCanMove(true);
         
        zombie.SetIsAlerted(true);
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        zombie.PlayZombieChasingSfx();

        if (zombie.isCrippled)
        {
            zombie.PlayZombieAnimationBoolClientRpc("IsCrippled", zombie.isCrippled);
            //zombie.anim.SetBool("IsCrippled", true);
        }
  
        if (zombie.IsPlayerInAttackArea() && !zombie.isDead)
        {
            zombie.SwitchState(zombie.attack);
        }

        if(elapsed <= cooldown)
        {
            elapsed += Time.deltaTime;
        }
        else
        {
            zombie.UpdateTarget();
            elapsed = 0;
        }
    }

    private void ExitState(ZombieStateManager zombie, ZombieBaseState state)
    {
        zombie.destinationSetter.enabled = false;
        zombie.SwitchState(state);
    }
}
