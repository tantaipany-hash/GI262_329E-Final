using UnityEngine;

public class AmmoSpawner : MonoBehaviour
{
    [Header("Spawn References")]
    public GameObject ammoPickupPrefab; // ลาก Prefab กล่องกระสุนมาใส่ในช่องนี้
    public Transform player;            // ตัวละคร Player (ถ้าไม่ลากใส่ ระบบจะหาให้เองอัตโนมัติ)

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;    // ระยะเวลาเสกกระสุนใหม่ทุกๆ กี่วินาที
    public int maxAmmoInScene = 10;     // จำนวนกล่องกระสุนสูงสุดบนพื้นพร้อมกัน
    public float minSpawnRadius = 4f;   // รัศมีเกิดใกล้ที่สุด (ขอบๆ หน้าจอ)
    public float maxSpawnRadius = 10f;  // รัศมีเกิดไกลที่สุด (ไม่ไกลจนเกินไป เดินเจอง่าย)

    private float spawnTimer;

    void Start()
    {
        // ถ้าไม่ได้ลาก Player มาใส่ใน Inspector ให้หาจาก Tag "Player" อัตโนมัติ
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                // ถ้าหาจาก Tag ไม่เจอ ให้หาจาก Component Player แทน
                var pScript = FindObjectOfType<MonoBehaviour>();
                if (pScript != null && pScript.gameObject.name.Contains("Player"))
                {
                    player = pScript.transform;
                }
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnAmmo();
        }
    }

    void TrySpawnAmmo()
    {
        // 1. เช็กจำนวนกล่องกระสุนที่มีอยู่ในฉากตอนนั้น
        int currentAmmoCount = GameObject.FindGameObjectsWithTag("AmmoPickup").Length;
        if (currentAmmoCount >= maxAmmoInScene) return;

        // 2. สุ่มทิศทางและระยะห่างรอบตัวผู้เล่น
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPosition = player.position + (Vector3)(randomDir * randomDistance);

        // 3. เสกกล่องกระสุนขึ้นมาบนแผนที่
        if (ammoPickupPrefab != null)
        {
            Instantiate(ammoPickupPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // วาดวงกลมแสดงขอบเขตระยะการเกิดในหน้า Scene View
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, minSpawnRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, maxSpawnRadius);
    }
}