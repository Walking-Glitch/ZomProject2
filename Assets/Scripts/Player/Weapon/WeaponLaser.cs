using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections;

public class WeaponLaser : MonoBehaviour
{
    public Transform laserOrigin;
    public Transform laserTarget;
    public float range;

    public LineRenderer laserLine;
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
 
    public void DisplayLaser()
    {
        CheckAnimationProgress(); 

        if (laserReady)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, laserOrigin.position);
            laserLine.SetPosition(1, laserTarget.position);
        }
    }

    private void CheckAnimationProgress()
    {
        laserReady = anim.GetLayerWeight(1) > 0.65f;
    }

   
}
