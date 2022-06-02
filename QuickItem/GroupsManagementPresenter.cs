using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class GroupsManagementPresenter : Presenter<GroupsManagementView, GroupCollection>
    {
        public GroupsManagementPresenter(GroupsManagementView view, GroupCollection model) : base(view, model)
        {
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            this.View.GroupSelectionChanged += View_GroupSelectionChanged;
            this.View.CopySelectedItem += View_CopySelectedItem;

            return base.Load(progress);
        }

        private void View_CopySelectedItem(object sender, EventArgs e)
        {
            var menuItem = this.View.GroupsList.SelectedMenuItem as MenuItem;
            if (menuItem != null)
            {
                var name = menuItem.Text;
                var groupInfo = Model.FirstOrDefault((info) => info.Name == name);
                if (groupInfo != null)
                {
                    Model.CopyGroup(groupInfo);
                }
            }
        }

        private void Model_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Below handling of changes assumes the underlying collection never fires a multi-item change, which is true for
            // the default implementation of ObservableCollection used by GroupCollection.
            var groupListItems = this.View.GroupsList.Children;
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        var item = e.NewItems[0] as ItemGroupInfo;
                        MenuItem menuItem = new MenuItem(item.Name);
                        groupListItems.Insert(e.NewStartingIndex, menuItem);
                        menuItem.Parent = this.View.GroupsList;
                        this.View.GroupsList.Invalidate();
                        break;
                    }

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    groupListItems.RemoveAt(e.OldStartingIndex);
                    this.View.GroupsList.Invalidate();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    {
                        var item = e.NewItems[0] as ItemGroupInfo;
                        MenuItem menuItem = new MenuItem(item.Name);
                        groupListItems[e.OldStartingIndex] = menuItem;
                        menuItem.Parent = this.View.GroupsList;
                        this.View.GroupsList.Invalidate();
                        break;
                    }

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    {
                        var item = groupListItems.ElementAt(e.OldStartingIndex);
                        groupListItems.RemoveAt(e.OldStartingIndex);
                        groupListItems.Insert(e.NewStartingIndex, item);
                        this.View.GroupsList.Invalidate();
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    View.GroupsList.ClearChildren();
                    break;
            }
        }

        private void View_GroupSelectionChanged(object sender, ControlActivatedEventArgs e)
        {
            var menuItem = e.ActivatedControl as MenuItem;
            if (menuItem != null)
            {
                var name = menuItem.Text;
                var groupInfo = Model.FirstOrDefault((info) => info.Name == name);
                if (groupInfo != null)
                {
                    View.GroupConfigContainer.Show(new GroupConfigView(groupInfo));
                }
            }
        }

        protected override void UpdateView()
        {
            this.View.GroupsList.ClearChildren();
            foreach (var groupInfo in Model)
            {
                MenuItem menuItem = new MenuItem(groupInfo.Name)
                {
                    Parent = this.View.GroupsList,
                };
            }
            this.Model.CollectionChanged += Model_CollectionChanged;
        }
    }
}
