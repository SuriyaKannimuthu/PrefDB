using System.Collections.Generic;
using UnityEngine;

namespace PrefDB
{
    /// <summary>
    /// Static database manager for handling PrefTable instances and providing global database operations.
    /// Manages a registry of named tables for structured PlayerPrefs data storage and querying.
    /// </summary>
    public static class PrefDatabase
    {
        #region PRIVATE STATIC VARIABLES
        
        /// <summary>
        /// Dictionary storing all registered PrefTable instances, keyed by their table names.
        /// Provides centralized table management and prevents duplicate table creation.
        /// </summary>
        private static readonly Dictionary<string, PrefTable> _tables = new Dictionary<string, PrefTable>();
        
        #endregion

        #region PUBLIC STATIC METHODS
        
        /// <summary>
        /// Creates and registers a new PrefTable with the specified name, or returns existing table if already registered.
        /// </summary>
        /// <param name="tableName">Unique identifier for the table to create or retrieve.</param>
        /// <returns>Returns the newly created PrefTable instance if it didn't exist, otherwise returns the existing table.</returns>
        public static PrefTable CreateTable(string tableName)
        {
            if (!_tables.TryGetValue(tableName, out PrefTable table))
            {
                PrefTable prefTable = new PrefTable(tableName);
                _tables.Add(tableName, prefTable);
                return prefTable;
            }
            else
            {
                return table;
            }
        }

        /// <summary>
        /// Creates a query builder for the specified table to perform data retrieval operations.
        /// </summary>
        /// <param name="tableName">Name of the table to query.</param>
        /// <returns>Returns a PrefQuery instance for building queries, or null if the table doesn't exist.</returns>
        public static PrefQuery Query(string tableName)
        {
            if (!_tables.TryGetValue(tableName, out var table))
            {
                return null;
            }

            return PrefQuery.FROM(_tables[tableName]);
        }

        /// <summary>
        /// Clears all registered PrefTable data and saves PlayerPrefs changes.
        /// WARNING: This operation permanently removes all stored data managed by PrefDatabase.
        /// </summary>
        public static void Clear()
        {
            PrefKeyRegistry.ClearAll();
            PlayerPrefs.Save();
        }
        
        #endregion
    }
}