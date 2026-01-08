namespace PrefDB
{
    /// <summary>
    /// Represents a named table for storing structured data in PlayerPrefs.
    /// Each table organizes data into rows and columns, providing a structured way to manage related preferences.
    /// </summary>
    /// <remarks>
    /// This class is sealed to prevent inheritance and ensure consistent table behavior.
    /// Tables are created and managed through the PrefDatabase class.
    /// </remarks>
    public sealed class PrefTable
    {
        #region INTERNAL PROPERTIES
        
        /// <summary>
        /// Gets the unique name identifier for this table.
        /// Used as a namespace for storing and retrieving table-specific data from PlayerPrefs.
        /// </summary>
        internal string Name { get; }
        
        #endregion

        #region INTERNAL CONSTRUCTORS
        
        /// <summary>
        /// Initializes a new instance of the PrefTable class with the specified name.
        /// </summary>
        /// <param name="name">Unique identifier for the table. Used as a key prefix for all data stored by this table.</param>
        /// <remarks>
        /// This constructor is internal to enforce table creation through PrefDatabase.CreateTable() method.
        /// </remarks>
        internal PrefTable(string name)
        {
            Name = name;
        }
        
        #endregion

        #region PUBLIC METHODS
        
        /// <summary>
        /// Creates a query starting from this table, filtered by a specific row identifier.
        /// </summary>
        /// <param name="rowId">The unique identifier of the row to filter by.</param>
        /// <returns>Returns a PrefQuery instance initialized to query this table with the specified row filter.</returns>
        /// <example>
        /// <code>
        /// var query = myTable.WHERE("player1");
        /// </code>
        /// </example>
        public PrefQuery WHERE(string rowId)
        {
            return PrefQuery.FROM(this).WHERE(rowId);
        }

        /// <summary>
        /// Creates a query starting from this table, selecting a specific column to retrieve.
        /// </summary>
        /// <param name="column">The name of the column to select in the query.</param>
        /// <returns>Returns a PrefQuery instance initialized to query this table with the specified column selection.</returns>
        /// <example>
        /// <code>
        /// var query = myTable.SELECT("score");
        /// </code>
        /// </example>
        public PrefQuery SELECT(string column)
        {
            return PrefQuery.FROM(this).SELECT(column);
        }
        
        #endregion
    }
}