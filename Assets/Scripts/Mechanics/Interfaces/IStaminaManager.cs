public interface IStaminaManager
{
    /// <summary>
    /// The current stamina of the entity.
    /// </summary>
    short CurrentStamina { get; }

    /// <summary>
    /// Decreases the stamina of the entity with their own Decrease Rate.
    /// </summary>
    void Decrease();
    void Recavery();
    
    /// <summary>
    /// Starts regenerating the stamina of the entity with their own Regen Rate.
    /// </summary>
    void Regenerate();
}