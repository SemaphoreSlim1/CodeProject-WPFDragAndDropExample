using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DragDrop
{
public class DragDropPreviewBase : UserControl
{
    public DragDropPreviewBase()
    {
        ScaleTransform scale = new ScaleTransform(1f, 1f);
        SkewTransform skew = new SkewTransform(0f, 0f);
        RotateTransform rotate = new RotateTransform(0f);
        TranslateTransform trans = new TranslateTransform(0f, 0f);
        TransformGroup transGroup = new TransformGroup();
        transGroup.Children.Add(scale);
        transGroup.Children.Add(skew);
        transGroup.Children.Add(rotate);
        transGroup.Children.Add(trans);

        this.RenderTransform = transGroup;
    }

    #region DropState Dependency Property

    #region Binding Property

    /// <summary>
    /// Gets and sets drop state for this drag and drop preview control
    /// </summary>
    public DropState DropState
    {
        get { return (DropState)GetValue(DropStateProperty); }
        set { SetValue(DropStateProperty, value); }
    }

    #endregion

    #region Dependency Property

    /// <summary>
    /// The backing <see cref="DependencyProperty"/> which enables animation, styling, binding, etc. for <see cref="DropState"/>
    /// </summary>
    public static readonly DependencyProperty DropStateProperty =
        DependencyProperty.Register("DropState", typeof(DropState), typeof(DragDropPreviewBase), new UIPropertyMetadata(DropStateChanged));

    /// <summary>
    /// Handles when drop state changes
    /// </summary>
    /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="DropStateProperty"/>, is attched to.</param>
    /// <param name="e"><see cref="DependencyPropertyChangedEventArgs"/> from the changed event</param>
    public static void DropStateChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        var instance = (DragDropPreviewBase)element;
        instance.StateChangedHandler(element, e);
    }

    public virtual void StateChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { }

    #endregion

    #endregion
}
}
