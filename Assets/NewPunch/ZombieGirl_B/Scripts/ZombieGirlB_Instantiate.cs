using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieGirlB_Instantiate : MonoBehaviour
{
    public Transform prefabObject;
    // Start is called before the first frame update

    private int bodyTyp;
    private int hairTyp;
    private int eyesTyp;



    public enum BodyType
    {
        V1,
        V2,
        V3,
        V4
    }

    public enum HairType
    {
        V1,
        V2
    }

    public enum EyesGlow
    {
        No,
        Yes
    }


    public BodyType bodyType;
    public HairType hairType;
    public EyesGlow eyesGlow;

    void Start()
    {
        Transform pref = Instantiate(prefabObject, gameObject.transform.position, gameObject.transform.rotation);
        bodyTyp = (int)bodyType;
        hairTyp = (int)hairType;
        eyesTyp = (int)eyesGlow;


        pref.gameObject.GetComponent<ZombieGirlB_Customization>().charCustomize(bodyTyp, hairTyp, eyesTyp);


    }

}