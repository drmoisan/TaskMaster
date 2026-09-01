# P2-T1 — CSharpier Format, Changed File Only

Timestamp: 2026-09-01T14-22

Command: `dotnet tool run csharpier format QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`
(run from the checkout root, with `PATH` and `DOTNET_ROOT` pointed at the repository-local
`.dotnet-sdk` directory)

EXIT_CODE: 0

SHA256Before: c7f4ae79f251e1c2503d57237479fe8301f75fdb6b5697cb8de2a0a43cf7eee1
SHA256After: c7f4ae79f251e1c2503d57237479fe8301f75fdb6b5697cb8de2a0a43cf7eee1

Output Summary:

This artifact records **execution 2** of P2-T1, which is the execution the rest of the Phase 2 loop
proceeded from. The two hashes are recorded from that execution.

**The two hashes are equal.** CSharpier did not rewrite the file on this execution. The scope is
deliberately the single owned path rather than the whole tree, because a repository-wide write-mode
pass would rewrite files this issue does not own and break the AC-6 single-changed-path condition.

Tree observation before the run:

```
$ git status --porcelain -- QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
 M QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
```

Tree observation after the run:

```
$ git status --porcelain -- QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
 M QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
```

The porcelain status is identical on both sides because the file is modified relative to the index on
both sides; that is expected and is not itself evidence either way. The before-and-after SHA-256 pair
is the observation that distinguishes a repairing run from a clean one, and on this execution it shows
a clean run.

The command printed:

```
Formatted 1 files in 623ms.
```

That summary line reports the number of files CSharpier **processed**, not the number it rewrote, so
it is recorded rather than asserted over. CSharpier exits 0 whether or not it rewrote the file, which
is why the hash pair rather than the exit code is the discriminating observation. The identical
`Formatted 1 files` wording on both a repairing and a clean execution is the direct demonstration of
that property in this run pair.

## Execution 1, superseded

Execution 1 of P2-T1 recorded `SHA256Before: 3fa83d3eee142b3539d3311e86504354b76b88b072af25e4e92e327c0f20efeb`
and `SHA256After: c7f4ae79f251e1c2503d57237479fe8301f75fdb6b5697cb8de2a0a43cf7eee1`. Those hashes
were unequal, so that execution rewrote a tracked file and the Phase 2 loop rule required a restart
from P2-T1, which is what execution 2 above is. The rewrite was a line-ending normalisation: the file
had been authored with LF endings while the repository uses CRLF, and a read-only
`dotnet tool run csharpier check` on the same file before execution 1 had reported its only complaint
as `The file contained different line endings than formatting it would result in.` No content
reshaping was applied by either execution.

`LoopIterations` at the point this artifact was finalised is 2. P2-T11 records the final count.
