using UnityEngine;
using UnityEngine.UIElements;

public class MapSelectionView : UIView
{
    private Button _backButton;
    private Button _nextButton;
    
    private VisualElement _mapSelectionContainer;
    private VisualElement _selectionImage;
    
    private const string BackButtonName = "BackButton";
    private const string NextButtonName = "NextButton";
    private const string MapSelectionContainerName = "unity-content-container";
    private const string SelectionImageName = "SelectionImage";

    private const string MapResourcePath = "Map Data";
    
    private const string MapButtonClassName = "mapButton";
    
    public MapSelectionView(VisualElement topElement) : base(topElement)
    {
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        _backButton = TopElement.Q<Button>(BackButtonName);
        _nextButton = TopElement.Q<Button>(NextButtonName);
        _mapSelectionContainer = TopElement.Q<VisualElement>(MapSelectionContainerName);
        _selectionImage = TopElement.Q<VisualElement>(SelectionImageName);
    }
    
    protected override void RegisterButtonCallbacks()
    {
        _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
        _nextButton.RegisterCallback<ClickEvent>(OnNextButtonClicked);
    }

    public override void Initialize()
    {
        base.Initialize();
        InstantiateMapSelectionButtons();
    }

    private void InstantiateMapSelectionButtons()
    {
        MapSO[] maps = Resources.LoadAll<MapSO>(MapResourcePath);

        foreach (var map in maps)
        {
            Button mapButton = new Button
            {
                text = map.Name
            };
            
            mapButton.AddToClassList(MapButtonClassName);
            
            BindMapButton(mapButton, map);
            _mapSelectionContainer.Add(mapButton);
        }

    }

    private void BindMapButton(Button button, MapSO map)
    {
        button.RegisterCallback<ClickEvent>(OnMapButtonClicked);

        void OnMapButtonClicked(ClickEvent evt)
        {
            _selectionImage.style.backgroundImage = new StyleBackground(map.MapImage);
            MapSelectionEvents.OnMapSelected?.Invoke(map);
        }
    }
    
    private void OnNextButtonClicked(ClickEvent evt)
    {
        MapSelectionEvents.OnNextButtonClicked?.Invoke();
    }
    
    private void OnBackButtonClicked(ClickEvent evt)
    {
        MapSelectionEvents.OnBackButtonClicked?.Invoke();
    }
    
}
