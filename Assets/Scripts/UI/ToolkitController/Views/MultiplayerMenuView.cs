using UnityEngine;
using UnityEngine.UIElements;

public class MultiplayerMenuView : UIView
{
    private Button _backButton;
    private Button _hostGameButton;
    private Button _findGameButton;
    
    private TextField _playerHandleField;
    
    private DropdownField _regionDropdown;
    
    private const string BackButtonName = "BackButton";
    private const string HostGameButtonName = "HostButton";
    private const string FindGameButtonName = "FindButton";
    
    private const string PlayerHandleFieldName = "PlayerNameTextField";
    
    private const string RegionDropdownName = "RegionDropdown";
    
    public MultiplayerMenuView(VisualElement topElement) : base(topElement)
    {
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        _backButton = TopElement.Q<Button>(BackButtonName);
        _hostGameButton = TopElement.Q<Button>(HostGameButtonName);
        _findGameButton = TopElement.Q<Button>(FindGameButtonName);
        _playerHandleField = TopElement.Q<TextField>(PlayerHandleFieldName);
        _regionDropdown = TopElement.Q<DropdownField>(RegionDropdownName);
    }

    protected override void RegisterButtonCallbacks()
    {
        _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
        _hostGameButton.RegisterCallback<ClickEvent>(OnHostGameButtonClicked);
        _findGameButton.RegisterCallback<ClickEvent>(OnFindGameButtonClicked);
        _playerHandleField.RegisterCallback<KeyDownEvent>(OnPlayerHandleChanged);
        
        _regionDropdown.RegisterValueChangedCallback(OnRegionChanged);
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeToEvents();
    }

    public override void Dispose()
    {
        base.Dispose();
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        MultiplayerMenuEvents.OnStart_PlayerHandleLoaded += OnStart_PlayerHandleLoaded;
        MultiplayerMenuEvents.OnRegionSelected += OnRegionSelected;
    }

    private void UnsubscribeFromEvents()
    {
        MultiplayerMenuEvents.OnStart_PlayerHandleLoaded -= OnStart_PlayerHandleLoaded;
        MultiplayerMenuEvents.OnRegionSelected -= OnRegionSelected;
    }
    
    /// <summary>
    /// Overall callback invoking the region dropdown change event.
    /// </summary>
    /// <param name="evt"></param>
    private void OnRegionChanged(ChangeEvent<string> evt)
    {
        int selectedRegionIndex = _regionDropdown.index;
        
        if (selectedRegionIndex >= 0)
        {
            string selectedRegion = _regionDropdown.choices[selectedRegionIndex];
            MultiplayerMenuEvents.OnRegionSelected?.Invoke(selectedRegion);
        }
    }

    /// <summary>
    /// UI Callback for when the region dropdown is changed.
    /// </summary>
    /// <param name="region">Selected region</param>
    private void OnRegionSelected(string region)
    {
        _regionDropdown.value = region;
    }

    private void OnStart_PlayerHandleLoaded(string playerName)
    {
        _playerHandleField.value = playerName;
    }

    private void OnPlayerHandleChanged(KeyDownEvent evt)
    {
        MultiplayerMenuEvents.OnPlayerHandleChanged?.Invoke(_playerHandleField.value);
    }

    private void OnBackButtonClicked(ClickEvent evt)
    {
        MultiplayerMenuEvents.OnBackButtonClicked?.Invoke();
    }

    private void OnHostGameButtonClicked(ClickEvent evt)
    {
        MultiplayerMenuEvents.OnHostGameButtonClicked?.Invoke();
    }

    private void OnFindGameButtonClicked(ClickEvent evt)
    {
        MultiplayerMenuEvents.OnFindGameButtonClicked?.Invoke();
    }
}
