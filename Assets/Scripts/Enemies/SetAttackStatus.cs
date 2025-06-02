using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class SetAttackStatus : NetworkBehaviour
{
    private ZombieStateManager _manager;
    private IAttackable _currentTarget;
    void Start()
    {
        _manager = GetComponentInParent<ZombieStateManager>();
    }

    private void Update()
    {
        if (!IsServer || _manager.isDead) return;

        if (_currentTarget == null || !_currentTarget.GetTransform().gameObject.activeInHierarchy || _currentTarget.GetHealth() <= 0)
        {
            _currentTarget = null;
            _manager.SetAttackStatus(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, _currentTarget.GetTransform().position);
        if (distance < 0.85f)
        {
            _manager.SetAttackStatus(true);
        }
        else
        {
            _manager.SetAttackStatus(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || _currentTarget != null) return;

        IAttackable attackable = other.GetComponentInParent<IAttackable>();
        if (attackable != null && attackable.GetTransform().gameObject.activeInHierarchy && attackable.GetHealth() > 0)
        {
            _currentTarget = attackable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer || _currentTarget == null) return;

        IAttackable attackable = other.GetComponentInParent<IAttackable>();
        if (attackable == _currentTarget)
        {
            _currentTarget = null;
            _manager.SetAttackStatus(false);
        }
    }


}
