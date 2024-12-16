using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantZombie_Char_BodyParts_Instantiate : MonoBehaviour
{
    // Start is called before the first frame update
    private int headTyp;
    private int bodyTyp;
    private int trousersTyp;
    private int tanktopTyp;
    private int tshirtTyp;
    private int sneakersTyp;
    private int legsTyp;
    private int eyesTyp;




    public enum FaceType
    {
        FaceV1,
        FaceV2,
        FaceV3,
        FaceV4
    }

    public enum BodySkin
    {
        BodyV1,
        BodyV2,
        BodyV3,
        BodyV4
    }

    public enum LegsSkin
    {
        LegsV1,
        LegsV2,
        LegsV3,
        LegsV4
    }

    public enum TrousersSkin
    {
        TrousersV1,
        TrousersV2,
        TrousersV3,
        TrousersV4

    }

    public enum SneakersSkin
    {
        SneakersV1,
        SneakersV2,
        SneakersV3,
        SneakersV4

    }

    public enum TankTopSkin
    {
        None,
        TankTopV1,
        TankTopV2,
        TankTopV3,
        TankTopV4
    }
    public enum TshirtSkin
    {
        None,
        TshirtV1,
        TshirtV2,
        TshirtV3,
        TshirtV4
    }

    public enum EyesGlow
    {
        No,
        Yes
    }




    public Transform prefabObject;
    public FaceType faceType;
    public BodySkin bodySkin;
    public LegsSkin legsSkin;
    public TrousersSkin trousersSkin;
    public SneakersSkin sneakersSkin;
    public TankTopSkin tanktopSkin;
    public TshirtSkin tshirtSkin;
    public bool trousersVisible = true;
    public bool sneakerLeftVisible = true;
    public bool sneakerRightVisible = true;
    public EyesGlow eyesGlow;
    void Start()
    {
        Transform pref = Instantiate(prefabObject, gameObject.transform.position, gameObject.transform.rotation);

        headTyp = (int)faceType;
        bodyTyp = (int)bodySkin;
        trousersTyp = (int)trousersSkin;
        tanktopTyp = (int)tanktopSkin;
        tshirtTyp = (int)tshirtSkin;
        sneakersTyp = (int)sneakersSkin;
        legsTyp = (int)legsSkin;
        eyesTyp = (int)eyesGlow;


        pref.gameObject.GetComponent<GiantZombie_CharacterCustomize_BodyParts>().trousersVisible = trousersVisible;
		pref.gameObject.GetComponent<GiantZombie_CharacterCustomize_BodyParts>().sneakerLeftVisible = sneakerLeftVisible;
		pref.gameObject.GetComponent<GiantZombie_CharacterCustomize_BodyParts>().sneakerRightVisible = sneakerRightVisible;
		pref.gameObject.GetComponent<GiantZombie_CharacterCustomize_BodyParts>().charCustomize(bodyTyp, trousersTyp, tanktopTyp, tshirtTyp, headTyp, trousersVisible, sneakersTyp, legsTyp, eyesTyp);




	}


	// Update is called once per frame
	void Update()
    {
        
    }
}
