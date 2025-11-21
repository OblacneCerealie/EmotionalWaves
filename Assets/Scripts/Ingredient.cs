using UnityEngine;
using UnityEngine.InputSystem;

public class Ingredient : MonoBehaviour
{
    [Header("Ingredient Type")]
    [SerializeField] private IngredientType ingredientType;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;

    [Header("Player Reference")]
    [SerializeField] private Transform player;
    [SerializeField] private CoffeeHolder coffeeHolder;

    public enum IngredientType
    {
        Milk,
        Sugar
    }

    private void Update()
    {
        if (player == null || coffeeHolder == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= interactionDistance && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryAddIngredient();
        }
    }

    private void TryAddIngredient()
    {
        if (!coffeeHolder.IsHoldingCoffee())
        {
            Debug.Log($"Ingredient: Player must be holding coffee to add {ingredientType}!");
            return;
        }

        CoffeeCup currentCoffee = coffeeHolder.GetCurrentCoffee();

        if (currentCoffee == null) return;

        switch (ingredientType)
        {
            case IngredientType.Milk:
                currentCoffee.AddMilk();
                break;

            case IngredientType.Sugar:
                currentCoffee.AddSugar();
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = ingredientType == IngredientType.Milk ? Color.white : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}

