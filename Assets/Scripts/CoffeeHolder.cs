using UnityEngine;

public class CoffeeHolder : MonoBehaviour
{
    [Header("Coffee Holding Settings")]
    [SerializeField] private Transform coffeeHoldPoint;

    private CoffeeCup currentCoffee = null;

    public bool IsHoldingCoffee()
    {
        return currentCoffee != null;
    }

    public CoffeeCup GetCurrentCoffee()
    {
        return currentCoffee;
    }

    public void PickupCoffee(CoffeeCup coffee)
    {
        if (currentCoffee != null)
        {
            Debug.LogWarning("CoffeeHolder: Already holding coffee!");
            return;
        }

        if (coffeeHoldPoint == null)
        {
            Debug.LogError("CoffeeHolder: Coffee hold point not assigned!");
            return;
        }

        currentCoffee = coffee;

        // Move coffee to hold point
        coffee.transform.SetParent(coffeeHoldPoint);
        coffee.transform.localPosition = Vector3.zero;
        coffee.transform.localRotation = Quaternion.identity;

        // Disable physics if any
        Rigidbody rb = coffee.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Debug.Log("CoffeeHolder: Now holding coffee!");
    }

    public void DropCoffee()
    {
        if (currentCoffee == null) return;

        // Unparent coffee
        currentCoffee.transform.SetParent(null);

        // Re-enable physics if any
        Rigidbody rb = currentCoffee.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        currentCoffee.Drop();
        currentCoffee = null;

        Debug.Log("CoffeeHolder: Dropped coffee!");
    }

    public void DestroyCoffee()
    {
        if (currentCoffee == null) return;

        Destroy(currentCoffee.gameObject);
        currentCoffee = null;

        Debug.Log("CoffeeHolder: Coffee destroyed!");
    }

    private void OnDrawGizmosSelected()
    {
        if (coffeeHoldPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(coffeeHoldPoint.position, 0.1f);
        }
    }
}

