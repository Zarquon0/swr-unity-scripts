using UnityEngine;

public class RandomShower : MonoBehaviour
{
    [Header("Settings")]
    public GameObject objectToSpawn; // 1. Drag your Prefab here in Inspector
    public float spawnDelta = 1f;    // Time between spawns
    public float startHeight = 3f;
    public float spawnRange = 5f;    // How far from center (5 means a 10x10 area)

    [Header("References")]
    public Transform floorLoc;       // Drag your Floor object here (optional)

    private float timer = 0f;

    void Update()
    {
        // 2. The Timer Logic
        // Add the time passed since last frame to our counter
        timer += Time.deltaTime;

        // If enough time has passed...
        if (timer >= spawnDelta)
        {
            SpawnObject();
            timer = 0f; // Reset timer
        }
    }

    void SpawnObject()
    {
        // 3. Calculate Position
        // If floorLoc is assigned, use its position. If null, assume (0,0,0).
        Vector3 center = (floorLoc != null) ? floorLoc.position : Vector3.zero;

        // Unity's Random.Range is much easier than System.Random
        float randX = Random.Range(-spawnRange, spawnRange);
        float randZ = Random.Range(-spawnRange, spawnRange);

        Vector3 spawnPos = new Vector3(center.x + randX, startHeight, center.z + randZ);

        // 4. Create the Object (Instantiate)
        // Instantiate(What to build, Where to put it, Rotation)
        Instantiate(objectToSpawn, spawnPos, Quaternion.identity);
    }
}
