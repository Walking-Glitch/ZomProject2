using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TurretAntiTank : TurretBase
{
    [Header("Anti-Tank")]
    [Tooltip("Zombies around a candidate target from the last cluster check (runtime).")]
    [SerializeField] protected List<ZombieStateManager> enemiesInBlast = new List<ZombieStateManager>();
    [Tooltip("Radius (m) used to decide whether a group is worth a missile. Match it to the missile's Explosion Radius.")]
    [SerializeField] protected float blastRadius;
    [Tooltip("Minimum zombies within Blast Radius of a target before a missile is fired.")]
    [SerializeField] protected int minZombiesInBlast = 4;

    [SerializeField] GameObject missilePrefab;
    

    [SerializeField] int index = 0;

    [Tooltip("True while a missile is in the air; the turret can't fire again until it explodes (runtime).")]
    public bool MissileTraveling;

    private static readonly Collider[] blastScanBuffer = new Collider[256];
 

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
             
            // the cluster search runs on the 0.2s scan; just drop a target that died in between
            if (!IsValidTarget(currentEnemy))
            {
                currentEnemy = null;
                return;
            }


            if (TrackTarget(currentEnemy) && CanFire())
            {
                Fire();
            }
        }
        else
        {
            currentEnemy = null;
        }
    }

    // the missile flies over cover, so instead of the base line-of-sight pick it looks for a cluster;
    // runs on the 0.2s scan rather than every frame since it's one overlap per zombie in range
    protected override void SelectTarget()
    {
        if (IsValidTarget(currentEnemy) && enemies.Contains(currentEnemy)) return;

        currentEnemy = null;
        FindCurrentEnemy();
    }

    protected override bool CanFire()
    {
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
        if (!IsValidTarget(tempEnemy)) return false;

        enemiesInBlast.Clear();

        int count = Physics.OverlapSphereNonAlloc(tempEnemy.transform.position, blastRadius, blastScanBuffer, ZombieLayer);

        for (int i = 0; i < count; i++)
        {
            ZombieStateManager zombie = blastScanBuffer[i].GetComponentInParent<ZombieStateManager>();

            if (IsValidTarget(zombie) && !enemiesInBlast.Contains(zombie))
            {
                enemiesInBlast.Add(zombie);

                // enough found, no need to count the rest of the crowd
                if (enemiesInBlast.Count >= minZombiesInBlast) return true;
            }
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
        missile.Launch(currentEnemy);
    }

     
}