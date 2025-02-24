using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurretAntiTank : TurretBase
{
    [SerializeField] protected List<ZombieStateManager> enemiesInBlast = new List<ZombieStateManager>();
    [SerializeField] protected float blastRadius;

    [SerializeField] GameObject missilePrefab;
    

    [SerializeField] int index = 0;
     
    public Transform CurrentMissileTarget;

    public bool MissileTraveling;

    public Missile missileReference;


    // network variables 
    public NetworkVariable<Vector3> missilePosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<Vector3> explosionPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> missileActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> missileBodyActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> missileExploded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    protected override void Start()
    {
        base.Start();

        //missileReference = GetComponentInChildren<Missile>();
        
        if(missileReference == null)
        {
            Debug.Log("this is null");
        }

       

    }
    protected override void Update()
    {
        base.Update();
 
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        missilePosition.OnValueChanged += (prev, curr) =>
        {
           // Debug.Log($"Missile transform changed: {prev} ? {curr}");
            missileReference.gameObject.transform.position = curr; 
        };

        explosionPosition.OnValueChanged += (prev, curr) =>
        {

            if (missileReference.exploded)
            {
                missileReference.explosionPosition = missilePosition.Value;
                //Debug.Log("Missile Explosion Position Set: " + explosionPosition);
            }
        };

        missileActive.OnValueChanged += (prev, curr) =>
        {
            missileReference.gameObject.SetActive(curr);
        };

        missileBodyActive.OnValueChanged += (prev, curr) =>
        {
            missileReference.MissileBody.SetActive(curr);
        };

        missileExploded.OnValueChanged += (prev, curr) =>
        {
            //Debug.Log($"Missile exploded changed: {prev} ? {curr}");
            missileReference.exploded = curr;
        };

    }

    [ClientRpc]
    public void ReparentMissileClientRpc()
    {
        missileReference.ReparentMissile();

    }

    [ClientRpc]
    public void PlayExplosionVfxClientRpc()
    {
        missileReference.PlayExplosionVfx();
    }


    [ClientRpc]
    public void PlayExplosionSfxClientRpc()
    {
        missileReference.PlayExplosionSfx();
    }
    protected override void AimAtTarget()
    {
        if (!IsServer) return;       
  

        if (enemies.Count > 0)
        {
             
            if (currentEnemy == null || !enemies.Contains(currentEnemy))
            {
                 return;
            } 
            

            Vector3 direction = (currentEnemy.transform.position - transform.position).normalized;

            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);

            Vector3 verticalDirection = new Vector3(0, direction.y, 0);


            if (horizontalDirection != Vector3.zero)
            {
                Quaternion horizontalRotation = Quaternion.LookRotation(horizontalDirection);
                PanTransform.rotation = Quaternion.Slerp(PanTransform.rotation, horizontalRotation, Time.deltaTime * AimingSpeed);

                // Sync with clients
                panRotation.Value = PanTransform.rotation.eulerAngles;
            }

            if (PitchTransform != null)
            {
                Vector3 targetPosition = currentEnemy.transform.parent.position + Vector3.up * 1.1f;
                Vector3 barrelDirection = (targetPosition - PitchTransform.position).normalized;

                Quaternion verticalRotation = Quaternion.LookRotation(barrelDirection);
                PitchTransform.rotation = Quaternion.Slerp(PitchTransform.rotation, verticalRotation, Time.deltaTime * AimingSpeed);

                // Lock the barrel's horizontal rotation to match the turret base
                PitchTransform.rotation = Quaternion.Euler(PitchTransform.rotation.eulerAngles.x, PanTransform.rotation.eulerAngles.y, 0);

                // Sync with clients
                pitchRotation.Value = PitchTransform.rotation.eulerAngles;
            }


            // Check alignment for firing
            float horizontalAlignment = Vector3.Dot(PanTransform.forward, horizontalDirection);
            float verticalAlignment = Vector3.Dot(PitchTransform.up, verticalDirection);

           
            
                if (horizontalAlignment >= 0.98f && CanFire())
                {
                     
                    CurrentMissileTarget = currentEnemy.transform;
                    Fire(); 
                    //Debug.Log("FIRING");                  
                }
                else
                {
                   // Debug.Log("NOT ALIGNED");
                    //Debug.Log(horizontalAlignment + " " + verticalAlignment);
       
                }
            
          
        }
        else
        {
            currentEnemy = null;
        }
    }

    protected override bool CanFire()
    {
        fireRateTimer += Time.deltaTime;
        if (fireRateTimer < WeaponFireRate) return false;
        if (MissileTraveling) return false;
        if (currentEnemy != null) return true;

        return false;
    }

    protected override void FindEnemiesInRange()
    {
        if (!IsServer) return;

        // Check if currentEnemy is still valid before clearing the list
        if (currentEnemy != null)
        {
            float currentEnemyDistance = Vector3.Distance(transform.position, currentEnemy.transform.position);
            if (currentEnemyDistance >= MinWeaponRange && currentEnemyDistance <= MaxWeaponRange && currentEnemy.health > 0)
            {
                // If currentEnemy is still valid, return early to avoid unnecessary reassignment
                return;
            }
        }



        enemies.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, MaxWeaponRange, ZombieLayer);

        foreach (Collider col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance >= MinWeaponRange && distance <= MaxWeaponRange)
            {
                ZombieStateManager zombie = col.gameObject.GetComponentInParent<ZombieStateManager>();

                if (zombie != null)
                {
                    if (col.gameObject.GetComponentInParent<ZombieStateManager>().health > 0 && !enemies.Contains(zombie))
                        enemies.Add(col.GetComponentInParent<ZombieStateManager>());
                }
            }

        }
        if (enemies.Count > 0)
        {
            int i = 0;
            while (i < enemies.Count && !EnoughEnemiesInBlastRange(enemies[i]))
            {
                i++;
            }

            if (i < enemies.Count)
            {
                currentEnemy = enemies[i];

                // Sync target to clients
                if (currentEnemy != null)
                {
                    targetEnemyId.Value = currentEnemy.GetComponentInParent<NetworkObject>().NetworkObjectId;
                }
            }

        }
        else
        {
            currentEnemy = null;
            targetEnemyId.Value = 0;
        }
    }

    
    protected virtual bool EnoughEnemiesInBlastRange(ZombieStateManager tempEnemy)
    { 
        if(tempEnemy != null)
        {
            enemiesInBlast.Clear();

            Collider[] colliders = Physics.OverlapSphere(tempEnemy.transform.position, blastRadius, ZombieLayer);

            foreach (Collider col in colliders)
            {
                ZombieStateManager zombie = col.gameObject.GetComponentInParent<ZombieStateManager>();

                if (zombie != null)
                {
                    if (col.gameObject.GetComponentInParent<ZombieStateManager>().health > 0 && !enemiesInBlast.Contains(zombie))
                        enemiesInBlast.Add(col.GetComponentInParent<ZombieStateManager>());
                }
            }

            if (enemiesInBlast.Count > 3) return true;
            else return false;
        }
        return false;

    }

    protected void Fire()
    {

        fireRateTimer = 0;
        MissileTraveling = true;

        //Missile missile = missilePrefab.GetComponent<Missile>();
        //missile.gameObject.SetActive(true);
        //missile.MissileBody.SetActive(true);
        missileActive.Value = true;
        missileBodyActive.Value = true;
    }

     
}