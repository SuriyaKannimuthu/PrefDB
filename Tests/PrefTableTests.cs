using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace PrefDB.Tests
{
    [TestFixture]
    public class PrefTableTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void Constructor_ShouldSetNameCorrectly()
        {
            // Arrange
            string tableName = "TestTable";
            
            // Use reflection to access internal constructor
            var constructor = typeof(PrefTable).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(string) },
                null);

            // Act
            var table = (PrefTable)constructor.Invoke(new object[] { tableName });

            // Assert
            var nameProperty = typeof(PrefTable).GetProperty("Name", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var actualName = (string)nameProperty.GetValue(table);
            
            Assert.AreEqual(tableName, actualName);
        }

        [Test]
        public void WHERE_ShouldReturnPrefQueryWithRowId()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");

            // Act
            var query = table.WHERE("TestRow");

            // Assert
            Assert.IsNotNull(query);
            
            // Verify row is set by using reflection to check private field
            var rowField = typeof(PrefQuery).GetField("_row", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rowValue = (string)rowField.GetValue(query);
            
            Assert.AreEqual("TestRow", rowValue);
        }

        [Test]
        public void SELECT_ShouldReturnPrefQueryWithColumn()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");

            // Act
            var query = table.SELECT("TestColumn");

            // Assert
            Assert.IsNotNull(query);
            
            // Verify column is set by using reflection to check private field
            var columnField = typeof(PrefQuery).GetField("_column", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var columnValue = (string)columnField.GetValue(query);
            
            Assert.AreEqual("TestColumn", columnValue);
        }

        [Test]
        public void WHERE_WithNullRowId_ShouldNotThrow()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");

            // Act & Assert
            Assert.DoesNotThrow(() => table.WHERE(null));
        }

        [Test]
        public void SELECT_WithNullColumn_ShouldNotThrow()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");

            // Act & Assert
            Assert.DoesNotThrow(() => table.SELECT(null));
        }

        [Test]
        public void WHERE_WithEmptyString_ShouldWork()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");

            // Act
            var query = table.WHERE("");

            // Assert
            Assert.IsNotNull(query);
        }

        [Test]
        public void SELECT_WithEmptyString_ShouldWork()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");

            // Act
            var query = table.SELECT("");

            // Assert
            Assert.IsNotNull(query);
        }

        [Test]
        public void MethodChaining_ShouldWork()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("ChainedTable");

            // Act - Test both orders of chaining
            var query1 = table.WHERE("Row1").SELECT("Column1");
            var query2 = table.SELECT("Column2").WHERE("Row2");

            // Assert
            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);
        }

        [Test]
        public void TableName_ShouldBeImmutable()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("ImmutableTable");
            var nameProperty = typeof(PrefTable).GetProperty("Name", 
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var name = (string)nameProperty.GetValue(table);

            // Assert
            Assert.AreEqual("ImmutableTable", name);
            
            // Verify it's read-only (no setter)
            var setter = nameProperty.GetSetMethod(true);
            Assert.IsNull(setter, "Name property should be read-only");
        }
    }
}