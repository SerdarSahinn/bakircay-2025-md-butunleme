using UnityEngine;

public class Card : MonoBehaviour
{
    // Kart �zellikleri
    public Color cardColor { get; private set; }
    public bool isPlaced { get; set; }

    // S�r�kleme de�i�kenleri
    private bool isDragging = false;
    private float yOffset = 0.5f;
    private Vector3 startPosition;

    // Referanslar
    private GridSystem gridSystem;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        gridSystem = GameObject.FindFirstObjectByType<GridSystem>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null || meshRenderer.material == null)
        {
            Debug.LogError("Card: MeshRenderer veya Material eksik!");
        }
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.up * yOffset);

            if (plane.Raycast(ray, out float distance))
            {
                transform.position = ray.GetPoint(distance);
            }
        }
    }

    void OnMouseDown()
    {
        Debug.Log($"Karta t�kland�: {cardColor}");
        // Kart se�imi i�in UIManager'� bul
        UIManager uiManager = GameObject.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.SelectCard(this);
        }

        // E�er yerle�tirilmemi� kart ise s�r�klemeyi ba�lat
        if (!isPlaced)
        {
            isDragging = true;
        }
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;

            if (gridSystem != null && gridSystem.IsGridPositionEmpty(transform.position))
            {
                Vector3 gridPos = gridSystem.GetNearestGridPosition(transform.position);
                transform.position = gridPos;
                isPlaced = true;
                Debug.Log($"Kart yerle�tirildi: {cardColor}");
                gridSystem.PlaceCard(this, transform.position);
            }
            else
            {
                Debug.Log("Ge�ersiz pozisyon veya pozisyon dolu, kart ba�lang�� konumuna d�nd�");
                transform.position = startPosition;
                isPlaced = false;
            }
        }
    }

    public void SetColor(Color color)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            // Her karta kendi materyalinin bir kopyas�n� ver
            meshRenderer.material = new Material(meshRenderer.material);

            cardColor = color;
            meshRenderer.material.color = color;

            // Emission'� ba�lang��ta kapal� tut
            meshRenderer.material.DisableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
        else
        {
            Debug.LogError("Card: SetColor i�in MeshRenderer veya Material eksik!");
        }
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        isPlaced = false;
    }

    // Kart� se�ili g�ster
    public void ShowSelected()
    {
        transform.position += Vector3.up * 0.2f;
    }

    // Kart�n se�imini kald�r
    public void HideSelected()
    {
        transform.position -= Vector3.up * 0.2f;
    }

    // Kart� parlat
    public void Highlight(Color highlightColor)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", highlightColor);
        }
    }

    // Parlamay� kald�r
    public void RemoveHighlight()
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.DisableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}