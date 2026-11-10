using UnityEngine;

namespace Lampkeeper
{
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }

    /// <summary>체력을 가진 모든 캐릭터의 베이스. 플레이어와 적이 상속한다.</summary>
    public abstract class Actor : MonoBehaviour, IDamageable
    {
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        protected void ResetHealth(float max)
        {
            MaxHealth = max;
            CurrentHealth = max;
            IsDead = false;
            OnHealthChanged();
        }

        protected void IncreaseMaxHealth(float amount)
        {
            MaxHealth += amount;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            OnHealthChanged();
        }

        public virtual void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHealthChanged();
            OnDamaged(amount);

            if (CurrentHealth <= 0f)
            {
                IsDead = true;
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged();
        }

        protected virtual void OnHealthChanged() { }
        protected virtual void OnDamaged(float amount) { }
        protected abstract void Die();
    }
}
