using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    // Referanslar
    private GridSystem gridSystem;
    private ScoreManager scoreManager;

    // Yetenek cooldown süresi
    public float skillCooldown = 5f;

    void Start()
    {
        gridSystem = GameObject.FindFirstObjectByType<GridSystem>();
        scoreManager = GameObject.FindFirstObjectByType<ScoreManager>();

        if (gridSystem == null || scoreManager == null)
        {
            Debug.LogError("SkillManager: Gerekli referanslar eksik!");
            return;
        }
    }

    // Göster yeteneði - Seçilen kartýn eþini parlat
    public void UseColorReveal(Card selectedCard)
    {
        if (selectedCard == null)
        {
            Debug.LogWarning("Önce bir kart seçmelisiniz!");
            return;
        }

        Debug.Log($"Göster yeteneði kullanýldý: {selectedCard.cardColor}");
        bool foundMatch = false;

        Card[] allCards = GameObject.FindObjectsByType<Card>(FindObjectsSortMode.None);
        if (allCards != null)
        {
            foreach (Card card in allCards)
            {
                if (card != null && card != selectedCard && card.cardColor == selectedCard.cardColor)
                {
                    foundMatch = true;
                    StartCoroutine(HighlightMatchingCard(card));
                    Debug.Log($"Eþleþen kart bulundu ve parlatýlýyor: {card.transform.position}");
                    break;
                }
            }
        }

        if (!foundMatch)
        {
            Debug.LogWarning("Bu kartýn eþi bulunamadý!");
        }

        scoreManager?.OnSkillUsed();
    }

    private IEnumerator HighlightMatchingCard(Card card)
    {
        if (card == null) yield break;

        card.Highlight(card.cardColor * 2f);
        yield return new WaitForSeconds(2f);
        if (card != null) card.RemoveHighlight();
    }

    // Çek yeteneði - Seçilen kartýn eþini yanýna çek
    public void UseMagnet(Card selectedCard)
    {
        if (selectedCard == null)
        {
            Debug.LogWarning("Önce bir kart seçmelisiniz!");
            return;
        }

        Debug.Log($"Çek yeteneði kullanýldý: {selectedCard.cardColor}");
        bool foundMatch = false;

        Card[] allCards = GameObject.FindObjectsByType<Card>(FindObjectsSortMode.None);
        if (allCards != null)
        {
            foreach (Card card in allCards)
            {
                if (card != null && card != selectedCard && card.cardColor == selectedCard.cardColor)
                {
                    foundMatch = true;
                    Vector3 targetPos = selectedCard.transform.position + Vector3.right * 1.5f;
                    StartCoroutine(MoveCardToTargetAndMatch(card, targetPos, selectedCard));
                    Debug.Log($"Eþleþen kart bulundu ve çekiliyor: {card.transform.position} -> {targetPos}");
                    break;
                }
            }
        }

        if (!foundMatch)
        {
            Debug.LogWarning("Bu kartýn eþi bulunamadý!");
        }

        scoreManager?.OnSkillUsed();
    }

    private IEnumerator MoveCardToTargetAndMatch(Card card, Vector3 targetPos, Card selectedCard)
    {
        if (card == null || selectedCard == null) yield break;

        Vector3 startPos = card.transform.position;
        float duration = 0.5f;
        float elapsed = 0f;

        gridSystem?.RemoveCard(card);

        while (elapsed < duration)
        {
            if (card == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float height = Mathf.Sin(t * Mathf.PI) * 0.5f;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += height;

            card.transform.position = currentPos;
            yield return null;
        }

        if (card == null || selectedCard == null) yield break;
        card.transform.position = targetPos;

        if (gridSystem != null)
        {
            gridSystem.RemoveCard(selectedCard);
            gridSystem.RemoveCard(card);

            selectedCard.isPlaced = false;
            card.isPlaced = false;

            scoreManager?.OnMatchFound();
            StartCoroutine(FadeOutCards(selectedCard, card));
        }
    }

    private IEnumerator FadeOutCards(Card card1, Card card2)
    {
        if (card1 == null || card2 == null) yield break;

        yield return new WaitForSeconds(0.5f);

        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos1 = card1.transform.position;
        Vector3 startPos2 = card2.transform.position;

        while (elapsed < duration)
        {
            if (card1 == null || card2 == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            card1.transform.position = startPos1 + Vector3.up * (t * 2f);
            card2.transform.position = startPos2 + Vector3.up * (t * 2f);

            var renderer1 = card1.GetComponent<MeshRenderer>();
            var renderer2 = card2.GetComponent<MeshRenderer>();
            if (renderer1 != null && renderer2 != null)
            {
                Color c1 = renderer1.material.color;
                Color c2 = renderer2.material.color;
                c1.a = 1 - t;
                c2.a = 1 - t;
                renderer1.material.color = c1;
                renderer2.material.color = c2;
            }

            yield return null;
        }

        if (card1 != null) Destroy(card1.gameObject);
        if (card2 != null) Destroy(card2.gameObject);
    }

    // Karýþtýr yeteneði
    public void UseShuffle()
    {
        Card[] cards = GameObject.FindObjectsByType<Card>(FindObjectsSortMode.None);
        if (cards != null)
        {
            foreach (Card card in cards)
            {
                if (card != null && !card.isPlaced)
                {
                    Vector3 randomPos = GetRandomSpawnPosition();
                    StartCoroutine(MoveCardToPosition(card, randomPos));
                }
            }
        }

        scoreManager?.OnSkillUsed();
    }

    // Temizle yeteneði
    public void UseCleaner()
    {
        gridSystem?.ClearGrid();
        scoreManager?.OnSkillUsed();
    }

    // Kartý yeni pozisyona taþý
    private IEnumerator MoveCardToPosition(Card card, Vector3 targetPos)
    {
        if (card == null) yield break;

        Vector3 startPos = card.transform.position;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (card == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            card.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-2f, 2f);
        float z = Random.Range(-1f, 1f);
        return new Vector3(x, 0, z);
    }
}