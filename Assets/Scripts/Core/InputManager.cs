using UnityEngine;

/// <summary>
/// Handles mouse input for cell selection and unit interaction.
/// </summary>
public class InputManager : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private GameManager gameManager;

    private Camera mainCamera;
    private HexCell selectedCell;
    private HexCell hoveredCell;

    void Start()
    {
        mainCamera = Camera.main;

        if (hexGrid == null)
            hexGrid = FindObjectOfType<HexGrid>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        HandleMouseHover();
        HandleMouseClick();
    }

    private void HandleMouseHover()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            HexCell cell = hit.collider.GetComponent<HexCell>();
            if (cell != null && cell != hoveredCell)
            {
                if (hoveredCell != null && hoveredCell != selectedCell)
                {
                    hoveredCell.Highlight(false);
                }

                hoveredCell = cell;
                if (hoveredCell != selectedCell)
                {
                    hoveredCell.Highlight(true);
                }
            }
        }
        else
        {
            if (hoveredCell != null && hoveredCell != selectedCell)
            {
                hoveredCell.Highlight(false);
                hoveredCell = null;
            }
        }
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                HexCell clickedCell = hit.collider.GetComponent<HexCell>();
                if (clickedCell != null)
                {
                    OnCellClicked(clickedCell);
                }
            }
        }
    }

    private void OnCellClicked(HexCell cell)
    {
        // Clear previous selection highlight
        if (selectedCell != null)
        {
            selectedCell.Highlight(false);
        }

        selectedCell = cell;
        selectedCell.Highlight(true);

        // Notify GameManager about cell selection
        if (gameManager != null)
        {
            gameManager.OnCellSelected(cell);
        }
    }

    public HexCell GetSelectedCell()
    {
        return selectedCell;
    }

    public void ClearSelection()
    {
        if (selectedCell != null)
        {
            selectedCell.Highlight(false);
            selectedCell = null;
        }
    }
}
