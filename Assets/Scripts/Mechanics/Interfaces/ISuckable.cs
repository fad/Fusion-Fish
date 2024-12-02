public interface ISuckable
{
    float NeededSuckingPower { get; }
    
    /// <summary>
    /// Handles the destruction logic for the Suckable
    /// </summary>
    /// <returns>The XP value for the suckable</returns>
    int GetSuckedIn();

    /// <summary>
    /// Sets up the XP value for the suckable
    /// </summary>
    /// <param name="xp">The XP to apply when sucked in.</param>
    void SetupXP(int xp)
    {
        // Noop
    }
}
