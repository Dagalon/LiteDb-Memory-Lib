# Database Memory Lib

A comprehensive C# library that provides simplified toolkits for working with both **SQLite** and **LiteDB** databases, supporting in-memory and file-based operations with a focus on performance and ease of use.

## 📋 Features

### SQLite Features
- **In-memory SQLite databases**: Create and manage SQLite databases in memory
- **File-based SQLite operations**: Work with persistent SQLite databases
- **Database attachment**: Attach multiple databases to a single connection
- **Table creation and management**: Simplified table creation with automatic type inference
- **CSV import/export**: Direct import from CSV files to tables with automatic type detection
- **Query execution**: Support for parameterized queries and file-based SQL scripts
- **WAL mode support**: Write-Ahead Logging for better performance
- **Backup operations**: Database backup and restore functionality
- **Parameter extraction**: Automatic detection of SQL parameters from query files
- **Transaction management**: Built-in transaction handling for bulk operations

### LiteDB Features
- **In-memory NoSQL databases**: Create and manage multiple LiteDB instances in memory
- **Optional persistence**: Save in-memory databases to files when needed  
- **Collection management**: Easy creation and manipulation of collections
- **Advanced filtering**: Search with Lambda expressions, BSON queries, and Query objects
- **JSON file support**: Direct import from JSON files
- **Bulk operations**: Optimized bulk insert operations for better performance
- **Reference handling**: Automatic loading of related documents
- **Index support**: Automatic indexing for better query performance

### Common Features
- **Unified connection management**: Centralized management for both database types
- **Singleton pattern**: Thread-safe singleton implementations
- **Memory optimization**: Efficient memory usage for in-memory operations
- **Cross-database operations**: Work with both SQLite and LiteDB simultaneously
- **Error handling**: Comprehensive error reporting with structured enums
- **Performance optimization**: Built-in optimizations for large datasets





