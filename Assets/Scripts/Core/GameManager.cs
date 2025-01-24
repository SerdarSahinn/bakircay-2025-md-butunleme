using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Prefab ve spawn ayarlarý
    public GameObject cardPrefab;
    public Transform spawnArea;
    public Vector2 spawnAreaSize = new Vector2(4f, 2f);

    // Renk listesi
    private Color[] cardColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        Color.white,
        Color.gray
    };

    void Start()
    {
        SpawnCards();
    }

    void SpawnCards()
    {
        List<Color> colors = new List<Color>();

        // Her renkten 2 adet ekle
        foreach (Color color in cardColors)
        {
            colors.Add(color);
            colors.Add(color);
        }

        // Renkleri karýþtýr
        for (int i = 0; i < colors.Count; i++)
        {
            Color temp = colors[i];
            int randomIndex = Random.Range(i, colors.Count);
            colors[i] = colors[randomIndex];
            colors[randomIndex] = temp;
        }

        // Kartlarý oluþtur
        for (int i = 0; i < colors.Count; i++)
        {
            float x = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float z = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
            Vector3 spawnPos = spawnArea.position + new Vector3(x, 0, z);

            GameObject cardObj = Instantiate(cardPrefab, spawnPos, Quaternion.identity);
            cardObj.transform.parent = transform;
            Card card = cardObj.GetComponent<Card>();
            card.SetColor(colors[i]);
        }
    }

    // Oyunu yeniden baþlat
    public void RestartGame()
    {
        // Tüm kartlarý yok et
        Card[] cards = FindObjectsOfType<Card>();
        foreach (Card card in cards)
        {
            Destroy(card.gameObject);
        }

        // Yeni kartlarý oluþtur
        SpawnCards();
    }
}