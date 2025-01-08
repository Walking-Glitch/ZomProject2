using Pathfinding;
using UnityEngine;

public class Chasing : ZombieBaseState
{ 
    public override void EnterState(ZombieStateManager zombie)
    {
 
        zombie.SetIsAlerted(true);
        zombie.destinationSetter.enabled = true;
        zombie.destinationSetter.target = zombie.PlayerTransform;
        zombie.SetCanMove(true);

        //zombie.aiPath.maxSpeed = 0.3f;

        zombie.SetIsAlerted(true);
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        zombie.PlayZombieChasingSfx();

        if (zombie.isCrippled)
        {
            zombie.anim.SetBool("IsCrippled", true);
        }
  
        if (zombie.IsPlayerInAttackArea())
        {
            zombie.SwitchState(zombie.attack);
        }
    }

    private void ExitState(ZombieStateManager zombie, ZombieBaseState state)
    {
        zombie.destinationSetter.enabled = false;
        zombie.SwitchState(state);
    }
}
