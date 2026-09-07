# Issue #483 — `MoveMailAsync` Return Type Unchanged, Sole Caller Unmodified

Timestamp: 2026-08-26T09-38
Task: [P3-T15]

## Fact 1 — the declaration line, showing the return type `Task`

Command: `grep -n "public async Task MoveMailAsync()" QuickFiler/Controllers/QfcItemController.MailActions.cs`
EXIT_CODE: 0

```
105:        public async Task MoveMailAsync()
```

The return type is still `Task`. It was not changed to a result object: `Task MoveMailAsync()` is declared
on the public interface `QuickFiler/Interfaces/IQfcItemController.cs:78` and implemented by the
out-of-scope `QuickFiler/Controllers/EfcItemController.cs`, so changing it would require writing two
files this feature does not own. Rethrow is the only in-scope way to let the caller distinguish a failed
move from a successful one.

## Fact 2 — the sole production caller's file is unmodified

Command: `git diff --name-only 61edc19befcf6c4e95b5acd32542f2dcdab41b78 -- QuickFiler/Controllers/QfcCollectionController.cs`
EXIT_CODE: 0

```
(no output)
```

The command produced no output lines, establishing that
`QuickFiler/Controllers/QfcCollectionController.cs` is byte-identical to its state at `BASE_SHA`
`61edc19befcf6c4e95b5acd32542f2dcdab41b78`.

## Why no caller change is needed

The sole production caller is
`QfcCollectionController.TryMoveEmailByGroupAsync` (`QfcCollectionController.cs:2236-2258`). It already
wraps the call in `try`/`catch (System.Exception)`, logs with subject context, and returns, so the bulk
loop `MoveEmailsAsync` (`:2206-2228`) cannot be aborted by the new rethrow. What changes for it is that a
failed file now reaches its catch and is logged, instead of being reported as a success.

Output Summary: `MoveMailAsync` still returns `Task`, and
`QuickFiler/Controllers/QfcCollectionController.cs` is unmodified relative to `BASE_SHA`.
