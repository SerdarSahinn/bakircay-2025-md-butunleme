using UnityEngine;
using System.Collections;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Ayarlar�")]
    public int width = 4;  // Grid geni�li�i
    public int height = 2; // Grid y�ksekli�i
    public float cellSize = 1.5f; // H�cre boyutu

    [Header("Debug")]
    public bool showGrid = true; // Grid �izgilerini g�ster

    // Grid'deki kartlar� tutacak dizi
    private Card[,] grid;
    private ScoreManager scoreManager;

    void Start()
    {
        // Grid dizisini olu�tur
        grid = new Card[width, height];
        scoreManager = GameObject.FindFirstObjectByType<ScoreManager>();

        if (scoreManager == null)
        {
            Debug.LogError("ScoreManager bulunamad�! L�tfen sahnede ScoreManager oldu�undan emin olun.");
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

    // Verilen d�nya pozisyonuna en yak�n grid pozisyonunu bulur
    public Vector3 GetNearestGridPosition(Vector3 worldPosition)
    {
        // D�nya pozisyonunu grid'e g�re yerel pozisyona �evir
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        // En yak�n grid x ve y koordinatlar�n� bul
        int x = Mathf.Clamp(Mathf.RoundToInt(localPosition.x / cellSize), 0, width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(localPosition.z / cellSize), 0, height - 1);

        // Grid pozisyonunu d�nya pozisyonuna �evir
        return transform.TransformPoint(new Vector3(x * cellSize, 0, y * cellSize));
    }

    // Grid pozisyonunun bo� olup olmad���n� kontrol et
    public bool IsGridPositionEmpty(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        return grid[gridPos.x, gridPos.y] == null;
    }

    // D�nya pozisyonunu grid koordinatlar�na �evirir
    private Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        int x = Mathf.Clamp(Mathf.RoundToInt(localPosition.x / cellSize), 0, width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(localPosition.z / cellSize), 0, height - 1);
        return new Vector2Int(x, y);
    }

    // Kart� grid'e yerle�tir
    public void PlaceCard(Card card, Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);

        // Debug i�in pozisyonlar� yazd�ral�m
        Debug.Log($"Grid pozisyonu: ({gridPos.x}, {gridPos.y})");
        Debug.Log($"Yerle�tirilen kart rengi: {card.cardColor}");

        // �nce kart� grid'e yerle�tir
        grid[gridPos.x, gridPos.y] = card;

        // Kom�u kartlar� kontrol et ve yazd�r
        if (gridPos.x < width - 1 && grid[gridPos.x + 1, gridPos.y] != null)
        {
            Debug.Log($"Sa�daki kart rengi: {grid[gridPos.x + 1, gridPos.y].cardColor}");
        }
        if (gridPos.x > 0 && grid[gridPos.x - 1, gridPos.y] != null)
        {
            Debug.Log($"Soldaki kart rengi: {grid[gridPos.x - 1, gridPos.y].cardColor}");
        }

        // T�m olas� e�le�meleri kontrol et
        bool matchFound = false;

        // Sa�daki kartla e�le�me kontrol�
        if (gridPos.x < width - 1 && grid[gridPos.x + 1, gridPos.y] != null)
        {
            if (card.cardColor == grid[gridPos.x + 1, gridPos.y].cardColor)
            {
                Debug.Log("Sa�da e�le�me bulundu!");
                StartCoroutine(MatchAndDestroy(card, grid[gridPos.x + 1, gridPos.y]));
                matchFound = true;
            }
        }

        // Soldaki kartla e�le�me kontrol�
        if (gridPos.x > 0 && grid[gridPos.x - 1, gridPos.y] != null)
        {
            if (card.cardColor == grid[gridPos.x - 1, gridPos.y].cardColor)
            {
                Debug.Log("Solda e�le�me bulundu!");
                StartCoroutine(MatchAndDestroy(card, grid[gridPos.x - 1, gridPos.y]));
                matchFound = true;
            }
        }

        // E�le�me bulunduysa skor ekle
        if (matchFound)
        {
            Debug.Log("E�le�me bulundu, skor ekleniyor...");
            if (scoreManager != null)
            {
                scoreManager.OnMatchFound();
            }
            else
            {
                Debug.LogError("ScoreManager referans� eksik!");
            }
        }
    }

    // E�le�en kartlar� yok et
    private IEnumerator MatchAndDestroy(Card card1, Card card2)
    {
        Debug.Log($"E�le�en kartlar yok ediliyor: {card1.cardColor} ve {card2.cardColor}");

        // Kartlar� grid'den kald�r
        Vector2Int pos1 = WorldToGridPosition(card1.transform.position);
        Vector2Int pos2 = WorldToGridPosition(card2.transform.position);

        // E�le�me efekti
        card1.transform.position += Vector3.up * 0.5f;
        card2.transform.position += Vector3.up * 0.5f;

        yield return new WaitForSeconds(0.5f);

        // Grid'den kald�r
        grid[pos1.x, pos1.y] = null;
        grid[pos2.x, pos2.y] = null;

        // Kartlar� yok et
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

    // Kart� grid'den kald�r
    public void RemoveCard(Card card)
    {
        if (card == null) return;

        // Grid pozisyonunu bul
        Vector3 gridPos = GetNearestGridPosition(card.transform.position);
        Vector2Int gridIndex = WorldToGridPosition(gridPos);

        // Grid s�n�rlar� i�inde mi kontrol et
        if (IsValidGridPosition(gridIndex))
        {
            // Grid pozisyonunu bo�alt
            grid[gridIndex.x, gridIndex.y] = null;
            Debug.Log($"Kart grid'den kald�r�ld�: {gridPos}");
        }
    }

    // Grid pozisyonunun ge�erli olup olmad���n� kontrol et
    private bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
}