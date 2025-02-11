using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [Header("UI related"), SerializeField]
    private UIDocument ui;
    
    private VisualElement _root;
    
    private List<UIView> _views = new();
    
    // Full screen views
    private UIView _mainMenuView;
    
    // VisualTree string IDs for UIViews; each one represents a branch in the VisualTree
    private const string MainMenuVisualElementName = "MainMenu";
    
    
    private void Awake()
    {
        if(!ui) return;
        
        _root = ui.rootVisualElement;
    }
    
    private void OnEnable()
    {
        if(!ui) return;
        
        AddSoundsToButtons();
        SetupViews();
        
        _mainMenuView.Show();
    }

    private void OnDisable()
    {
        foreach (var view in _views)
        {
            view.Dispose();
        }
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
        
        // Track UI views in a list for disposal
        _views.Add(_mainMenuView);
        
        
    }
    

}
