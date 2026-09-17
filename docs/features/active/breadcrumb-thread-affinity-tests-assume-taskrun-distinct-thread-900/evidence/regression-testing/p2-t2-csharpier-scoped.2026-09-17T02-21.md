# P2-T2 — Scoped CSharpier Format and Verification

Timestamp: 2026-09-17T02-21

Command (three, in order, plus the census):

1. `dotnet tool run csharpier format QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`
2. `dotnet tool run csharpier check QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`
3. `CMD-CENSUS` over the same file

SHA-256 of the file was captured immediately before command 1 and immediately after it.

EXIT_CODE: 0

The exit code recorded is that of the `check` command.

CHANNEL: COMMAND

## Output Summary

HASH-BEFORE: 8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164

HASH-AFTER: 8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164

REWRITTEN: False

The two hashes are identical, so the formatter did not rewrite the file: the hand-written
replacement blocks were already in CSharpier 1.2.6 canonical form. The console line the `format`
command printed was `Formatted 1 files in 1059ms.`, which is a processed-file count and not a
rewrite count; it reads identically whether or not the file changed, which is why the hash
comparison rather than that line is the observation recorded as `REWRITTEN:`.

`check` command final summary line, verbatim:

    Checked 1 files in 478ms.

CHECK-EXIT: 0

### Census after formatting

    TOKEN Task.Run( = 1
    TOKEN .GetAwaiter() = 1
    TOKEN Throw<InvalidOperationException>( = 0
    TOKEN error.Message.Contains( = 0
    TOKEN Contains( = 0
    TOKEN NotBeOfType<ObjectDisposedException>() = 2
    TOKEN ClearViewerDispatcher(scope.Viewer); = 1
    TOKEN [TestMethod] = 7
    TOKEN vacuously = 3
    TOKEN RunOnDedicatedWorkerThread( = 3
    TOKEN Exception captured = RunOnDedicatedWorkerThread( = 2
    TOKEN new Thread( = 1
    TOKEN IsBackground = true = 1
    TOKEN thread.Join(); = 1
    TOKEN Join( = 4
    TOKEN Join() = 4
    TOKEN UiDispatcher.CheckAccess() = 2
    TOKEN isOwnerThread = 4
    TOKEN BeOfType<InvalidOperationException>() = 2
    TOKEN captured.Message.Should().Contain( = 2
    TOKEN #900 = 3
    TOKEN action(); = 1
    TOKEN Thread.Sleep = 0
    TOKEN Task.Delay = 0
    TOKEN [Timeout = 0
    TOKEN DoNotParallelize = 0
    LINES = 490
    SHA256 = 8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164

## Acceptance

All three conditions hold.

- The `check` exits 0.
- Every `TOKEN` value still equals the "After P2-T1 and P2-T2" column. Formatting can re-wrap lines,
  but every counted token is single-line by construction, so the counts are wrap-tolerant. On this
  run the point is moot because the formatter changed nothing.
- `LINES` is 490, at most 500. `FILE SIZE LIMIT EXCEEDED` was not reached, with 10 lines of headroom
  against the repository's 500-line file limit.

The XML remarks were not shortened to gain headroom, and must not be: they carry the pinned `#900`
count of 3 and contribute two of the three `vacuously` occurrences, so editing them can move counts
that later gates assert.

`pipe-files` is not a gate and was not used.

## Anchors established

`HASH-AFTER:` is the value P2-T5 records as `FIX-HASH:` after committing, and every Phase 3 revert
check compares the file's SHA-256 against it. It is also the value P5-T7 compares against when
P5-T1 records `FORMAT_CHANGED_TREE: False`.

## Build lock

The two `csharpier` invocations ran inside a held shared build lock for item 900, released
immediately after the `check` completed. The census that follows reads a file and runs no build
tool, so it ran outside the lock.
