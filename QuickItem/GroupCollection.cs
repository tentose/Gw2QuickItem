using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using Blish_HUD;

namespace QuickItem
{
    public class GroupCollection : ObservableCollection<ItemGroupInfo>
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        public void InitializeFromDisk()
        {
            this.Clear();
            var groupFiles = Directory.EnumerateFiles(QuickItemModule.Instance.GroupsDirectory, "*.group");

            // Use a temp dictionary to ensure there are no duplicate names
            Dictionary<string, ItemGroupInfo> tempInfo = new Dictionary<string, ItemGroupInfo>();
            foreach(var groupFile in groupFiles)
            {
                try
                {
                    var groupInfo = JsonConvert.DeserializeObject<ItemGroupInfo>(File.ReadAllText(groupFile));
                    groupInfo.Name = Path.GetFileNameWithoutExtension(groupFile);
                    tempInfo.Add(groupInfo.Name, groupInfo);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to load group file. Skipping.");
                }
            }

            foreach (var kv in tempInfo)
            {
                this.Add(kv.Value);
                kv.Value.GroupChanged += HandleGroupChanged;
            }
        }

        private void HandleGroupChanged(object sender, string oldName)
        {
            var group = sender as ItemGroupInfo;
            if (group != null)
            {
                int groupIndex = -1;
                if (oldName != null)
                {
                    // If oldName is populated, this is a rename change
                    // First ensure the new name is not a conflict
                    bool conflictDetected = false;
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (this[i].Name == group.Name)
                        {
                            if (this[i] == group)
                            {
                                groupIndex = i;
                            }
                            else
                            {
                                conflictDetected = true;
                            }
                        }
                    }

                    if (conflictDetected)
                    {
                        Logger.Info($"Rename failed because the new name already exists. From:{oldName}. To:{group.Name}.");
                        group.PauseChangedNotifications = true;
                        group.Name = oldName;
                        group.PauseChangedNotifications = false;
                        return;
                    }
                }

                // Update the group on disk. And if this is a rename, remove the old file.
                if (UpdateGroup(group) && oldName != null)
                {
                    try
                    {
                        var oldPath = Path.Combine(QuickItemModule.Instance.GroupsDirectory, oldName + ".group");
                        File.Delete(oldPath);

                        // Fire collection changed event for rename events
                        if (groupIndex >= 0)
                        {
                            var collectionChangedArgs = new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Replace, group, group, groupIndex);
                            OnCollectionChanged(collectionChangedArgs);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"Failed to remove old group {oldName}.");
                    }
                }
            }
        }

        public bool UpdateGroup(ItemGroupInfo group)
        {
            try
            {
                var path = Path.Combine(QuickItemModule.Instance.GroupsDirectory, group.Name + ".group");
                var data = JsonConvert.SerializeObject(group, Formatting.Indented);
                File.WriteAllText(path, data);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to update group {group.Name}.");
                return false;
            }
            return true;
        }

        public ItemGroupInfo CopyGroup(ItemGroupInfo group)
        {
            int originalIndex = 0;
            for (originalIndex = 0; originalIndex < this.Count; originalIndex++)
            {
                if (this[originalIndex] == group)
                {
                    break;
                }
            }

            var originalPath = Path.Combine(QuickItemModule.Instance.GroupsDirectory, $"{group.Name}.group");
            for (int i = 2; i < 9999; i++)
            {
                var newPath = Path.Combine(QuickItemModule.Instance.GroupsDirectory, $"{group.Name}-{i}.group");
                if (File.Exists(newPath))
                {
                    continue;
                }
                else
                {
                    try
                    {
                        File.Copy(originalPath, newPath);
                        var groupInfo = JsonConvert.DeserializeObject<ItemGroupInfo>(File.ReadAllText(newPath));
                        groupInfo.Name = Path.GetFileNameWithoutExtension(newPath);
                        this.Insert(originalIndex == this.Count ? originalIndex : originalIndex + 1, groupInfo);
                        return groupInfo;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"Failed to copy group. From: {group.Name}, To: {group.Name}-{i}");
                    }
                }
            }
            return null;
        }
    }
}
