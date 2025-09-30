# LiteDb-Memory-Lib

## Overview

LiteDb-Memory-Lib is an experimental extension for LiteDB that allows for fully in-memory document database usage and seamless integration or migration utilizing SQLite as an internal backend. The goal is to provide fast, transient, and safe document storage for .NET environments, with easy interchangeability between LiteDB and SQLite engines.

## Features

- NoSQL document database compatible with LiteDB
- Pure in-memory operation (no persistent storage unless SQLite backend is configured)
- Optional support to run operations backed by SQLite or hybrid modes
- API inspired by MongoDB and LiteDB
- Suitable for unit testing, prototypes, and integration scenarios requiring transparent switch to SQLite
- Supports collections, indexes, and LINQ queries
- Lightweight and embeddable for .NET projects

## Installation

dotnet add package LiteDb-Memory-Lib

text
Or reference the built DLL manually in your project.

## Basic Usage

using LiteDbMemoryLib;

var db = new LiteDbMemoryDatabase();
var col = db.GetCollection<MyClass>("entities");
col.Insert(new MyClass { Name = "Test" });
var results = col.FindAll().ToList();

text

### Using SQLite Backend (if supported)

// Initialize with SQLite persistence
var db = new LiteDbMemoryDatabase("Filename=:memory:; Mode=SQLite;");

text

## Example Use Cases

- Automated testing: run fast, isolated tests with transient data
- Prototyping: short-lived model data without migration headaches
- Integration: facilitate migration between LiteDB and SQLite

## Limitations

- Data in pure memory mode is lost when process exits
- Some advanced LiteDB features may not be supported (see issues and roadmap)

## Credits

Built using LiteDB and community contributions.

## License

MIT license (see LICENSE file).