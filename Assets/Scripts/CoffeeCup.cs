using UnityEngine;
using UnityEngine.InputSystem;

public class CoffeeCup : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;

    [Header("Player Reference")]
    [SerializeField] private Transform player;
    [SerializeField] private CoffeeHolder coffeeHolder;

    // Coffee properties
    private bool hasMilk = false;
    private bool hasSugar = false;
    private bool isBeingHeld = false;

    public bool HasMilk => hasMilk;
    public bool HasSugar => hasSugar;
    public bool IsBeingHeld => isBeingHeld;

    private void Update()
    {
        // Only allow pickup if not being held
        if (isBeingHeld) return;

        // Auto-find references if not set
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (coffeeHolder == null)
        {
            coffeeHolder = FindObjectOfType<CoffeeHolder>();
        }

        if (player == null || coffeeHolder == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= interactionDistance && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickup();
        }
    }

    private void TryPickup()
    {
        if (coffeeHolder.IsHoldingCoffee())
        {
            Debug.Log("Coffee Cup: Player already holding coffee!");
            return;
        }

        coffeeHolder.PickupCoffee(this);
        isBeingHeld = true;
        Debug.Log("Coffee Cup: Picked up!");
    }

    public void AddMilk()
    {
        if (!hasMilk)
        {
            hasMilk = true;
            Debug.Log("Coffee Cup: Milk added!");
        }
        else
        {
            Debug.Log("Coffee Cup: Already has milk!");
        }
    }

    public void AddSugar()
    {
        if (!hasSugar)
        {
            hasSugar = true;
            Debug.Log("Coffee Cup: Sugar added!");
        }
        else
        {
            Debug.Log("Coffee Cup: Already has sugar!");
        }
    }

    public void Drop()
    {
        isBeingHeld = false;
        Debug.Log("Coffee Cup: Dropped!");
    }

    private void OnDrawGizmosSelected()
    {
        if (!isBeingHeld)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}

