using UnityEngine;

namespace Trea.Example
{
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Debug.Log($"{name} 被击败了");
            }
        }
    }
}
