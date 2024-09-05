using UnityEngine;

public class Singleton<T> : MonoBehaviour where T: MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance) return instance;
            instance = FindObjectOfType<T>();
            if (!instance) instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
            return instance;
        }
    }
}
