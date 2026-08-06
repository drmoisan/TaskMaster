# Reference-Resolution and Inventory-Delta Disposition

- Task: `[P2-T5]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Toolchain pass: **1**
- Timestamp: 2026-08-05T00-12
- Inventory under disposition: `evidence/qa-gates/analyzer-build.2026-08-05T05-00.md` (`[P2-T4]`)
- Basis: `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md` § 2

## Verdict

The inventory is **not** identical to the basis, so the literal line
`Inventory identical to basis; no delta to disposition` does **not** apply. The delta is one removal and
zero additions, dispositioned in full below.

| Direction | Count | Codes |
|---|---|---|
| **Added** | **0** | — |
| **Removed** | **1** | `CS2002` (`UtilitiesCS.Test`) |

## Added diagnostics: none

`[P2-T4]`'s five diagnostics each match a basis entry in code, count, text, and emitting project. There
is no added diagnostic of any code, so the added-diagnostic branch of `[P2-T5]`'s rule is not engaged and
**no loop restart is triggered on that account**.

### The `MSB3243` / `MSB3245` / `MSB3277` branch was not reached

`[P2-T5]` anticipated that adding an `ExCSS` reference might provoke a reference-resolution diagnostic and
prescribed how to handle one. Measured outcome:

```
Command: grep -cE 'MSB3243|MSB3245|MSB3277' <[P2-T4] build log>
Output:  0
```

**Zero occurrences, for `SVGControl.Test` or any other project.** Corroborated independently at
`[P1-T4]`, over the build that genuinely recompiled `SVGControl.Test`:
`evidence/other/excss-copy-local.2026-08-05T05-00.md` § 5 records the same count of **0**.

There is therefore **no accepted-with-evidence finding to escalate to the orchestrator** from this task,
because no reference-resolution diagnostic exists to escalate. The plan's escalation clause was
conditional on such a line being emitted, and none was.

Why no such diagnostic arose, so the absence is explained rather than merely reported: `MSB3243`,
`MSB3245`, and `MSB3277` report an assembly-identity mismatch, an unresolved reference, and a conflicting
version respectively. The added identity is
`ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a`, and the assembly actually
present at the `HintPath` was measured at `[P1-T4]` to carry exactly that identity, with
`FileVersion=4.3.2.0` and length 368128 matching the package asset byte-for-byte. Name, version, and
public key token agree exactly, and the file exists, so `ResolveAssemblyReference` had no mismatch,
no missing file, and no conflict to report. The identity was copied byte-for-byte from
`SVGControl/SVGControl.csproj:55` (verified byte-identical at `[P1-T1]`), which is the production project
that has referenced the same package version without any such diagnostic in the basis inventory.

**Basis-inventory cross-check, as `[P2-T5]` directs:** the basis records that `SVGControl` emitted
**zero** warnings and **zero** errors, with **zero** `MSB3277` and **zero** `MSB3245` solution-wide. So
`SVGControl` does **not** emit any of these codes for `ExCSS` in the basis either. There is no
pre-existing precedent line to compare against, and none was needed.

### None of the three forbidden responses was taken

Recorded for the audit trail, since none was required: **no `app.config` was edited** (verified at
`[P1-T7]`, `grep -ci 'app\.config$'` over the diff returning 0), **no `<NoWarn>` was added** (no `.csproj`
change beyond the five added lines enumerated at `[P1-T7]`), and **the reference was not removed** (it is
present and `ExCSS.dll` is in the output per `[P1-T4]`).

## Removed diagnostic: `CS2002` in `UtilitiesCS.Test` — expected, not a regression

| Field | Value |
|---|---|
| Code | `CS2002` |
| Severity | warning |
| Emitting project | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` |
| Verbatim text (from the basis) | `Source file 'C:\Users\DanMoisan\repos\TaskMaster\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` |
| Present in basis | yes (1 occurrence) |
| Present in `[P2-T4]` | **no** (`grep -c 'CS2002'` = 0) |
| Disposition | **Expected removal. Not a regression. No fix required. No loop restart.** |

### Reason, measured rather than assumed

`CS2002` is emitted by the C# compiler, so it appears only when a project's `CoreCompile` target actually
executes. Two measurements establish that `UtilitiesCS.Test` did not compile in the `[P2-T4]` run:

```
[P2-T4] log:  grep -c 'csc.exe'                          -> 0
[P2-T4] log:  grep -c 'CoreCompile:'                     -> 18
[P2-T4] log:  grep -c 'Skipping target "CoreCompile"'    -> 18   (all 18 skipped)
```

Zero `csc.exe` invocations solution-wide and all 18 `CoreCompile` targets skipped: **no project
recompiled**, `UtilitiesCS.Test` included. A `CoreCompile`-gated diagnostic therefore could not be
emitted.

By contrast the basis run executed **34** `csc.exe` invocations, which is why it observed `CS2002`.

This is exactly the case `[P2-T5]` names in advance: "The basis `CS2002` row in `UtilitiesCS.Test` is the
known instance — this cycle's only changed inputs are under `SVGControl.Test`, so `UtilitiesCS.Test` may
not recompile and the code may not be emitted. A `CoreCompile`-gated diagnostic that disappears because
its emitting project did not recompile is **not** a regression, requires no fix, and triggers **no** loop
restart."

The precondition of that rule is satisfied on both limbs:

1. **The diagnostic is `CoreCompile`-gated.** `CS2002` is a `csc` diagnostic about duplicate `<Compile>`
   items. The basis artifact itself records the same gating behavior, having observed 5 warnings in an
   incrementally vacuous run versus 6 in its 34-`csc.exe` run, the difference being precisely this
   `CS2002`.
2. **The emitting project did not recompile.** Measured above: 0 `csc.exe`, 18 of 18 `CoreCompile`
   targets skipped.

### The removal is not attributable to this cycle's change

Two independent grounds:

1. **`UtilitiesCS.Test` is untouched.** `[P1-T7]` measured the complete diff: exactly two tracked files
   carry functional change, both under `SVGControl.Test`, with **0** `.cs` paths anywhere in the diff.
   Neither `UtilitiesCS.Test/UtilitiesCS.Test.csproj` nor
   `UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs` was modified.
2. **The underlying condition still exists on disk.** The duplicate `<Compile>` item that causes `CS2002`
   is unchanged in `UtilitiesCS.Test.csproj`; it is latent, not removed. The diagnostic will reappear the
   next time that project recompiles for any reason. Nothing was suppressed, `<NoWarn>`-ed, or fixed —
   which is correct, since `UtilitiesCS.Test` is outside this cycle's Scope Lock and repairing it would
   be scope widening prohibited by the binding `## Do Not Do` list.

### No removal requiring an on-the-merits explanation

`[P2-T5]` requires that "a removal in a project that *did* recompile, or a removal of a diagnostic that is
not `CoreCompile`-gated, must be explained on its merits before the pass is accepted." Neither condition
obtains here: no project recompiled at all, and the single removed diagnostic is `CoreCompile`-gated.
There is no such removal in this delta.

## Loop-restart determination

| Trigger | Present | Action |
|---|---|---|
| A newly introduced diagnostic that is not `MSB3243`/`MSB3245`/`MSB3277` naming `ExCSS` | **no** — zero added diagnostics of any code | no restart |
| An added diagnostic emitted by a project other than `SVGControl.Test` | **no** — zero added diagnostics | no restart |
| An added `MSB3243`/`MSB3245`/`MSB3277` naming `ExCSS` from `SVGControl.Test` | **no** — zero such lines | nothing to escalate |
| A removal in a project that recompiled | **no** — no project recompiled | no restart |
| A removal of a non-`CoreCompile`-gated diagnostic | **no** — the sole removal is `CoreCompile`-gated | no restart |

**No loop restart is triggered by this task.** Stage 2 of toolchain pass 1 is accepted and the loop
proceeds to `[P2-T6]`.

## Output Summary

The `[P2-T4]` inventory is not identical to the basis, so every delta element is dispositioned here.
**Added diagnostics: zero** — in particular **zero** `MSB3243`/`MSB3245`/`MSB3277` lines for
`SVGControl.Test` or any project, corroborated independently at `[P1-T4]`, so the plan's
accepted-with-evidence escalation clause was never triggered and there is nothing to escalate; the
absence is explained by exact name/version/public-key-token agreement between the added identity and the
deployed assembly, and the basis confirms `SVGControl` emits none of these codes for `ExCSS` either.
**Removed diagnostics: one** — `CS2002` in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, verbatim text
recorded above, dispositioned as an **expected, non-regressive** removal because it is `CoreCompile`-gated
and the `[P2-T4]` run executed 0 `csc.exe` invocations with all 18 `CoreCompile` targets skipped, so its
emitting project did not recompile; the underlying duplicate `<Compile>` item is untouched and latent, and
`UtilitiesCS.Test` appears nowhere in this cycle's diff. Totals reconcile: 6 − 1 + 0 = 5. **No fix was
required and no loop restart was triggered.** No `app.config` was edited, no `<NoWarn>` was added, and the
reference was not removed.
