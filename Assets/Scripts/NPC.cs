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
    [SerializeField] private CoffeeHolder coffeeHolder;

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
        
        // Find coffee holder
        coffeeHolder = FindObjectOfType<CoffeeHolder>();
        
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
            // Second interaction - check if player has coffee
            if (coffeeHolder != null && coffeeHolder.IsHoldingCoffee())
            {
                // Player gave coffee - take it and leave
                CoffeeCup coffee = coffeeHolder.GetCurrentCoffee();
                Debug.Log($"NPC received coffee! Has Milk: {coffee.HasMilk}, Has Sugar: {coffee.HasSugar}");
                
                coffeeHolder.DestroyCoffee();
                
                // Return to spawn and despawn
                if (assignedRandomDestination != null)
                {
                    occupiedDestinations.Remove(assignedRandomDestination);
                }

                currentState = NPCState.ReturningToSpawn;
                currentDestination = spawnPoint;

                Debug.Log("NPC satisfied! Returning to spawn point");
            }
            else
            {
                // No coffee - just regular interaction (old behavior)
                Debug.Log("NPC: Player doesn't have coffee!");
                canInteract = true; // Allow interaction again
            }
        }
    }

    private Transform GetRandomAvailableDestination()
    {
        List<Transform> availableDestinations = new List<Transform>();

        foreach (Transform destination in randomDestinations)
        {
            if (!occupiedDestinations.Contains(destination))
            {
                availableDestinations.Add(destination);
            }
        }

        if (availableDestinations.Count > 0)
        {
            int randomIndex = Random.Range(0, availableDestinations.Count);
            return availableDestinations[randomIndex];
        }

        return null;
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
