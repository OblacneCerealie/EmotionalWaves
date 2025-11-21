using UnityEngine;
using UnityEngine.InputSystem; // { changed code }
 /// <summary>
 /// handles Anxiety and other high level metrics
 /// </summary>
public class BarManager : MonoBehaviour
{
    public static BarManager Instance { get; private set; }

    [Header("Anxiety")]
    [SerializeField] private int anxiety = 0;
    [SerializeField] private bool logChanges = true;
    [Tooltip("Maximum anxiety value shown on the UI bar")]
    [SerializeField] private int maxAnxiety = 100;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize UI with the current anxiety value (if UIManager exists)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetAnxiety(Mathf.Clamp(anxiety, 0, maxAnxiety), maxAnxiety);
        }
    }

    // Dev helper: press K to add +5 anxiety
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            AddAnxiety(5);
        }
    }

    // Adds anxiety (can be positive or negative)
    public void AddAnxiety(int amount)
    {
        int old = anxiety;
        anxiety += amount;
        
        anxiety = Mathf.Clamp(anxiety, 0, maxAnxiety);

        if (logChanges)
        {
            Debug.Log($"BarManager: Anxiety changed by {amount} (was {old}). New value: {anxiety}");
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetAnxiety(anxiety, maxAnxiety);
        }
    }

    public int GetAnxiety()
    {
        return anxiety;
    }
    
}
