using UnityEngine;
using System.Collections;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Ayarlarý")]
    public int width = 4;  // Grid geniþliði
    public int height = 2; // Grid yüksekliði
    public float cellSize = 1.5f; // Hücre boyutu

    [Header("Debug")]
    public bool showGrid = true; // Grid çizgilerini göster

    // Grid'deki kartlarý tutacak dizi
    private Card[,] grid;
    private ScoreManager scoreManager;

    void Start()
    {
        // Grid dizisini oluþtur
        grid = new Card[width, height];
        scoreManager = GameObject.FindFirstObjectByType<ScoreManager>();

        if (scoreManager == null)
        {
            Debug.LogError("ScoreManager bulunamadý! Lütfen sahnede ScoreManager olduðundan emin olun.");
        }
    }

    void OnDrawGizmos()
    {
        if (showGrid)
        {
            Gizmos.color = Color.yellow;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Vector3 pos = transform.position + new Vector3(x * cellSize, 0, z * cellSize);
                    Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }
    }

    // Verilen dünya pozisyonuna en yakýn grid pozisyonunu bulur
    public Vector3 GetNearestGridPosition(Vector3 worldPosition)
    {
        // Dünya pozisyonunu grid'e göre yerel pozisyona çevir
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        // En yakýn grid x ve y koordinatlarýný bul
        int x = Mathf.Clamp(Mathf.RoundToInt(localPosition.x / cellSize), 0, width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(localPosition.z / cellSize), 0, height - 1);

        // Grid pozisyonunu dünya pozisyonuna çevir
        return transform.TransformPoint(new Vector3(x * cellSize, 0, y * cellSize));
    }

    // Grid pozisyonunun boþ olup olmadýðýný kontrol et
    public bool IsGridPositionEmpty(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        return grid[gridPos.x, gridPos.y] == null;
    }

    // Dünya pozisyonunu grid koordinatlarýna çevirir
    private Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        int x = Mathf.Clamp(Mathf.RoundToInt(localPosition.x / cellSize), 0, width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(localPosition.z / cellSize), 0, height - 1);
        return new Vector2Int(x, y);
    }

    // Kartý grid'e yerleþtir
    public void PlaceCard(Card card, Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);

        // Debug için pozisyonlarý yazdýralým
        Debug.Log($"Grid pozisyonu: ({gridPos.x}, {gridPos.y})");
        Debug.Log($"Yerleþtirilen kart rengi: {card.cardColor}");

        // Önce kartý grid'e yerleþtir
        grid[gridPos.x, gridPos.y] = card;

        // Komþu kartlarý kontrol et ve yazdýr
        if (gridPos.x < width - 1 && grid[gridPos.x + 1, gridPos.y] != null)
        {
            Debug.Log($"Saðdaki kart rengi: {grid[gridPos.x + 1, gridPos.y].cardColor}");
        }
        if (gridPos.x > 0 && grid[gridPos.x - 1, gridPos.y] != null)
        {
            Debug.Log($"Soldaki kart rengi: {grid[gridPos.x - 1, gridPos.y].cardColor}");
        }

        // Tüm olasý eþleþmeleri kontrol et
        bool matchFound = false;

        // Saðdaki kartla eþleþme kontrolü
        if (gridPos.x < width - 1 && grid[gridPos.x + 1, gridPos.y] != null)
        {
            if (card.cardColor == grid[gridPos.x + 1, gridPos.y].cardColor)
            {
                Debug.Log("Saðda eþleþme bulundu!");
                StartCoroutine(MatchAndDestroy(card, grid[gridPos.x + 1, gridPos.y]));
                matchFound = true;
            }
        }

        // Soldaki kartla eþleþme kontrolü
        if (gridPos.x > 0 && grid[gridPos.x - 1, gridPos.y] != null)
        {
            if (card.cardColor == grid[gridPos.x - 1, gridPos.y].cardColor)
            {
                Debug.Log("Solda eþleþme bulundu!");
                StartCoroutine(MatchAndDestroy(card, grid[gridPos.x - 1, gridPos.y]));
                matchFound = true;
            }
        }

        // Eþleþme bulunduysa skor ekle
        if (matchFound)
        {
            Debug.Log("Eþleþme bulundu, skor ekleniyor...");
            if (scoreManager != null)
            {
                scoreManager.OnMatchFound();
            }
            else
            {
                Debug.LogError("ScoreManager referansý eksik!");
            }
        }
    }

    // Eþleþen kartlarý yok et
    private IEnumerator MatchAndDestroy(Card card1, Card card2)
    {
        Debug.Log($"Eþleþen kartlar yok ediliyor: {card1.cardColor} ve {card2.cardColor}");

        // Kartlarý grid'den kaldýr
        Vector2Int pos1 = WorldToGridPosition(card1.transform.position);
        Vector2Int pos2 = WorldToGridPosition(card2.transform.position);

        // Eþleþme efekti
        card1.transform.position += Vector3.up * 0.5f;
        card2.transform.position += Vector3.up * 0.5f;

        yield return new WaitForSeconds(0.5f);

        // Grid'den kaldýr
        grid[pos1.x, pos1.y] = null;
        grid[pos2.x, pos2.y] = null;

        // Kartlarý yok et
        Destroy(card1.gameObject);
        Destroy(card2.gameObject);

        Debug.Log("Kartlar yok edildi");
    }

    // Grid'i temizle
    public void ClearGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].ResetPosition();
                    grid[x, y] = null;
                }
            }
        }
    }

    // Kartý grid'den kaldýr
    public void RemoveCard(Card card)
    {
        if (card == null) return;

        // Grid pozisyonunu bul
        Vector3 gridPos = GetNearestGridPosition(card.transform.position);
        Vector2Int gridIndex = WorldToGridPosition(gridPos);

        // Grid sýnýrlarý içinde mi kontrol et
        if (IsValidGridPosition(gridIndex))
        {
            // Grid pozisyonunu boþalt
            grid[gridIndex.x, gridIndex.y] = null;
            Debug.Log($"Kart grid'den kaldýrýldý: {gridPos}");
        }
    }

    // Grid pozisyonunun geçerli olup olmadýðýný kontrol et
    private bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
}