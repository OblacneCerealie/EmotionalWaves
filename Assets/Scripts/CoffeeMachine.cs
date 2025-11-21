using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoffeeMachine : MonoBehaviour
{
    [Header("Coffee Settings")]
    [SerializeField] private GameObject coffeeCupPrefab;
    [SerializeField] private Transform cupSpawnPoint;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private float brewTime = 5f;

    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private float shakeSpeed = 20f;

    [Header("Player Reference")]
    [SerializeField] private Transform player;
    [SerializeField] private CoffeeHolder coffeeHolder;

    private bool isBrewing = false;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position;
    }

    private void Update()
    {
        if (isBrewing) return;

        if (player == null || coffeeHolder == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= interactionDistance && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (coffeeHolder.IsHoldingCoffee())
            {
                Debug.Log("Coffee Machine: Player already holding coffee!");
            }
            else
            {
                StartCoroutine(BrewCoffee());
            }
        }
    }

    private IEnumerator BrewCoffee()
    {
        isBrewing = true;
        Debug.Log("Coffee Machine: Brewing coffee...");

        float elapsed = 0f;

        // Shake the machine
        while (elapsed < brewTime)
        {
            elapsed += Time.deltaTime;

            // Calculate shake offset
            float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float shakeZ = Mathf.Cos(Time.time * shakeSpeed * 1.5f) * shakeIntensity;

            transform.position = originalPosition + new Vector3(shakeX, 0, shakeZ);

            yield return null;
        }

        // Reset position
        transform.position = originalPosition;

        // Spawn coffee cup
        if (coffeeCupPrefab != null && cupSpawnPoint != null)
        {
            GameObject cup = Instantiate(coffeeCupPrefab, cupSpawnPoint.position, Quaternion.identity);
            Debug.Log("Coffee Machine: Coffee ready!");
        }
        else
        {
            Debug.LogError("Coffee Machine: Cup prefab or spawn point not assigned!");
        }

        isBrewing = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Draw spawn point
        if (cupSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cupSpawnPoint.position, 0.2f);
        }
    }
}

