﻿// (c) Copyright Microsoft Corporation.
// This source is subject to [###LICENSE_NAME###].
// Please see [###LICENSE_LINK###] for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;

namespace Microsoft.Silverlight.Testing.Controls
{
    /// <summary>
    /// Represents a control that displays hierarchical data in a tree structure
    /// that has items that can expand and collapse.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    [TemplateVisualState(Name = VisualStates.StateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateMouseOver, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StatePressed, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateDisabled, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateFocused, GroupName = VisualStates.GroupFocus)]
    [TemplateVisualState(Name = VisualStates.StateUnfocused, GroupName = VisualStates.GroupFocus)]
    [TemplateVisualState(Name = VisualStates.StateValid, GroupName = VisualStates.GroupValidation)]
    [TemplateVisualState(Name = VisualStates.StateInvalidFocused, GroupName = VisualStates.GroupValidation)]
    [TemplateVisualState(Name = VisualStates.StateInvalidUnfocused, GroupName = VisualStates.GroupValidation)]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TreeViewItem))]
    public partial class TreeView : ItemsControl, IUpdateVisualState
    {
        /// <summary>
        /// A value indicating whether a read-only dependency property change
        /// handler should allow the value to be set.  This is used to ensure
        /// that read-only properties cannot be changed via SetValue, etc.
        /// </summary>
        private bool _allowWrite;

        /// <summary>
        /// A value indicating whether a dependency property change handler
        /// should ignore the next change notification.  This is used to reset
        /// the value of properties without performing any of the actions in
        /// their change handlers.
        /// </summary>
        private bool _ignorePropertyChange;

        #region public object SelectedItem
        /// <summary>
        /// Gets the selected item in a
        /// <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeView" />.
        /// </summary>
        /// <value>
        /// The currently selected item or null if no item is selected. The
        /// default value is null.
        /// </value>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            private set
            {
                try
                {
                    _allowWrite = true;
                    SetValue(SelectedItemProperty, value);
                }
                finally
                {
                    _allowWrite = false;
                }
            }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedItem" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedItem" />
        /// property.
        /// </value>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(TreeView),
                new PropertyMetadata(null, OnSelectedItemPropertyChanged));

        /// <summary>
        /// SelectedItemProperty property changed handler.
        /// </summary>
        /// <param name="d">TreeView that changed its SelectedItem.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeView source = d as TreeView;

            // Ignore the change if requested
            if (source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            // Ensure the property is only written when expected
            if (!source._allowWrite)
            {
                // Reset the old value before it was incorrectly written
                source._ignorePropertyChange = true;
                source.SetValue(SelectedItemProperty, e.OldValue);

                throw new InvalidOperationException();
            }

            source.UpdateSelectedValue(e.NewValue);
        }
        #endregion public object SelectedItem

        #region public object SelectedValue
        /// <summary>
        /// Gets the value of the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedItem" />
        /// property that is specified by the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedValuePath" />
        /// property.
        /// </summary>
        /// <value>
        /// The value of the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedItem" />
        /// property that is specified by the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedValuePath" />
        /// property, or null if no item is selected. The default value is null.
        /// </value>
        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            private set
            {
                try
                {
                    _allowWrite = true;
                    SetValue(SelectedValueProperty, value);
                }
                finally
                {
                    _allowWrite = false;
                }
            }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedValue" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedValue" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(
                "SelectedValue",
                typeof(object),
                typeof(TreeView),
                new PropertyMetadata(null, OnSelectedValuePropertyChanged));

        /// <summary>
        /// SelectedValueProperty property changed handler.
        /// </summary>
        /// <param name="d">TreeView that changed its SelectedValue.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSelectedValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeView source = d as TreeView;

            // Ignore the change if requested
            if (source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            // Ensure the property is only written when expected
            if (!source._allowWrite)
            {
                // Reset the old value before it was incorrectly written
                source._ignorePropertyChange = true;
                source.SetValue(SelectedValueProperty, e.OldValue);

                throw new InvalidOperationException();
            }
        }
        #endregion public object SelectedValue

        #region public string SelectedValuePath
        /// <summary>
        /// Gets or sets the property path that is used to get the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedValue" />
        /// property of the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedItem" />
        /// property in a <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeView" />.
        /// </summary>
        /// <value>
        /// The property path that is used to get the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedValue" />
        /// property of the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedItem" />
        /// property in a <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeView" />. The
        /// default value is <see cref="F:System.String.Empty" />.
        /// </value>
        public string SelectedValuePath
        {
            get { return GetValue(SelectedValuePathProperty) as string; }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedValuePath" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedValuePath" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register(
                "SelectedValuePath",
                typeof(string),
                typeof(TreeView),
                new PropertyMetadata(string.Empty, OnSelectedValuePathPropertyChanged));

        /// <summary>
        /// SelectedValuePathProperty property changed handler.
        /// </summary>
        /// <param name="d">TreeView that changed its SelectedValuePath.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSelectedValuePathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeView source = d as TreeView;
            source.UpdateSelectedValue(source.SelectedItem);
        }
        #endregion public string SelectedValuePath

        #region public Style ItemContainerStyle
        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Style" /> that is
        /// applied to the container element generated for each item.
        /// </summary>
        /// <value>
        /// The <see cref="T:System.Windows.Style" /> applied to the container
        /// element that contains each item.
        /// </value>
        public Style ItemContainerStyle
        {
            get { return GetValue(ItemContainerStyleProperty) as Style; }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.ItemContainerStyle" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.ItemContainerStyle" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(
                "ItemContainerStyle",
                typeof(Style),
                typeof(TreeView),
                new PropertyMetadata(null, OnItemContainerStylePropertyChanged));

        /// <summary>
        /// ItemContainerStyleProperty property changed handler.
        /// </summary>
        /// <param name="d">
        /// TreeView that changed its ItemContainerStyle.
        /// </param>
        /// <param name="e">Event arguments.</param>
        private static void OnItemContainerStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeView source = d as TreeView;
            Style value = e.NewValue as Style;
            source.ItemsControlHelper.UpdateItemContainerStyle(value);
        }
        #endregion public Style ItemContainerStyle

        /// <summary>
        /// Gets the currently selected TreeViewItem container.
        /// </summary>
        internal TreeViewItem SelectedContainer { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the currently selected TreeViewItem
        /// container is properly hooked up to the TreeView.
        /// </summary>
        internal bool IsSelectedContainerHookedUp
        {
            get { return SelectedContainer != null && SelectedContainer.ParentTreeView == this; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the selected item is
        /// currently being changed.
        /// </summary>
        internal bool IsSelectionChangeActive { get; set; }

        /// <summary>
        /// Gets the ItemsControlHelper that is associated with this control.
        /// </summary>
        internal ItemsControlHelper ItemsControlHelper { get; private set; }

        /// <summary>
        /// Gets the helper that provides all of the standard
        /// interaction functionality.
        /// </summary>
        internal InteractionHelper Interaction { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the Control key is currently
        /// pressed.
        /// </summary>
        internal static bool IsControlKeyDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control; }
        }

        /// <summary>
        /// Gets a value indicating whether the Shift key is currently pressed.
        /// </summary>
        internal static bool IsShiftKeyDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift; }
        }

        /// <summary>
        /// Occurs when the value of the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedItem" />
        /// property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeView" /> class.
        /// </summary>
        public TreeView()
        {
            DefaultStyleKey = typeof(TreeView);
            ItemsControlHelper = new ItemsControlHelper(this);
            Interaction = new InteractionHelper(this);
        }

        /// <summary>
        /// Returns a
        /// <see cref="T:System.Windows.Automation.Peers.TreeViewAutomationPeer" />
        /// for use by the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        /// A
        /// <see cref="T:System.Windows.Automation.Peers.TreeViewAutomationPeer" />
        /// for the <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeView" /> control.
        /// </returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TreeViewAutomationPeer(this);
        }

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeView" /> control when a new
        /// control template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            ItemsControlHelper.OnApplyTemplate();
            Interaction.OnApplyTemplateBase();
            base.OnApplyTemplate();
        }

        /// <summary>
        /// Update the visual state of the TreeView.
        /// </summary>
        /// <param name="useTransitions">
        /// A value indicating whether to use transitions when updating the
        /// visual state.
        /// </param>
        void IUpdateVisualState.UpdateVisualState(bool useTransitions)
        {
            Interaction.UpdateVisualStateBase(useTransitions);
        }

        #region ItemsControl
        /// <summary>
        /// Creates a <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeViewItem" /> to
        /// display content.
        /// </summary>
        /// <returns>
        /// A <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeViewItem" /> to use as a
        /// container for content.
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItem();
        }

        /// <summary>
        /// Determines whether the specified item is a
        /// <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeViewItem" />, which is the
        /// default container for items in the
        /// <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeView" /> control.
        /// </summary>
        /// <param name="item">The object to evaluate.</param>
        /// <returns>
        /// True if the item is a
        /// <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeViewItem" />; otherwise,
        /// false.
        /// </returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeViewItem;
        }

        /// <summary>
        /// Prepares the container element to display the specified item.
        /// </summary>
        /// <param name="element">
        /// The container element used to display the specified item.
        /// </param>
        /// <param name="item">The item to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            TreeViewItem node = element as TreeViewItem;
            if (node != null)
            {
                // Associate the Parent ItemsControl
                node.ParentItemsControl = this;
            }

            ItemsControlHelper.PrepareContainerForItemOverride(element, ItemContainerStyle);
            HeaderedItemsControl.PreparePrepareHeaderedItemsControlContainerForItemOverride(element, item, this, ItemContainerStyle);
            base.PrepareContainerForItemOverride(element, item);
        }

        /// <summary>
        /// Removes all templates, styles, and bindings for the object displayed
        /// as a <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeViewItem" />.
        /// </summary>
        /// <param name="element">
        /// The <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeViewItem" /> element to
        /// clear.
        /// </param>
        /// <param name="item">
        /// The item that is contained in the
        /// <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeViewItem" />.
        /// </param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            // Remove the association with the Parent ItemsControl
            TreeViewItem node = element as TreeViewItem;
            if (node != null)
            {
                node.ParentItemsControl = null;
            }

            base.ClearContainerForItemOverride(element, item);
        }

        /// <summary>
        /// Makes adjustments to the
        /// <see cref="T:Microsoft.Silverlight.Testing.Controls.TreeView" /> control when the
        /// value of the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.ItemsControl.Items" /> property
        /// changes.
        /// </summary>
        /// <param name="e">
        /// A
        /// <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" />
        /// that contains data about the change.
        /// </param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            base.OnItemsChanged(e);

            // Associate any TreeViewItems with their parent
            if (e.NewItems != null)
            {
                foreach (TreeViewItem item in e.NewItems.OfType<TreeViewItem>())
                {
                    item.ParentItemsControl = this;
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    if (SelectedItem == null || IsSelectedContainerHookedUp)
                    {
                        break;
                    }
                    SelectFirstItem();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    object selectedItem = SelectedItem;
                    if (selectedItem == null || (e.OldItems != null && !object.Equals(selectedItem, e.OldItems[0])))
                    {
                        break;
                    }
                    ChangeSelection(selectedItem, SelectedContainer, false);
                    break;
            }

            // Remove the association with the Parent ItemsControl
            if (e.OldItems != null)
            {
                foreach (TreeViewItem item in e.OldItems.OfType<TreeViewItem>())
                {
                    item.ParentItemsControl = null;
                }
            }
        }

        /// <summary>
        /// Select any descendents when adding new TreeViewItems to a TreeView.
        /// </summary>
        /// <param name="item">The added item.</param>
        internal void CheckForSelectedDescendents(TreeViewItem item)
        {
            Debug.Assert(item != null, "item should not be null!");

            Stack<TreeViewItem> items = new Stack<TreeViewItem>();
            items.Push(item);

            // Recurse into subtree of each added item to ensure none of
            // its descendents are selected.
            while (items.Count > 0)
            {
                TreeViewItem current = items.Pop();
                if (current.IsSelected)
                {
                    // Make IsSelected false so that its property changed
                    // handler will be fired when it's set to true in
                    // ChangeSelection
                    current.IgnorePropertyChange = true;
                    current.IsSelected = false;

                    ChangeSelection(current, current, true);

                    // If the item is not in the visual tree, we will make sure
                    // every check for ContainsSelection will try and update the
                    // sequence of ContainsSelection flags for the
                    // SelectedContainer.
                    if (SelectedContainer.ParentItemsControl == null)
                    {
                        SelectedContainer.RequiresContainsSelectionUpdate = true;
                    }
                }
                foreach (TreeViewItem nestedItem in current.Items.OfType<TreeViewItem>())
                {
                    items.Push(nestedItem);
                }
            }
        }
        #endregion ItemsControl

        #region Input Events
        /// <summary>
        /// Propagate OnKeyDown messages from the root TreeViewItems to their
        /// TreeView.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <remarks>
        /// Because Silverlight's ScrollViewer swallows many useful key events
        /// (which it can ignore on WPF if you override HandlesScrolling or use
        /// an internal only variable in Silverlight), the root TreeViewItems
        /// explicitly propagate KeyDown events to their parent TreeView.
        /// </remarks>
        internal void PropagateKeyDown(KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.KeyDown" /> event when a key
        /// is pressed while the control has focus.
        /// </summary>
        /// <param name="e">
        /// A <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains
        /// the event data.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="e " />is null.
        /// </exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Complexity metric is inflated by the switch statements")]
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!Interaction.AllowKeyDown(e))
            {
                return;
            }

            base.OnKeyDown(e);

            if (e.Handled)
            {
                return;
            }

            // The Control key modifier is used to scroll the viewer instead of
            // the selection
            if (IsControlKeyDown)
            {
                switch (e.Key)
                {
                    case Key.Home:
                    case Key.End:
                    case Key.PageUp:
                    case Key.PageDown:
                    case Key.Left:
                    case Key.Right:
                    case Key.Up:
                    case Key.Down:
                        if (HandleScrollKeys(e.Key))
                        {
                            e.Handled = true;
                        }
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.PageUp:
                    case Key.PageDown:
                        if (SelectedContainer != null)
                        {
                            if (HandleScrollByPage(e.Key == Key.PageUp))
                            {
                                e.Handled = true;
                            }
                            break;
                        }
                        if (FocusFirstItem())
                        {
                            e.Handled = true;
                        }
                        break;
                    case Key.Home:
                        if (FocusFirstItem())
                        {
                            e.Handled = true;
                        }
                        break;
                    case Key.End:
                        if (FocusLastItem())
                        {
                            e.Handled = true;
                        }
                        break;
                    case Key.Up:
                    case Key.Down:
                        if (SelectedContainer == null && FocusFirstItem())
                        {
                            e.Handled = true;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Handle keys related to scrolling.
        /// </summary>
        /// <param name="key">The key to handle.</param>
        /// <returns>A value indicating whether the key was handled.</returns>
        private bool HandleScrollKeys(Key key)
        {
            ScrollViewer scrollHost = ItemsControlHelper.ScrollHost;
            if (scrollHost != null)
            {
                switch (key)
                {
                    case Key.PageUp:
                        // Move horizontally if we've run out of room vertically
                        if (!NumericExtensions.IsGreaterThan(scrollHost.ExtentHeight, scrollHost.ViewportHeight))
                        {
                            scrollHost.PageLeft();
                        }
                        else
                        {
                            scrollHost.PageUp();
                        }
                        return true;
                    case Key.PageDown:
                        // Move horizontally if we've run out of room vertically
                        if (!NumericExtensions.IsGreaterThan(scrollHost.ExtentHeight, scrollHost.ViewportHeight))
                        {
                            scrollHost.PageRight();
                        }
                        else
                        {
                            scrollHost.PageDown();
                        }
                        return true;
                    case Key.Home:
                        scrollHost.ScrollToTop();
                        return true;
                    case Key.End:
                        scrollHost.ScrollToBottom();
                        return true;
                    case Key.Left:
                        scrollHost.LineLeft();
                        return true;
                    case Key.Right:
                        scrollHost.LineRight();
                        return true;
                    case Key.Up:
                        scrollHost.LineUp();
                        return true;
                    case Key.Down:
                        scrollHost.LineDown();
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handle scrolling a page up or down.
        /// </summary>
        /// <param name="up">
        /// A value indicating whether the page should be scrolled up.
        /// </param>
        /// <returns>
        /// A value indicating whether the scroll was handled.
        /// </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Necessary complexity")]
        private bool HandleScrollByPage(bool up)
        {
            // NOTE: This implementation assumes that items are laid out
            // vertically and the Headers of the TreeViewItems appear above
            // their ItemsPresenter.  The same assumptions are made in WPF.

            ScrollViewer scrollHost = ItemsControlHelper.ScrollHost;
            if (scrollHost != null)
            {
                double viewportHeight = scrollHost.ViewportHeight;
                
                double top;
                double bottom;
                (SelectedContainer.HeaderElement ?? SelectedContainer).GetTopAndBottom(scrollHost, out top, out bottom);

                TreeViewItem selected = null;
                TreeViewItem next = SelectedContainer;
                ItemsControl parent = SelectedContainer.ParentItemsControl;

                if (parent != null)
                {
                    // We need to start at the root TreeViewItem if we're
                    // scrolling up, but can start at the SelectedItem if
                    // scrolling down.
                    if (up)
                    {
                        while (parent != this)
                        {
                            TreeViewItem parentItem = parent as TreeViewItem;
                            if (parentItem == null)
                            {
                                break;
                            }

                            ItemsControl grandparent = parentItem.ParentItemsControl;
                            if (grandparent == null)
                            {
                                break;
                            }

                            next = parentItem;
                            parent = grandparent;
                        }
                    }

                    int index = parent.ItemContainerGenerator.IndexFromContainer(next);
                    int count = parent.Items.Count;
                    while (parent != null && next != null)
                    {
                        if (next.IsEnabled)
                        {
                            double delta;
                            if (next.HandleScrollByPage(up, scrollHost, viewportHeight, top, bottom, out delta))
                            {
                                // This item or one of its children was focused
                                return true;
                            }
                            else if (NumericExtensions.IsGreaterThan(delta, viewportHeight))
                            {
                                // If the item doesn't fit on the page but it's
                                // already selected, we'll select the next item
                                // even though it doesn't completely fit into
                                // the current view
                                if (selected == SelectedContainer || selected == null)
                                {
                                    return up ?
                                        SelectedContainer.HandleUpKey() :
                                        SelectedContainer.HandleDownKey();
                                }
                                break;
                            }
                            else
                            {
                                selected = next;
                            }
                        }

                        index += up ? -1 : 1;
                        if (0 <= index && index < count)
                        {
                            next = parent.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
                        }
                        else if (parent == this)
                        {
                            // We just finished with the last item in the
                            // TreeView
                            next = null;
                        }
                        else
                        {
                            // Move up the parent chain to the next item
                            while (parent != null)
                            {
                                TreeViewItem oldParent = parent as TreeViewItem;
                                parent = oldParent.ParentItemsControl;
                                if (parent != null)
                                {
                                    count = parent.Items.Count;
                                    index = parent.ItemContainerGenerator.IndexFromContainer(oldParent) + (up ? -1 : 1);
                                    if (0 <= index && index < count)
                                    {
                                        next = parent.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
                                        break;
                                    }
                                    else if (parent == this)
                                    {
                                        next = null;
                                        parent = null;
                                    }
                                }
                            }
                        }
                    }
                }

                if (selected != null)
                {
                    if (up)
                    {
                        if (selected != SelectedContainer)
                        {
                            return selected.Focus();
                        }
                    }
                    else
                    {
                        selected.FocusInto();
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Provides handling for the KeyUp event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (Interaction.AllowKeyUp(e))
            {
                base.OnKeyUp(e);
            }
        }

        /// <summary>
        /// Provides handling for the MouseEnter event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (Interaction.AllowMouseEnter(e))
            {
                Interaction.OnMouseEnterBase();
                base.OnMouseEnter(e);
            }
        }

        /// <summary>
        /// Provides handling for the MouseLeave event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (Interaction.AllowMouseLeave(e))
            {
                Interaction.OnMouseLeaveBase();
                base.OnMouseLeave(e);
            }
        }

        /// <summary>
        /// Provides handling for the MouseMove event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" />
        /// event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that
        /// contains the event data.
        /// </param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (Interaction.AllowMouseLeftButtonDown(e))
            {
                if (!e.Handled && HandleMouseButtonDown())
                {
                    e.Handled = true;
                }

                Interaction.OnMouseLeftButtonDownBase();
                base.OnMouseLeftButtonDown(e);
            }            
        }

        /// <summary>
        /// Provides handling for the MouseLeftButtonUp event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (Interaction.AllowMouseLeftButtonUp(e))
            {
                Interaction.OnMouseLeftButtonUpBase();
                base.OnMouseLeftButtonUp(e);
            }
        }

        /// <summary>
        /// Provides handling for mouse button events.
        /// </summary>
        /// <returns>A value indicating whether the event was handled.</returns>
        internal bool HandleMouseButtonDown()
        {
            if (SelectedContainer != null)
            {
                if (SelectedContainer != FocusManager.GetFocusedElement())
                {
                    SelectedContainer.Focus();
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Provides handling for the GotFocus event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (Interaction.AllowGotFocus(e))
            {
                Interaction.OnGotFocusBase();
                base.OnGotFocus(e);
            }
        }

        /// <summary>
        /// Provides handling for the LostFocus event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (Interaction.AllowLostFocus(e))
            {
                Interaction.OnLostFocusBase();
                base.OnLostFocus(e);
            }
        }
        #endregion Input Events

        #region Selection
        /// <summary>
        /// Raises the
        /// <see cref="E:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedItemChanged" />
        /// event when the
        /// <see cref="P:Microsoft.Silverlight.Testing.Controls.TreeView.SelectedItem" />
        /// property value changes.
        /// </summary>
        /// <param name="e">
        /// A <see cref="T:System.Windows.RoutedPropertyChangedEventArgs`1" />
        /// that contains the event data.
        /// </param>
        protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            RoutedPropertyChangedEventHandler<object> handler = SelectedItemChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Change whether a TreeViewItem is selected.
        /// </summary>
        /// <param name="itemOrContainer">
        /// Item whose selection is changing.
        /// </param>
        /// <param name="container">
        /// Container of the item whose selection is changing.
        /// </param>
        /// <param name="selected">
        /// A value indicating whether the TreeViewItem is selected.
        /// </param>
        internal void ChangeSelection(object itemOrContainer, TreeViewItem container, bool selected)
        {
            // Ignore any change notifications if we're alread in the middle of
            // changing the selection
            if (IsSelectionChangeActive)
            {
                return;
            }

            object oldValue = null;
            object newValue = null;
            bool raiseSelectionChanged = false;
            TreeViewItem element = SelectedContainer;

            // Start changing the selection
            IsSelectionChangeActive = true;
            try
            {
                if (selected && container != SelectedContainer)
                {
                    // Unselect the old value
                    oldValue = SelectedItem;
                    if (SelectedContainer != null)
                    {
                        SelectedContainer.IsSelected = false;
                        SelectedContainer.UpdateContainsSelection(false);
                    }

                    // Select the new value
                    newValue = itemOrContainer;
                    SelectedContainer = container;
                    SelectedContainer.UpdateContainsSelection(true);
                    SelectedItem = itemOrContainer;
                    UpdateSelectedValue(itemOrContainer);
                    raiseSelectionChanged = true;

                    // Scroll the selected item into view.  We only want to
                    // scroll the header into view, if possible, because an
                    // expanded TreeViewItem contains all of its child items
                    // as well.
                    ItemsControlHelper.ScrollIntoView(container.HeaderElement ?? container);
                }
                else if (!selected && container == SelectedContainer)
                {
                    // Unselect the old value
                    SelectedContainer.UpdateContainsSelection(false);
                    SelectedContainer = null;
                    SelectedItem = null;
                    SelectedValue = null;
                    oldValue = itemOrContainer;
                    raiseSelectionChanged = true;
                }

                container.IsSelected = selected;
            }
            finally
            {
                // Finish changing the selection
                IsSelectionChangeActive = false;
            }

            // Notify when the selection changes
            if (raiseSelectionChanged)
            {
                if (SelectedContainer != null && AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected))
                {
                    AutomationPeer peer = FrameworkElementAutomationPeer.CreatePeerForElement(SelectedContainer);
                    if (peer != null)
                    {
                        peer.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
                    }
                }
                if (element != null && AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
                {
                    AutomationPeer peer = FrameworkElementAutomationPeer.CreatePeerForElement(element);
                    if (peer != null)
                    {
                        peer.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
                    }
                }

                OnSelectedItemChanged(new RoutedPropertyChangedEventArgs<object>(oldValue, newValue));
            }
        }

        /// <summary>
        /// Update the selected value of the of the TreeView based on the value
        /// of the currently selected TreeViewItem and the SelectedValuePath.
        /// </summary>
        /// <param name="item">
        /// Value of the currently selected TreeViewItem.
        /// </param>
        private void UpdateSelectedValue(object item)
        {
            if (item != null)
            {
                string path = SelectedValuePath;
                if (string.IsNullOrEmpty(path))
                {
                    SelectedValue = item;
                }
                else
                {
                    // Since we don't have the ability to evaluate a
                    // BindingExpression, we'll just create a new temporary
                    // control to bind the value to which we can then copy out
                    Binding binding = new Binding(path) { Source = item };
                    ContentControl temp = new ContentControl();
                    temp.SetBinding(ContentControl.ContentProperty, binding);
                    SelectedValue = temp.Content;

                    // Remove the Binding once we have the value (this is
                    // especially important if the value is a UIElement because
                    // it should not exist in the visual tree once we've
                    // finished)
                    temp.ClearValue(ContentControl.ContentProperty);
                }
            }
            else
            {
                ClearValue(SelectedValueProperty);
            }
        }

        /// <summary>
        /// Select the first item of the TreeView.
        /// </summary>
        private void SelectFirstItem()
        {
            TreeViewItem container = ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
            bool found = container != null;
            if (!found)
            {
                container = SelectedContainer;
            }

            object item = found ?
                ItemContainerGenerator.ItemFromContainer(container) :
                SelectedItem;

            ChangeSelection(item, container, found);
        }
        #endregion Selection

        #region Focus Navigation
        /// <summary>
        /// Focus the first item in the TreeView.
        /// </summary>
        /// <returns>A value indicating whether the item was focused.</returns>
        private bool FocusFirstItem()
        {
            // Get the first item in the TreeView.
            TreeViewItem item = ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
            return (item != null) ?
                (item.IsEnabled && item.Focus()) || item.FocusDown() :
                false;
        }

        /// <summary>
        /// Focus the last item in the TreeView.
        /// </summary>
        /// <returns>A value indicating whether the item was focused.</returns>
        private bool FocusLastItem()
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                TreeViewItem item = ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (item != null && item.IsEnabled)
                {
                    return item.FocusInto();
                }
            }
            return false;
        }
        #endregion Focus Navigation
    }
}