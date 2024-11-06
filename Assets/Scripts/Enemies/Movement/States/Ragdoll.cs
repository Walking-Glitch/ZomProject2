using UnityEngine;

public class Ragdoll : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {
        zombie.aiPath.canMove = false;
        zombie.RagdollModeOn();
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
      
    }
}
