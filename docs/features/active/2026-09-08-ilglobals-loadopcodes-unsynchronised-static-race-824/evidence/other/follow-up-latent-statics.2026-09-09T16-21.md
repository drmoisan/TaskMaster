# Follow-up: two latent unsafe public static members (Issue #824, task P6-T13)

Timestamp: 2026-09-09T16-21

`spec.md` lists two members under "Out of scope / non-goals — latent items identified by the research
but not fixed here". This artifact records them so the epic orchestration layer can promote them into
a GitHub issue. Filing that issue is owned by the epic layer and is not a task in this plan.

**Neither member is remediated by #824.**

## Item 1 — `ILGlobals.Cache`

| Property | Value |
|---|---|
| File | `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` |
| Line, re-derived at this point in the run | **113** |
| Line recorded in `spec.md` | 112 |
| Declaration | `public static Dictionary<int, object> Cache = new Dictionary<int, object>();` |

An unsynchronised public mutable static. It is written nowhere after its field initializer, and a
repository-wide `Grep` for `ILGlobals\.Cache` over `*.cs` with no result limit returns exactly one
read site:

```
UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs:267:            ILGlobals.Cache.Should().NotBeNull();
```

No production code reads it. It is dormant rather than safe: it has the same shape as the two fields
this issue fixed, and it would become a live race the moment any caller began writing to it.

## Item 2 — `ILGlobals.modules`

| Property | Value |
|---|---|
| File | `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` |
| Line, re-derived at this point in the run | **131** |
| Line recorded in `spec.md` | 119 |
| Declaration | `public static Module[]? modules = null;` |

A repository-wide `Grep` for `ILGlobals\.modules` over `*.cs` with no result limit returns **zero**
matches, so the field has no reference anywhere outside its own declaration.

The `modules` identifiers in `MethodBodyReader.cs` are a local `Module[]` variable inside
`GetRefferencedOperand`, declared as `Module[] modules = Assembly.Load(...).GetModules();`, and are
unrelated to this field. That exclusion is recorded here because a naive unqualified search for
`modules` would report them and reach the wrong conclusion.

## Why the line numbers differ from spec.md

`spec.md` cites `:112` for `Cache` and `:119` for `modules`, both re-derived against the pre-change
tree while the spec was authored. The current numbers are 113 and 131. The shifts are caused by this
feature's own edits to the same file, which the plan requires be re-derived at this point in the run
rather than carried over:

- `Cache` moved down 1 line, because P2-T3 inserted the
  `using System.Runtime.CompilerServices;` directive above it.
- `modules` moved down 12 lines, because of that same directive plus the eleven lines of XML
  documentation P2-T1 added above the two opcode-table field declarations, which sit between `Cache`
  and `modules`.

Neither member's declaration text was modified by this feature. Only their positions moved.

## Recommended disposition

File a single follow-up issue covering both members as dead or unsafe public static surface, rather
than widening #824. This follows the Bugfix Workflow rule in `CLAUDE.md` to open a new issue instead
of expanding scope when a deeper design problem is uncovered. A future fix would plausibly delete
`modules` outright, since it is entirely unreferenced, and either delete `Cache` or give it the same
publication treatment the opcode tables received.
