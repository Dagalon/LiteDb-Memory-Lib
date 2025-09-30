# LiteDb-Memory-Lib (branch **feature-sqlite**)

Version with **SQLite support**  
(branch `feature-sqlite`)

## Overview

LiteDb-Memory-Lib is a lightweight library designed to emulate the behavior of **LiteDB** (or other embedded databases) entirely in memory, while still providing persistence options.  
The `feature-sqlite` branch integrates support for **SQLite** as a backend, enabling:

- Persistent storage using SQLite as the physical data store.
- Hybrid queries: fast in-memory operations with SQLite synchronization.
- Incremental synchronization between the volatile in-memory layer and the SQLite database.
- Useful for scenarios requiring both high in-memory performance and durable persistence.

## Key Features (`feature-sqlite`)

- Initialize in-memory database with optional SQLite persistence.
- Fast read/write operations in memory with configurable flush/commit modes to SQLite.
- Serialization / deserialization of collections and documents.
- Configurable options: SQLite file path, flush mode (immediate, deferred, batch).
- LINQ-style queries, indexing, and filtering inherited from the memory layer.
- Concurrency and locking mechanisms to avoid inconsistencies.

## Requirements

- .NET (>= .NET 6.0 recommended)
- SQLite provider (`Microsoft.Data.Sqlite`, `System.Data.SQLite`, or compatible)
- Dependencies from LiteDb-Memory-Lib base layer

## Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/Dagalon/LiteDb-Memory-Lib.git
   cd LiteDb-Memory-Lib
   ```

2. Checkout the branch:

   ```bash
   git checkout feature-sqlite
   ```

3. Restore packages:

   ```bash
   dotnet restore
   ```

4. Build:

   ```bash
   dotnet build
   ```

5. (Optional) Run tests:

   ```bash
   dotnet test
   ```

## Basic Usage

```csharp
using LiteDbMemoryLib.Sqlite;  // suggested namespace

// Create an in-memory store with SQLite persistence
var store = new SqliteMemoryStore(
    sqliteFilePath: "data.db",
    flushMode: FlushMode.OnCommit // or other modes
);

// Get or create a collection
var users = store.GetCollection<User>("users");

// Insert documents
users.Insert(new User { Id = 1, Name = "Alice" });
users.Insert(new User { Id = 2, Name = "Bob" });

// Query with LINQ-like syntax
var bobs = users.Find(u => u.Name == "Bob");

// Persist (if not using immediate flush)
await store.FlushAsync();

// Release resources
store.Dispose();
```

> **Note**: Adjust class/method names according to the actual API provided by this branch.

## Synchronization Modes

| Mode         | Description                              | Pros                 | Cons                      |
|--------------|------------------------------------------|----------------------|---------------------------|
| `OnCommit`   | Write-through to SQLite on every commit  | Strong durability    | Performance overhead      |
| `Batch`      | Accumulate changes, flush in batches     | Better performance   | Higher crash risk         |
| `ManualFlush`| User manually decides when to flush      | Full control         | Error handling required   |
| `Readonly`   | Only read from SQLite                    | Load existing data   | No write support          |

## Current Limitations

- Synchronization may add latency for large datasets.
- Complex/nested objects may not map directly to SQLite tables.
- Strong concurrency scenarios may require manual conflict resolution.
- Schema migrations to SQLite may need manual handling.
- Crash safety depends on flush mode.

## Roadmap

- Automatic migrations when schema changes.
- Compression or optimized serialization when persisting.
- Partial caching/pagination for very large datasets.
- SQLite WAL integration for better concurrency.
- Performance metrics and monitoring.

## Contributing

1. Fork the repository.
2. Create a feature branch (e.g. `feature-indexing`, `improve-flush`).
3. Implement your change with proper tests.
4. Submit a pull request describing your improvement.

## License

This project follows the same license as the root repository.  
Please check the `LICENSE` file in the main branch for details.
