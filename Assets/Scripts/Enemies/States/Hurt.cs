using UnityEngine;

public class Hurt : ZombieBaseState
{
    private const string HitState = "ZombieHit1";

    private float elapsed;

    public override void EnterState(ZombieStateManager zombie)
    {
        elapsed = 0f;

        zombie.SetIsAlerted(true);
        zombie.SetCanMove(false);
        zombie.anim.SetTrigger("IsHit");

        zombie.zombieAudioSource.Stop();
        zombie.PlayZombieHurtSfx();
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        elapsed += Time.deltaTime;

        if (zombie.isCrippled)
        {
            // crawlers have no hit animation, the trigger would stay armed and fire later
            zombie.anim.ResetTrigger("IsHit");
            zombie.SwitchState(zombie.chasing);
        }
        else if (HitAnimationFinished(zombie.anim))
        {
            zombie.SwitchState(zombie.chasing);
        }
        else if (elapsed >= zombie.MaxStaggerTime)
        {
            // safety net in case the animator never plays the hit (e.g. stuck behind another transition)
            zombie.anim.ResetTrigger("IsHit");
            zombie.SwitchState(zombie.chasing);
        }
    }

    // only release the zombie once the hit it entered this state for has played out; reading just the
    // current state let it walk off while a new hit was still blending in
    private static bool HitAnimationFinished(Animator anim)
    {
        // the animator hasn't taken the trigger yet
        if (anim.GetBool("IsHit")) return false;

        if (anim.IsInTransition(0))
        {
            // blending into a hit (first hit or a restart): still staggered; blending out of one: done
            return !anim.GetNextAnimatorStateInfo(0).IsName(HitState);
        }

        AnimatorStateInfo current = anim.GetCurrentAnimatorStateInfo(0);

        return !current.IsName(HitState) || current.normalizedTime >= 0.95f;
    }
}
