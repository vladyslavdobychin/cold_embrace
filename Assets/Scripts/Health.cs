using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;

    private int currentHealth;

    void Awake() => currentHealth = maxHealth;

    void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} took {amount} damage, {currentHealth}/{maxHealth} left");

        if (currentHealth <= 0)
        {
            Debug.Log($"{name} destroyed");
            Destroy(gameObject);
        }
    }
}
