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
            if (spawnRequirements.FuelBarrelCost == 0) 
            {
                FuelBarrels.gameObject.SetActive(false);
            }
            else
            {
                FuelBarrels.gameObject.SetActive(true);
                FuelBarrels.text = fuelBarrelPreText + spawnRequirements.FuelBarrelCost + " / " + gameManager.EconomyManager.FuelBarrels;
            }

            if(spawnRequirements.GeneratorCost == 0)
            {
                Generators.gameObject.SetActive (false);
            }
            else
            {
                Generators.gameObject.SetActive(true);
                Generators.text = generatorPreText + spawnRequirements.GeneratorCost + " / " + gameManager.EconomyManager.Generators;
            }

            if (spawnRequirements.SparePartsCost == 0)
            {
                SpareParts.gameObject.SetActive(false);
            }
            else
            {
                SpareParts.gameObject.SetActive(true);
                SpareParts.text = sparePartsPreText + spawnRequirements.SparePartsCost + " / " + gameManager.EconomyManager.SpareParts;
            }

        }
    }
}
