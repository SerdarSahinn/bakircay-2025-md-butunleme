using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int currentScore = 0;
    private UIManager uiManager;

    // Puan de�erleri
    [Header("Puan Ayarlar�")]
    public int matchScore = 10;        // E�le�me ba��na puan
    public int skillUsePenalty = -5;   // Yetenek kullanma cezas�
    public int comboBonus = 5;         // Ard���k e�le�me bonusu

    private int comboCount = 0;        // Ard���k e�le�me sayac�
    private float comboTimer = 0f;     // Combo s�resi
    private const float COMBO_TIME = 3f; // Combo s�resi limiti

    void Start()
    {
        uiManager = GameObject.FindFirstObjectByType<UIManager>();
        ResetScore();
    }

    void Update()
    {
        // Combo s�resini kontrol et
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
        Debug.Log("E�le�me bulundu - Skor ekleniyor");

        // Temel puan� ekle
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

        // Combo s�resini yenile
        comboTimer = COMBO_TIME;

        // Skoru g�ncelle
        AddScore(points);
        Debug.Log($"Yeni skor: {currentScore}");
    }

    public void OnSkillUsed()
    {
        AddScore(skillUsePenalty);
        // Yetenek kullan�nca combo'yu s�f�rla
        comboCount = 0;
        comboTimer = 0;
    }

    public void AddScore(int points)
    {
        currentScore = Mathf.Max(0, currentScore + points);
        Debug.Log($"Skor g�ncellendi: {currentScore}");

        if (uiManager != null)
        {
            uiManager.UpdateScoreText();
        }
        else
        {
            Debug.LogError("UIManager referans� eksik!");
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