using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int currentScore = 0;
    private UIManager uiManager;

    // Puan deðerleri
    [Header("Puan Ayarlarý")]
    public int matchScore = 10;        // Eþleþme baþýna puan
    public int skillUsePenalty = -5;   // Yetenek kullanma cezasý
    public int comboBonus = 5;         // Ardýþýk eþleþme bonusu

    private int comboCount = 0;        // Ardýþýk eþleþme sayacý
    private float comboTimer = 0f;     // Combo süresi
    private const float COMBO_TIME = 3f; // Combo süresi limiti

    void Start()
    {
        uiManager = GameObject.FindFirstObjectByType<UIManager>();
        ResetScore();
    }

    void Update()
    {
        // Combo süresini kontrol et
        if (comboCount > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                comboCount = 0;
            }
        }
    }

    public void OnMatchFound()
    {
        Debug.Log("Eþleþme bulundu - Skor ekleniyor");

        // Temel puaný ekle
        int points = matchScore;

        // Combo varsa bonus ekle
        if (comboTimer > 0)
        {
            comboCount++;
            points += comboBonus * comboCount;
            Debug.Log($"Combo x{comboCount} - Bonus: {comboBonus * comboCount}");
        }
        else
        {
            comboCount = 1;
        }

        // Combo süresini yenile
        comboTimer = COMBO_TIME;

        // Skoru güncelle
        AddScore(points);
        Debug.Log($"Yeni skor: {currentScore}");
    }

    public void OnSkillUsed()
    {
        AddScore(skillUsePenalty);
        // Yetenek kullanýnca combo'yu sýfýrla
        comboCount = 0;
        comboTimer = 0;
    }

    public void AddScore(int points)
    {
        currentScore = Mathf.Max(0, currentScore + points);
        Debug.Log($"Skor güncellendi: {currentScore}");

        if (uiManager != null)
        {
            uiManager.UpdateScoreText();
        }
        else
        {
            Debug.LogError("UIManager referansý eksik!");
        }
    }

    public void ResetScore()
    {
        currentScore = 0;
        comboCount = 0;
        comboTimer = 0;
        uiManager.UpdateScoreText();
    }

    public int GetScore()
    {
        return currentScore;
    }

    public int GetComboCount()
    {
        return comboCount;
    }
}