using UnityEngine;

public class WeaponLaser : MonoBehaviour
{
    public Transform laserOrigin;
    public Transform laseroffTarget;
    public Transform laseronTarget;
    public float range;

    public LineRenderer laserLine;
    public Material[] LaserMaterial;
    [SerializeField]private bool laserReady;

    private Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
        laserLine.enabled = false;
    }

    public void DisableLaser()
    {
        laserReady = false;
        laserLine.enabled = false;
    }
 
    public void DisplayLaser(bool isOnTargert)
    {
        CheckAnimationProgress(); 

        if (laserReady)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, laserOrigin.position);
            laserLine.SetPosition(1, isOnTargert ? laseronTarget.position : laseroffTarget.position);
        }
    }

    private void CheckAnimationProgress()
    {
        laserReady = anim.GetLayerWeight(1) > 0.85f;
    }

    public void ChangeLazerColorInventory()
    {
        laserLine.material = LaserMaterial[1];
    }

    public void ChangeLazerColorDefault()
    {
        laserLine.material = LaserMaterial[0];
    }


}
