using UnityEngine;

public class Card : MonoBehaviour
{
    // Kart özellikleri
    public Color cardColor { get; private set; }
    public bool isPlaced { get; set; }

    // Sürükleme deðiþkenleri
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
        Debug.Log($"Karta týklandý: {cardColor}");
        // Kart seçimi için UIManager'ý bul
        UIManager uiManager = GameObject.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.SelectCard(this);
        }

        // Eðer yerleþtirilmemiþ kart ise sürüklemeyi baþlat
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
                Debug.Log($"Kart yerleþtirildi: {cardColor}");
                gridSystem.PlaceCard(this, transform.position);
            }
            else
            {
                Debug.Log("Geçersiz pozisyon veya pozisyon dolu, kart baþlangýç konumuna döndü");
                transform.position = startPosition;
                isPlaced = false;
            }
        }
    }

    public void SetColor(Color color)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            // Her karta kendi materyalinin bir kopyasýný ver
            meshRenderer.material = new Material(meshRenderer.material);

            cardColor = color;
            meshRenderer.material.color = color;

            // Emission'ý baþlangýçta kapalý tut
            meshRenderer.material.DisableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
        else
        {
            Debug.LogError("Card: SetColor için MeshRenderer veya Material eksik!");
        }
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        isPlaced = false;
    }

    // Kartý seçili göster
    public void ShowSelected()
    {
        transform.position += Vector3.up * 0.2f;
    }

    // Kartýn seçimini kaldýr
    public void HideSelected()
    {
        transform.position -= Vector3.up * 0.2f;
    }

    // Kartý parlat
    public void Highlight(Color highlightColor)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", highlightColor);
        }
    }

    // Parlamayý kaldýr
    public void RemoveHighlight()
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.DisableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}