using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace PrefDB.Tests
{
    [TestFixture]
    public class PrefQueryTests
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
        public void FROM_ShouldCreateQueryWithTable()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");
            var fromMethod = typeof(PrefQuery).GetMethod("FROM", 
                BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var query = (PrefQuery)fromMethod.Invoke(null, new object[] { table });

            // Assert
            Assert.IsNotNull(query);
            
            var tableField = typeof(PrefQuery).GetField("_table", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var actualTable = (PrefTable)tableField.GetValue(query);
            
            Assert.AreSame(table, actualTable);
        }

        [Test]
        public void WHERE_ShouldSetRowAndReturnSelf()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");
            var query = table.SELECT("Column");

            // Act
            var result = query.WHERE("TestRow");

            // Assert
            Assert.AreSame(query, result);
            
            var rowField = typeof(PrefQuery).GetField("_row", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rowValue = (string)rowField.GetValue(query);
            
            Assert.AreEqual("TestRow", rowValue);
        }

        [Test]
        public void SELECT_ShouldSetColumnAndReturnSelf()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");
            var query = table.WHERE("Row");

            // Act
            var result = query.SELECT("TestColumn");

            // Assert
            Assert.AreSame(query, result);
            
            var columnField = typeof(PrefQuery).GetField("_column", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var columnValue = (string)columnField.GetValue(query);
            
            Assert.AreEqual("TestColumn", columnValue);
        }

        #region Basic Type Tests

        [Test]
        public void GET_INT_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");
            int expectedDefault = 999;

            // Act
            var value = table.SELECT("NonExistentColumn").GET_INT(expectedDefault);

            // Assert
            Assert.AreEqual(expectedDefault, value);
        }

        [Test]
        public void SET_INT_And_GET_INT_ShouldWorkCorrectly()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("IntTable");
            int expectedValue = 42;

            // Act
            table.WHERE("Row1").SELECT("Value").SET_INT(expectedValue);
            var actualValue = table.WHERE("Row1").SELECT("Value").GET_INT();

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void GET_FLOAT_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");
            float expectedDefault = 3.14f;

            // Act
            var value = table.SELECT("NonExistentColumn").GET_FLOAT(expectedDefault);

            // Assert
            Assert.AreEqual(expectedDefault, value);
        }

        [Test]
        public void SET_FLOAT_And_GET_FLOAT_ShouldWorkCorrectly()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("FloatTable");
            float expectedValue = 3.14159f;

            // Act
            table.WHERE("Row1").SELECT("Pi").SET_FLOAT(expectedValue);
            var actualValue = table.WHERE("Row1").SELECT("Pi").GET_FLOAT();

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void GET_STRING_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");
            string expectedDefault = "default";

            // Act
            var value = table.SELECT("NonExistentColumn").GET_STRING(expectedDefault);

            // Assert
            Assert.AreEqual(expectedDefault, value);
        }

        [Test]
        public void SET_STRING_And_GET_STRING_ShouldWorkCorrectly()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("StringTable");
            string expectedValue = "Hello, World!";

            // Act
            table.WHERE("Row1").SELECT("Message").SET_STRING(expectedValue);
            var actualValue = table.WHERE("Row1").SELECT("Message").GET_STRING();

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void GET_BOOL_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");
            bool expectedDefault = true;

            // Act
            var value = table.SELECT("NonExistentColumn").GET_BOOL(expectedDefault);

            // Assert
            Assert.AreEqual(expectedDefault, value);
        }

        [Test]
        public void SET_BOOL_And_GET_BOOL_ShouldWorkCorrectly()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("BoolTable");
            bool expectedValue = true;

            // Act
            table.WHERE("Row1").SELECT("Flag").SET_BOOL(expectedValue);
            var actualValue = table.WHERE("Row1").SELECT("Flag").GET_BOOL();

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        #endregion

        #region Object Operations Tests

        [Test]
        public void GET_OBJECT_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");
            var expectedDefault = new TestObject { Id = 1, Name = "Default" };

            // Act
            var obj = table.SELECT("NonExistentColumn").GET_OBJECT(expectedDefault);

            // Assert
            Assert.AreEqual(expectedDefault.Id, obj.Id);
            Assert.AreEqual(expectedDefault.Name, obj.Name);
        }

        [Test]
        public void SET_OBJECT_And_GET_OBJECT_ShouldWorkCorrectly()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("ObjectTable");
            var expectedObject = new TestObject { Id = 42, Name = "Test Object" };

            // Act
            table.WHERE("Row1").SELECT("Object").SET_OBJECT(expectedObject);
            var actualObject = table.WHERE("Row1").SELECT("Object").GET_OBJECT<TestObject>();

            // Assert
            Assert.AreEqual(expectedObject.Id, actualObject.Id);
            Assert.AreEqual(expectedObject.Name, actualObject.Name);
        }

        [Test]
        public void SET_OBJECT_WithNull_ShouldStoreEmptyString()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("NullObjectTable");

            // Act
            table.WHERE("Row1").SELECT("Object").SET_OBJECT<TestObject>(null);
            var storedValue = PlayerPrefs.GetString("NullObjectTable.Row1.Object");

            // Assert
            Assert.AreEqual("", storedValue);
        }

        #endregion

        #region Collection Tests

        [Test]
        public void SET_ARRAY_And_GET_ARRAY_ShouldWorkCorrectly()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("ArrayTable");
            var expectedArray = new[] { 1, 2, 3, 4, 5 };

            // Act
            table.WHERE("Row1").SELECT("Numbers").SET_ARRAY(expectedArray);
            var actualArray = table.WHERE("Row1").SELECT("Numbers").GET_ARRAY<int>();

            // Assert
            CollectionAssert.AreEqual(expectedArray, actualArray);
        }

        [Test]
        public void SET_LIST_And_GET_LIST_ShouldWorkCorrectly()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("ListTable");
            var expectedList = new List<string> { "Apple", "Banana", "Cherry" };

            // Act
            table.WHERE("Row1").SELECT("Fruits").SET_LIST(expectedList);
            var actualList = table.WHERE("Row1").SELECT("Fruits").GET_LIST<string>();

            // Assert
            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [Test]
        public void ADD_TO_LIST_ShouldAddItem()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("ListTable");

            // Act
            table.WHERE("Row1").SELECT("Items").ADD_TO_LIST("Item1");
            table.WHERE("Row1").SELECT("Items").ADD_TO_LIST("Item2");
            var list = table.WHERE("Row1").SELECT("Items").GET_LIST<string>();

            // Assert
            Assert.AreEqual(2, list.Count);
            CollectionAssert.Contains(list, "Item1");
            CollectionAssert.Contains(list, "Item2");
        }

        [Test]
        public void REMOVE_FROM_LIST_ShouldRemoveItem()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("ListTable");
            table.WHERE("Row1").SELECT("Items").SET_LIST(new List<string> { "Item1", "Item2", "Item3" });

            // Act
            table.WHERE("Row1").SELECT("Items").REMOVE_FROM_LIST("Item2");
            var list = table.WHERE("Row1").SELECT("Items").GET_LIST<string>();

            // Assert
            Assert.AreEqual(2, list.Count);
            CollectionAssert.DoesNotContain(list, "Item2");
        }

        [Test]
        public void SET_DICTIONARY_And_GET_DICTIONARY_ShouldWorkCorrectly()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("DictionaryTable");
            var expectedDict = new Dictionary<string, int>
            {
                { "One", 1 },
                { "Two", 2 },
                { "Three", 3 }
            };

            // Act
            table.WHERE("Row1").SELECT("Numbers").SET_DICTIONARY(expectedDict);
            var actualDict = table.WHERE("Row1").SELECT("Numbers").GET_DICTIONARY<string, int>();

            // Assert
            CollectionAssert.AreEqual(expectedDict, actualDict);
        }

        [Test]
        public void ADD_TO_DICTIONARY_ShouldAddOrUpdateItem()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("DictTable");

            // Act
            table.WHERE("Row1").SELECT("Data").ADD_TO_DICTIONARY("Key1", "Value1");
            table.WHERE("Row1").SELECT("Data").ADD_TO_DICTIONARY("Key2", "Value2");
            table.WHERE("Row1").SELECT("Data").ADD_TO_DICTIONARY("Key1", "UpdatedValue1");
            
            var dict = table.WHERE("Row1").SELECT("Data").GET_DICTIONARY<string, string>();

            // Assert
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("UpdatedValue1", dict["Key1"]);
            Assert.AreEqual("Value2", dict["Key2"]);
        }

        #endregion

        [Test]
        public void DELETE_ShouldRemoveKey()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("DeleteTable");
            table.WHERE("Row1").SELECT("Value").SET_INT(42);

            // Act
            table.WHERE("Row1").SELECT("Value").DELETE();
            var exists = table.WHERE("Row1").SELECT("Value").EXISTS();

            // Assert
            Assert.IsFalse(exists);
        }

        [Test]
        public void EXISTS_ShouldReturnFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("ExistsTable");

            // Act
            var exists = table.WHERE("Row1").SELECT("Value").EXISTS();

            // Assert
            Assert.IsFalse(exists);
        }

        [Test]
        public void EXISTS_ShouldReturnTrue_WhenKeyExists()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("ExistsTable");
            table.WHERE("Row1").SELECT("Value").SET_INT(42);

            // Act
            var exists = table.WHERE("Row1").SELECT("Value").EXISTS();

            // Assert
            Assert.IsTrue(exists);
        }

        [Test]
        public void BuildKey_ShouldUseSingleAsDefaultRow_WhenRowNotSpecified()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("TestTable");
            var query = table.SELECT("Column");

            // Act
            var buildKeyMethod = typeof(PrefQuery).GetMethod("BuildKey", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var key = (string)buildKeyMethod.Invoke(query, null);

            // Assert
            Assert.AreEqual("TestTable.(single).Column", key);
        }

        [Test]
        public void MultipleOperations_ShouldNotInterfere()
        {
            // Arrange
            var table = PrefDatabase.CreateTable("MultiTable");

            // Act
            table.WHERE("Row1").SELECT("Value1").SET_INT(100);
            table.WHERE("Row2").SELECT("Value2").SET_STRING("Hello");
            table.WHERE("Row1").SELECT("Value3").SET_BOOL(true);

            // Assert
            Assert.AreEqual(100, table.WHERE("Row1").SELECT("Value1").GET_INT());
            Assert.AreEqual("Hello", table.WHERE("Row2").SELECT("Value2").GET_STRING());
            Assert.AreEqual(true, table.WHERE("Row1").SELECT("Value3").GET_BOOL());
        }

        [Serializable]
        private class TestObject
        {
            public int Id;
            public string Name;
        }
    }
}