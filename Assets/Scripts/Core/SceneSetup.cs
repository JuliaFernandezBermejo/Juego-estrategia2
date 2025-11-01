using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Automatic scene setup for the turn-based strategy game.
/// Creates all required GameObjects and connects references with one click.
/// </summary>
public class SceneSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Click the button below to auto-generate the game scene")]
    [SerializeField] private bool setupComplete = false;

    [ContextMenu("Setup Game Scene")]
    public void SetupGameScene()
    {
        if (setupComplete)
        {
            Debug.LogWarning("Scene already set up! Delete existing objects first if you want to reset.");
            return;
        }

        Debug.Log("Setting up game scene...");

        // 1. Setup Camera
        SetupCamera();

        // 2. Setup Lighting
        SetupLighting();

        // 3. Setup HexGrid
        GameObject hexGridObj = SetupHexGrid();

        // 4. Setup GameManager
        GameObject gameManagerObj = SetupGameManager();

        // 5. Setup InputManager
        SetupInputManager(gameManagerObj);

        // 6. Setup StrategicManager (AI)
        SetupStrategicManager(gameManagerObj, hexGridObj);

        setupComplete = true;
        Debug.Log("✓ Game scene setup complete! Press Play to start the game.");
        Debug.Log("Controls: Click units to select, click cells to move/attack, SPACE to end turn");
    }

    private void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }

        // Position camera to see the entire grid
        // Pointy-top 10x10 grid spans: X(0-16.5), Z(0-13.5), center at (8.2, 0, 6.75)
        mainCam.transform.position = new Vector3(8f, 22f, -2f);
        mainCam.transform.rotation = Quaternion.Euler(65f, 0f, 0f);

        // Add camera controller
        if (mainCam.GetComponent<CameraController>() == null)
        {
            mainCam.gameObject.AddComponent<CameraController>();
        }

        Debug.Log("✓ Camera setup complete");
    }

    private void SetupLighting()
    {
        Light mainLight = FindObjectOfType<Light>();
        if (mainLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            mainLight = lightObj.AddComponent<Light>();
            mainLight.type = LightType.Directional;
        }

        // Position light to illuminate the grid from above
        mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        mainLight.color = Color.white;
        mainLight.intensity = 1.0f;

        Debug.Log("✓ Lighting setup complete");
    }

    private GameObject SetupHexGrid()
    {
        GameObject hexGridObj = GameObject.Find("HexGrid");
        if (hexGridObj == null)
        {
            hexGridObj = new GameObject("HexGrid");
        }

        HexGrid hexGrid = hexGridObj.GetComponent<HexGrid>();
        if (hexGrid == null)
        {
            hexGrid = hexGridObj.AddComponent<HexGrid>();
        }

        Debug.Log("✓ HexGrid setup complete");
        return hexGridObj;
    }

    private GameObject SetupGameManager()
    {
        GameObject gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj == null)
        {
            gameManagerObj = new GameObject("GameManager");
        }

        GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
        if (gameManager == null)
        {
            gameManager = gameManagerObj.AddComponent<GameManager>();
        }

        Debug.Log("✓ GameManager setup complete");
        return gameManagerObj;
    }

    private void SetupInputManager(GameObject gameManagerObj)
    {
        GameObject inputManagerObj = GameObject.Find("InputManager");
        if (inputManagerObj == null)
        {
            inputManagerObj = new GameObject("InputManager");
        }

        InputManager inputManager = inputManagerObj.GetComponent<InputManager>();
        if (inputManager == null)
        {
            inputManager = inputManagerObj.AddComponent<InputManager>();
        }

        // Connect reference to GameManager
        GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
        SerializedObject so = new SerializedObject(inputManager);
        so.FindProperty("gameManager").objectReferenceValue = gameManager;
        so.ApplyModifiedProperties();

        Debug.Log("✓ InputManager setup complete");
    }

    private void SetupStrategicManager(GameObject gameManagerObj, GameObject hexGridObj)
    {
        GameObject strategicManagerObj = GameObject.Find("StrategicManager");
        if (strategicManagerObj == null)
        {
            strategicManagerObj = new GameObject("StrategicManager");
        }

        StrategicManager strategicManager = strategicManagerObj.GetComponent<StrategicManager>();
        if (strategicManager == null)
        {
            strategicManager = strategicManagerObj.AddComponent<StrategicManager>();
        }

        // Connect references
        GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
        HexGrid hexGrid = hexGridObj.GetComponent<HexGrid>();

        SerializedObject so = new SerializedObject(strategicManager);
        so.FindProperty("gameManager").objectReferenceValue = gameManager;
        so.FindProperty("hexGrid").objectReferenceValue = hexGrid;
        so.FindProperty("playerID").intValue = 1; // AI controls Player 1
        so.ApplyModifiedProperties();

        Debug.Log("✓ StrategicManager setup complete");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneSetup))]
public class SceneSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneSetup sceneSetup = (SceneSetup)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Setup Game Scene", GUILayout.Height(40)))
        {
            sceneSetup.SetupGameScene();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Click the button above to automatically set up the entire game scene.\n\n" +
            "This will create:\n" +
            "• Camera with controls\n" +
            "• Hexagonal grid (10x10)\n" +
            "• Game manager\n" +
            "• Input system\n" +
            "• AI system\n\n" +
            "After setup, press Play to start the game!",
            MessageType.Info
        );
    }
}
#endif
