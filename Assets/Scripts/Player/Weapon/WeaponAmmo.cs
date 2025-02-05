using Assets.Scripts.Player.Weapon;
using Unity.Netcode;
using UnityEngine;

public class WeaponAmmo : NetworkBehaviour
{
    public int clipSize;
    public int extraAmmo;
    public int currentAmmo;

    public AudioClip magInClip;
    public AudioClip magOutClip;
    public AudioClip releaseSlideClip;
    public AudioClip switchFireMode;

    private WeaponManager weaponManager;


    void Start()
    {
        currentAmmo = clipSize;
        weaponManager = GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Reload()
    {
        if (extraAmmo >= clipSize)
        {
            int ammoToReload = clipSize - currentAmmo;
            extraAmmo -= ammoToReload;
            currentAmmo += ammoToReload;
        }
        else if (extraAmmo > 0)
        {
            if (extraAmmo + currentAmmo > clipSize)
            {
                int leftOverAmmo = extraAmmo + currentAmmo - clipSize;
                extraAmmo = leftOverAmmo;
                currentAmmo = clipSize;
            }
            else
            {
                currentAmmo += extraAmmo;
                extraAmmo = 0;
            }
        }

        //weaponManager.RefreshDisplay(currentAmmo, extraAmmo);

    }



}
