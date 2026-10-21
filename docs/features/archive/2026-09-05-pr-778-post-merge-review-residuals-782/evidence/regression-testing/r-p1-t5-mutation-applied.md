# [P1-T5] Temporary falsification mutation applied

Timestamp: 2026-09-06T01-38

Command:

```powershell
Select-String -SimpleMatch 'before yielding folder tree work' -Path 'UtilitiesCS\OutlookObjects\Folder\WpfDispatcherYield.cs'
git status --porcelain -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
```

EXIT_CODE: 0

Output Summary: the mutated line is present exactly once, and exactly one worktree path is modified.

```text
TAIL_MATCHES=1
PORCELAIN_LINES=1
```

## The mutation

`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, line 65.

Before:

```csharp
                throw new InvalidOperationException(UiThread.DispatcherNotInitializedMessage);
```

After:

```csharp
                throw new InvalidOperationException(UiThread.DispatcherNotInitializedMessage + " before yielding folder tree work");
```

The appended text restores, at this one throw site, the caller-specific tail the delivery removed
under SD5. It is appended to the shared constant rather than replacing it, so the mutated message
still contains every character of the constant plus the tail.

## Porcelain state

```text
 M UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
```

Exactly one line, naming only the mutated file. No other path is modified by this task.

## This mutation is temporary

It exists only to make the falsification observation in [P1-T7] possible, and it is reverted by
[P1-T8] with `git checkout --` on this one path. [P1-T8] then re-verifies the revert three ways —
porcelain status, an anchored name-listing diff, and a zero-hit search for the appended literal — and
re-runs the analyzer build. No production file is changed by the delivered result of this
remediation.

**No CSharpier run occurs while the mutation is in place.** The mutated line measures beyond
CSharpier's 100-column print width, so a formatter run would rewrite it and the revert would then be
a revert of formatter output rather than of the mutation alone. The Phase 4 toolchain loop, which
begins with `csharpier format .`, does not start until [P1-T8] has verified the revert.
