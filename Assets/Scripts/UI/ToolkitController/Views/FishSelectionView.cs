using UnityEngine;
using UnityEngine.UIElements;

public class FishSelectionView : UIView
{
    private Button _backButton;
    private Button _startButton;
    
    private VisualElement _fishSelectionContainer;
    
    private const string BackButtonName = "BackButton";
    private const string StartButtonName = "StartButton";
    
    private const string FishSelectionContainerName = "unity-content-container";
    private const string UIFishResourcePath = "UI Fish Data";
    
    private const string FishButtonClassName = "fishSelectionButton";
    
    public FishSelectionView(VisualElement topElement) : base(topElement)
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        InstantiateFishSelectionButtons();
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        _backButton = TopElement.Q<Button>(BackButtonName);
        _startButton = TopElement.Q<Button>(StartButtonName);
        _fishSelectionContainer = TopElement.Q<VisualElement>(FishSelectionContainerName);
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
        _startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
    }

    // Maybe put this in the presenter? Does this hurt the MVP principle?
    private void InstantiateFishSelectionButtons()
    {
        UI_PlayableFishSO[] fishes = Resources.LoadAll<UI_PlayableFishSO>(UIFishResourcePath);
        
        foreach (UI_PlayableFishSO fish in fishes)
        {
            Button fishButton = new Button
            {
                text = fish.FishNameToDisplay
            };
            
            fishButton.AddToClassList(FishButtonClassName);
            
            BindFishButton(fishButton, fish);
            _fishSelectionContainer.Add(fishButton);
        }
    }

    private void BindFishButton(Button fishButton, UI_PlayableFishSO fish)
    {
        fishButton.RegisterCallback<ClickEvent>(OnFishButtonClicked);
        
        void OnFishButtonClicked(ClickEvent evt)
        {
            FishSelectionEvents.OnFishButtonClicked?.Invoke(fish);
        }
    }

    private void OnBackButtonClicked(ClickEvent evt)
    {
        FishSelectionEvents.OnBackButtonClicked?.Invoke();
    }
    
    private void OnStartButtonClicked(ClickEvent evt)
    {
        FishSelectionEvents.OnStartButtonClicked?.Invoke();
    }
}
