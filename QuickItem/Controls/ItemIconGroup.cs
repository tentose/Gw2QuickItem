using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public enum ItemIconBinding
    {
        Account,
        Character,
    }

    public enum GroupDragMode
    {
        None,
        Group,
        Item,
    }

    public class ItemIconGroup : Container, IHasContextMenu
    {
        private List<ItemIcon> _items = new List<ItemIcon>();
        public List<ItemIcon> Items { get => _items; }

        private bool _allowActivation = true;
        public bool AllowActivation
        {
            get
            {
                return _allowActivation;
            }
            set
            {
                _allowActivation = value;
                foreach (var item in _items)
                {
                    item.AllowActivation = value;
                }
            }
        }

        private bool _allowEdit = false;
        public bool AllowEdit
        {
            get
            {
                return _allowEdit;
            }
            set
            {
                _allowEdit = value;
                foreach (var item in _items)
                {
                    item.AllowEdit = value;
                }
            }
        }

        public GroupDragMode DragMode { get; set; } = GroupDragMode.None;

        private bool _pauseLayout = false;

        private ItemGroupInfo _groupInfo = new ItemGroupInfo();
        public ItemGroupInfo GroupInfo
        {
            get
            {
                return _groupInfo;
            }
            set
            {
                _pauseLayout = true;
                _groupInfo = value;
                foreach (var itemInfo in _groupInfo.Items)
                {
                    ItemIcon icon = new ItemIcon(itemInfo);
                    icon.Parent = this;
                    icon.Location = itemInfo.IconPosition;
                }
                _pauseLayout = false;
                FixSize();
            }
        }

        public event EventHandler ItemsChanged;

        public event EventHandler PositionChanged;

        public event EventHandler DeleteRequested;

        public ItemIconGroup()
        {
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            var itemIcon = e.ChangedChild as ItemIcon;
            if (itemIcon != null)
            {
                _items.Add(itemIcon);
                itemIcon.DeleteRequested += ItemIcon_DeleteRequested;
                if (!_pauseLayout)
                {
                    FixSize();
                }
            }

            base.OnChildAdded(e);
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            var itemIcon = e.ChangedChild as ItemIcon;
            if (itemIcon != null)
            {
                _items.Remove(itemIcon);
                itemIcon.DeleteRequested -= ItemIcon_DeleteRequested;
                if (!_pauseLayout)
                {
                    FixSize();
                }
            }
            base.OnChildRemoved(e);
        }

        private void ItemIcon_DeleteRequested(object sender, EventArgs e)
        {
            var itemIcon = sender as ItemIcon;
            var senderItemInfo = _groupInfo.Items.Where(itemInfo => itemInfo == itemIcon.Item).FirstOrDefault();
            if (senderItemInfo != null)
            {
                this.RemoveChild(itemIcon);
                _groupInfo.Items.Remove(senderItemInfo);
            }
        }

        public void FixSize()
        {
            if (Children.Count == 0)
            {
                return;
            }

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (var child in Children)
            {
                if (child.Left < minX)
                {
                    minX = child.Left;
                }
                if (child.Top < minY)
                {
                    minY = child.Top;
                }
                if (child.Right > maxX)
                {
                    maxX = child.Right;
                }
                if (child.Bottom > maxY)
                {
                    maxY = child.Bottom;
                }
            }

            this.Size = new Point(maxX - minX, maxY - minY);

            var offset = new Point(minX, minY);
            foreach(var child in Children)
            {
                child.Location -= offset;
            }

            this.Location += offset;
        }


        private bool _dragging;
        public bool Dragging
        {
            get => _dragging;
            private set => SetProperty(ref _dragging, value);
        }

        private Point _dragMouseStart = Point.Zero;
        private Point _dragDesiredLocation = Point.Zero;
        private SortedSet<Point> _possibleLocations;
        private Control _draggedItem;
        public override void UpdateContainer(GameTime gameTime)
        {
            if (this.Dragging)
            {
                var offset = Input.Mouse.Position - _dragMouseStart;
                _dragDesiredLocation += offset;

                if (_draggedItem != null)
                {
                    // dragging an item
                    _draggedItem.Location = GetClosestLocationToPossibleLocations(_dragDesiredLocation);
                }
                else
                {
                    // Dragging the group
                    Location += offset;
                }

                _dragMouseStart = Input.Mouse.Position;
            }

            //GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;
            //FixSize();
        }

        private void OnGlobalMouseRelease(object sender, MouseEventArgs e)
        {
            if (this.Visible && this.Dragging)
            {
                this.Dragging = false;
                GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseRelease;
                FixSize();

                if (_draggedItem != null)
                {
                    _draggedItem = null;

                    foreach (var item in this.Items)
                    {
                        item.Item.IconPosition = item.Location;
                    }
                    this.GroupInfo.Items = this.Items.Select(item => item.Item).ToObservableCollection();

                    OnItemsChanged();
                }
                else
                {
                    OnPositionChanged();
                }
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (DragMode != GroupDragMode.None)
            {
                GameService.Input.Mouse.LeftMouseButtonReleased += OnGlobalMouseRelease;

                this.Dragging = true;
                _dragMouseStart = Input.Mouse.Position;

                var effectiveDragMode = DragMode;
                if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(Microsoft.Xna.Framework.Input.ModifierKeys.Shift))
                {
                    if (DragMode == GroupDragMode.Group)
                    {
                        effectiveDragMode = GroupDragMode.Item;
                    }
                    else if (DragMode == GroupDragMode.Item)
                    {
                        effectiveDragMode = GroupDragMode.Group;
                    }
                }

                if (effectiveDragMode == GroupDragMode.Item)
                {
                    // Dragging an item
                    _draggedItem = GetChildOverMousePosition(e.MousePosition);

                    if (_draggedItem != null)
                    {
                        foreach (var child in Children)
                        {
                            child.Location += this.Location;
                        }
                        this.Location = Point.Zero;
                        this.Size = this.Parent.Size;
                        _dragDesiredLocation = _draggedItem.Location;
                        GetPossibleLocations();
                    }
                    else
                    {
                        this.Dragging = false;
                        GameService.Input.Mouse.LeftMouseButtonReleased += OnGlobalMouseRelease;
                    }
                }
            }


            base.OnLeftMouseButtonPressed(e);
        }

        private Control GetChildOverMousePosition(Point mousePosition)
        {
            List<Control> children = Children.ToList();
            IOrderedEnumerable<Control> zSortedChildren = children.OrderByDescending(i => i.ZIndex).ThenByDescending(c => children.IndexOf(c));

            foreach (var childControl in zSortedChildren)
            {
                if (childControl.AbsoluteBounds.Contains(mousePosition) && childControl.Visible)
                {
                    return childControl;
                }
            }
            return null;
        }

        private void GetPossibleLocations()
        {
            var iconSize = Items[0].Item.IconSize;
            var possibleLocations = new SortedSet<Point>(Comparer<Point>.Create((a, b) =>
            {
                if (a.Y == b.Y)
                {
                    return a.X.CompareTo(b.X);
                }
                else
                {
                    return a.Y.CompareTo(b.Y);
                }
            }));
            var left = new Point(-iconSize, 0);
            var right = new Point(iconSize, 0);
            var top = new Point(0, -iconSize);
            var bottom = new Point(0, iconSize);

            // Add 4 adjacent locations for each child
            foreach (var child in Children)
            {
                if (child == _draggedItem)
                {
                    continue;
                }

                possibleLocations.Add(child.Location + left);
                possibleLocations.Add(child.Location + right);
                possibleLocations.Add(child.Location + top);
                possibleLocations.Add(child.Location + bottom);
            }

            // Remove already occupied locations
            foreach (var child in Children)
            {
                if (child == _draggedItem)
                {
                    // dragged item's original location is still open
                    continue;
                }
                possibleLocations.Remove(child.Location);
            }

            _possibleLocations = possibleLocations;
        }

        private Point GetClosestLocationToPossibleLocations(Point desired)
        {
            long minDist = long.MaxValue;
            Point minPoint = Point.Zero;
            foreach (var possibleLocation in _possibleLocations)
            {
                var dX = possibleLocation.X - desired.X;
                var dY = possibleLocation.Y - desired.Y;
                var dist = (dX * dX) + (dY * dY);
                if (dist < minDist)
                {
                    minDist = dist;
                    minPoint = possibleLocation;
                }
            }
            return minPoint;
        }

        private void OnItemsChanged()
        {
            ItemsChanged?.Invoke(this, null);
        }

        private void OnPositionChanged()
        {
            PositionChanged?.Invoke(this, null);
        }

        public IEnumerable<ContextMenuStripItem> GetContextMenuItems()
        {
            if (AllowEdit)
            {
                ContextMenuStripItem deleteGroup = new ContextMenuStripItem(Strings.ContextMenu_Group_Delete);
                deleteGroup.Click += DeleteGroup_Click;
                yield return deleteGroup;
            }
        }

        private void DeleteGroup_Click(object sender, MouseEventArgs e)
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        protected override void DisposeControl()
        {
            GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseRelease;
        }
    }
}
