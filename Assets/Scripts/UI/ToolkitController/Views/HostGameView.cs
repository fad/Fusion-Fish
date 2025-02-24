using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HostGameView : UIView
{
    private Button _backButton;
    private Button _startButton;
    
    private DropdownField _mapDropdown;
    private DropdownField _fishDropdown;
    private DropdownField _regionDropdown;

    private TextField _sessionNameField;
    private TextField _passwordField;
    
    private const string BackButtonName = "BackButton";
    private const string StartButtonName = "StartButton";
    
    private const string MapDropdownName = "MapField";
    private const string FishDropdownName = "FishField";
    private const string RegionDropdownName = "RegionDropdown";
    
    private const string SessionNameFieldName = "HostNameField";
    private const string PasswordFieldName = "PasswordField";
    
    private const string MapsResourcePath = "Map Data";
    private const string FishResourcePath ="UI Fish Data";
    
    private List<MapSO> _maps;
    private List<UI_PlayableFishSO> _fishes;
    
    public HostGameView(VisualElement topElement) : base(topElement)
    {
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        _backButton = TopElement.Q<Button>(BackButtonName);
        _startButton = TopElement.Q<Button>(StartButtonName);
        _mapDropdown = TopElement.Q<DropdownField>(MapDropdownName);
        _fishDropdown = TopElement.Q<DropdownField>(FishDropdownName);
        _regionDropdown = TopElement.Q<DropdownField>(RegionDropdownName);
        _sessionNameField = TopElement.Q<TextField>(SessionNameFieldName);
        _passwordField = TopElement.Q<TextField>(PasswordFieldName);
        
    }
    
    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
        _startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
        
        _sessionNameField.RegisterCallback<KeyDownEvent>(OnHostNameFieldChanged);
        _passwordField.RegisterCallback<KeyDownEvent>(OnPasswordFieldChanged);
        
        _mapDropdown.RegisterValueChangedCallback(OnMapChanged);
        _fishDropdown.RegisterValueChangedCallback(OnFishChanged);
        _regionDropdown.RegisterValueChangedCallback(OnRegionChanged);
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeToEvents();
        
        // TODO: Refactor this
        LoadMaps();
        LoadFishes();
    }

    public override void Dispose()
    {
        base.Dispose();
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        MultiplayerMenuEvents.OnRegionSelected += OnRegionSelection_ViewUpdate;
    }
    
    private void UnsubscribeFromEvents()
    {
        MultiplayerMenuEvents.OnRegionSelected -= OnRegionSelection_ViewUpdate;
    }

    private void LoadFishes()
    {
        _fishes = new List<UI_PlayableFishSO>();
        UI_PlayableFishSO[] loadedFishes = Resources.LoadAll<UI_PlayableFishSO>(FishResourcePath);
        
        foreach (UI_PlayableFishSO fish in loadedFishes)
        {
            _fishDropdown.choices.Add(fish.FishNameToDisplay);
            _fishes.Add(fish);
        }
        
        _fishDropdown.index = 0;
        MultiplayerMenuEvents.OnFishSelected?.Invoke(_fishes[0]);
    }

    private void LoadMaps()
    {
        _maps = new List<MapSO>();
        MapSO[] loadedMaps = Resources.LoadAll<MapSO>(MapsResourcePath);

        foreach(MapSO map in loadedMaps)
        {
            _mapDropdown.choices.Add(map.Name);
            _maps.Add(map);
        }
        
        _mapDropdown.index = 0;
        MultiplayerMenuEvents.OnMapSelected?.Invoke(_maps[0]);
    }

    private void OnMapChanged(ChangeEvent<string> evt)
    {
        int selectedMapIndex = _mapDropdown.index;

        if (selectedMapIndex >= 0)
        {
            MapSO selectedMap = _maps[selectedMapIndex];
            MultiplayerMenuEvents.OnMapSelected?.Invoke(selectedMap);
        }
    }
    
    private void OnHostNameFieldChanged(KeyDownEvent evt)
    {
        MultiplayerMenuEvents.OnSessionNameChanged?.Invoke(_sessionNameField.value);
    }

    private void OnPasswordFieldChanged(KeyDownEvent evt)
    {
        
    }

    private void OnStartButtonClicked(ClickEvent evt)
    {
        MultiplayerMenuEvents.OnStartHostClicked?.Invoke();
    }

    private void OnBackButtonClicked(ClickEvent evt)
    {
        MultiplayerMenuEvents.OnHostViewBackButtonClicked?.Invoke();
    }
    
    
    private void OnRegionChanged(ChangeEvent<string> evt)
    {
        int selectedRegionIndex = _regionDropdown.index;
        
        if (selectedRegionIndex >= 0)
        {
            string selectedRegion = _regionDropdown.choices[selectedRegionIndex];
            MultiplayerMenuEvents.OnRegionSelected?.Invoke(selectedRegion);
        }
    }

    private void OnFishChanged(ChangeEvent<string> evt)
    {
        int selectedFishIndex = _fishDropdown.index;
        
        if (selectedFishIndex >= 0)
        {
            UI_PlayableFishSO selectedFish = _fishes[selectedFishIndex];
            MultiplayerMenuEvents.OnFishSelected?.Invoke(selectedFish);
        }
    }
    
    
    private void OnRegionSelection_ViewUpdate(string region)
    {
        _regionDropdown.value = region;
    }
}
