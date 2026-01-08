# PrefDB (Structured PlayerPrefs Database for Unity)

## Overview
PrefDB is a lightweight, SQL‑like abstraction layer built on top of Unity PlayerPrefs. It provides a table → row → column data model, fluent query APIs, and a dedicated Unity Editor window to inspect and manage saved data. The goal of PrefDB is to make PlayerPrefs structured, readable, debuggable, and scalable for game data.

## Features
- 📦 Table‑based data organization (similar to SQL tables)
- 🧩 Fluent query API (WHERE, SELECT, GET, SET, EXISTS)
- 🔢 Supports multiple types, objects, arrays, lists, hashsets, dictionaries
- 🧠 Automatic key tracking
- 🛠 Built‑in Unity Editor window to view & clear data
- 🚀 Zero external dependencies

## 📁 Core Concepts
### 📦 Tables
- A PrefTable represents a logical group of related data.
```PrefTable playersTable = PrefDatabase.CreateTable("Players");```
- Internally, all data is stored in PlayerPrefs using the format:
<TableName>.<RowId>.<ColumnName>

##### Rows & Columns
- Row → Entity identifier (e.g., player id, level id)
- Column → Property name (e.g., score, coins, settings)

Single‑row tables are also supported using an internal (single) row.

## 🔍 Querying Data

##### Basic Example
```csharp
PrefDatabase.Query("Players").WHERE("player1").SELECT("score").SET_INT(1200);
int score = PrefDatabase.Query("Players").WHERE("player1").SELECT("score").GET_INT();
```

###### Using PrefTable Directly

```csharp
var settings = PrefDatabase.CreateTable("Settings");
settings.SELECT("MusicEnabled").SET_BOOL(true);
bool enabled = settings.SELECT("MusicEnabled").GET_BOOL();
```

### 🧱 Supported Data Types

##### Primitive Types

```SET_INT(int)```
```SET_FLOAT(float)```
```SET_STRING(string)```
```SET_BOOL(bool)```
```GET_INT()```
```GET_FLOAT()```
```GET_STRING()```
```GET_BOOL()```

##### Objects (Serializable)
```SET_OBJECT(new Class())```
```GET_OBJECT()```

```csharp
[Serializable]
class PlayerData
{
    public int level;
    public int xp;
}

query.SET_OBJECT(new PlayerData { level = 5, xp = 200 });
var data = query.GET_OBJECT<PlayerData>();
```

##### Collections
###### Arrays
```SET_ARRAY(int[])```
```GET_ARRAY<int>()```
######  Lists

```SET_LIST(List<T>)```
```GET_LIST<T>()```
```ADD_TO_LIST(item)```
```REMOVE_FROM_LIST(item)```

######  HashSets
```SET_HASHSET(HashSet<T>)```
```GET_HASHSET<T>()```
```ADD_TO_HASHSET(item)```
```REMOVE_FROM_HASHSET(item)```
###### Dictionaries

```SET_DICTIONARY(Dictionary<TKey, TValue>)```
```GET_DICTIONARY<TKey, TValue>()```
```ADD_TO_DICTIONARY(key, value)```
```REMOVE_FROM_DICTIONARY(key)```

## 🗑 Data Management

###### Check if Value Exists

```bool exists = query.EXISTS();```
###### Delete a Value

```query.DELETE();```

###### Clear All PrefDB Data
```PrefDatabase.Clear();```
⚠️ This permanently deletes all PrefDB‑registered PlayerPrefs keys.

### 🛠 Unity Editor Window

PrefDB ships with a built‑in editor tool.

##### Open via
Tools → Pref Database Editor

##### Features

- View all tables, rows, and columns data
- Inspect stored values
- Delete individual entries
- Clear entire tables
- Clear all PlayerPrefs (with confirmation)
- This tool is Editor‑only and does not affect runtime builds.


#####  PrefDB – Making PlayerPrefs Structured & Maintainable

