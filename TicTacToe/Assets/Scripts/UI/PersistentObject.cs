using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private void Awake()
    {
        // Keep this object between scenes
        DontDestroyOnLoad(gameObject);

        // Optional: Avoid duplicates if you reload a scene that has this prefab
        PersistentObject[] existingObjects = FindObjectsOfType<PersistentObject>();
        if (existingObjects.Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
