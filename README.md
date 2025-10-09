# LiteDb-Memory-Lib

LiteDb-Memory-Lib is a lightweight helper around [LiteDB](https://www.litedb.org/) that focuses on making in-memory databases easy to manage from .NET 8 applications. It wraps common tasks such as creating temporary databases, populating collections from JSON files, running queries, and working with LiteDB's file storage API while keeping your application code minimal.

## Why use this library?

- **Centralised connection management.** Create, share, and close in-memory `LiteDatabase` instances with the `ConnectionManager`, including the ability to persist the in-memory data to disk when you are finished.【F:LiteDb-Memory-Lib/ConnectionManager.cs†L25-L90】
- **Collection helpers.** Quickly create collections from POCOs or JSON files, list collection names, and access typed `ILiteCollection<T>` instances.【F:LiteDb-Memory-Lib/ConnectionManager.cs†L44-L96】
- **Query utilities.** Retrieve documents using strongly-typed expressions, raw `BsonExpression`, or `Query` objects without rewriting boilerplate each time.【F:LiteDb-Memory-Lib/FilterTools.cs†L7-L75】
- **Data maintenance tools.** Create indexes, run SQL-like commands, and perform create/update/delete operations with friendly return codes.【F:LiteDb-Memory-Lib/GeneralTools.cs†L7-L71】【F:LiteDb-Memory-Lib/EnumsLiteDbMemory.cs†L3-L14】
- **File storage support.** Upload and fetch files via LiteDB's `FileStorage` API using either disk files or in-memory streams.【F:LiteDb-Memory-Lib/FileStorageTools.cs†L7-L67】
- **JSON utilities.** Deserialize JSON test data into objects with a single helper method.【F:LiteDb-Memory-Lib/Tools.cs†L5-L13】

## Getting started

### Prerequisites

- .NET 8 SDK
- [LiteDB](https://www.nuget.org/packages/LiteDB/) and [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) packages (already referenced in the project)

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/<your-account>/LiteDb-Memory-Lib.git
   cd LiteDb-Memory-Lib
   ```
2. Add a project reference from your application:

   ```bash
   dotnet add <your-project>.csproj reference ../LiteDb-Memory-Lib/LiteDb-Memory-Lib.csproj
   ```

## Quick start

```csharp
using LiteDb_Memory_Lib;

var manager = ConnectionManager.Instance();

// Create a brand-new in-memory database identified by an alias
manager.CreateDatabase(alias: "demo-db");

// Optionally preload a collection from a JSON file
manager.CreateCollection<Person>("demo-db", "people", path: "./Seed/people.json");

// Query using strongly typed expressions
var adults = FilterTools.Find<Person>(manager, "demo-db", "people", p => p.Age >= 18);

// Persist the in-memory data to disk before closing (optional)
manager.Close("demo-db", pathToKeep: "./output/demo-db.db");
```

> **Tip:** `EnumsLiteDbMemory.Output` provides consistent return codes so you can handle errors such as missing databases or collections without throwing exceptions.【F:LiteDb-Memory-Lib/EnumsLiteDbMemory.cs†L3-L14】

## Running the tests

The repository includes an extensive test suite that exercises connections, filters, inserts, and file storage helpers. Run all tests with:

```bash
dotnet test LiteDb-Memory-Tests
```

## Project structure

```
LiteDb-Memory-Lib/
├── LiteDb-Memory-Lib/         # Library source files
├── LiteDb-Memory-Tests/       # xUnit test project covering the helpers
├── MSBuild/                   # Build configuration helpers
└── README.md
```

## Contributing

Issues and pull requests are welcome! If you spot a scenario that could use another helper method, feel free to open a discussion or contribution.

## License

This project is distributed under the terms specified in the repository. Please review the license file if present, or contact the maintainers for more details.
