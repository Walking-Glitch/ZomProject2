using Assets.Scripts.Player;
using UnityEngine;

public class SetDamageArea : MonoBehaviour
{
    private ZombieStateManager _manager;
    void Awake()
    {
        _manager = GetComponentInParent<ZombieStateManager>();

        Targeting.EnsureTriggerBody(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        IAttackable attackable = other.GetComponentInParent<IAttackable>();

        if (attackable == null) return;

        // only the player's tagged collider counts, as before
        if (attackable is PlayerStatus && !other.CompareTag("Player")) return;

        _manager.SetAttackableInDamageArea(attackable, true);
    }


    private void OnTriggerExit(Collider other)
    {
        IAttackable attackable = other.GetComponentInParent<IAttackable>();

        if (attackable == null) return;

        if (attackable is PlayerStatus && !other.CompareTag("Player")) return;

        _manager.SetAttackableInDamageArea(attackable, false);
    }
}
