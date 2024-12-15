using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrbanZombie_Custom_Instantiate : MonoBehaviour
{
	// Start is called before the first frame update

	private int headTyp;
	private int bodyTyp;
	private int trousersTyp;
	private int tanktopTyp;
	private int hoodieTyp;
    private int eyesTyp;


    private bool tanktopOld;
	private Transform hoodieT;
	private Transform tanktopT;
	private Transform bodyToHideT;
	private Transform bodyExposedT;

	private Transform headT_A;
	private Transform headT_B;


	private UrbanZombie_AssetsList materialsList;

	private SkinnedMeshRenderer skinnedMeshRenderer;

	public enum FaceType
	{
		V1,
		V2
	}

	public enum BodySkin
	{
		V1,
		V2,
		V3,
		V4,
		V5
	}

	public enum TrousersSkin
	{
		V1,
		V2,
		V3,
		V4

	}

	public enum TankTopSkin
	{
		None,
		V1,
		V2,
		V3,
		V4
	}
	public enum HoodieSkin
	{
		None,
		V1,
		V2,
		V3,
		V4
	}
    public enum EyesGlow
    {
        No,
        Yes
    }

    public Transform prefabObject;
	public FaceType faceType;
	public BodySkin bodyType;
	public TrousersSkin trousersType;
	public TankTopSkin tanktopType;
	public HoodieSkin hoodieType;
    public EyesGlow eyesGlow;

    void Start()
	{
		Transform pref = Instantiate(prefabObject, gameObject.transform.position, gameObject.transform.rotation);
		headTyp = (int)faceType;
		bodyTyp = (int)bodyType;
		trousersTyp = (int)trousersType;
		tanktopTyp = (int)tanktopType;
		hoodieTyp = (int)hoodieType;
        eyesTyp = (int)eyesGlow;


        pref.gameObject.GetComponent<UrbanZombie_CharacterCustomize>().charCustomize(bodyTyp, trousersTyp, tanktopTyp, hoodieTyp, headTyp, eyesTyp);
		// Update is called once per frame
		
	}
}
