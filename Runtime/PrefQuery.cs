using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrefDB
{
    /// <summary>
    /// Provides a fluent interface for querying and manipulating data stored in a PrefTable.
    /// Supports type-safe operations for primitive types, objects, and various collection types.
    /// </summary>
    /// <remarks>
    /// This class implements a builder pattern for constructing queries. Queries are typically started
    /// using PrefDatabase.Query() or PrefTable.WHERE()/SELECT() methods.
    /// </remarks>
    public sealed class PrefQuery
    {
        #region PRIVATE VARIABLES

        /// <summary>
        /// Reference to the PrefTable being queried.
        /// </summary>
        private PrefTable _table = null;

        /// <summary>
        /// Row identifier filter for the query. If null, defaults to "(single)".
        /// </summary>
        private string _row = null;

        /// <summary>
        /// Column identifier for the query. Must be specified before GET/SET operations.
        /// </summary>
        private string _column = null;

        #endregion

        #region PRIVATE CONSTRUCTORS

        /// <summary>
        /// Initializes a new PrefQuery instance for the specified table.
        /// </summary>
        /// <param name="table">The table to query.</param>
        private PrefQuery(PrefTable table)
        {
            _table = table;
        }

        #endregion

        #region QUERY METHODS

        /// <summary>
        /// Creates a new query starting from the specified table.
        /// This is the entry point for building queries.
        /// </summary>
        /// <param name="table">The table to query.</param>
        /// <returns>A new PrefQuery instance.</returns>
        internal static PrefQuery FROM(PrefTable table)
        {
            return new PrefQuery(table);
        }

        /// <summary>
        /// Specifies the row identifier for the query.
        /// </summary>
        /// <param name="rowId">The unique identifier of the row to query.</param>
        /// <returns>The current PrefQuery instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// PrefDatabase.Query("Players").WHERE("player1").SELECT("score").GET_INT();
        /// </code>
        /// </example>
        public PrefQuery WHERE(string rowId)
        {
            _row = rowId;
            return this;
        }

        /// <summary>
        /// Specifies the column to select in the query.
        /// </summary>
        /// <param name="column">The name of the column to query.</param>
        /// <returns>The current PrefQuery instance for method chaining.</returns>
        /// <remarks>
        /// This method must be called before any GET or SET operations.
        /// </remarks>
        public PrefQuery SELECT(string column)
        {
            _column = column;
            return this;
        }

        #endregion

        #region Basic GETTERS/SETTERS

        /// <summary>
        /// Retrieves an integer value from PlayerPrefs.
        /// </summary>
        /// <param name="defaultValue">The value to return if the key doesn't exist. Default is 0.</param>
        /// <returns>The stored integer value, or defaultValue if not found.</returns>
        public int GET_INT(int defaultValue = 0) => PlayerPrefs.GetInt(BuildKey(), defaultValue);

        /// <summary>
        /// Retrieves a float value from PlayerPrefs.
        /// </summary>
        /// <param name="defaultValue">The value to return if the key doesn't exist. Default is 0.0.</param>
        /// <returns>The stored float value, or defaultValue if not found.</returns>
        public float GET_FLOAT(float defaultValue = 0.0F) => PlayerPrefs.GetFloat(BuildKey(), defaultValue);

        /// <summary>
        /// Retrieves a string value from PlayerPrefs.
        /// </summary>
        /// <param name="defaultValue">The value to return if the key doesn't exist. Default is empty string.</param>
        /// <returns>The stored string value, or defaultValue if not found.</returns>
        public string GET_STRING(string defaultValue = "") => PlayerPrefs.GetString(BuildKey(), defaultValue);

        /// <summary>
        /// Retrieves a boolean value from PlayerPrefs.
        /// </summary>
        /// <param name="defaultValue">The value to return if the key doesn't exist. Default is false.</param>
        /// <returns>The stored boolean value, or defaultValue if not found.</returns>
        public bool GET_BOOL(bool defaultValue = false) => bool.Parse(PlayerPrefs.GetString(BuildKey(), defaultValue.ToString()));

        /// <summary>
        /// Stores an integer value in PlayerPrefs.
        /// </summary>
        /// <param name="value">The integer value to store.</param>
        /// <remarks>
        /// Automatically registers the key with PrefKeyRegistry for tracking.
        /// </remarks>
        public void SET_INT(int value)
        {
            string key = BuildKey();
            PlayerPrefs.SetInt(key, value);
            PrefKeyRegistry.Register(key);
        }

        /// <summary>
        /// Stores a float value in PlayerPrefs.
        /// </summary>
        /// <param name="value">The float value to store.</param>
        /// <remarks>
        /// Automatically registers the key with PrefKeyRegistry for tracking.
        /// </remarks>
        public void SET_FLOAT(float value)
        {
            string key = BuildKey();
            PlayerPrefs.SetFloat(key, value);
            PrefKeyRegistry.Register(key);
        }

        /// <summary>
        /// Stores a string value in PlayerPrefs.
        /// </summary>
        /// <param name="value">The string value to store.</param>
        /// <remarks>
        /// Automatically registers the key with PrefKeyRegistry for tracking.
        /// </remarks>
        public void SET_STRING(string value)
        {
            string key = BuildKey();
            PlayerPrefs.SetString(key, value);
            PrefKeyRegistry.Register(key);
        }

        /// <summary>
        /// Stores a boolean value in PlayerPrefs.
        /// </summary>
        /// <param name="value">The boolean value to store.</param>
        /// <remarks>
        /// Stored as a string representation. Automatically registers the key with PrefKeyRegistry.
        /// </remarks>
        public void SET_BOOL(bool value)
        {
            string key = BuildKey();
            PlayerPrefs.SetString(key, value.ToString());
            PrefKeyRegistry.Register(key);
        }

        #endregion

        #region Object Operations

        /// <summary>
        /// Retrieves a serialized object from PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="defaultValue">The value to return if the key doesn't exist or contains empty JSON.</param>
        /// <returns>The deserialized object, or defaultValue if not found.</returns>
        /// <remarks>
        /// Uses Unity's JsonUtility for serialization. The type T must have a parameterless constructor.
        /// </remarks>
        public T GET_OBJECT<T>(T defaultValue = default) where T : new()
        {
            string json = GET_STRING();
            if (string.IsNullOrEmpty(json))
                return defaultValue;

            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// Stores an object as JSON in PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="obj">The object to serialize and store.</param>
        /// <remarks>
        /// Uses Unity's JsonUtility for serialization. If obj is null, stores an empty string.
        /// </remarks>
        public void SET_OBJECT<T>(T obj)
        {
            if (obj == null)
            {
                SET_STRING(string.Empty);
                return;
            }

            string json = JsonUtility.ToJson(obj);
            SET_STRING(json);
        }

        /// <summary>
        /// Retrieves a serialized object from PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <returns>The deserialized object, or default(T) if not found.</returns>
        /// <remarks>
        /// Overload that uses default value for the type T.
        /// </remarks>
        public T GET_OBJECT<T>() where T : new()
        {
            return GET_OBJECT<T>(default);
        }

        #endregion

        #region Array Operations

        /// <summary>
        /// Retrieves an array from PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of array elements.</typeparam>
        /// <param name="defaultValue">The array to return if the key doesn't exist.</param>
        /// <returns>The deserialized array, or defaultValue if not found.</returns>
        public T[] GET_ARRAY<T>(T[] defaultValue = null)
        {
            string json = GET_STRING();
            if (string.IsNullOrEmpty(json))
                return defaultValue ?? Array.Empty<T>();

            return JsonUtility.FromJson<SerializableArray<T>>(json).Items;
        }

        /// <summary>
        /// Stores an array in PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of array elements.</typeparam>
        /// <param name="array">The array to serialize and store.</param>
        public void SET_ARRAY<T>(T[] array)
        {
            var serializable = new SerializableArray<T> { Items = array };
            SET_STRING(JsonUtility.ToJson(serializable));
        }

        /// <summary>
        /// Retrieves a List from PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of list elements.</typeparam>
        /// <param name="defaultValue">The list to return if the key doesn't exist.</param>
        /// <returns>The deserialized list, or defaultValue if not found.</returns>
        public List<T> GET_LIST<T>(List<T> defaultValue = null)
        {
            string json = GET_STRING();
            if (string.IsNullOrEmpty(json))
                return defaultValue ?? new List<T>();

            return JsonUtility.FromJson<SerializableList<T>>(json).Items;
        }

        /// <summary>
        /// Stores a List in PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of list elements.</typeparam>
        /// <param name="list">The list to serialize and store.</param>
        public void SET_LIST<T>(List<T> list)
        {
            var serializable = new SerializableList<T> { Items = list };
            SET_STRING(JsonUtility.ToJson(serializable));
        }

        /// <summary>
        /// Retrieves a HashSet from PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of hashset elements.</typeparam>
        /// <param name="defaultValue">The hashset to return if the key doesn't exist.</param>
        /// <returns>The deserialized hashset, or defaultValue if not found.</returns>
        public HashSet<T> GET_HASHSET<T>(HashSet<T> defaultValue = null)
        {
            string json = GET_STRING();
            if (string.IsNullOrEmpty(json))
                return defaultValue ?? new HashSet<T>();

            return new HashSet<T>(JsonUtility.FromJson<SerializableList<T>>(json).Items);
        }

        /// <summary>
        /// Stores a HashSet in PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of hashset elements.</typeparam>
        /// <param name="hashSet">The hashset to serialize and store.</param>
        /// <remarks>
        /// Converts the HashSet to a List for serialization.
        /// </remarks>
        public void SET_HASHSET<T>(HashSet<T> hashSet)
        {
            var serializable = new SerializableList<T> { Items = hashSet.ToList() };
            SET_STRING(JsonUtility.ToJson(serializable));
        }

        /// <summary>
        /// Retrieves a Dictionary from PlayerPrefs.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of dictionary values.</typeparam>
        /// <param name="defaultValue">The dictionary to return if the key doesn't exist.</param>
        /// <returns>The deserialized dictionary, or defaultValue if not found.</returns>
        public Dictionary<TKey, TValue> GET_DICTIONARY<TKey, TValue>(
            Dictionary<TKey, TValue> defaultValue = null)
        {
            string json = GET_STRING();
            if (string.IsNullOrEmpty(json))
                return defaultValue ?? new Dictionary<TKey, TValue>();

            return JsonUtility.FromJson<SerializableDictionary<TKey, TValue>>(json).ToDictionary();
        }

        /// <summary>
        /// Stores a Dictionary in PlayerPrefs.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of dictionary values.</typeparam>
        /// <param name="dictionary">The dictionary to serialize and store.</param>
        public void SET_DICTIONARY<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            var serializable = SerializableDictionary<TKey, TValue>.FromDictionary(dictionary);
            SET_STRING(JsonUtility.ToJson(serializable));
        }

        #endregion

        #region Collection Utilities

        /// <summary>
        /// Adds an item to a List stored in PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of list elements.</typeparam>
        /// <param name="item">The item to add to the list.</param>
        /// <remarks>
        /// If no list exists at this key, creates a new list with the item.
        /// </remarks>
        public void ADD_TO_LIST<T>(T item)
        {
            var list = GET_LIST<T>(new List<T>());
            list.Add(item);
            SET_LIST(list);
        }

        /// <summary>
        /// Removes an item from a List stored in PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of list elements.</typeparam>
        /// <param name="item">The item to remove from the list.</param>
        public void REMOVE_FROM_LIST<T>(T item)
        {
            var list = GET_LIST<T>(new List<T>());
            list.Remove(item);
            SET_LIST(list);
        }

        /// <summary>
        /// Checks if a List stored in PlayerPrefs contains a specific item.
        /// </summary>
        /// <typeparam name="T">The type of list elements.</typeparam>
        /// <param name="item">The item to search for.</param>
        /// <returns>True if the item exists in the list, false otherwise.</returns>
        public bool LIST_CONTAINS<T>(T item)
        {
            var list = GET_LIST<T>(new List<T>());
            return list.Contains(item);
        }

        /// <summary>
        /// Adds an item to a HashSet stored in PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of hashset elements.</typeparam>
        /// <param name="item">The item to add to the hashset.</param>
        /// <remarks>
        /// If no hashset exists at this key, creates a new hashset with the item.
        /// </remarks>
        public void ADD_TO_HASHSET<T>(T item)
        {
            var hashSet = GET_HASHSET<T>(new HashSet<T>());
            hashSet.Add(item);
            SET_HASHSET(hashSet);
        }

        /// <summary>
        /// Removes an item from a HashSet stored in PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type of hashset elements.</typeparam>
        /// <param name="item">The item to remove from the hashset.</param>
        public void REMOVE_FROM_HASHSET<T>(T item)
        {
            var hashSet = GET_HASHSET<T>(new HashSet<T>());
            hashSet.Remove(item);
            SET_HASHSET(hashSet);
        }

        /// <summary>
        /// Adds or updates a key-value pair in a Dictionary stored in PlayerPrefs.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of dictionary values.</typeparam>
        /// <param name="key">The key to add or update.</param>
        /// <param name="value">The value to associate with the key.</param>
        /// <remarks>
        /// If no dictionary exists at this key, creates a new dictionary with the key-value pair.
        /// </remarks>
        public void ADD_TO_DICTIONARY<TKey, TValue>(TKey key, TValue value)
        {
            var dict = GET_DICTIONARY<TKey, TValue>(new Dictionary<TKey, TValue>());
            dict[key] = value;
            SET_DICTIONARY(dict);
        }

        /// <summary>
        /// Removes a key-value pair from a Dictionary stored in PlayerPrefs.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of dictionary values.</typeparam>
        /// <param name="key">The key to remove.</param>
        public void REMOVE_FROM_DICTIONARY<TKey, TValue>(TKey key)
        {
            var dict = GET_DICTIONARY<TKey, TValue>(new Dictionary<TKey, TValue>());
            dict.Remove(key);
            SET_DICTIONARY(dict);
        }

        #endregion

        /// <summary>
        /// Deletes the key-value pair associated with the current query from PlayerPrefs.
        /// </summary>
        /// <remarks>
        /// Also unregisters the key from PrefKeyRegistry.
        /// </remarks>
        public void DELETE()
        {
            string key = BuildKey();
            PlayerPrefs.DeleteKey(key);
            PrefKeyRegistry.UnregisterKey(key);
        }

        /// <summary>
        /// Checks if a value exists for the current query.
        /// </summary>
        /// <returns>True if a value exists for the current key, false otherwise.</returns>S
        public bool EXISTS()
        {
            return PlayerPrefs.HasKey(BuildKey());
        }

        /// <summary>
        /// Builds the PlayerPrefs key string from the current table, row, and column.
        /// </summary>
        /// <returns>The formatted key string.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if column is not specified before building the key.
        /// </exception>
        /// <remarks>
        /// Formats the key as: {TableName}.{RowId}.{ColumnName}
        /// Whitespace is removed from all components. If row is not specified, defaults to "(single)".
        /// </remarks>
        private string BuildKey()
        {
            _row = string.IsNullOrEmpty(_row) ? "(single)" : _row;

            if (string.IsNullOrEmpty(_column))
                throw new InvalidOperationException("PrefQuery error: Column is not specified. Call SELECT(column) with valud columnId before GET/SET.");

            string t = Regex.Replace(_table.Name, @"\s+", "");
            string r = Regex.Replace(_row, @"\s+", "");
            string c = Regex.Replace(_column, @"\s+", "");

            return $"{t}.{r}.{c}";
        }
    }

    #region Serializable Collection Classes

    /// <summary>
    /// Serializable wrapper for arrays.
    /// </summary>
    /// <typeparam name="T">The type of array elements.</typeparam>
    /// <remarks>
    /// Used internally for JSON serialization of arrays with Unity's JsonUtility.
    /// </remarks>
    [Serializable]
    public class SerializableArray<T>
    {
        /// <summary>
        /// The array of items.
        /// </summary>
        public T[] Items;
    }

    /// <summary>
    /// Serializable wrapper for lists.
    /// </summary>
    /// <typeparam name="T">The type of list elements.</typeparam>
    /// <remarks>
    /// Used internally for JSON serialization of lists with Unity's JsonUtility.
    /// </remarks>
    [Serializable]
    public class SerializableList<T>
    {
        /// <summary>
        /// The list of items.
        /// </summary>
        public List<T> Items;
    }

    /// <summary>
    /// Serializable wrapper for dictionaries.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of dictionary values.</typeparam>
    /// <remarks>
    /// Used internally for JSON serialization of dictionaries with Unity's JsonUtility.
    /// Converts dictionaries to/from lists of key-value pairs since JsonUtility doesn't
    /// support dictionary serialization directly.
    /// </remarks>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        /// <summary>
        /// Serializable representation of a key-value pair.
        /// </summary>
        [Serializable]
        public class KeyValuePair
        {
            /// <summary>
            /// The key of the pair.
            /// </summary>
            public TKey Key;

            /// <summary>
            /// The value of the pair.
            /// </summary>
            public TValue Value;
        }

        /// <summary>
        /// List of key-value pairs representing the dictionary.
        /// </summary>
        public List<KeyValuePair> Items = new List<KeyValuePair>();

        /// <summary>
        /// Creates a SerializableDictionary from a standard Dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to convert.</param>
        /// <returns>A SerializableDictionary containing the same data.</returns>
        public static SerializableDictionary<TKey, TValue> FromDictionary(Dictionary<TKey, TValue> dictionary)
        {
            var serializable = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in dictionary)
            {
                serializable.Items.Add(new KeyValuePair { Key = kvp.Key, Value = kvp.Value });
            }
            return serializable;
        }

        /// <summary>
        /// Converts the SerializableDictionary to a standard Dictionary.
        /// </summary>
        /// <returns>A Dictionary containing the same data.</returns>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            return Items.ToDictionary(item => item.Key, item => item.Value);
        }
    }

    #endregion
}