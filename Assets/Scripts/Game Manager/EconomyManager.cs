using Assets.Scripts.Game_Manager;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    // resources
    [Header("Resources")]
    [Tooltip("Shared fuel. Turrets and spotlights burn it every second; at 0 turrets stop aiming and firing.")]
    public float Fuel;
    public int Generators;
    [Tooltip("Shared turret ammo. Each turret shot spends 1 (shotgun spends 8); at 0 turrets stop firing.")]
    public int Ammo;
    [Tooltip("Player money, earned from zombie kills.")]
    public int CurrentMoney;
    [Tooltip("All money earned this run, before death penalties (runtime). Shown on the game over screen.")]
    public int TotalMoneyEarned;
    [Tooltip("Fuel barrels carried by the truck.")]
    public int FuelBarrels;
    [Tooltip("Spare parts carried by the truck.")]
    public int SpareParts;

    //Consumtion rate
    [Header("Fuel Consumption (per second)")]
    [Tooltip("Fuel burned per second by each Vulcan, Shotgun and Sniper turret (the AntiTank burns none).")]
    [SerializeField] private int turretsRate = 5;
    [Tooltip("Fuel burned per second by each spotlight.")]
    [SerializeField] private int lightsRate = 1;

    // lists of turrets 
    public List<TurretBase> VulcanTurretList;
    public List<TurretShotgun> ShotgunTurretList;
    public List<TurretSniper> SniperTurretList;
    public List<TurretAntiTank> AntiTankTurretList;

    //lists of lights
    public List<GameObject> spotLightsList;

    private GameManager gameManager;
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        SpendFuelFromPooledResources();
    }

    public void CollectMoney(int money)
    {
        CurrentMoney += money;
        TotalMoneyEarned += money;
        gameManager.UIManager.UpdateMoneyUI(CurrentMoney);
    }

    public void LoseMoney(int money)
    {
        CurrentMoney = Mathf.Max(0, CurrentMoney - money);
        gameManager.UIManager.UpdateMoneyUI(CurrentMoney);
    }

    public void AddTurretToEconomyManager(GameObject turret)
    {
        if (turret == null)
        {
            Debug.LogWarning("Turret is null. Cannot add to EconomyManager.");
            return;
        }

        if(turret.TryGetComponent<TurretAntiTank>(out TurretAntiTank antiTank))
        {
            AntiTankTurretList.Add(antiTank); 
        }
        else if (turret.TryGetComponent<TurretShotgun>(out TurretShotgun turretShotgun))
            {
                ShotgunTurretList.Add(turretShotgun);
            }
        else if (turret.TryGetComponent<TurretSniper>(out TurretSniper turretSniper))
        {
            SniperTurretList.Add(turretSniper);
        }
        else if (turret.TryGetComponent<TurretBase>(out TurretBase turretBase))
        {
            VulcanTurretList.Add(turretBase);
        }
    }

    public void RemoveTurret(TurretBase turret)
    {
        // mirrors AddTurretToEconomyManager: each turret lives in exactly one list
        if (turret is TurretAntiTank antiTank)
        {
            AntiTankTurretList.Remove(antiTank);
        }
        else if (turret is TurretShotgun turretShotgun)
        {
            ShotgunTurretList.Remove(turretShotgun);
        }
        else if (turret is TurretSniper turretSniper)
        {
            SniperTurretList.Remove(turretSniper);
        }
        else
        {
            VulcanTurretList.Remove(turret);
        }
    }
    
    public void SpendFuelFromPooledResources()
    {
        if (Fuel > 0)
        {
            float fuelToSubtract = 0;

            if (VulcanTurretList != null)
            {
                fuelToSubtract += turretsRate * VulcanTurretList.Count;
            }
            if (SniperTurretList != null)
            {
                fuelToSubtract += turretsRate * SniperTurretList.Count;
            }
            if (ShotgunTurretList != null)
            {
                fuelToSubtract += turretsRate * ShotgunTurretList.Count;
            }
            if(spotLightsList != null)
            {
                fuelToSubtract += lightsRate * spotLightsList.Count;
            }

            Fuel -= fuelToSubtract * Time.deltaTime;
            Fuel = Mathf.Clamp(Fuel, 0, Fuel);
            CheckEnoughFuel();
        }
    }

    public bool CheckEnoughFuel()
    {
        if (Fuel > 0) return true;
        return false;
    }
    public void SpendAmmoFromPooledResources(int shots)
    {
        Ammo -= shots;
        Ammo = Mathf.Clamp(Ammo, 0, Ammo);
        CheckEnoughAmmo();
    }

    public void SpendFuelBarrelsFromPooledResources(int barrel)
    {
        FuelBarrels -= barrel;
        FuelBarrels = Mathf.Clamp(FuelBarrels, 0, FuelBarrels);
        CheckEnoughAmmo();
    }

    public bool CheckEnoughAmmo()
    {
        if (Ammo > 0) return true;
        return false;
    }


}
