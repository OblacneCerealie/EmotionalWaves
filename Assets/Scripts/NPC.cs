using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPC : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Position Assignments")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform waitingPoint;
    [SerializeField] private Transform[] randomDestinations;

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    [Header("Interaction Timeouts")]
    [SerializeField] private float secondInteractionTimeout = 10f; // seconds
    [SerializeField] private int anxietyPenaltyOnTimeout = 5; // amount to add when customer leaves unserved
    private Coroutine secondInteractionTimeoutCoroutine;

    // Static list to track occupied destinations across all NPCs
    private static List<Transform> occupiedDestinations = new List<Transform>();
    

    private enum NPCState
    {
        NotSpawned,
        MovingToWaitingPoint,
        WaitingForFirstInteraction,
        MovingToRandomDestination,
        WaitingForSecondInteraction,
        ReturningToSpawn,
        Despawned
    }

    private NPCState currentState;
    private Transform currentDestination;
    private Transform assignedRandomDestination;
    private bool canInteract = false;
    private bool isInitialized = false;

    // Call this method to initialize the NPC with references (useful for prefabs)
    public void Initialize(Transform spawn, Transform waiting, Transform[] destinations, Transform playerTransform)
    {
        spawnPoint = spawn;
        waitingPoint = waiting;
        randomDestinations = destinations;
        player = playerTransform;
        isInitialized = true;

        // Start the NPC behavior immediately
        StartNPCBehavior();
    }

    void Start()
    {
        // If already initialized by manager, skip
        if (isInitialized) return;

        // Check if manually assigned in inspector
        if (spawnPoint == null || waitingPoint == null || randomDestinations.Length == 0)
        {
            Debug.LogError("NPC: Please assign spawn point, waiting point, and random destinations in inspector, or use Initialize method!");
            enabled = false;
            return;
        }

        // Start inactive, waiting for spawn (for manual T key spawning)
        currentState = NPCState.NotSpawned;
        gameObject.SetActive(false);
    }

    private void StartNPCBehavior()
    {
        transform.position = spawnPoint.position;
        currentState = NPCState.MovingToWaitingPoint;
        currentDestination = waitingPoint;
    }

    void Update()
    {
        // Dev tool - Press T to spawn NPC (remove later)
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame && currentState == NPCState.NotSpawned)
        {
            SpawnNPC();
        }

        switch (currentState)
        {
            case NPCState.NotSpawned:
                // Waiting to be spawned
                break;

            case NPCState.MovingToWaitingPoint:
                MoveToDestination(waitingPoint);
                if (HasReachedDestination(waitingPoint))
                {
                    currentState = NPCState.WaitingForFirstInteraction;
                    canInteract = true;
                    Debug.Log("NPC reached waiting point and is ready for first interaction!");
                }
                break;

            case NPCState.WaitingForFirstInteraction:
                CheckForPlayerInteraction();
                break;

            case NPCState.MovingToRandomDestination:
                MoveToDestination(assignedRandomDestination);
                if (HasReachedDestination(assignedRandomDestination))
                {
                    currentState = NPCState.WaitingForSecondInteraction;
                    canInteract = true;

                    // Start the 2nd-interaction timeout
                    StartSecondInteractionTimeout();

                    Debug.Log("NPC reached random destination and is waiting for second interaction!");
                }
                break;

            case NPCState.WaitingForSecondInteraction:
                CheckForPlayerInteraction();
                break;

            case NPCState.ReturningToSpawn:
                MoveToDestination(spawnPoint);
                if (HasReachedDestination(spawnPoint))
                {
                    currentState = NPCState.Despawned;
                    Despawn();
                }
                break;

            case NPCState.Despawned:
                // Do nothing, NPC is gone
                break;
        }
    }

    private void MoveToDestination(Transform destination)
    {
        if (destination == null) return;

        // Move towards destination
        Vector3 direction = (destination.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotate towards destination
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private bool HasReachedDestination(Transform destination)
    {
        if (destination == null) return false;
        return Vector3.Distance(transform.position, destination.position) < 0.1f;
    }

    private void CheckForPlayerInteraction()
    {
        if (player == null)
        {
            Debug.LogWarning("NPC: Player reference is null!");
            return;
        }

        if (!canInteract)
        {
            Debug.Log("NPC: Cannot interact yet (canInteract = false)");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Debug: Show distance when E is pressed
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log($"E pressed! Distance to player: {distanceToPlayer}, Interaction distance: {interactionDistance}, Can interact: {canInteract}");
        }

        if (distanceToPlayer <= interactionDistance && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Interaction triggered!");
            OnInteract();
        }
    }

    private void OnInteract()
    {
        canInteract = false;

        // If a timeout coroutine is running, cancel it because the player interacted
        StopSecondInteractionTimeout();

        if (currentState == NPCState.WaitingForFirstInteraction)
        {
            // First interaction - go to random destination
            Transform chosenDestination = GetRandomAvailableDestination();

            if (chosenDestination != null)
            {
                assignedRandomDestination = chosenDestination;
                occupiedDestinations.Add(assignedRandomDestination);
                currentState = NPCState.MovingToRandomDestination;
                currentDestination = assignedRandomDestination;

                Debug.Log($"NPC moving to random destination: {assignedRandomDestination.name}");
            }
            else
            {
                Debug.LogWarning("NPC: No available destinations! All occupied.");
            }
        }
        else if (currentState == NPCState.WaitingForSecondInteraction)
        {
            // Second interaction - return to spawn and despawn
            if (assignedRandomDestination != null)
            {
                occupiedDestinations.Remove(assignedRandomDestination);
            }

            currentState = NPCState.ReturningToSpawn;
            currentDestination = spawnPoint;

            Debug.Log("NPC returning to spawn point");
        }
    }
    
    private void StartSecondInteractionTimeout()
    {
        StopSecondInteractionTimeout();

        if (secondInteractionTimeout > 0f)
        {
            secondInteractionTimeoutCoroutine = StartCoroutine(SecondInteractionTimeoutCoroutine());
        }
    }

    private void StopSecondInteractionTimeout()
    {
        if (secondInteractionTimeoutCoroutine != null)
        {
            StopCoroutine(secondInteractionTimeoutCoroutine);
            secondInteractionTimeoutCoroutine = null;
        }
    }

    private IEnumerator SecondInteractionTimeoutCoroutine()
    {
        float remaining = secondInteractionTimeout;
        while (remaining > 0f)
        {
            Debug.Log($"Second interaction timeout: {Mathf.CeilToInt(remaining)}s remaining");
            float wait = Mathf.Min(1f, remaining);
            yield return new WaitForSeconds(wait);
            remaining -= wait;
        }

        // Timeout reached: behave as if second interaction happened, but without player
        Debug.Log($"Second interaction timeout ({secondInteractionTimeout}s) reached — NPC will leave.");

        // Increment anxiety because customer left without being served
        if (BarManager.Instance != null)
        {
            BarManager.Instance.AddAnxiety(anxietyPenaltyOnTimeout);
        }
        else
        {
            Debug.LogWarning("BarManager instance not found. Cannot add anxiety.");
        }

        // Clean up occupied destination
        if (assignedRandomDestination != null && occupiedDestinations.Contains(assignedRandomDestination))
        {
            occupiedDestinations.Remove(assignedRandomDestination);
        }

        // Clear assigned destination to avoid double-remove later
        assignedRandomDestination = null;

        // Transition to returning to spawn
        currentState = NPCState.ReturningToSpawn;
        currentDestination = spawnPoint;
        canInteract = false;

        secondInteractionTimeoutCoroutine = null;
        yield break;
    }

    private void SpawnNPC()
    {
        gameObject.SetActive(true);
        transform.position = spawnPoint.position;
        currentState = NPCState.MovingToWaitingPoint;
        currentDestination = waitingPoint;
        Debug.Log("NPC spawned with T key");
    }

    private void Despawn()
    {
        Debug.Log("NPC despawned");

        // Ensure timeout coroutine is stopped when despawning
        StopSecondInteractionTimeout();

        currentState = NPCState.NotSpawned;
        gameObject.SetActive(false);
        // Or use: Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Clean up occupied destination when NPC is destroyed
        if (assignedRandomDestination != null && occupiedDestinations.Contains(assignedRandomDestination))
        {
            occupiedDestinations.Remove(assignedRandomDestination);
        }

        // Ensure coroutine is stopped
        StopSecondInteractionTimeout();
    }

    // Picks a random destination from randomDestinations that is not currently occupied.
    private Transform GetRandomAvailableDestination()
    {
        if (randomDestinations == null || randomDestinations.Length == 0) return null;

        List<Transform> candidates = new List<Transform>();
        foreach (Transform dest in randomDestinations)
        {
            if (dest == null) continue;
            if (!occupiedDestinations.Contains(dest))
            {
                candidates.Add(dest);
            }
        }

        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    // Visual helper in editor
    private void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);
        }

        if (waitingPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(waitingPoint.position, 0.5f);
        }

        if (randomDestinations != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform destination in randomDestinations)
            {
                if (destination != null)
                {
                    Gizmos.DrawWireSphere(destination.position, 0.5f);
                }
            }
        }

        // Draw interaction radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
