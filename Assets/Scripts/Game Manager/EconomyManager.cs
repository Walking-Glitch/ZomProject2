using Assets.Scripts.Game_Manager;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    // resources
    public float Fuel;
    public int Generators;
    public int Ammo; 
    public int CurrentMoney;
    public int FuelBarrels;
    public int SpareParts;

    // lists of turrets 
    public List<TurretBase> VulcanTurretList;
    public List<TurretShotgun> ShotgunTurretList;
    public List<TurretSniper> SniperTurretList;
    public List<TurretAntiTank> AntiTankTurretList;

    //lists of lights
    public List<GameObject> spotLightsList;

    //Consumtion rate
    private int turretsRate;
    private int lightsRate; 

    // lists of ligts 
    private GameManager gameManager;
    void Start()
    {
        gameManager = GameManager.Instance;

        turretsRate = 5;
        lightsRate = 1;
    }

    private void Update()
    {
        SpendFuelFromPooledResources();
    }

    public void CollectMoney(int money)
    {
        CurrentMoney += money;
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
