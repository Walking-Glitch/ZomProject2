using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TurretAntiTank : TurretBase
{
    [SerializeField] protected List<ZombieStateManager> enemiesInBlast = new List<ZombieStateManager>();
    [SerializeField] protected float blastRadius;

    [SerializeField] GameObject missilePrefab;
    

    [SerializeField] int index = 0;
     
    public Transform CurrentMissileTarget;

    public bool MissileTraveling;
 

    protected override void Start()
    {
        base.Start();
        
    }
    protected override void Update()
    {
        base.Update();
 
    }
    protected override void AimAtTarget()
    {

        if (enemies.Count > 0)
        {
             
            if (currentEnemy == null || !enemies.Contains(currentEnemy))
            {
                FindCurrentEnemy();
                if (currentEnemy == null) return;
            } 
            

            Vector3 direction = (currentEnemy.transform.position - transform.position).normalized;

            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);

            Vector3 verticalDirection = new Vector3(0, direction.y, 0);


            if (horizontalDirection != Vector3.zero)
            {
                Quaternion horizontalRotation = Quaternion.LookRotation(horizontalDirection);
                PanTransform.rotation = Quaternion.Slerp(PanTransform.rotation, horizontalRotation, Time.deltaTime * AimingSpeed);
            }

            if (PitchTransform != null)
            {
                Vector3 targetPosition = currentEnemy.transform.parent.position + Vector3.up * 1.1f;
                Vector3 barrelDirection = (targetPosition - PitchTransform.position).normalized;

                Quaternion verticalRotation = Quaternion.LookRotation(barrelDirection);
                PitchTransform.rotation = Quaternion.Slerp(PitchTransform.rotation, verticalRotation, Time.deltaTime * AimingSpeed);

                // Lock the barrel's horizontal rotation to match the turret base
                PitchTransform.rotation = Quaternion.Euler(PitchTransform.rotation.eulerAngles.x, PanTransform.rotation.eulerAngles.y, 0);
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
    private void FindCurrentEnemy()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (EnoughEnemiesInBlastRange(enemies[i]))
            {
                currentEnemy = enemies[i];
                break;
            }
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

            if (enemiesInBlast.Count > 2) return true;
            else return false;
        }
        return false;

    }

    protected void Fire()
    {

        fireRateTimer = 0;
        MissileTraveling = true;

        Missile missile = missilePrefab.GetComponent<Missile>();        
        missile.gameObject.SetActive(true);
        missile.MissileBody.SetActive(true);
    }

     
}