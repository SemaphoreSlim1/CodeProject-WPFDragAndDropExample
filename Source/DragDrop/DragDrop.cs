using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;


namespace DragDrop
{
    /// <summary>
    /// 
    /// </summary>
    public class DragDrop
    {
        private Window _topWindow;

        /// <summary>
        /// The location where we first started the drag operation
        /// </summary>
        private Point _initialMousePosition;

        /// <summary>
        /// The outmost canvas which the user can drag the <see cref="_dragDropPreviewControl"/>
        /// </summary>
        private Canvas _dragDropContainer;

        /// <summary>
        /// The control which will serve as the drop arget
        /// </summary>
        private UIElement _dropTarget;

        /// <summary>
        /// Determines if we're currently tracking the mouse
        /// </summary>
        private Boolean _mouseCaptured;

        /// <summary>
        /// The control that's displayed (and moving with the mouse) during a drag drop operation
        /// </summary>
        private DragDropPreviewBase _dragDropPreviewControl;

        /// <summary>
        /// The data context of the <see cref="_dragDropPreviewControl"/>
        /// </summary>
        private Object _dragDropPreviewControlDataContext;

        /// <summary>
        /// The command to execute when items are dropped
        /// </summary>
        private ICommand _itemDroppedCommand;

        private Point _delta;


        #region Instance

        /// <summary>
        /// Lazy loaded backing member variable for <see cref="Instance"/>
        /// </summary>
        private static readonly Lazy<DragDrop> _Instance = new Lazy<DragDrop>(() => new DragDrop());

        /// <summary>
        /// Gets a static instance of <see cref="DragDrop"/>
        /// </summary>
        private static DragDrop Instance
        {
            get { return _Instance.Value; }
        }

        #endregion

        #region ItemDropped Attached Property

        #region Backing Dependency Property

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> which enables animation, styling, binding, etc. for ItemDropped
        /// </summary>
        public static readonly DependencyProperty ItemDroppedProperty = DependencyProperty.RegisterAttached(
            "ItemDropped", typeof(ICommand), typeof(DragDrop), new PropertyMetadata(new PropertyChangedCallback(AttachOrRemoveItemDroppedEvent)));



        #endregion

        #region Get and set

        /// <summary>
        /// Gets the attached value of ItemDropped
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="ItemDroppedProperty"/>, is attched to.</param>
        /// <returns>The attached value</returns>
        public static ICommand GetItemDropped(DependencyObject element)
        {
            return (ICommand)element.GetValue(ItemDroppedProperty);
        }

        /// <summary>
        /// Sets the attached value of ItemDropped
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="ItemDroppedProperty"/>, is attched to.</param>
        /// <param name="value">the value to set</param>
        public static void SetItemDropped(DependencyObject element, ICommand value)
        {
            element.SetValue(ItemDroppedProperty, value);
        }

        private static void AttachOrRemoveItemDroppedEvent(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
        }

        #endregion

        #endregion

        #region IsDragSource Attached Property

        #region Backing Dependency Property

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> which enables animation, styling, binding, etc. for IsDragSource
        /// </summary>
        public static readonly DependencyProperty IsDragSourceProperty = DependencyProperty.RegisterAttached(
            "IsDragSource", typeof(Boolean), typeof(DragDrop), new PropertyMetadata(false, IsDragSourceChanged));

        #endregion

        #region Get and set

        /// <summary>
        /// Gets the attached value of IsDragSource
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="IsDragSourceProperty"/>, is attched to.</param>
        /// <returns>The attached value</returns>
        public static Boolean GetIsDragSource(DependencyObject element)
        {
            return (Boolean)element.GetValue(IsDragSourceProperty);
        }

        /// <summary>
        /// Sets the attached value of IsDragSource
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="IsDragSourceProperty"/>, is attched to.</param>
        /// <param name="value">the value to set</param>
        public static void SetIsDragSource(DependencyObject element, Boolean value)
        {
            element.SetValue(IsDragSourceProperty, value);
        }

        /// <summary>
        /// Handles when <see cref="IsDragSourceProperty"/>'s value changes
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="IsDragSourceProperty"/>, is attched to.</param>
        /// <param name="e"><see cref="DependencyPropertyChangedEventArgs"/> from the changed event</param>
        private static void IsDragSourceChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            var dragSource = element as UIElement;
            if (dragSource == null)
            { return; }

            if (Object.Equals(e.NewValue, true))
            {
                dragSource.PreviewMouseLeftButtonDown += Instance.DragSource_PreviewMouseLeftButtonDown;
                dragSource.PreviewMouseLeftButtonUp += Instance.DragSource_PreviewMouseLeftButtonUp;
                dragSource.PreviewMouseMove += Instance.DragSource_PreviewMouseMove;
            }
            else
            {
                dragSource.PreviewMouseLeftButtonDown -= Instance.DragSource_PreviewMouseLeftButtonDown;
                dragSource.PreviewMouseLeftButtonUp -= Instance.DragSource_PreviewMouseLeftButtonUp;
                dragSource.PreviewMouseMove -= Instance.DragSource_PreviewMouseMove;
            }
        }

        #region Drag handlers

        /// <summary>
        /// Tunneled event handler for when the mouse left button is about to be depressed
        /// </summary>
        /// <param name="sender">The object which invokes this event</param>
        /// <param name="e">event args from the sender</param>
        private void DragSource_PreviewMouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            try
            {
                var visual = e.OriginalSource as Visual;
                _topWindow = (Window)DragDrop.FindAncestor(typeof(Window), visual);

                _initialMousePosition = e.GetPosition(_topWindow);

                //first, determine if the outer container property is bound
                _dragDropContainer = DragDrop.GetDragDropContainer(sender as DependencyObject) as Canvas;

                if (_dragDropContainer == null)
                {
                    //set the container to the canvas ancestor of the bound visual
                    _dragDropContainer = (Canvas)DragDrop.FindAncestor(typeof(Canvas), visual);
                }

                _dropTarget = GetDropTarget(sender as DependencyObject);

                //get the data context for the preview control
                _dragDropPreviewControlDataContext = DragDrop.GetDragDropPreviewControlDataContext(sender as DependencyObject);

                if (_dragDropPreviewControlDataContext == null)
                { _dragDropPreviewControlDataContext = (sender as FrameworkElement).DataContext; }


                _itemDroppedCommand = DragDrop.GetItemDropped(sender as DependencyObject);

            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception in DragDropHelper: " + exc.InnerException.ToString());
            }
        }

        /// <summary>
        /// Tunneled event handler for when the dragged item is released
        /// </summary>
        /// <param name="sender">The object which invokes this event</param>
        /// <param name="e">Event args from the sender</param>
        private void DragSource_PreviewMouseLeftButtonUp(Object sender, MouseButtonEventArgs e)
        {
            _dragDropPreviewControlDataContext = null;
            _mouseCaptured = false;

            if (_dragDropPreviewControl != null)
            { _dragDropPreviewControl.ReleaseMouseCapture(); }
        }

        /// <summary>
        /// Tunneled event handler for when the mouse is moving
        /// </summary>
        /// <param name="sender">The object which invokes this event</param>
        /// <param name="e">Event args from the sender</param>
        private void DragSource_PreviewMouseMove(Object sender, MouseEventArgs e)
        {
            if (_mouseCaptured || _dragDropPreviewControlDataContext == null)
            {
                return; //we're already capturing the mouse, or we don't have a data context for the preview control
            }

            if (DragDrop.IsMovementBigEnough(_initialMousePosition, e.GetPosition(_topWindow)) == false)
            {
                return; //only drag when the user moved the mouse by a reasonable amount
            }

            _dragDropPreviewControl = (DragDropPreviewBase)GetDragDropPreviewControl(sender as DependencyObject);
            _dragDropPreviewControl.DataContext = _dragDropPreviewControlDataContext;
            _dragDropPreviewControl.Opacity = 0.7;

            _dragDropContainer.Children.Add(_dragDropPreviewControl);
            _mouseCaptured = Mouse.Capture(_dragDropPreviewControl); //have the preview control recieve and be able to handle mouse events    

            //offset it just a bit so it looks like it's underneath the mouse
            Mouse.OverrideCursor = Cursors.Hand;


            Canvas.SetLeft(_dragDropPreviewControl, _initialMousePosition.X - 20);
            Canvas.SetTop(_dragDropPreviewControl, _initialMousePosition.Y - 15);

            _dragDropContainer.PreviewMouseMove += DragDropContainer_PreviewMouseMove;
            _dragDropContainer.PreviewMouseUp += DragDropContainer_PreviewMouseUp;

        }

        /// <summary>
        /// Tunneled event handler for when the mouse is moving
        /// </summary>
        /// <param name="sender">The object which invokes this event</param>
        /// <param name="e">Event args from the sender</param>
        private void DragDropContainer_PreviewMouseMove(Object sender, MouseEventArgs e)
        {
            var currentPoint = e.GetPosition(_topWindow);

            //offset it just a bit so it looks like it's underneath the mouse
            Mouse.OverrideCursor = Cursors.Hand;
            currentPoint.X = currentPoint.X - 20;
            currentPoint.Y = currentPoint.Y - 15;

            _delta = new Point(_initialMousePosition.X - currentPoint.X, _initialMousePosition.Y - currentPoint.Y);
            var target = new Point(_initialMousePosition.X - _delta.X, _initialMousePosition.Y - _delta.Y);

            Canvas.SetLeft(_dragDropPreviewControl, target.X);
            Canvas.SetTop(_dragDropPreviewControl, target.Y);

            _dragDropPreviewControl.DropState = DropState.CannotDrop;

            if (_dropTarget == null)
            {
                AnimateDropState();
                return;
            }

            var transform = _dropTarget.TransformToVisual(_dragDropContainer);
            var dropBoundingBox = transform.TransformBounds(new Rect(0, 0, _dropTarget.RenderSize.Width, _dropTarget.RenderSize.Height));

            if (e.GetPosition(_dragDropContainer).X > dropBoundingBox.Left &&
                e.GetPosition(_dragDropContainer).X < dropBoundingBox.Right &&
                e.GetPosition(_dragDropContainer).Y > dropBoundingBox.Top &&
                e.GetPosition(_dragDropContainer).Y < dropBoundingBox.Bottom)
            {
                _dragDropPreviewControl.DropState = DropState.CanDrop;
            }

            //bounding box might allow us to drop, but now we need to check with the command
            if (_itemDroppedCommand != null && _itemDroppedCommand.CanExecute(_dragDropPreviewControlDataContext) == false)
            {
                _dragDropPreviewControl.DropState = DropState.CannotDrop; //commanding trumps visual.                                    
            }

            AnimateDropState();
        }

        private void AnimateDropState()
        {
            //determine if we need to animate states
            switch (_dragDropPreviewControl.DropState)
            {
                case DropState.CanDrop:

                    if (_dragDropPreviewControl.Resources.Contains("canDropChanged"))
                    {
                        ((Storyboard)_dragDropPreviewControl.Resources["canDropChanged"]).Begin(_dragDropPreviewControl);
                    }

                    break;
                case DropState.CannotDrop:
                    if (_dragDropPreviewControl.Resources.Contains("cannotDropChanged"))
                    {
                        ((Storyboard)_dragDropPreviewControl.Resources["cannotDropChanged"]).Begin(_dragDropPreviewControl);
                    }
                    break;
                default:
                    break;
            }
        }

        private static DoubleAnimation CreateDoubleAnimation(Double to)
        {
            var anim = new DoubleAnimation();
            anim.To = to;
            anim.Duration = TimeSpan.FromMilliseconds(250);
            anim.AccelerationRatio = 0.1;
            anim.DecelerationRatio = 0.9;

            return anim;
        }

        /// <summary>
        /// Event handler for when the mouse button is released in the context of the drag and drop preview
        /// </summary>
        /// <param name="sender">The object which invokes this event</param>
        /// <param name="e">event args from the sender</param>
        private void DragDropContainer_PreviewMouseUp(Object sender, MouseEventArgs e)
        {
            switch (_dragDropPreviewControl.DropState)
            {
                case DropState.CanDrop:
                    try
                    {

                        var scaleXAnim = CreateDoubleAnimation(0);
                        Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));

                        var scaleYAnim = CreateDoubleAnimation(0);
                        Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

                        var opacityAnim = CreateDoubleAnimation(0);
                        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath("(UIElement.Opacity)"));

                        var canDropSb = new Storyboard() { FillBehavior = FillBehavior.Stop };
                        canDropSb.Children.Add(scaleXAnim);
                        canDropSb.Children.Add(scaleYAnim);
                        canDropSb.Children.Add(opacityAnim);
                        canDropSb.Completed += (s, args) => { FinalizePreviewControlMouseUp(); };

                        canDropSb.Begin(_dragDropPreviewControl);

                        if (_itemDroppedCommand != null)
                        { _itemDroppedCommand.Execute(_dragDropPreviewControlDataContext); }
                    }
                    catch (Exception ex)
                    { }
                    break;
                case DropState.CannotDrop:
                    try
                    {
                        var translateXAnim = CreateDoubleAnimation(_delta.X);
                        Storyboard.SetTargetProperty(translateXAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.X)"));

                        var translateYAnim = CreateDoubleAnimation(_delta.Y);
                        Storyboard.SetTargetProperty(translateYAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)"));

                        var opacityAnim = CreateDoubleAnimation(0);
                        opacityAnim.BeginTime = TimeSpan.FromMilliseconds(150);
                        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath("(UIElement.Opacity)"));

                        var cannotDropSb = new Storyboard() { FillBehavior = FillBehavior.Stop };
                        cannotDropSb.Children.Add(translateXAnim);
                        cannotDropSb.Children.Add(translateYAnim);
                        cannotDropSb.Children.Add(opacityAnim);
                        cannotDropSb.Completed += (s, args) => { FinalizePreviewControlMouseUp(); };

                        cannotDropSb.Begin(_dragDropPreviewControl);
                    }
                    catch (Exception ex) { }
                    break;
            }

            _dragDropPreviewControlDataContext = null;
            _mouseCaptured = false;
        }

        /// <summary>
        /// Removes the drag and drop preview control from the drag drop canvas, and clears the reference for the next drag/drop operation
        /// </summary>
        private void FinalizePreviewControlMouseUp()
        {
            _dragDropContainer.Children.Remove(_dragDropPreviewControl);
            _dragDropContainer.PreviewMouseMove -= DragDropContainer_PreviewMouseMove;
            _dragDropContainer.PreviewMouseUp -= DragDropContainer_PreviewMouseUp;

            if (_dragDropPreviewControl != null)
            {
                _dragDropPreviewControl.ReleaseMouseCapture();
            }
            _dragDropPreviewControl = null;
            Mouse.OverrideCursor = null;
        }

        #endregion

        #endregion

        #endregion

        #region DragDropContainer Attached Property

        #region Backing Dependency Property

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> which enables animation, styling, binding, etc. for the outmost container of the <see cref="DragDropPreviewControlProperty"/>
        /// </summary>
        public static readonly DependencyProperty DragDropContainerProperty = DependencyProperty.RegisterAttached(
            "DragDropContainer", typeof(Panel), typeof(DragDrop), new PropertyMetadata(default(UIElement)));

        #endregion

        #region Get and set

        /// <summary>
        /// Gets the attached value of <see cref="DragDropContainerProperty"/>
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="DragDropContainerProperty"/>, is attched to.</param>
        /// <returns>The attached value</returns>
        public static Panel GetDragDropContainer(DependencyObject element)
        {
            return (Panel)element.GetValue(DragDropContainerProperty);
        }

        /// <summary>
        /// Sets the attached value of <see cref="DragDropContainerProperty"/>
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="DragDropContainerProperty"/>, is attched to.</param>
        /// <param name="value">the value to set</param>
        public static void SetDragDropContainer(DependencyObject element, Panel value)
        {
            element.SetValue(DragDropContainerProperty, value);
        }

        #endregion

        #endregion

        #region DragDropPreviewControl Attached Property

        #region Backing Dependency Property

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> which enables animation, styling, binding, etc. for the control that is displayed (and moves with the mouse) during a drag drop operation
        /// </summary>
        public static readonly DependencyProperty DragDropPreviewControlProperty = DependencyProperty.RegisterAttached(
            "DragDropPreviewControl", typeof(DragDropPreviewBase), typeof(DragDrop), new PropertyMetadata(default(UIElement)));

        #endregion

        #region Get and set

        /// <summary>
        /// Gets the attached value of <see cref="DragDropPreviewControlProperty"/>
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="DragDropPreviewControlProperty"/>, is attched to.</param>
        /// <returns>The attached value</returns>
        public static DragDropPreviewBase GetDragDropPreviewControl(DependencyObject element)
        {
            return (DragDropPreviewBase)element.GetValue(DragDropPreviewControlProperty);
        }

        /// <summary>
        /// Sets the attached value of <see cref="DragDropPreviewControlProperty"/>
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="DragDropPreviewControlProperty"/>, is attched to.</param>
        /// <param name="value">the value to set</param>
        public static void SetDragDropPreviewControl(DependencyObject element, DragDropPreviewBase value)
        {
            element.SetValue(DragDropPreviewControlProperty, value);
        }

        #endregion

        #endregion

        #region DragDropPreviewControlDataContext Attached Property

        #region Backing Dependency Property

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> which enables animation, styling, binding, etc. for DragDropPreviewControlDataContext
        /// </summary>
        public static readonly DependencyProperty DragDropPreviewControlDataContextProperty = DependencyProperty.RegisterAttached(
            "DragDropPreviewControlDataContext", typeof(Object), typeof(DragDrop), new PropertyMetadata(default(Object)));

        #endregion

        #region Get and set

        /// <summary>
        /// Gets the attached value of DragDropPreviewControlDataContext
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="DragDropPreviewControlDataContextProperty"/>, is attched to.</param>
        /// <returns>The attached value</returns>
        public static Object GetDragDropPreviewControlDataContext(DependencyObject element)
        {
            return (Object)element.GetValue(DragDropPreviewControlDataContextProperty);
        }

        /// <summary>
        /// Sets the attached value of DragDropPreviewControlDataContext
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="DragDropPreviewControlDataContextProperty"/>, is attched to.</param>
        /// <param name="value">the value to set</param>
        public static void SetDragDropPreviewControlDataContext(DependencyObject element, Object value)
        {
            element.SetValue(DragDropPreviewControlDataContextProperty, value);
        }

        #endregion

        #endregion

        #region DropTarget Attached Property

        #region Backing Dependency Property

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> which enables animation, styling, binding, etc. for <see cref="DropTarget"/>
        /// </summary>
        public static readonly DependencyProperty DropTargetProperty = DependencyProperty.RegisterAttached(
            "DropTarget", typeof(UIElement), typeof(DragDrop), new PropertyMetadata(default(String)));


        #endregion

        #region Get and set

        /// <summary>
        /// Gets the attached value of DropTarget
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="DropTargetProperty"/>, is attched to.</param>
        /// <returns>The attached value</returns>
        public static UIElement GetDropTarget(DependencyObject element)
        {
            return (UIElement)element.GetValue(DropTargetProperty);
        }

        /// <summary>
        /// Sets the attached value of DropTarget
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> that the attached <see cref="DependencyProperty"/>, <see cref="DropTargetProperty"/>, is attched to.</param>
        /// <param name="value">the value to set</param>
        public static void SetDropTarget(DependencyObject element, UIElement value)
        {
            element.SetValue(DropTargetProperty, value);
        }

        #endregion

        #endregion

        #region Utilities

        /// <summary>
        /// Walks the visual tree, and finds the ancestor of the <see cref="visual"/> which is an instance of <paramref name="ancestorType"/>
        /// </summary>
        /// <param name="ancestorType">The type of ancestor to look for in the visual tree</param>
        /// <param name="visual">The object to start at in the visual tree</param>
        /// <returns>The <see cref="FrameworkElement"/> which matches <paramref name="ancestorType"/></returns>
        public static FrameworkElement FindAncestor(Type ancestorType, Visual visual)
        {
            while (visual != null && !ancestorType.IsInstanceOfType(visual))
            {
                visual = (Visual)VisualTreeHelper.GetParent(visual);
            }
            return visual as FrameworkElement;
        }

        /// <summary>
        /// Determines if the delta between two points exceeds the minimum horizontal and vertical drag distance, as defined by the system
        /// </summary>
        /// <param name="initialMousePosition">The starting position</param>
        /// <param name="currentPosition">The current position</param>
        /// <returns>True, if the delta exceeds the minimum horizontal and vertical drag distance</returns>
        public static Boolean IsMovementBigEnough(Point initialMousePosition, Point currentPosition)
        {
            return (Math.Abs(currentPosition.X - initialMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - initialMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance);
        }

        #endregion
    }
}
