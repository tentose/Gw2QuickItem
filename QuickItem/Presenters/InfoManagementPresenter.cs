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
    public abstract class InfoManagementPresenter<TView, TModel, TInfo> : Presenter<TView, TModel> where TView : InfoManagementView
                                                                                         where TModel : InfoCollection<TInfo>
                                                                                         where TInfo : class, INamedObject, INotifyInfoPropertyChanged, new()
    {
        public InfoManagementPresenter(TView view, TModel model) : base(view, model)
        {
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            this.View.SelectionChanged += View_SelectionChanged;
            this.View.CopySelectedItem += View_CopySelectedItem;
            this.View.DeleteSelectedItem += View_DeleteSelectedItem;

            return base.Load(progress);
        }

        private void View_DeleteSelectedItem(object sender, EventArgs e)
        {
            var menuItem = this.View.ItemsList.SelectedMenuItem as MenuItem;
            if (menuItem != null)
            {
                var name = menuItem.Text;
                var groupInfo = Model.FirstOrDefault((info) => info.Name == name);
                if (groupInfo != null)
                {
                    Model.DeleteInfo(groupInfo);
                }
            }
        }

        private void View_CopySelectedItem(object sender, EventArgs e)
        {
            var menuItem = this.View.ItemsList.SelectedMenuItem as MenuItem;
            if (menuItem != null)
            {
                var name = menuItem.Text;
                var groupInfo = Model.FirstOrDefault((info) => info.Name == name);
                if (groupInfo != null)
                {
                    Model.CopyInfo(groupInfo);
                }
            }
        }

        private void Model_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Below handling of changes assumes the underlying collection never fires a multi-item change, which is true for
            // the default implementation of ObservableCollection used by GroupCollection.
            var groupListItems = this.View.ItemsList.Children;
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        var item = e.NewItems[0] as TInfo;
                        MenuItem menuItem = new MenuItem(item.Name);
                        groupListItems.Insert(e.NewStartingIndex, menuItem);
                        menuItem.Parent = this.View.ItemsList;
                        this.View.ItemsList.Invalidate();
                        break;
                    }

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    groupListItems.RemoveAt(e.OldStartingIndex);
                    this.View.ItemsList.Invalidate();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    {
                        var item = e.NewItems[0] as TInfo;
                        MenuItem menuItem = new MenuItem(item.Name);
                        groupListItems[e.OldStartingIndex] = menuItem;
                        menuItem.Parent = this.View.ItemsList;
                        this.View.ItemsList.Invalidate();
                        break;
                    }

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    {
                        var item = groupListItems.ElementAt(e.OldStartingIndex);
                        groupListItems.RemoveAt(e.OldStartingIndex);
                        groupListItems.Insert(e.NewStartingIndex, item);
                        this.View.ItemsList.Invalidate();
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    View.ItemsList.ClearChildren();
                    break;
            }
        }

        protected abstract IView CreateConfigView(TInfo info);

        private void View_SelectionChanged(object sender, ControlActivatedEventArgs e)
        {
            var menuItem = e.ActivatedControl as MenuItem;
            if (menuItem != null)
            {
                var name = menuItem.Text;
                var groupInfo = Model.FirstOrDefault((info) => info.Name == name);
                if (groupInfo != null)
                {
                    View.ConfigContainer.Show(CreateConfigView(groupInfo));
                }
            }
        }

        protected override void UpdateView()
        {
            this.View.ItemsList.ClearChildren();
            MenuItem firstChild = null;
            foreach (var groupInfo in Model)
            {
                var menuItem = this.View.ItemsList.AddMenuItem(groupInfo.Name);
                if (firstChild == null)
                {
                    firstChild = menuItem;
                }
            }
            this.Model.CollectionChanged += Model_CollectionChanged;

            // Select the first child after a slight delay. Work around issue where sizing is inconsistent
            // if selecting immediately
            Task.Run(async () =>
            {
                await Task.Delay(200);
                this.View.ItemsList.Select(firstChild);
            });
        }
    }
}
