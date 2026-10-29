using Assets.Scripts.Game_Manager;
using TMPro;
using UnityEngine;

public class BuildCanvas : MonoBehaviour
{
    private Transform camTransform;
    public TextMeshProUGUI PrefabId;
    public TextMeshProUGUI FuelBarrels;
    public TextMeshProUGUI Generators;
    public TextMeshProUGUI SpareParts;


    private string generatorPreText;
    private string fuelBarrelPreText;
    private string sparePartsPreText;
    private string PreText;

    private GameManager gameManager;
    private void Start()
    {
        gameManager = GameManager.Instance;
        camTransform = Camera.main.transform;
        generatorPreText = "Generators" + " ";
        fuelBarrelPreText = "Fuel barrels" + " ";
        sparePartsPreText = "Spare parts" + " ";
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
                         camTransform.rotation * Vector3.up);
    }

    public void DisplayPrefabRequirments()
    {
        SpawnRequirements spawnRequirements = GetComponentInParent<SpawnRequirements>();

        if (spawnRequirements != null)
        {
            
            PrefabId.text = spawnRequirements.PrefabName;

            EconomyManager economy = gameManager.EconomyManager;
            DisplayCost(FuelBarrels, fuelBarrelPreText, spawnRequirements.FuelBarrelCost, economy.FuelBarrels);
            DisplayCost(Generators, generatorPreText, spawnRequirements.GeneratorCost, economy.Generators);
            DisplayCost(SpareParts, sparePartsPreText, spawnRequirements.SparePartsCost, economy.SpareParts);
        }
    }

    // "cost / available", with the line in red when there isn't enough
    private static void DisplayCost(TextMeshProUGUI label, string preText, int cost, int available)
    {
        if (cost == 0)
        {
            label.gameObject.SetActive(false);
            return;
        }

        label.gameObject.SetActive(true);

        string line = preText + cost + " / " + available;
        label.text = available >= cost ? line : "<color=#FF5050>" + line + "</color>";
    }
}
