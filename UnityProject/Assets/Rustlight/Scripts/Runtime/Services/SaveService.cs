using System;
using System.IO;
using Rustlight.Core.Save;
using UnityEngine;

namespace Rustlight.Runtime.Services
{
    /// <summary>
    /// Reads and writes the save as JSON in persistentDataPath. Writes go to a temp file first and the
    /// previous save is kept as a backup, so a crash mid-write never destroys progress.
    /// </summary>
    public sealed class SaveService
    {
        private readonly string _path;
        private readonly string _backupPath;
        private readonly string _tempPath;

        public SaveService(string directory, string fileName = "save.json")
        {
            _path = Path.Combine(directory, fileName);
            _backupPath = _path + ".bak";
            _tempPath = _path + ".tmp";
        }

        /// <summary>Loads the save (falling back to the backup); unreadable files are set aside, never overwritten.</summary>
        public SaveData LoadOrCreate()
        {
            if (TryLoad(_path, out var data)) return data;
            if (TryLoad(_backupPath, out data))
            {
                Debug.LogWarning("[Save] Main save unreadable, restored from backup.");
                return data;
            }
            return SaveData.CreateNew();
        }

        /// <summary>Atomically writes the save; returns false (and logs) if anything failed.</summary>
        public bool TrySave(SaveData data)
        {
            try
            {
                File.WriteAllText(_tempPath, JsonUtility.ToJson(data));
                if (File.Exists(_path)) File.Copy(_path, _backupPath, overwrite: true);
                if (File.Exists(_path)) File.Delete(_path);
                File.Move(_tempPath, _path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Write failed: {e}");
                return false;
            }
        }

        private static bool TryLoad(string path, out SaveData data)
        {
            data = null;
            if (!File.Exists(path)) return false;
            try
            {
                data = SaveMigrator.Migrate(JsonUtility.FromJson<SaveData>(File.ReadAllText(path)));
                return true;
            }
            catch (Exception e)
            {
                string quarantine = $"{path}.unreadable-{DateTime.UtcNow:yyyyMMddHHmmss}";
                Debug.LogError($"[Save] Could not load {path}, moved to {quarantine}: {e.Message}");
                try { File.Copy(path, quarantine, overwrite: true); } catch { /* best effort */ }
                return false;
            }
        }
    }
}
