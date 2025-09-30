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
- Fast read/write operations in memory.
- Serialization / deserialization of collections and documents.
- LINQ-style queries, indexing, and filtering inherited from the memory layer.
- Concurrency and locking mechanisms to avoid inconsistencies.

## Requirements

- .NET (>= .NET 8.0 recommended)
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
