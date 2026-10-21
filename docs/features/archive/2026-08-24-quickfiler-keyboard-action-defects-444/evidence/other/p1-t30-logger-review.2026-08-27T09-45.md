# [P1-T30] Code review — `logger.Error` immediately precedes the constructor throw

Timestamp: 2026-08-27T09-45
File reviewed: `QuickFiler/Controllers/KbdActions.cs` (post-format, as committed by `[P1-T21]`)
EXIT_CODE: 0

## The new constructor guard block (verbatim)

```csharp
                    if (
                        _list[i].SourceId == _list[j].SourceId
                        && StoredKeyEquals(_list[i].Key, _list[j].Key)
                    )
                    {
                        string message =
                            $"Cannot add key because it already exists. Key {_list[j].Key} SourceId {_list[j].SourceId}";
                        logger.Error(message);
                        throw new ArgumentException(message, nameof(list));
                    }
```

`logger.Error(message);` is the statement **immediately preceding** the `throw`, with no intervening
statement.

## The two existing `Add` guard blocks it mirrors (verbatim)

`Add(string sourceId, TKey key, VDelegate @delegate)`:

```csharp
            if (_list.Any(x => x.SourceId == sourceId && StoredKeyEquals(x.Key, key)))
            {
                string message =
                    $"Cannot add key because it already exists. Key {key} SourceId {sourceId}";
                logger.Error(message);
                throw new ArgumentException(message);
            }
```

`Add(UClass instance)`:

```csharp
            if (
                _list.Any(x =>
                    x.SourceId == instance.SourceId && StoredKeyEquals(x.Key, instance.Key)
                )
            )
            {
                string message =
                    $"Cannot add key because it already exists. Key {instance.Key} SourceId {instance.SourceId}";

                logger.Error(message);
                throw new ArgumentException(message, nameof(instance));
            }
```

## Conformance findings

| Aspect | `Add(string, TKey, VDelegate)` | `Add(UClass)` | New constructor guard | Verdict |
| --- | --- | --- | --- | --- |
| Message prefix | `Cannot add key because it already exists.` | same | same | MATCH |
| Contains the literal fragment `already exists` | yes | yes | yes | MATCH |
| Message names the key and the source id | yes | yes | yes | MATCH |
| Logger call | `logger.Error(message)` | `logger.Error(message)` | `logger.Error(message)` | MATCH |
| Logger call position | statement immediately before the throw | statement immediately before the throw | statement immediately before the throw | MATCH |
| Exception type | `ArgumentException` | `ArgumentException` | `ArgumentException` | MATCH |
| Parameter name argument | none | `nameof(instance)` | `nameof(list)` | MATCH in form; the name correctly identifies this member's own parameter |
| Comparison used | `StoredKeyEquals` | `StoredKeyEquals` | `StoredKeyEquals` | MATCH |

The single `logger` instance is the pre-existing `private static readonly log4net.ILog logger` declared
at the top of the class. No new logger, sink, level, or configuration is introduced; the change adds
exactly one new `logger.Error` call site.

Output Summary: `logger.Error(message)` is the statement immediately preceding the `throw` in the new
enumerable-constructor guard, and the block matches both existing `Add` guard blocks on message shape,
logger call, position, exception type, and comparison function.
