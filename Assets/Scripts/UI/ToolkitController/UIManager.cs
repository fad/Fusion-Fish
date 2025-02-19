using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [Header("UI related"), SerializeField]
    private UIDocument ui;
    
    private VisualElement _root;
    
    private readonly List<UIView> _views = new();
    
    // Full screen views
    private UIView _mainMenuView;
    private UIView _modeSelectionView;
    private UIView _mapSelectionView;
    private UIView _fishSelectionView;
    private UIView _multiplayerMenuView;
    private UIView _hostGameView;
    
    // VisualTree string IDs for UIViews; each one represents a branch in the VisualTree
    private const string MainMenuVisualElementName = "MainMenu";
    private const string ModeSelectionVisualElementName = "ModeSelection";
    private const string MapSelectionVisualElementName = "MapSelection";
    private const string FishSelectionVisualElementName = "FishSelection";
    private const string MultiplayerMenuVisualElementName = "MultiplayerMenu";
    private const string HostGameVisualElementName = "HostGameView";
    
    private UIView _currentView;
    private UIView _previousView;
    
    
    private void Awake()
    {
        if(!ui) return;
        
        _root = ui.rootVisualElement;
    }

    private void OnEnable()
    {
        if (!ui) return;
        
        SetupViews();
        SubscribeToEvents();
        
        AddSoundsToButtons();
        ShowView(_mainMenuView);
        
    }


    private void OnDisable()
    {
        foreach (var view in _views)
        {
            view.Dispose();
        }
        
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Adds a click sound to every button in the UI Document.
    /// </summary>
    private void AddSoundsToButtons()
    {
        List<Button> buttons = _root.Query<Button>().ToList();
        
        foreach (var button in buttons)
        {
            button.clicked += () => AudioManager.Instance.Play("click");
        }
    }

    private void SetupViews()
    {
        _mainMenuView = new MainMenuView(_root.Q(MainMenuVisualElementName));
        _modeSelectionView = new ModeSelectionView(_root.Q(ModeSelectionVisualElementName));
        _mapSelectionView = new MapSelectionView(_root.Q(MapSelectionVisualElementName));
        _fishSelectionView = new FishSelectionView(_root.Q(FishSelectionVisualElementName));
        _multiplayerMenuView = new MultiplayerMenuView(_root.Q(MultiplayerMenuVisualElementName));
        _hostGameView = new HostGameView(_root.Q(HostGameVisualElementName));
        
        
        // Track UI views in a list for disposal
        _views.Add(_mainMenuView);
        _views.Add(_modeSelectionView);
        _views.Add(_mapSelectionView);
        _views.Add(_fishSelectionView);
        _views.Add(_multiplayerMenuView);
        _views.Add(_hostGameView);
        
        
    }

    private void SubscribeToEvents()
    {
        MainMenuEvents.OnSingleplayerButtonClicked += SingleplayerButtonClicked;
        MainMenuEvents.OnMultiplayerButtonClicked += MultiplayerButtonClicked;
        // ModeSelectionEvents.OnBackButtonClicked += ModeBackButtonClicked;
        // ModeSelectionEvents.OnNextButtonClicked += ModeNextButtonClicked;
        MapSelectionEvents.OnBackButtonClicked += MapBackButtonClicked;
        MapSelectionEvents.OnNextButtonClicked += MapNextButtonClicked;
        FishSelectionEvents.OnBackButtonClicked += FishBackButtonClicked;
        MultiplayerMenuEvents.OnBackButtonClicked += MultiplayerBackButtonClicked;
        MultiplayerMenuEvents.OnHostGameButtonClicked += HostGameButtonClicked;
        MultiplayerMenuEvents.OnHostViewBackButtonClicked += HostViewBackButtonClicked;
        

    }



    private void UnsubscribeFromEvents()
    {
        MainMenuEvents.OnSingleplayerButtonClicked -= SingleplayerButtonClicked;
        MainMenuEvents.OnMultiplayerButtonClicked -= MultiplayerButtonClicked;
        // ModeSelectionEvents.OnBackButtonClicked -= ModeBackButtonClicked;
        // ModeSelectionEvents.OnNextButtonClicked -= ModeNextButtonClicked;
        MapSelectionEvents.OnBackButtonClicked -= MapBackButtonClicked;
        MapSelectionEvents.OnNextButtonClicked -= MapNextButtonClicked;
        FishSelectionEvents.OnBackButtonClicked -= FishBackButtonClicked;
        MultiplayerMenuEvents.OnBackButtonClicked -= MultiplayerBackButtonClicked;
        MultiplayerMenuEvents.OnHostGameButtonClicked -= HostGameButtonClicked;
        MultiplayerMenuEvents.OnHostViewBackButtonClicked -= HostViewBackButtonClicked;

    }

    private void HostViewBackButtonClicked()
    {
        ShowView(_multiplayerMenuView);
    }

    private void HostGameButtonClicked()
    {
        ShowView(_hostGameView);
    }

    private void MultiplayerBackButtonClicked()
    {
        ShowView(_mainMenuView);
    }

    private void SingleplayerButtonClicked()
    {
        // ShowView(_modeSelectionView);
        ShowView(_mapSelectionView);
    }

    private void MultiplayerButtonClicked()
    {
        ShowView(_multiplayerMenuView);
    }
    
    private void ModeBackButtonClicked()
    {
        ShowView(_mainMenuView);
    }
    
    private void ModeNextButtonClicked()
    {
        ShowView(_mapSelectionView);
    }
    
    private void MapBackButtonClicked()
    {
        // ShowView(_modeSelectionView);
        ShowView(_mainMenuView);
    }
    
    
    private void MapNextButtonClicked()
    {
        ShowView(_fishSelectionView);
    }
    
    private void FishBackButtonClicked()
    {
        ShowView(_mapSelectionView);
    }

    /// <summary>
    /// Toggle the visibility of the given view.
    /// </summary>
    /// <param name="newView">The new view to show</param>
    private void ShowView(UIView newView)
    {
        _currentView?.Hide();

        _previousView = _currentView;
        _currentView = newView;

        _currentView?.Show();
    }
    

}
