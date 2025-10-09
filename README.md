# LiteDb-Memory-Lib

LiteDb-Memory-Lib provides a lightweight façade over [LiteDB](https://www.litedb.org/) that focuses on managing fully in-memory
LiteDB instances for tests, prototypes and short lived workloads. The library offers a central `ConnectionManager` that keeps track
of named databases, helpers to populate collections from objects or JSON files, and convenience methods around LiteDB file
storage and querying.

## Features

- **Alias based database registry** – create multiple isolated in-memory databases and retrieve them on demand.
- **Optional persistence** – export the contents of an in-memory database to disk when closing a connection.
- **Collection helpers** – populate collections from in-memory objects or JSON files with a single call.
- **File storage utilities** – upload, query and retrieve LiteDB file storage entries.
- **Query helpers** – use strongly typed expressions or raw LiteDB constructs without repeating boilerplate.
- **Robust JSON tooling** – `Tools.ReadJson` now validates input paths and a new `Tools.TryReadJson` helper avoids exceptions when
  loading optional resources.

## Getting started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [LiteDB](https://www.nuget.org/packages/LiteDB) (referenced by the project)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) (referenced by the project)

### Building the library

Clone the repository and build the solution from the repository root:

```bash
dotnet build
```

### Referencing the project

Until the package is published to NuGet you can reference the project directly from another solution:

```bash
dotnet add <your-project> reference ../LiteDb-Memory-Lib/LiteDb-Memory-Lib/LiteDb-Memory-Lib.csproj
```

## Usage examples

### Creating and using an in-memory database

```csharp
using LiteDb_Memory_Lib;

var manager = ConnectionManager.Instance();
manager.CreateDatabase("people-db");

var status = manager.CreateCollection("people-db", "people", new List<Person>
{
    new() { Id = 1, Name = "Ada" },
    new() { Id = 2, Name = "Grace" }
});

if (status == EnumsLiteDbMemory.Output.SUCCESS)
{
    var collection = manager.GetCollection<Person>("people-db", "people");
    var people = collection?.FindAll().ToList();
}
```

### Loading seed data from JSON

```csharp
var seeded = manager.CreateCollection<Person>(
    alias: "people-db",
    collection: "people",
    path: "./data/people.json",
    useInsertBulk: true);
```

`Tools.ReadJson` throws informative exceptions for missing files or invalid content, while `Tools.TryReadJson` allows callers to
check whether the JSON could be parsed without using exceptions for control flow.

### Persisting an in-memory database

```csharp
var result = manager.Close("people-db", pathToKeep: "./backups/people.db");
```

When `pathToKeep` is provided the database is flushed to disk before the in-memory resources are disposed, enabling a quick way to
persist state created during a test run.

### Working with LiteDB file storage

```csharp
var uploadResult = FileStorageTools.Upload(
    manager,
    alias: "people-db",
    id: "avatars",
    fileName: "ada.png",
    pathFile: "./assets/ada.png");

var fileInfo = FileStorageTools.Find(manager, "people-db", "avatars", "ada.png");
```

### Executing ad-hoc queries

The `GeneralTools.Execute` helper executes raw LiteDB commands and maps the results back into strongly typed objects:

```csharp
var queryResults = GeneralTools.Execute<Person>(manager, "people-db", "SELECT * FROM people WHERE Name = 'Ada'");
```

## Testing

Unit tests are located under the `LiteDb-Memory-Tests` and `SqliteDb-Memory-Tests` projects. Execute the full test suite with:

```bash
dotnet test
```

## License

This project is licensed under the [MIT License](./LICENSE).
