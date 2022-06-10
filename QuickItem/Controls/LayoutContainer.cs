using Blish_HUD;
using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class LayoutContainer : Container
    {
        private LayoutInfo _layoutInfo = new LayoutInfo();
        public LayoutInfo ActiveLayout
        {
            get { return _layoutInfo; }
            set
            {
                _layoutInfo.PropertyChanged -= _layoutInfo_PropertyChanged;
                _layoutInfo = value;
                _layoutInfo.PropertyChanged += _layoutInfo_PropertyChanged;

                ApplyLayout(_layoutInfo);
            }
        }

        private bool _allowEdit = false;
        public bool AllowEdit
        {
            get { return _allowEdit; }
            set
            {
                _allowEdit = value;
                foreach (var child in Children)
                {
                    var group = child as ItemIconGroup;
                    group.AllowActivation = !_allowEdit;
                    group.DragMode = _allowEdit ? GroupDragMode.Group : GroupDragMode.None;
                }
            }
        }

        private void _layoutInfo_PropertyChanged(object sender, string e)
        {
            ApplyLayout(_layoutInfo);
        }

        private GroupCollection _groups;

        public LayoutContainer(GroupCollection groups)
        {
            _groups = groups;
            groups.CollectionChanged += Groups_CollectionChanged;
            Parent = GameService.Graphics.SpriteScreen;
            Size = GameService.Graphics.SpriteScreen.Size;

            GameService.Graphics.SpriteScreen.Resized += SpriteScreen_Resized;
        }

        private void Groups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ApplyLayout(_layoutInfo);
        }

        private void SpriteScreen_Resized(object sender, ResizedEventArgs e)
        {
            Size = GameService.Graphics.SpriteScreen.Size;
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.None;
        }

        private void ApplyLayout(LayoutInfo info)
        {
            this.ClearChildren();
            ItemFinderNative.Instance.ClearItems();

            foreach (var groupRef in info.Groups)
            {
                var groupInfo = _groups.FindGroupById(groupRef.GroupId);
                if (groupInfo != null)
                {
                    var group = new ItemIconGroup()
                    {
                        GroupInfo = groupInfo,
                        Location = groupRef.Position,
                        AllowActivation = !_allowEdit,
                        DragMode = _allowEdit ? GroupDragMode.Group : GroupDragMode.None,
                        Parent = this,
                    };
                    group.PositionChanged += Group_PositionChanged;
                    groupInfo.PropertyChanged += GroupInfo_PropertyChanged;

                    foreach (var item in groupInfo.Items)
                    {
                        ItemFinderNative.Instance.AddItem(item.ItemAssetId);
                    }
                }
            }
        }

        private void GroupInfo_PropertyChanged(object sender, string e)
        {
            ApplyLayout(_layoutInfo);
        }

        private void Group_PositionChanged(object sender, EventArgs e)
        {
            _layoutInfo.Groups = this.Children.Select(c =>
            {
                var group = c as ItemIconGroup;
                return new GroupPosition()
                {
                    GroupId = group.GroupInfo.Guid,
                    Position = group.Location,
                };
            }).ToObservableCollection();
        }
    }
}
