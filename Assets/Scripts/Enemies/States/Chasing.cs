using Pathfinding;
using UnityEngine;

public class Chasing : ZombieBaseState
{
    private float elapsed;
    private float timeToChange = 2f;
    public override void EnterState(ZombieStateManager zombie)
    {
        elapsed = 0;
        zombie.destinationSetter.enabled = true;
        zombie.destinationSetter.target = zombie.PlayerTransform;
        zombie.aiPath.canMove = true;

        zombie.aiPath.maxSpeed = 0.3f;

        zombie.SetIsAlerted(true);
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        if (zombie.isCrippled)
        {
            zombie.anim.SetBool("IsCrippled", true);
        }

        //if (!zombie.IsPlayerInDetectionArea())
        //{
        //    //Debug.Log(elapsed);
        //    elapsed += Time.deltaTime;

        //    if (elapsed >= timeToChange)
        //    {
        //        ExitState(zombie, zombie.roaming);
        //    }
        //}
        //else
        //{
        //    elapsed = 0; 
        //}

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
