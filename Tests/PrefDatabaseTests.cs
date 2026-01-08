using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace PrefDB.Tests
{
    [TestFixture]
    public class PrefDatabaseTests
    {
        [SetUp]
        public void SetUp()
        {
            // Clear all PlayerPrefs before each test
            PlayerPrefs.DeleteAll();
            // Note: PrefDatabase.Clear() would be better but it's not accessible in tests
            // as it requires PrefKeyRegistry which might have internal dependencies
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void CreateTable_ShouldCreateNewTable_WhenTableDoesNotExist()
        {
            // Arrange
            string tableName = "TestTable";

            // Act
            var table = PrefDatabase.CreateTable(tableName);

            // Assert
            Assert.IsNotNull(table);
            Assert.AreEqual(tableName, table.GetType().GetProperty("Name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(table));
        }

        [Test]
        public void CreateTable_ShouldReturnExistingTable_WhenTableAlreadyExists()
        {
            // Arrange
            string tableName = "ExistingTable";
            var firstTable = PrefDatabase.CreateTable(tableName);

            // Act
            var secondTable = PrefDatabase.CreateTable(tableName);

            // Assert
            Assert.AreSame(firstTable, secondTable);
        }

        [Test]
        public void Query_ShouldReturnPrefQuery_WhenTableExists()
        {
            // Arrange
            string tableName = "QueryableTable";
            PrefDatabase.CreateTable(tableName);

            // Act
            var query = PrefDatabase.Query(tableName);

            // Assert
            Assert.IsNotNull(query);
        }

        [Test]
        public void Query_ShouldReturnNull_WhenTableDoesNotExist()
        {
            // Arrange
            string nonExistentTable = "NonExistentTable";

            // Act
            var query = PrefDatabase.Query(nonExistentTable);

            // Assert
            Assert.IsNull(query);
        }

        [Test]
        public void CreateTable_ShouldHandleSpecialCharactersInName()
        {
            // Arrange
            string[] testNames = new string[]
            {
                "Table-With-Dashes",
                "Table_With_Underscores",
                "Table With Spaces",
                "Table123",
                "123Table",
                "Table.With.Dots"
            };

            foreach (var tableName in testNames)
            {
                // Act
                var table = PrefDatabase.CreateTable(tableName);

                // Assert
                Assert.IsNotNull(table, $"Failed to create table with name: {tableName}");
            }
        }

        [Test]
        public void CreateTable_ShouldHandleEmptyString()
        {
            // Arrange
            string emptyTableName = "";

            // Act
            var table = PrefDatabase.CreateTable(emptyTableName);

            // Assert
            Assert.IsNotNull(table);
        }

        [Test]
        public void CreateTable_ShouldHandleNullString()
        {
            // Arrange
            string nullTableName = null;

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => PrefDatabase.CreateTable(nullTableName));
        }

        [Test]
        public void MultipleTables_ShouldBeIndependent()
        {
            // Arrange
            var table1 = PrefDatabase.CreateTable("Table1");
            var table2 = PrefDatabase.CreateTable("Table2");

            // Act
            var query1 = PrefDatabase.Query("Table1");
            var query2 = PrefDatabase.Query("Table2");

            // Assert
            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);
            Assert.AreNotSame(query1, query2);
        }
    }
}