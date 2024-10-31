using System;

public interface IHealthManager
{
        event Action<float> OnHealthChanged;
        event Action OnDeath;
        
        bool Died { get; }
    
        void Damage(float amount);
        void Heal(float amount);
}
