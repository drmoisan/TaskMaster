# P4-T7 — Coverage delta and changed-line coverage

Timestamp: 2026-09-03T22-12

Command:
```text
env -C <worktree-root> git diff 87cb4df338322844abfa580abea14df77e738e5c -- UtilitiesCS/Threading/UiThread.cs
```

The Cobertura document `coverage/p4-t5.cobertura.xml` written by P4-T5 was then read to locate the
class node whose `filename` attribute ends in `Threading\UiThread.cs`, and that node's
`<line number=...>` elements were intersected with the added-line set derived from the diff.

EXIT_CODE: 0

## Redaction

`dotnet-coverage` writes the `filename` attribute as a full absolute host path containing a host
account name. Per this task's redaction rule, only the line-hit data is recorded below. The node is
identified by the repository-relative path `UtilitiesCS/Threading/UiThread.cs` and by its `name`
attribute `UtilitiesCS.UiThread`. No absolute path from the Cobertura file is reproduced in this
artifact.

## Output Summary

### Baseline coverage figures (from P0-T10)

- `lines-covered` = **105901**
- `lines-valid` = **149719**
- `line-rate` = **0.7073317347831605** — raw unstripped dotnet-coverage line-rate for the UtilitiesCS.Test process; not the repository first-party figure CLAUDE.md's 80% refers to

### Post-change coverage figures (from P4-T5)

- `lines-covered` = **105935**
- `lines-valid` = **149761**
- `line-rate` = **0.7073603942281368** — raw unstripped dotnet-coverage line-rate for the UtilitiesCS.Test process; not the repository first-party figure CLAUDE.md's 80% refers to

### Signed denominator difference

`post-change lines-valid` minus `baseline lines-valid` = 149761 - 149719 = **+42**

### Added-line set

The diff produced one hunk, `@@ -134,10 +134,19 @@`. Its `+` lines carry these new-file line numbers:

```text
137 138 139 140 141 142 143 144 145 146 149
```

Eleven added lines. As a cross-check, the hunk's new-file length is 19 and it carries 8 context lines
(134, 135, 136, 147, 148, 150, 151, 152), and 8 + 11 = 19.

### Intersection with the located class node

The class node `UtilitiesCS.UiThread` contributes these `<line number=... hits=...>` elements inside
the added-line range:

| line | hits |
|---|---|
| 138 | 1 |
| 139 | 1 |
| 140 | 1 |
| 141 | 1 |
| 142 | 1 |
| 143 | 1 |
| 145 | 1 |
| 146 | 1 |

Intersected set: **{138, 139, 140, 141, 142, 143, 145, 146}** — eight line numbers, every one with
`hits` of 1 or more.

Added lines 137, 144, and 149 carry no `<line>` element in the coverage report. They are the `get`
accessor header, the closing brace of the `if` block, and the field declaration
`private static Dispatcher? _dispatcher;`, none of which is an emitted sequence point. Lines with no
`<line>` element are not coverable and are not part of the intersected denominator. Line 147 does
carry a `<line>` element with `hits` of 1, but it is a context line
(`private set => _dispatcher = value;`), not an added line, so it is outside the intersection.

Two of the two other class nodes whose `filename` ends in `Threading\UiThread.cs` —
`UtilitiesCS.UiThread.SynchronizationContextAwaiter` and
`UtilitiesCS.UiThread.SynchronizationContextAwaiter.<>c` — contribute only lines 87 and 92-105, none
of which falls in the added-line set, so neither changes the intersection.

### Changed-line coverage

8 of 8 intersected lines have `hits` of 1 or more.

**Changed-line coverage = 100.0%**

Lines 141, 142, and 143 are the `throw new InvalidOperationException(` statement, its message
argument, and its closing `);`. Their `hits` of 1 records that the new regression test P1-T2 added
actually drives the throwing branch, so the guard's failure path is executed rather than merely
compiled.

## Acceptance

(a) **Satisfied.** The intersected set contains eight line numbers, which is at least two. The
coverage report resolved this file.

(b) **Satisfied.** Every intersected line has `hits` of 1 or more, giving 100% changed-line coverage,
which satisfies the `>= 90%` new-code target from CLAUDE.md.

(c) **Satisfied.** The signed `lines-valid` difference is **+42**, which is between 0 and 200
inclusive. No `COVERAGE DENOMINATOR MISMATCH` is recorded, so clause (d) is evaluated rather than
VOID. The +42 is consistent with the source this plan adds: approximately six coverable lines in
`UtilitiesCS/Threading/UiThread.cs` and approximately seventy-five in
`UtilitiesCS.Test/Threading/UiThread_Tests.cs`, of which only the emitted sequence points enter the
denominator.

(d) **Satisfied.** The post-change `line-rate` of 0.7073603942281368 is greater than or equal to the
baseline `line-rate` of 0.7073317347831605 minus 0.005, which is 0.7023317348. The observed change is
+0.0000286594, an increase, so the tolerance is not consumed at all.

## Observation, not a gate

The post-change `line-rate` of 0.7073603942281368 (70.74%) is below the `>= 80%` repository figure
from CLAUDE.md. That figure is explicitly non-comparable to this one: this is the raw, unstripped
`dotnet-coverage` line rate for the `UtilitiesCS.Test` process, whereas CLAUDE.md's 80% refers to the
repository's first-party testable denominator after third-party stripping.

PRE-EXISTING FLOOR SHORTFALL. The baseline figure recorded in P0-T10, 0.7073317347831605 (70.73%),
was also below that floor, so the shortfall predates this change and is not caused by it. This change
moves the figure upward by 0.0000286594.
