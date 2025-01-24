using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public Button restartButton;
    public Button[] skillButtons;
    public TextMeshProUGUI[] skillButtonTexts;

    [Header("Settings")]
    public float skillCooldown = 5f;

    private float[] currentCooldowns;
    private bool[] isOnCooldown;
    private Card selectedCard = null;

    private GameManager gameManager;
    private ScoreManager scoreManager;
    private SkillManager skillManager;

    void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
        scoreManager = GameObject.FindFirstObjectByType<ScoreManager>();
        skillManager = GameObject.FindFirstObjectByType<SkillManager>();

        if (scoreManager == null || skillManager == null || gameManager == null)
        {
            Debug.LogError("UIManager: Gerekli referanslar eksik!");
        }

        currentCooldowns = new float[skillButtons.Length];
        isOnCooldown = new bool[skillButtons.Length];

        restartButton.onClick.AddListener(OnRestartClick);
        SetupSkillButtons();
        UpdateScoreText();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Card clickedCard = FindSelectedCard();
            if (clickedCard != null)
            {
                if (selectedCard != null)
                {
                    selectedCard.transform.position -= Vector3.up * 0.2f;
                }

                selectedCard = clickedCard;
                selectedCard.transform.position += Vector3.up * 0.2f;
                Debug.Log($"Kart seçildi: {selectedCard.cardColor}");
            }
        }

        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (isOnCooldown[i])
            {
                currentCooldowns[i] -= Time.deltaTime;
                skillButtonTexts[i].text = Mathf.Ceil(currentCooldowns[i]).ToString();

                if (currentCooldowns[i] <= 0)
                {
                    EnableButton(i);
                }
            }
        }
    }

    void SetupSkillButtons()
    {
        string[] buttonNames = { "Göster", "Çek", "Karýþtýr", "Temizle" };

        for (int i = 0; i < skillButtons.Length; i++)
        {
            int index = i;
            skillButtons[i].onClick.AddListener(() => UseSkill(index));
            skillButtonTexts[i].text = buttonNames[i];
        }
    }

    void UseSkill(int buttonIndex)
    {
        if ((buttonIndex == 0 || buttonIndex == 1) && selectedCard == null)
        {
            Debug.LogWarning("Önce bir kart seçmelisiniz!");
            return;
        }

        switch (buttonIndex)
        {
            case 0:
                skillManager.UseColorReveal(selectedCard);
                break;
            case 1:
                skillManager.UseMagnet(selectedCard);
                break;
            case 2:
                skillManager.UseShuffle();
                break;
            case 3:
                skillManager.UseCleaner();
                break;
        }

        StartCooldown(buttonIndex);
    }

    void StartCooldown(int buttonIndex)
    {
        skillButtons[buttonIndex].interactable = false;
        currentCooldowns[buttonIndex] = skillCooldown;
        isOnCooldown[buttonIndex] = true;
    }

    void EnableButton(int buttonIndex)
    {
        isOnCooldown[buttonIndex] = false;
        skillButtons[buttonIndex].interactable = true;

        string[] buttonNames = { "Göster", "Çek", "Karýþtýr", "Temizle" };
        skillButtonTexts[buttonIndex].text = buttonNames[buttonIndex];
    }

    void OnRestartClick()
    {
        gameManager.RestartGame();
        scoreManager.ResetScore();
        UpdateScoreText();
    }

    public void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Skor: " + scoreManager.GetScore().ToString();
        }
        else
        {
            Debug.LogError("UIManager: ScoreText referansý eksik!");
        }
    }

    private Card FindSelectedCard()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 1f);

        if (Physics.Raycast(ray, out hit))
        {
            Card card = hit.collider.GetComponent<Card>();
            if (card != null)
            {
                Debug.Log($"Raycast çarptý: {hit.collider.name}, Card component: {(card != null)}");
                return card;
            }
        }

        Debug.Log("Raycast çarpmadý veya kart bulunamadý");
        return null;
    }

    public void ClearSelectedCard()
    {
        if (selectedCard != null)
        {
            selectedCard.transform.position -= Vector3.up * 0.2f;
            selectedCard = null;
        }
    }

    public void SelectCard(Card card)
    {
        // Önceki seçili kartý eski haline getir
        if (selectedCard != null)
        {
            selectedCard.HideSelected();
        }

        // Yeni kartý seç
        selectedCard = card;
        selectedCard.ShowSelected();
        Debug.Log($"Kart seçildi: {selectedCard.cardColor}");
    }
}