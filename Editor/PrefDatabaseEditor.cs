#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PrefDB.Editor
{
    /// <summary>
    /// Unity Editor window for viewing, managing, and debugging PrefDB data.
    /// Provides a visual interface to inspect and modify PlayerPrefs managed by PrefDB.
    /// </summary>
    /// <remarks>
    /// This editor window is accessible via Tools/Pref Database Editor menu.
    /// It displays structured table view of PrefDB data and allows individual or bulk operations.
    /// </remarks>
    internal sealed class PrefDatabaseEditor : EditorWindow
    {
        #region PRIVATE VARIABLES
        
        /// <summary>
        /// Scroll position for the main content area.
        /// </summary>
        private Vector2 _scrollPosition;
        
        /// <summary>
        /// Search filter string for filtering displayed tables and entries.
        /// </summary>
        private string _searchFilter = "";
        
        /// <summary>
        /// When true, only shows entries registered through PrefKeyRegistry.
        /// Currently not used in the current implementation.
        /// </summary>
        private bool _showOnlyPrefKeys = false;
        
        /// <summary>
        /// Dictionary of parsed PrefTableData objects, keyed by table name.
        /// Contains all PrefDB data parsed from PlayerPrefs.
        /// </summary>
        private Dictionary<string, PrefTableData> _tables = new Dictionary<string, PrefTableData>();
        
        #endregion

        #region EDITOR WINDOW INITIALIZATION
        
        /// <summary>
        /// Shows the Pref Database Editor window.
        /// </summary>
        /// <remarks>
        /// Menu item entry: Tools/Pref Database Editor
        /// </remarks>
        [MenuItem("Tools/Pref Database Editor")]
        internal static void ShowWindow()
        {
            GetWindow<PrefDatabaseEditor>("Pref Database");
        }

        /// <summary>
        /// Called when the editor window is enabled.
        /// Initializes the data by parsing PlayerPrefs.
        /// </summary>
        private void OnEnable()
        {
            RefreshData();
        }
        
        #endregion

        #region DATA MANAGEMENT
        
        /// <summary>
        /// Refreshes the displayed data by parsing all PlayerPrefs keys registered with PrefDB.
        /// </summary>
        /// <remarks>
        /// Parses PrefDB formatted keys (Table.Row.Column or Table.Column) and organizes them into table structures.
        /// </remarks>
        private void RefreshData()
        {
            _tables.Clear();

            // Collect all registered keys from PrefKeyRegistry
            var allKeys = PrefKeyRegistry.GetAllKeys();

            foreach (var key in allKeys)
            {
                var parts = key.Split('.');
                if (parts.Length < 2) continue;

                string tableName = parts[0];
                string rowId = null;
                string columnName;

                if (parts.Length == 2)
                {
                    // Single row table: Table.Column
                    columnName = parts[1];
                }
                else if (parts.Length == 3)
                {
                    // Multi row table: Table.Row.Column
                    rowId = parts[1];
                    columnName = parts[2];
                }
                else
                {
                    continue;
                }

                if (!_tables.ContainsKey(tableName))
                {
                    _tables[tableName] = new PrefTableData(tableName);
                }

                _tables[tableName].AddEntry(rowId, columnName, key);
            }
        }
        
        #endregion

        #region GUI RENDERING
        
        /// <summary>
        /// Main GUI method for rendering the editor window contents.
        /// </summary>
        private void OnGUI()
        {
            DrawToolbar();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_tables.Count == 0)
            {
                EditorGUILayout.HelpBox("No pref data found.", MessageType.Info);
            }
            else
            {
                DrawTables();
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the toolbar with refresh and clear buttons.
        /// </summary>
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                RefreshData();
            }

            if (GUILayout.Button("Clear All", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Clear All Preferences",
                    "Are you sure you want to delete ALL PlayerPrefs? This cannot be undone.",
                    "Yes", "No"))
                {
                    PlayerPrefs.DeleteAll();
                    RefreshData();
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws all tables with their entries, applying search filter if present.
        /// </summary>
        private void DrawTables()
        {
            foreach (var tablePair in _tables.OrderBy(t => t.Key))
            {
                var table = tablePair.Value;

                if (!string.IsNullOrEmpty(_searchFilter) &&
                    !table.Name.ToLower().Contains(_searchFilter.ToLower()) &&
                    !table.Entries.Any(e => e.ColumnName.ToLower().Contains(_searchFilter.ToLower()) ||
                                           (e.RowId != null && e.RowId.ToLower().Contains(_searchFilter.ToLower()))))
                {
                    continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Table header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Table: {table.Name}", EditorStyles.boldLabel);
                if (GUILayout.Button("Clear Table", GUILayout.Width(80)))
                {
                    ClearTable(table.Name);
                }
                EditorGUILayout.EndHorizontal();

                // Entries
                foreach (var entry in table.Entries.OrderBy(e => e.RowId ?? "").ThenBy(e => e.ColumnName))
                {
                    if (!string.IsNullOrEmpty(_searchFilter) &&
                        !entry.ColumnName.ToLower().Contains(_searchFilter.ToLower()) &&
                        !(entry.RowId != null && entry.RowId.ToLower().Contains(_searchFilter.ToLower())))
                    {
                        continue;
                    }

                    DrawEntry(entry);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(10);
            }
        }

        /// <summary>
        /// Draws an individual PrefDB entry with its value and controls.
        /// </summary>
        /// <param name="entry">The PrefEntry to display.</param>
        private void DrawEntry(PrefEntry entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            // Entry header
            EditorGUILayout.BeginHorizontal();

            string label = entry.RowId != null
                ? $"Row: {entry.RowId} | Column: {entry.ColumnName}"
                : $"Column: {entry.ColumnName}";

            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                ClearEntry(entry);
            }

            EditorGUILayout.EndHorizontal();

            // Value display
            EditorGUILayout.BeginHorizontal();

            // Try to detect type
            if (PlayerPrefs.HasKey(entry.Key))
            {
                string stringValue = PlayerPrefs.GetString(entry.Key, null);
                int intValue = PlayerPrefs.GetInt(entry.Key, int.MinValue);
                float floatValue = PlayerPrefs.GetFloat(entry.Key, float.NaN);

                EditorGUILayout.LabelField("Value:", GUILayout.Width(40));

                if (stringValue != null && intValue == int.MinValue && float.IsNaN(floatValue))
                {
                    EditorGUILayout.LabelField(stringValue, EditorStyles.wordWrappedMiniLabel);
                }
                else if (!float.IsNaN(floatValue))
                {
                    EditorGUILayout.LabelField(floatValue.ToString());
                }
                else if (intValue != int.MinValue)
                {
                    EditorGUILayout.LabelField(intValue.ToString());
                }
            }
            else
            {
                EditorGUILayout.LabelField("Key not found", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();

            // Key display
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key:", GUILayout.Width(30));
            EditorGUILayout.SelectableLabel(entry.Key, EditorStyles.miniLabel,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        
        #endregion

        #region DATA OPERATIONS
        
        /// <summary>
        /// Clears all entries from a specific table.
        /// </summary>
        /// <param name="tableName">The name of the table to clear.</param>
        /// <remarks>
        /// Prompts for confirmation before deleting data. Also unregisters keys from PrefKeyRegistry.
        /// </remarks>
        private void ClearTable(string tableName)
        {
            if (EditorUtility.DisplayDialog($"Clear Table {tableName}",
                $"Are you sure you want to clear the table?",
                "Yes", "No"))
            {
                var keysToDelete = _tables[tableName].Entries.Select(e => e.Key).ToList();

                foreach (var key in keysToDelete)
                {
                    PlayerPrefs.DeleteKey(key);
                    PrefKeyRegistry.UnregisterKey(key);
                }

                RefreshData();
            }
        }

        /// <summary>
        /// Clears a single PrefDB entry.
        /// </summary>
        /// <param name="entry">The PrefEntry to clear.</param>
        /// <remarks>
        /// Prompts for confirmation before deleting the entry. Also unregisters the key from PrefKeyRegistry.
        /// </remarks>
        private void ClearEntry(PrefEntry entry)
        {
            if (EditorUtility.DisplayDialog($"Clear Item",
            $"Are you sure you want to clear the item from the table?",
            "Yes", "No"))
            {
                PlayerPrefs.DeleteKey(entry.Key);
                PrefKeyRegistry.UnregisterKey(entry.Key);
                RefreshData();
            }
        }
        
        #endregion

        #region SERIALIZATION CLASSES
        
        /// <summary>
        /// Wrapper class for serializing PrefDB data to JSON format.
        /// </summary>
        /// <remarks>
        /// Used for export functionality. Currently not used in the main GUI but available for future features.
        /// </remarks>
        [System.Serializable]
        private class SerializationWrapper
        {
            /// <summary>
            /// List of tables to serialize.
            /// </summary>
            public List<PrefTableExport> tables = new List<PrefTableExport>();

            /// <summary>
            /// Initializes a new instance of the SerializationWrapper class.
            /// </summary>
            /// <param name="data">Dictionary of table data to wrap.</param>
            public SerializationWrapper(Dictionary<string, object> data)
            {
                foreach (var table in data)
                {
                    tables.Add(new PrefTableExport
                    {
                        tableName = table.Key,
                        data = table.Value
                    });
                }
            }
        }

        /// <summary>
        /// Represents a single table for export operations.
        /// </summary>
        [System.Serializable]
        private class PrefTableExport
        {
            /// <summary>
            /// Name of the table.
            /// </summary>
            public string tableName;
            
            /// <summary>
            /// Data contained in the table.
            /// </summary>
            public object data;
        }
        
        #endregion
    }

    #region DATA MODEL CLASSES
    
    /// <summary>
    /// Represents a PrefDB table with its entries for display in the editor.
    /// </summary>
    [System.Serializable]
    internal class PrefTableData
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// Gets the list of entries in this table.
        /// </summary>
        public List<PrefEntry> Entries { get; private set; } = new List<PrefEntry>();

        /// <summary>
        /// Initializes a new instance of the PrefTableData class.
        /// </summary>
        /// <param name="name">The name of the table.</param>
        public PrefTableData(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Adds a new entry to the table.
        /// </summary>
        /// <param name="rowId">The row identifier, or null for single-row tables.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="key">The full PlayerPrefs key.</param>
        public void AddEntry(string rowId, string columnName, string key)
        {
            Entries.Add(new PrefEntry
            {
                RowId = rowId,
                ColumnName = columnName,
                Key = key
            });
        }
    }

    /// <summary>
    /// Represents a single PrefDB entry for display in the editor.
    /// </summary>
    [System.Serializable]
    internal sealed class PrefEntry
    {
        /// <summary>
        /// The row identifier, or null for single-row tables.
        /// </summary>
        public string RowId;
        
        /// <summary>
        /// The column name.
        /// </summary>
        public string ColumnName;
        
        /// <summary>
        /// The full PlayerPrefs key.
        /// </summary>
        public string Key;
    }
    
    #endregion
}
#endif