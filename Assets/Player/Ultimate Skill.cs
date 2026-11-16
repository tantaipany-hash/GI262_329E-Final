using UnityEngine;
using TMPro;
using System.Collections;

public class UltimateSkill : MonoBehaviour
{
    [Header("Ultimate Configuration")]
    public KeyCode ultKey = KeyCode.E;          // ปุ่มกดใช้อัลติ
    public int scoreRequiredPerCharge = 3000;   // คะแนนต่อ 1 ชาร์จ
    public float attackRadius = 5.0f;           // รัศมีการฟันกวาด 360 องศา

    [Header("Visual & Audio FX")]
    public GameObject slashEffectPrefab;        // Prefab เอฟเฟกต์ฟันกวาด
    public float effectDestroyTime = 1.0f;      // เวลาลบเอฟเฟกต์
    public AudioClip ultSound;                  // ลากไฟล์เสียง (SFX) มาใส่ช่องนี้
    [Range(0f, 1f)] public float soundVolume = 1f; // ความดังเสียง

    [Header("Camera Shake Settings")]
    public bool enableCameraShake = true;       // เปิด/ปิด การสั่นของกล้อง
    public float shakeDuration = 0.25f;          // ระยะเวลาที่กล้องสั่น (วินาที)
    public float shakeMagnitude = 0.3f;          // ความแรงของการสั่น

    [Header("UI Reference")]
    public TextMeshProUGUI ultText;             // ข้อความแสดงสถานะบน UI

    // System Variables
    private int availableCharges = 0;
    private int lastProcessedScore = 0;
    private AudioSource audioSource;
    private Camera mainCamera;

    void Awake()
    {
        // สร้าง AudioSource อัตโนมัติถ้ายังไม่มีในตัว Player
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        mainCamera = Camera.main;
    }

    void Update()
    {
        CheckScoreForCharges();
        UpdateUI();

        if (Input.GetKeyDown(ultKey) && availableCharges > 0)
        {
            ExecuteUltimateSlash();
        }
    }

    int GetCurrentScoreFromManager()
    {
        if (ScoreManager.instance == null) return 0;
        var instance = ScoreManager.instance;
        var type = instance.GetType();

        var scoreField = type.GetField("currentScore", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                      ?? type.GetField("score", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (scoreField != null) return (int)scoreField.GetValue(instance);

        var scoreMethod = type.GetMethod("GetScore");
        if (scoreMethod != null) return (int)scoreMethod.Invoke(instance, null);

        return 0;
    }

    void CheckScoreForCharges()
    {
        int currentScore = GetCurrentScoreFromManager();

        if (currentScore >= lastProcessedScore + scoreRequiredPerCharge)
        {
            int newCharges = (currentScore - lastProcessedScore) / scoreRequiredPerCharge;
            availableCharges += newCharges;
            lastProcessedScore += newCharges * scoreRequiredPerCharge;
        }
    }

    void ExecuteUltimateSlash()
    {
        availableCharges--;

        // 1. เล่นเสียง SFX เมื่อกดใช้อัลติ
        if (ultSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(ultSound, soundVolume);
        }

        // 2. เรียกใช้ระบบกล้องสั่น (Camera Shake)
        if (enableCameraShake && mainCamera != null)
        {
            StopAllCoroutines(); // หยุดการสั่นเดิมถ้ากดถี่ๆ
            StartCoroutine(ShakeCamera(shakeDuration, shakeMagnitude));
        }

        // 3. สร้าง Visual Effect (Slash FX)
        if (slashEffectPrefab != null)
        {
            GameObject fx = Instantiate(slashEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, effectDestroyTime);
        }

        // 4. สแกนหาศัตรูรอบตัว 360 องศา
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRadius);

        foreach (Collider2D col in hitEnemies)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(999999, AmmoType.MagicSilver);
                }
            }
        }
    }

    // โค้ดคำนวณกล้องสั่นอัตโนมัติ (Coroutine)
    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    void UpdateUI()
    {
        if (ultText == null) return;

        if (availableCharges > 0)
        {
            ultText.text = $"ULT READY! [{ultKey}] (x{availableCharges})";
        }
        else
        {
            int currentScore = GetCurrentScoreFromManager();
            int progress = currentScore - lastProcessedScore;
            ultText.text = $"ULT: {progress}/{scoreRequiredPerCharge}";
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}