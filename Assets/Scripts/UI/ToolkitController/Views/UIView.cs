using System;
using UnityEngine.UIElements;

/// <summary>
/// Base class for a functional unit of the UI. This can make up a whole screen or a part of it.
/// </summary>
public class UIView : IDisposable
{
    /// <summary>
    /// Whether the view should be hidden on awake.
    /// </summary>
    protected bool HideOnAwake = true;
    
    /// <summary>
    /// Whether the UI reveals other underlying UI or is partially see-through
    /// </summary>
    protected bool IsOverlay;
    
    /// <summary>
    /// The Root element of the view.
    /// </summary>
    protected VisualElement TopElement;

    /// <summary>
    /// The Root of the view
    /// </summary>
    public VisualElement Root => TopElement;
    
    /// <summary>
    /// Whether the view is currently transparent or not.
    /// </summary>
    public bool IsTransparent => IsOverlay;
    
    /// <summary>
    /// Whether the view is currently hidden or not.
    /// </summary>
    public bool IsHidden => TopElement.style.display == DisplayStyle.None;
    
    /// <summary>
    /// Initializes the view with the given top element.
    /// </summary>
    /// <param name="topElement">The top most VisualElement in the UXML hierarchy.</param>
    /// <exception cref="ArgumentNullException">When null is given as a <paramref name="topElement"/></exception>
    public UIView(VisualElement topElement)
    {
        TopElement = topElement ?? throw new ArgumentNullException(nameof(topElement));
        Initialize();
    }

    public virtual void Initialize()
    {
        if (HideOnAwake)
        {
            Hide();
        }
        
        SetVisualElements();
        RegisterButtonCallbacks();
    }

    /// <summary>
    /// Sets up the VisualElements for the UI. Override for customization.
    /// </summary>
    protected virtual void SetVisualElements()
    {
        
    }

    /// <summary>
    /// Registers callbacks for buttons. Override for customization.
    /// </summary>
    protected virtual void RegisterButtonCallbacks()
    {
        
    }
    
    /// <summary>
    /// Displays the UI.
    /// </summary>
    public virtual void Show()
    {
        TopElement.style.display = DisplayStyle.Flex;
    }
    
    /// <summary>
    /// Hides the UI.
    /// </summary>
    public virtual void Hide()
    {
        TopElement.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Unregisters any callbacks or event handlers. Override for customization.
    /// </summary>
    public virtual void Dispose()
    {
        
    }
}
