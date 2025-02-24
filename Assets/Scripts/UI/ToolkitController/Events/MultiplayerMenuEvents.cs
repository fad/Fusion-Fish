using System;

public static class MultiplayerMenuEvents
{
    public static Action OnBackButtonClicked;
    public static Action OnHostGameButtonClicked;
    public static Action OnFindGameButtonClicked;
    
    public static Action<string> OnRegionSelected;
    public static Action<string> OnPlayerHandleChanged;
    public static Action<string> OnSessionNameChanged;

    public static Action<string> OnStart_PlayerHandleLoaded;
    
    public static Action<MapSO> OnMapSelected;
    public static Action<UI_PlayableFishSO> OnFishSelected;


    public static Action OnHostViewBackButtonClicked;
    public static Action OnStartHostClicked;
}
