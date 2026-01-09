using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrefDB
{
    /// <summary>
    /// public registry for tracking all PlayerPrefs keys managed by the PrefDB system.
    /// Provides key lifecycle management and ensures proper cleanup of PrefDB data.
    /// </summary>
    /// <remarks>
    /// This class maintains a special registry key in PlayerPrefs that stores a list of all
    /// keys created by the PrefDB system, enabling bulk operations and proper cleanup.
    /// </remarks>
    public static class PrefKeyRegistry
    {
        #region CONSTANTS
        
        /// <summary>
        /// The PlayerPrefs key used to store the registry of all PrefDB-managed keys.
        /// </summary>
        /// <remarks>
        /// This key stores a JSON-serialized list of all keys created through the PrefDB system.
        /// </remarks>
        private const string RegistryKey = "__PREF_DB_KEYS__";
        
        #endregion

        #region PUBLIC METHODS
        
        /// <summary>
        /// Registers a new key with the PrefDB system.
        /// </summary>
        /// <param name="key">The PlayerPrefs key to register.</param>
        /// <remarks>
        /// Adds the key to the registry if it doesn't already exist and saves the updated registry.
        /// This method is called automatically whenever a SET operation is performed through PrefQuery.
        /// </remarks>
        public static void Register(string key)
        {
            var keys = GetAllKeys();
            if (keys.Add(key))
                Save(keys);
        }

        /// <summary>
        /// Removes a key from the PrefDB registry.
        /// </summary>
        /// <param name="key">The PlayerPrefs key to unregister.</param>
        /// <remarks>
        /// Removes the key from the registry and saves the updated registry.
        /// This method is called automatically when a DELETE operation is performed through PrefQuery.
        /// </remarks>
        public static void UnregisterKey(string key)
        {
            var keys = GetAllKeys();
            if (keys.Remove(key))
            {
                Save(keys);
            }
        }

        /// <summary>
        /// Retrieves all keys currently registered with the PrefDB system.
        /// </summary>
        /// <returns>A HashSet containing all registered PlayerPrefs keys.</returns>
        /// <remarks>
        /// Returns an empty HashSet if no keys are registered or if the registry key doesn't exist.
        /// </remarks>
        public static HashSet<string> GetAllKeys()
        {
            string json = PlayerPrefs.GetString(RegistryKey, "");
            if (string.IsNullOrEmpty(json))
                return new HashSet<string>();

            var wrapper = JsonUtility.FromJson<KeyWrapper>(json);
            return wrapper?.Keys != null
                ? new HashSet<string>(wrapper.Keys)
                : new HashSet<string>();
        }

        /// <summary>
        /// Clears all PrefDB-managed data from PlayerPrefs.
        /// </summary>
        /// <remarks>
        /// Deletes all registered PlayerPrefs keys and removes the registry itself.
        /// This provides a complete cleanup of all data managed by the PrefDB system.
        /// Warning: This operation cannot be undone.
        /// </remarks>
        public static void ClearAll()
        {
            HashSet<string> keys = GetAllKeys();

            foreach (var key in keys)
                PlayerPrefs.DeleteKey(key);

            PlayerPrefs.DeleteKey(RegistryKey);
        }
        
        #endregion

        #region PRIVATE METHODS
        
        /// <summary>
        /// Saves the current set of registered keys to PlayerPrefs.
        /// </summary>
        /// <param name="keys">The HashSet of keys to save to the registry.</param>
        /// <remarks>
        /// Serializes the keys as JSON and stores them in the special registry key.
        /// This method is called publicly whenever the registry is modified.
        /// </remarks>
        private static void Save(HashSet<string> keys)
        {
            PlayerPrefs.SetString(RegistryKey, JsonUtility.ToJson(new KeyWrapper { Keys = new List<string>(keys) }));
        }
        
        #endregion

        #region SERIALIZATION CLASSES
        
        /// <summary>
        /// Serializable wrapper class for storing a list of keys in JSON format.
        /// </summary>
        /// <remarks>
        /// Used publicly by Unity's JsonUtility to serialize/deserialize the key registry.
        /// JsonUtility cannot directly serialize HashSet or List, so this wrapper is necessary.
        /// </remarks>
        [Serializable]
        private class KeyWrapper
        {
            /// <summary>
            /// The list of keys stored in the registry.
            /// </summary>
            public List<string> Keys;
        }
        
        #endregion
    }
}