using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BZ_CharInstantiate;

public class BZ_CharInstantiate : MonoBehaviour
{
	private int bodyTyp;
	private int shirtTyp;
	private int shortsTyp;
    private int eyesTyp;

    //private BZ_AssetsList materialsList;

    private SkinnedMeshRenderer skinnedMeshRenderer;

	public enum BodyType
	{
		V1,
		V2,
		V3,
		V4
	}

	public enum ShirtType
	{
		V1,
		V2,
		V3,
		V4,
		No


	}

	public enum ShortsType
	{
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
	public BodyType bodyType;
	public ShirtType shirtType;
	public ShortsType shortsType;
    public EyesGlow eyesGlow;
    void Start()
    {
		Transform pref = Instantiate(prefabObject, gameObject.transform.position, gameObject.transform.rotation);
		bodyTyp = (int)bodyType;
		shirtTyp = (int)shirtType;
		shortsTyp = (int)shortsType;
        eyesTyp = (int)eyesGlow;
        if (pref.gameObject.GetComponent<BZ_Char_Customize>() != null)
		{
			pref.gameObject.GetComponent<BZ_Char_Customize>().charCustomize(bodyTyp, shirtTyp, shortsTyp, eyesTyp);
        }
        else
        {
			pref.gameObject.GetComponent<BZ_CharCustomizeBP>().charCustomize(bodyTyp, shirtTyp, shortsTyp, eyesTyp);
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
