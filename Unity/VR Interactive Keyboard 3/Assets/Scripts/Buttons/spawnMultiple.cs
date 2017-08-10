using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Leap.Unity.Interaction;

public class spawnMultiple : MonoBehaviour
{

    public GameObject prefab;                         // Defualt prefab
    public GameObject positionRelativeObject;         // Spawn grid relative to this object
    private InteractionButton interactButtonScript;   // Access interaction button methods and variables 
    private buttonPress buttonPressScript;            // Access buttonPress script methods and variables

    public bool EnableDifferentPrefabs;

    ////////////////////////////////////////////// SHAPE VARIABLES FOR UNITY INSPECTOR //////////////////////////////////////////////
    [HeaderAttribute("Spawn Shape Options (Select one only)")]

    // Circle spawn shape
    public bool EnableCircle = false;
    [ConditionalHide("EnableCircle", true)]
    public int numberOfObjects = 10;
    [ConditionalHide("EnableCircle", true)]
    public float radius = 1;

    // Choose different prefabs and scales
    [Tooltip("For 'Size' copy the value in 'Number of Objects'")]
    public GameObject[] prefabsC = new GameObject[10];
    public float[] scaleC = new float[10];

    private GameObject[] circleArr;

    // spawn 2D Grid
    public bool Enable2DGrid = false;
    [ConditionalHide("Enable2DGrid", true)]
    public float rowAmount2D = 2;
    [ConditionalHide("Enable2DGrid", true)]
    public float columnAmount2D = 2;
    [ConditionalHide("Enable2DGrid", true)]
    public float spacing2D = 1;

    // Choose different prefabs and scales
    [Tooltip("For 'Size' enter the result of multiplying the values in 'rowAmount2D' and 'columnAmount2D'")]
    public GameObject[] prefabs2G = new GameObject[4];
    public float[] scale2G = new float[4];

    private GameObject[,] grid2DArr;

    // spawn 3D Grid
    public bool Enable3DGrid = false;
    [ConditionalHide("Enable3DGrid", true)]
    public float rowAmount3D = 2;
    [ConditionalHide("Enable3DGrid", true)]
    public float columnAmount3D = 2;
    [ConditionalHide("Enable3DGrid", true)]
    public float volumeAmount3D = 2;
    [ConditionalHide("Enable3DGrid", true)]
    public float spacing3D = 1;

    // Choose different prefabs and scales
    [Tooltip("For 'Size' enter the result of multiplying the values in 'rowAmount3D','columnAmount3D', and 'volumeAmount3D'")]
    public GameObject[] prefabs3G = new GameObject[8];
    public float[] scale3G = new float[8];

    private GameObject[,,] grid3DArr;

    // Triangle spawn shape
    public bool EnableTriangle = false;
    [ConditionalHide("EnableTriangle", true)]
    public float rowAmountTriangle = 3;
    [ConditionalHide("EnableTriangle", true)]
    public float spacingTriangle = 1;

    // Choose different prefabs and scales
    [Tooltip("For 'Size' copy the value in 'rowAmountTriangle' multiplied by itself")]
    public GameObject[] prefabsT = new GameObject[9];
    public float[] scaleT = new float[9];

    private GameObject[,] triangleRArr;
    private GameObject[,] triangleLArr;

    // Use this for initialization
    void Start()
    {

        if (positionRelativeObject == null)
        {

            positionRelativeObject = gameObject;

        }

        // Init circle array defaults
        if (EnableCircle == true)
        {

            if (EnableDifferentPrefabs == true)
            {

                for (int i = 0; i < prefabsC.Length; i++)
                {

                    if(prefabsC[i] == null)
                    {

                        prefabsC[i] = prefab;
                        scaleC[i] = 0;

                    }

                }


            }

            initCircle();

        }

        // Init 2DGrid array defaults
        if (Enable2DGrid == true)
        {

            if (EnableDifferentPrefabs == true)
            {

                for (int i = 0; i < prefabs2G.Length; i++)
                {

                    if (prefabs2G[i] == null)
                    {

                        prefabs2G[i] = prefab;
                        scale2G[i] = 0;

                    }

                }


            }

            init2DGrid();

        }

        // Init 3DGrid array defaults
        if (Enable3DGrid == true)
        {

            if (EnableDifferentPrefabs == true)
            {

                for (int i = 0; i < prefabs3G.Length; i++)
                {

                    if (prefabs3G[i] == null)
                    {

                        prefabs3G[i] = prefab;
                        scale3G[i] = 0;

                    }

                }


            }

            init3DGrid();

        }

        // Init triangle array defaults
        if (EnableTriangle == true)
        {

            if (EnableDifferentPrefabs == true)
            {

                for (int i = 0; i < prefabsT.Length; i++)
                {

                    if (prefabsT[i] == null)
                    {

                        prefabsT[i] = prefab;
                        scaleT[i] = 0;

                    }

                }


            }

            initTriangle();

        }


    }

    // Update is called once per frame
    public void Update()
    {


    }

    ////////////////////////////////////// CIRCLE //////////////////////////////////////
    private void initCircle()
    {

        // Init Array
        circleArr = new GameObject[numberOfObjects];

        // Init Shape
        for (int i = 0; i < numberOfObjects; i++)
        {

            // Math for circle shape
            float angle = i * Mathf.PI * 2 / numberOfObjects;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius + positionRelativeObject.transform.position;

            if (EnableDifferentPrefabs == false)
            {

                circleArr[i] = Instantiate(prefab, pos, prefab.transform.localRotation);

                // Init Default Prefab
                if (prefab.name == "button_PF")
                {

                    interactButtonScript = circleArr[i].GetComponentInChildren<InteractionButton>();
                    buttonPressScript = circleArr[i].GetComponentInChildren<buttonPress>();

                    interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                    interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                }

            } else {

                circleArr[i] = Instantiate(prefabsC[i], pos, prefabsC[i].transform.localRotation);

                if (scaleC[i] != 0)
                {

                    circleArr[i].transform.localScale = new Vector3(scaleC[i], scaleC[i], scaleC[i]);

                }

                //Init Prefab
                if (prefabsC[i].name == "button_PF")
                {

                    interactButtonScript = circleArr[i].GetComponentInChildren<InteractionButton>();
                    buttonPressScript = circleArr[i].GetComponentInChildren<buttonPress>();

                    interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                    interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                }

            }

        }


    }

    ////////////////////////////////////// 2D GRID //////////////////////////////////////
    private void init2DGrid()
    {

        int totalCount = 0;

        // Init Array
        grid2DArr = new GameObject[(int)rowAmount2D, (int)columnAmount2D];

        // Init Shape
        if (Enable2DGrid == true)
        {

            for (int y = 0; y < rowAmount2D; y++)
            {

                for (int x = 0; x < columnAmount2D; x++)
                {

                    Vector3 pos = new Vector3(x, 0, y) * spacing2D + positionRelativeObject.transform.position;

                    if (EnableDifferentPrefabs == false)
                    {

                        grid2DArr[y, x] = Instantiate(prefab, pos, prefab.transform.localRotation);

                        // Init Default Prefab
                        if (prefab.name == "button_PF")
                        {

                            interactButtonScript = grid2DArr[y, x].GetComponentInChildren<InteractionButton>();
                            buttonPressScript = grid2DArr[y, x].GetComponentInChildren<buttonPress>();
                            interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                            interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                        }

                    } else {

                        grid2DArr[y, x] = Instantiate(prefabs2G[totalCount], pos, prefabs2G[totalCount].transform.localRotation);

                        if (scale2G[totalCount] != 0)
                        {

                            grid2DArr[y, x].transform.localScale = new Vector3(scale2G[totalCount], scale2G[totalCount], scale2G[totalCount]);

                        }

                        // Init Prefab
                        if (prefabs2G[totalCount].name == "button_PF")
                        {

                            interactButtonScript = grid2DArr[y, x].GetComponentInChildren<InteractionButton>();
                            buttonPressScript = grid2DArr[y, x].GetComponentInChildren<buttonPress>();
                            interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                            interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                        }

                        totalCount++;

                    }

                }

            }

        }

    }

    ////////////////////////////////////// 3D GRID //////////////////////////////////////
    private void init3DGrid()
    {

        int totalCount = 0;

        // Init Array
        grid3DArr = new GameObject[(int)volumeAmount3D, (int)rowAmount3D, (int)columnAmount3D];

        // Init Shape
        if (Enable3DGrid == true)
        {
            for (int z = 0; z < volumeAmount3D; z++)
            {

                for (int y = 0; y < rowAmount3D; y++)
                {

                    for (int x = 0; x < columnAmount3D; x++)
                    {

                        Vector3 pos = new Vector3(x, z, y) * spacing3D + positionRelativeObject.transform.position;

                        if (EnableDifferentPrefabs == false)
                        {

                            grid3DArr[z, y, x] = Instantiate(prefab, pos, prefab.transform.localRotation);

                            // Init Default Prefab
                            if (prefab.name == "button_PF")
                            {

                                interactButtonScript = grid3DArr[z, y, x].GetComponentInChildren<InteractionButton>();
                                buttonPressScript = grid3DArr[z, y, x].GetComponentInChildren<buttonPress>();
                                interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                                interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                            }

                        } else {

                            grid3DArr[z, y, x] = Instantiate(prefabs3G[totalCount], pos, prefabs3G[totalCount].transform.localRotation);

                            if (scale3G[totalCount] != 0)
                            {

                                grid3DArr[z, y, x].transform.localScale = new Vector3(scale3G[totalCount], scale3G[totalCount], scale3G[totalCount]);

                            }

                            // Init Prefab
                            if (prefabs3G[totalCount].name == "button_PF")
                            {

                                interactButtonScript = grid3DArr[z, y, x].GetComponentInChildren<InteractionButton>();
                                buttonPressScript = grid3DArr[z, y, x].GetComponentInChildren<buttonPress>();
                                interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                                interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                            }

                            totalCount++;

                        }

                    }

                }

            }

        }

    }

    ////////////////////////////////////// TRIANGLE //////////////////////////////////////
    private void initTriangle()
    {

        int totalCount = 0;

        // Init Array
        triangleRArr = new GameObject[(int)rowAmountTriangle + 1, (int)rowAmountTriangle + 1];
        triangleLArr = new GameObject[(int)rowAmountTriangle + 1, (int)rowAmountTriangle + 1];

        // Init Shape
        if (EnableTriangle == true)
        {

            for (int y = (int)rowAmountTriangle; y >= 0; y--)
            {

                for (int x = (int)rowAmountTriangle - y; x > 0; x--)
                {

                    // Right half of triangle
                    Vector3 pos = new Vector3(x, 0, y) * spacingTriangle + positionRelativeObject.transform.position;

                    if (EnableDifferentPrefabs == false)
                    {

                        triangleRArr[y, x] = Instantiate(prefab, pos, prefab.transform.localRotation);

                        // Init Default Prefab
                        if (prefab.name == "button_PF")
                        {

                            interactButtonScript = triangleRArr[y, x].GetComponentInChildren<InteractionButton>();
                            buttonPressScript = triangleRArr[y, x].GetComponentInChildren<buttonPress>();
                            interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                            interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                        }

                    } else {

                        triangleRArr[y, x] = Instantiate(prefabsT[totalCount], pos, prefabsT[totalCount].transform.localRotation);

                        if (scaleT[totalCount] != 0)
                        {

                            triangleRArr[y, x].transform.localScale = new Vector3(scaleT[totalCount], scaleT[totalCount], scaleT[totalCount]);

                        }

                        // Init Prefab
                        if (prefabsT[totalCount].name == "button_PF")
                        {

                            interactButtonScript = triangleRArr[y, x].GetComponentInChildren<InteractionButton>();
                            buttonPressScript = triangleRArr[y, x].GetComponentInChildren<buttonPress>();
                            interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                            interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                        }

                        totalCount++;

                    }

                    // Left half of triangle
                    if (x > 1)
                    {

                        pos = new Vector3(-x + 2, 0, y) * spacingTriangle + positionRelativeObject.transform.position;

                        if (EnableDifferentPrefabs == false)
                        {

                            triangleLArr[y, x] = Instantiate(prefab, pos, prefab.transform.localRotation);

                            // Init Default Prefab
                            if (prefab.name == "button_PF")
                            {

                                interactButtonScript = triangleLArr[y, x].GetComponentInChildren<InteractionButton>();
                                buttonPressScript = triangleLArr[y, x].GetComponentInChildren<buttonPress>();
                                interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                                interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                            }

                        }
                        else
                        {

                            triangleLArr[y, x] = Instantiate(prefabsT[totalCount], pos, prefabsT[totalCount].transform.localRotation);

                            if (scaleT[totalCount] != 0)
                            {

                                triangleLArr[y, x].transform.localScale = new Vector3(scaleT[totalCount], scaleT[totalCount], scaleT[totalCount]);

                            }

                            // Init Prefab
                            if (prefabsT[totalCount].name == "button_PF")
                            {

                                interactButtonScript = triangleLArr[y, x].GetComponentInChildren<InteractionButton>();
                                buttonPressScript = triangleLArr[y, x].GetComponentInChildren<buttonPress>();
                                interactButtonScript.OnPress.AddListener(buttonPressScript.ButtonPressed);
                                interactButtonScript.OnUnpress.AddListener(buttonPressScript.ButtonUnPressed);

                            }

                            totalCount++;

                        }

                    }

                }

            }

        }

    }

}


//When playing around with the 2D Triangle I accidentally made an arrow... This is the code

/*for (int y = (int) rowAmount2DTriangle; y > 0; y--)
{

    for (int x = (int)rowAmount2DTriangle - y; x > 0; x--)
    {

        Vector3 pos = new Vector3(x, spawnHeight, y) * spacing2DTriangle;
        Instantiate(prefab, pos, prefab.transform.localRotation);
        pos = new Vector3(x+y, spawnHeight, y+x) * spacing2DTriangle;
        Instantiate(prefab, pos, prefab.transform.localRotation);

    }

}*/

