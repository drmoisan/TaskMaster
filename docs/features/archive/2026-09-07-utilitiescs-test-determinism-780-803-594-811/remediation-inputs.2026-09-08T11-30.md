# Remediation Inputs — utilitiescs-test-determinism (Issue #811)

- Date: 2026-09-08T11-30
- Branch: `bug/utilitiescs-test-determinism-780-803-594-811`
- Head: `3805ca89f0c72e2bb448726695f18f420bd260f7`
- Base: `bb1c7d4b60f7b782227956f36859314d5c47bb03`

## Source Artifacts

- `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/policy-audit.2026-09-08T11-30.md`
- `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/code-review.2026-09-08T11-30.md`
- `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/feature-audit.2026-09-08T11-30.md`

## Blocking Findings — Remediation Required

Count: **1**.

### R-1 (from policy-audit F-1) — File and promote the `ILGlobals` unsynchronised-static race

**Why it is blocking.** This defect is the sole reason acceptance criterion AC4 is unmet. It
currently exists only as prose inside `<FEATURE>/evidence/`, which is archived at merge. Two other
follow-ups from this item were filed as `docs/features/potential/` entries by plan task P8-T9, so
the write set demonstrably permitted that directory; the stated reason for omitting the third
("authoring it was not in this plan's write set",
`evidence/issue-updates/issue-811.2026-09-08T10-44.md` lines 77-78) is contradicted by the plan's own
conduct. A search of `docs/features/potential/` for `ILGlobals`, `SDIL` and `MethodBodyReader`
returns zero matches.

**Required action.** Author
`docs/features/potential/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md` and promote
it to a GitHub issue through the standard promotion lifecycle. No source change is required on this
branch.

**Content the entry must carry.**

- Mechanism: `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:123` reassigns
  `singleByteOpCodes` to a freshly allocated all-default `OpCode[0x100]` and then fills it in a
  reflection loop at line 137. Lines 117-118 declare both tables as plain mutable `public static`
  fields. There is no `lock`, no `Lazy<T>`, no `volatile`, and no static constructor.
- Reader: `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs:105` and `:110` read the
  tables with no synchronisation.
- Participants: `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` calls
  `LoadOpCodes()` at lines 15, 26, 37, 47; `MethodBodyReader_Tests.cs` calls it at line 364 inside
  the private `CreateReader` helper that every test in that class routes through. Neither class
  carries `[DoNotParallelize]`, and the assembly runs at
  `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`
  (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`). `ClassLevel` scope serialises tests within
  a class, so the second participant must be the sibling class.
- Observed signature (AC4 run 7): `GetBodyCode_ReturnsConcatenatedInstructions` failed with
  `Expected bodyCode ... to contain "ldstr"`. Only the entry at IL offset 0001 (`ldstr`, opcode
  `0x72`) degraded; `nop`, `stloc.0`, `br.s`, `ldloc.0` and `ret` all resolved. The two spaces where
  the opcode name belongs are the empty `Name` of `default(OpCode)`, whose `OperandType` is
  `InlineBrTarget` (0), which drives the numeric-operand branch and stores a raw metadata token
  instead of calling `module.ResolveString(...)`. The printed value 1879067923 lies in the
  `0x70……` String-metadata-token range, so the token was read correctly and merely not resolved.
  That combination is explained only by a partially filled table read mid-initialisation.
- Frequency: observed once in ten consecutive full-suite runs at 24 class-level workers. The base
  rate is not established; two clean baseline runs do not establish absence.
- Candidate fixes, in order of preference:
  1. Publish both tables once and immutably — a static constructor or a `Lazy<OpCode[]>` pair — and
     make `LoadOpCodes()` a no-op or remove it. `LoadOpCodes()` is idempotent and the tables are
     conceptually constant, so this removes the mutable window entirely.
  2. Take a `lock` inside `LoadOpCodes()` and around the reads in `MethodBodyReader`.
  3. Interim stopgap only: add `[DoNotParallelize]` to both `ILGlobals_Tests` and
     `MethodBodyReader_Tests`. This suppresses rather than eliminates and should not be the final
     state, by the same reasoning `spec.md` applies to the console stopgap under AC3.
- Impact statement for the issue: while this is open, the required `mstest-coverage` check can still
  fail intermittently on unrelated pull requests, which is the class of harm issue #811 exists to
  remove.

**Acceptance for R-1.**

- `docs/features/potential/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md` exists on
  the branch and carries the mechanism, the two participants with file and line citations, the
  observed signature, and the candidate fixes.
- A GitHub issue is created from it and its number is recorded in the entry.
- No source file under `UtilitiesCS/` or `UtilitiesCS.Test/` is modified for this item on this
  branch. Fixing the race here would breach the minimal-fix requirement of the CLAUDE.md Bugfix
  Workflow and would need its own RED-first plan.

## Non-blocking Findings — No Remediation Demanded on This Branch

These are recorded so the orchestrator can route them, not because they gate merge.

| ID | Source | Summary | Suggested disposition |
|---|---|---|---|
| F-2 | policy-audit | Repository branch coverage 66.3978% is below the 75% floor in `.claude/rules/quality-tiers.md`. Pre-existing (baseline 66.3058%), improved by this change. | Repository-wide programme; do not gate a three-defect determinism bugfix on it. Also note the unreconciled CLAUDE.md 80/no-branch versus rules-file 85/75 conflict. |
| F-3 / C-1 | policy-audit, code-review | `EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart` performs two Acts, justified in-code by the 1846-line file's line ceiling. | Follow-up: extract into a small separate test file, which also reduces the over-cap file. |
| F-4 / C-5 | policy-audit, code-review | `DfDeedle.MessageBoxInvoker` remains a mutated process-wide static. Verified to have no concurrent reader today, but the invariant is unenforced and rests on the MSTest phase-ordering assumption `spec.md` Risk 3 flags as unverified. | Add the enforcement caveat to the `DfDeedle_COM_Tests` class header; consider parameterising the seam in separate work. |
| F-5 | policy-audit | `artifacts/csharp/coverage.xml` and the two `artifacts/pr_context.*` files are absent. Coverage substance was verified directly from `coverage/p7-final.cobertura.xml`; the diff was supplied as a pre-generated patch and full diffstat. | Procedural. Regenerate the canonical artifacts in the next cycle if a hook requires them. |
| F-6 | policy-audit, feature-audit | AC4 literally unmet, 9 of 10 runs clean, correctly left unchecked and correctly not re-run to green. | Accept as reported. Re-evaluate AC4 after R-1's race is fixed under its own issue. |
| C-2 | code-review | `Main_RunsSampleScenarioWithoutThrowing` name is stronger than its surviving `NotThrow` assertion. | Optional rename. |
| C-3 | code-review | Two null-writer tests write to the real `Console.Out` under an assembly-wide assumption that nothing captures it. | Add a class-header note recording the dependency. |
| C-6 | code-review | Named and positional tuple access mixed within four lines in `DfDeedle.cs`. | Cosmetic; unify on the named form in a future edit. |

## Routing

R-1 is a documentation and promotion action with no source change and no toolchain re-run. It does
not require an atomic plan. It requires one new file under `docs/features/potential/` and one issue
creation, followed by a re-check that the file exists and carries the required content.

After R-1 is closed, this branch is clear to merge on the evidence recorded in the three audit
artifacts dated 2026-09-08T11-30.
