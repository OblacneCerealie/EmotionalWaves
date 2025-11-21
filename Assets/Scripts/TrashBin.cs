using UnityEngine;
using UnityEngine.InputSystem;

public class TrashBin : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;

    [Header("Player Reference")]
    [SerializeField] private Transform player;
    [SerializeField] private CoffeeHolder coffeeHolder;

    private void Update()
    {
        if (player == null || coffeeHolder == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= interactionDistance && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryThrowAway();
        }
    }

    private void TryThrowAway()
    {
        if (!coffeeHolder.IsHoldingCoffee())
        {
            Debug.Log("Trash Bin: No coffee to throw away!");
            return;
        }

        coffeeHolder.DestroyCoffee();
        Debug.Log("Trash Bin: Coffee thrown away!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}

