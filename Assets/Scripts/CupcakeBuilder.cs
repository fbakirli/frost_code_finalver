using UnityEngine;
using System.Collections;

public enum Wrapper { None, Green, Blue, Pink, Purple }
public enum Base { None, Vanilla, Strawberry, Choco }
public enum Frosting { None, Normal, Strawberry, Choco }

public class CupcakeBuilder : MonoBehaviour
{
    [Header("Sprites  (index 0 = null)")]
    public Sprite[] wrapperSprites = new Sprite[5];
    public Sprite[] baseSprites = new Sprite[4];
    public Sprite[] frostingSprites = new Sprite[4];

    [Header("Renderers")]
    public SpriteRenderer wrapperR;
    public SpriteRenderer baseR;
    public SpriteRenderer frostingR;

    [Header("Button Groups")]
    public GameObject baseButtonsGroup;
    public GameObject frostingButtonsGroup;

    private Wrapper curWrap = Wrapper.None;
    private Base curBase = Base.None;

    public CupcakeGenerator generatedCupcake;

    public AudioClip wrongChoice;
    public AudioClip correctChoice;
    public AudioClip winSound;

    public Animator animController;

    public CupcakeGenerator tele;

    private int frostingSelect = -1;

    private float waitFor = 0.3f;

    private bool enableFlag = true;
    private int step = 0;

    private float animTime = 0.55f;

    private Color[] baseColors = {Color.white,
                        new Color(0xED / 255f, 0xD3 / 255f, 0x9A / 255f, 1f), //orange
                        new Color(0xFF / 255f, 0xD2 / 255f, 0xE0 / 255f, 1f), //pink
                        new Color(0x63 / 255f, 0x43 / 255f, 0x2D / 255f, 1f)}; //black

    public GameObject baseDispenseFlow;
    private Color[] frostingColors = {Color.white,

                        new Color(0xF3 / 255f, 0xEC / 255f, 0xDE / 255f, 1f), //white
                        new Color(0xBF / 255f, 0x50 / 255f, 0xAD / 255f, 1f), //pink
                        new Color(0x2B / 255f, 0x12 / 255f, 0x00 / 255f, 1f)}; //black 

    public GameObject frostingDispenseFlow;

    /* ---- public methods called by buttons ---- */
    public void SetWrapper(int id)
    {
        if (enableFlag && step == 0)
        {
            enableFlag = false;
            step++;
            transform.localPosition = transform.localPosition + Vector3.left * 400;
        }
        else
            return;

        animController.SetTrigger("cone1");
        StartCoroutine(SetWrapperEnum(id));
    }

    private IEnumerator SetWrapperEnum(int id)
    {
        yield return new WaitForSeconds(waitFor);
        curWrap = (Wrapper)id;
        baseButtonsGroup.SetActive(id != 0);
        id--;
        if (id != generatedCupcake.wrappingIndex)
            GetComponent<AudioSource>().clip = wrongChoice;
        else
            GetComponent<AudioSource>().clip = correctChoice;

        yield return new WaitForSeconds(waitFor);
        wrapperR.sprite = wrapperSprites[id + 1];
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(waitFor);
        Vector3 from = transform.localPosition;
        Vector3 to = transform.localPosition + Vector3.right * 400;
        float time = 0f;
        while (time < animTime)
        {
            yield return new WaitForFixedUpdate();
            time += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(from, to, time / animTime);
        }
        transform.localPosition = to;
        enableFlag = true;
    }

    public void SetBase(int id)
    {
        if (enableFlag && step == 1)
        {
            enableFlag = false;
            step++;
        }
        else
            return;
        if (curWrap == Wrapper.None) return;
        baseDispenseFlow.GetComponent<Renderer>().material.color = baseColors[id];
        StartCoroutine(SetBaseEnum(id));
    }

    private IEnumerator SetBaseEnum(int id)
{
    animController.SetTrigger("cone2");

    // Hide the base sprite while dropping
    baseR.sprite = null;

    yield return new WaitForSeconds(animTime); // Wait for drop animation to finish

    curBase = (Base)id;
    baseR.sprite = baseSprites[id]; // Set sprite after drop completes

    frostingButtonsGroup.SetActive(id != 0);

    yield return new WaitForSeconds(waitFor);

    id--;
    if (id != generatedCupcake.baseIndex)
        GetComponent<AudioSource>().clip = wrongChoice;
    else
        GetComponent<AudioSource>().clip = correctChoice;

    Vector3 from = transform.localPosition;
    Vector3 to = transform.localPosition + Vector3.right * 400;
    GetComponent<AudioSource>().Play();
    yield return new WaitForSeconds(waitFor + 0.15f);

    float time = 0f;
    while (time < animTime)
    {
        yield return new WaitForFixedUpdate();
        time += Time.deltaTime;
        transform.localPosition = Vector3.Lerp(from, to, time / animTime);
    }
    transform.localPosition = to;
    enableFlag = true;
}


    public void SetFrosting(int id)
    {
        if (enableFlag)
        {
            enableFlag = false;
            step = 3;
        }
        else
            return;
        if (curBase == Base.None) return;
        frostingDispenseFlow.GetComponent<Renderer>().material.color = frostingColors[id];
        StartCoroutine(SetFrostingEnum(id));
    }

    private IEnumerator SetFrostingEnum(int id)
{
    animController.SetTrigger("cone3");

    // Hide frosting until drop animation ends
    frostingR.sprite = null;

    yield return new WaitForSeconds(animTime); // Wait for drop animation

    frostingR.sprite = frostingSprites[id]; // Set frosting sprite

    yield return new WaitForSeconds(waitFor + 0.15f);

    frostingSelect = id - 1;
    if (frostingSelect != generatedCupcake.frostingIndex)
        GetComponent<AudioSource>().clip = wrongChoice;
    else
        GetComponent<AudioSource>().clip = correctChoice;

    GetComponent<AudioSource>().Play();
    yield return new WaitForSeconds(waitFor);
    enableFlag = true;
}


    public void CheckItAll()
    {
        if (!enableFlag || step != 3)
            return;
        enableFlag = false;
        if (frostingSelect != generatedCupcake.frostingIndex
        || (int)curBase != generatedCupcake.baseIndex + 1
        || (int)curWrap != generatedCupcake.wrappingIndex + 1)
            GetComponent<AudioSource>().clip = wrongChoice;
        else
            GetComponent<AudioSource>().clip = winSound;

        GetComponent<AudioSource>().Play();

        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(waitFor + 0.2f);

        wrapperR.sprite = wrapperSprites[0];
        baseR.sprite = baseSprites[0];
        frostingR.sprite = frostingSprites[0];

        tele.GenerateRandomCupcake();
        enableFlag = true;
        step = 0;
        transform.localPosition = transform.localPosition + Vector3.left * 400;
    }
}