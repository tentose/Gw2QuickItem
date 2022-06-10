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
    public abstract class InfoCollection<T> : ObservableCollection<T> where T : class, INotifyInfoPropertyChanged, INamedObject, new()
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        protected abstract string GetInfoFileExtension();
        protected abstract string GetInfoDirectory();

        public void InitializeFromDisk()
        {
            this.Clear();
            var infoFiles = Directory.EnumerateFiles(GetInfoDirectory(), $"*.{GetInfoFileExtension()}");

            // Use a temp dictionary to ensure there are no duplicate names
            Dictionary<string, T> tempInfo = new Dictionary<string, T>();
            foreach (var infoFile in infoFiles)
            {
                try
                {
                    var info = JsonConvert.DeserializeObject<T>(File.ReadAllText(infoFile));
                    info.Name = Path.GetFileNameWithoutExtension(infoFile);
                    tempInfo.Add(info.Name, info);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to load info file. Skipping.");
                }
            }

            foreach (var kv in tempInfo)
            {
                this.Add(kv.Value);
                kv.Value.PropertyChanged += HandleInfoPropertyChanged;
            }

            if (this.Count == 0)
            {
                // There was nothing to load from disk. Let's add an empty one and save it
                T info = new T();
                info.Name = "Default Empty";
                InitializeNewInfo(info);
                this.Add(info);
                info.PropertyChanged += HandleInfoPropertyChanged;
                UpdateInfo(info);
            }
        }

        private void HandleInfoPropertyChanged(object sender, string oldName)
        {
            var info = sender as T;
            if (info != null)
            {
                int infoIndex = -1;
                if (oldName != null)
                {
                    // If oldName is populated, this is a rename change
                    // First ensure the new name is not a conflict
                    bool conflictDetected = false;
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (this[i].Name == info.Name)
                        {
                            if (this[i] == info)
                            {
                                infoIndex = i;
                            }
                            else
                            {
                                conflictDetected = true;
                            }
                        }
                    }

                    if (conflictDetected)
                    {
                        Logger.Info($"Rename failed because the new name already exists. From:{oldName}. To:{info.Name}.");
                        info.PauseChangedNotifications = true;
                        info.Name = oldName;
                        info.PauseChangedNotifications = false;
                        return;
                    }
                }

                // Update the info on disk. And if this is a rename, remove the old file.
                if (UpdateInfo(info) && oldName != null)
                {
                    try
                    {
                        var oldPath = Path.Combine(GetInfoDirectory(), $"{oldName}.{GetInfoFileExtension()}");
                        File.Delete(oldPath);

                        // Fire collection changed event for rename events
                        if (infoIndex >= 0)
                        {
                            var collectionChangedArgs = new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Replace, info, info, infoIndex);
                            OnCollectionChanged(collectionChangedArgs);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"Failed to remove old info {oldName}.");
                    }
                }
            }
        }

        public bool UpdateInfo(T info)
        {
            try
            {
                var path = Path.Combine(GetInfoDirectory(), $"{info.Name}.{GetInfoFileExtension()}");
                var data = JsonConvert.SerializeObject(info, Formatting.Indented);
                File.WriteAllText(path, data);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to update info {info.Name}.");
                return false;
            }
            return true;
        }

        public T CopyInfo(T originalInfo)
        {
            // Make a copy of the given info by making a copy of its file on disk and deserializing it.
            int originalIndex = 0;
            for (; originalIndex < this.Count; originalIndex++)
            {
                if (this[originalIndex] == originalInfo)
                {
                    break;
                }
            }

            var originalPath = Path.Combine(GetInfoDirectory(), $"{originalInfo.Name}.{GetInfoFileExtension()}");
            for (int i = 2; i < 9999; i++)
            {
                var newPath = Path.Combine(GetInfoDirectory(), $"{originalInfo.Name}-{i}.{GetInfoFileExtension()}");
                if (File.Exists(newPath))
                {
                    continue;
                }
                else
                {
                    try
                    {
                        File.Copy(originalPath, newPath);
                        var info = JsonConvert.DeserializeObject<T>(File.ReadAllText(newPath));
                        info.Name = Path.GetFileNameWithoutExtension(newPath);
                        InitializeNewInfo(info);

                        // Insert the new item right after the existing item (or the very end).
                        this.Insert(originalIndex == this.Count ? originalIndex : originalIndex + 1, info);

                        return info;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"Failed to copy info. From: {originalInfo.Name}, To: {originalInfo.Name}-{i}");
                    }
                }
            }
            return null;
        }

        public void DeleteInfo(T originalInfo)
        {
            var path = Path.Combine(GetInfoDirectory(), $"{originalInfo.Name}.{GetInfoFileExtension()}");

            try
            {
                File.Delete(path);
                this.Remove(originalInfo);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to remove info {originalInfo.Name}");
            }
        }

        protected virtual void InitializeNewInfo(T newInfo)
        {
        }

        public T FindInfoByName(string name)
        {
            return this.FirstOrDefault(info => info.Name == name);
        }
    }
}
