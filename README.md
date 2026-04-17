# csharp-primer

A collection of modern C# examples covering language fundamentals, concurrency, collections, streams, and patterns. Built as a reference primer for developers and as context for AI coding assistants to produce idiomatic, high-quality C# code.

---

## Purpose

This repository provides:

- **Executable, tested examples** — every example compiles and is covered by xUnit tests.
- **Idiomatic patterns** — code that follows modern C# conventions and .NET guidelines.
- **Reference material** — curated coverage of the topics developers encounter most often.
- **AI context** — structured, high-quality code that helps AI assistants produce better C#.

---

## Structure

```
CSharpPrimer.slnx
├── src/
│   ├── Fundamentals/          # Language core
│   │   ├── TypeSystem/        # Value types, reference types, nullable, records
│   │   ├── PatternMatching/   # Switch expressions, type patterns, list patterns
│   │   ├── ModernSyntax/      # Strings, tuples, local functions, LINQ basics
│   │   └── Generics/          # Constraints, INumber<T>, variance (covariance/contravariance)
│   ├── Concurrency/           # Async and parallel programming
│   │   ├── AsyncAwait/        # async/await, ValueTask, async streams
│   │   ├── Parallelism/       # Parallel.ForEach, PLINQ, Parallel.ForEachAsync
│   │   └── Synchronization/  # CancellationToken, SemaphoreSlim, Channels
│   ├── Collections/           # Data structures and LINQ
│   │   ├── BasicCollections/  # List, Dictionary, HashSet, Queue, Stack, PriorityQueue
│   │   ├── Linq/              # Advanced LINQ, custom extension operators
│   │   └── Advanced/          # Span<T>/Memory<T>, concurrent collections
│   ├── Streams/               # I/O and pipelines
│   │   ├── FileIO/            # FileStream, StreamReader/Writer, async file helpers
│   │   ├── Memory/            # MemoryStream, BinaryReader/Writer, ArrayPool, compression
│   │   └── Pipelines/         # System.IO.Pipelines — producer/consumer line parsing
│   └── Patterns/              # Design and architectural patterns
│       ├── Creational/        # Factory Method, Abstract Factory, Builder
│       ├── Behavioral/        # Strategy, Observer, EventBus
│       ├── Structural/        # Decorator, Repository
│       └── Functional/        # Result<T,E>, Specification
└── tests/
    ├── Fundamentals.Tests/
    ├── Concurrency.Tests/
    ├── Collections.Tests/
    ├── Streams.Tests/
    └── Patterns.Tests/
```

---

## Topics Covered

### Fundamentals
| File | Highlights |
|------|-----------|
| `ValueAndReferenceTypes.cs` | Stack vs heap, boxing/unboxing, struct design |
| `NullableTypes.cs` | Nullable reference types, `?.`, `??`, null patterns |
| `RecordsAndStructs.cs` | Positional records, `with` expressions, record inheritance |
| `SwitchExpressions.cs` | Switch expressions, relational, logical, tuple, list patterns |
| `TypePatterns.cs` | `is` patterns, negation, nested property patterns |
| `StringFeatures.cs` | Interpolation, raw string literals, `Span<char>` parsing, `String.Create` |
| `TuplesAndLocalFunctions.cs` | Named tuples, deconstruction, local functions, closures |
| `ExpressionBodiedAndLinqBasics.cs` | Expression-bodied members, LINQ method & query syntax |
| `GenericConstraints.cs` | `where` constraints, `INumber<T>`, in-memory repository |
| `Variance.cs` | Covariant (`out`) / contravariant (`in`) generic interfaces |

### Concurrency
| File | Highlights |
|------|-----------|
| `AsyncBasics.cs` | `async/await`, `Task.WhenAll/WhenAny`, retry with back-off, `ValueTask` |
| `AsyncStreams.cs` | `IAsyncEnumerable<T>`, `await foreach`, `EnumeratorCancellation` |
| `ParallelExamples.cs` | `Parallel.ForEach` with local state, PLINQ, `Parallel.ForEachAsync` |
| `CancellationExamples.cs` | `CancellationToken`, linked sources, timeout, registration callbacks |
| `SemaphoreAndLocks.cs` | `SemaphoreSlim`, `lock`, `Interlocked`, `ReaderWriterLockSlim` |
| `Channels.cs` | `Channel<T>`, bounded vs unbounded, producer/consumer pipeline |

### Collections
| File | Highlights |
|------|-----------|
| `ListAndDictionaryExamples.cs` | `List<T>`, `Dictionary<K,V>`, `CollectionsMarshal`, `ILookup` |
| `SetQueueStackExamples.cs` | `HashSet<T>` set operations, BFS/DFS traversal, `PriorityQueue` |
| `AdvancedLinq.cs` | `GroupBy`, `Join`, `Aggregate`, `Zip`, `Chunk`, `MaxBy`, left outer join |
| `CustomLinqExtensions.cs` | `Batch`, `Window`, `Scan`, `TakeUntil`, safe argument validation |
| `SpanAndMemoryExamples.cs` | `Span<T>`, `stackalloc`, `Memory<T>`, `MemoryMarshal` |
| `ConcurrentCollectionExamples.cs` | `ConcurrentDictionary`, `ConcurrentQueue`, `BlockingCollection` |

### Streams
| File | Highlights |
|------|-----------|
| `FileStreams.cs` | Async file read/write, streaming lines, binary I/O, `FileOptions.Asynchronous` |
| `MemoryStreamExamples.cs` | `BinaryReader/Writer`, `ArrayPool<byte>`, allocation-free UTF-8 encoding |
| `CompressionExamples.cs` | GZip and Brotli compress/decompress, stream chaining |
| `PipelineExamples.cs` | `System.IO.Pipelines`, `PipeWriter/Reader`, back-pressure, line parsing |

### Patterns
| File | Highlights |
|------|-----------|
| `FactoryPatterns.cs` | Factory Method, Abstract Factory, UI widget families |
| `BuilderPattern.cs` | Fluent builder, `init` properties, mandatory-field validation |
| `StrategyPattern.cs` | Interface-based and delegate-based strategies, composition |
| `ObserverPattern.cs` | `EventHandler<T>`, `IObservable<T>/IObserver<T>`, in-process event bus |
| `DecoratorPattern.cs` | Composable decorators (logging, retry) sharing an interface |
| `RepositoryPattern.cs` | `IRepository<T>`, in-memory test double, domain service |
| `ResultPattern.cs` | `Result<TValue, TError>`, `Map`, `Bind`, `Match`, railway-oriented programming |
| `SpecificationPattern.cs` | `Specification<T>`, `&&` / `\|\|` / `!` operators, combinable business rules |

---

## Getting Started

**Prerequisites:** [.NET 9 SDK](https://dotnet.microsoft.com/download)

```bash
# Clone and restore
git clone https://github.com/altay-brusan/csharp-primer.git
cd csharp-primer
dotnet restore

# Build
dotnet build

# Run all tests
dotnet test
```

---

## Key Conventions

- **Nullable reference types** are enabled (`<Nullable>enable</Nullable>`) across all projects.
- **Implicit usings** are enabled; common BCL namespaces are available without explicit `using`.
- Source code lives in `src/`; tests mirror the same structure under `tests/`.
- Each file is self-contained with XML documentation summarising the concept.

