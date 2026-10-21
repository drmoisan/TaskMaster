# AC8 verification — no [DoNotParallelize], assembly parallelism preserved (Issue #824, task P4-T5)

Timestamp: 2026-09-09T15-51

Command: two `Grep` invocations for `DoNotParallelize`, then `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $b = git merge-base HEAD origin/main; git add -N .; git status --porcelain --untracked-files=all; Write-Output "---"; git diff --stat $b -- "UtilitiesCS.Test/Properties/AssemblyInfo.cs"'`, issued with a `HEAD`-anchored companion span per the adaptation recorded in `evidence/other/executor-deviations.2026-09-09T15-28.md`, and with the porcelain listing filtered to entries naming the gated path.

EXIT_CODE: 0

## Output Summary

`DoNotParallelize` match counts:

| File | Count | Required |
|---|---|---|
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 0 | zero |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` | 0 | zero |

Neither class gained the attribute. This matches their state at baseline, so the criterion is a
recommendation not to add one rather than a removal.

Observations for `UtilitiesCS.Test/Properties/AssemblyInfo.cs`:

| Observation | Result |
|---|---|
| `git diff --stat <merge-base> -- <path>` | **empty** |
| `git diff --stat HEAD -- <path>` | **empty** |
| `git status --porcelain --untracked-files=all`, entries naming that path | **none** |
| `Grep` with `-n` for `Parallelize` | `18:[assembly: Parallelize(` |

Both anchors agree and both are empty, so this gate's acceptance holds exactly as the plan writes
it. The P0-T15 measurement established that this path does not appear in the inherited listing, so
the merge-base form was never at risk here and the `HEAD`-anchored span is a redundant second
observation.

The assembly-level attribute is still reported at line 18, unchanged. `Workers = 0` and
`Scope = ExecutionScope.ClassLevel` are not weakened, narrowed, or removed.

## Why no stabilisation attribute was needed

After AC1 the two opcode tables are assigned exactly once by the type initializer, under the CLR's
once-only-with-blocking guarantee, so no shared mutable state remains between `ILGlobals_Tests` and
`MethodBodyReader_Tests`. Adding `[DoNotParallelize]` would have suppressed the observable symptom
without eliminating the defect, and would have created an undocumented attribute of exactly the kind
a sibling feature of this epic exists to remove. `ILGlobals.Cache` remains a public mutable static,
but it is written nowhere after its field initializer and read at exactly one place in the
repository, so it is not a live race between these two classes; it is recorded as a follow-up item
by P6-T13 rather than remediated here.
