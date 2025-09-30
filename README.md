# LiteDb-Memory-Lib

## Overview

LiteDb-Memory-Lib is an experimental extension for LiteDB that enables fully in-memory document database usage combined with the ability to leverage SQLite as an internal backend. This hybrid design allows developers to choose between the lightweight NoSQL mode of LiteDB or the robust, ACID-compliant relational capabilities of SQLite seamlessly within .NET applications.

## Features

- NoSQL document database compatible with LiteDB
- Pure in-memory operation for ultra-fast, transient data handling
- **Full integration with SQLite as a backend** allowing relational storage, SQL querying, and durability
- Flexible mode switching between LiteDB document store and SQLite engine
- API inspired by MongoDB and LiteDB with added power of SQL when using SQLite
- Ideal for unit testing, prototyping, and smooth data migration between NoSQL and SQL worlds
- Supports collections, indexes, LINQ queries, and SQLite SQL syntax in hybrid modes
- Lightweight and embeddable for .NET projects

## Installation

dotnet add package LiteDb-Memory-Lib

text

Or reference the compiled DLL in your project manually.

## Basic Usage

using LiteDbMemoryLib;

var db = new LiteDbMemoryDatabase();
var col = db.GetCollection<MyClass>("entities");
col.Insert(new MyClass { Name = "Test" });
var results = col.FindAll().ToList();

text

### Using SQLite Backend (Recommended for durability and SQL features)

// Initialize database with SQLite backend for persistent and relational data
var db = new LiteDbMemoryDatabase("Filename=:memory:; Mode=SQLite;");

// Use SQL queries alongside NoSQL style document operations
db.Execute("CREATE TABLE Sample (Id INTEGER PRIMARY KEY, Name TEXT);");
db.Execute("INSERT INTO Sample (Name) VALUES ('Example');");
var data = db.Query<Sample>("SELECT * FROM Sample WHERE Name = @name", new { name = "Example" });

text

## When to Use SQLite Backend

- When ACID-compliant transactional consistency is required  
- For applications needing SQL query power alongside document storage  
- To enable seamless migration or interoperability with existing SQLite databases  
- When data durability and recovery are crucial beyond the lifetime of the process  

## Limitations

- In-memory mode loses data after the process ends  
- Some advanced LiteDB-exclusive features might not fully carry over to SQLite mode  
- The hybrid approach may have slight performance trade-offs depending on usage patterns  

## Credits

Built upon LiteDB and SQLite, with contributions from the open-source community.

## License

MIT license (see LICENSE file).