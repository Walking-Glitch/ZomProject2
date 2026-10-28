using Assets.Scripts.Player;
using UnityEngine;

public class SetAttackStatus : MonoBehaviour
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

        // the player keeps the original tighter reach; barriers, turrets and cabins count as soon as they touch
        if (attackable is PlayerStatus)
        {
            if (!other.CompareTag("Player")) return;

            float distance = Vector3.Distance(this.transform.position, other.transform.position);
            if (distance >= 0.8f) return;
        }

        _manager.SetAttackableInReach(attackable, true);
    }

    private void OnTriggerExit(Collider other)
    {
        IAttackable attackable = other.GetComponentInParent<IAttackable>();

        if (attackable == null) return;

        if (attackable is PlayerStatus && !other.CompareTag("Player")) return;

        _manager.SetAttackableInReach(attackable, false);
    }

}
