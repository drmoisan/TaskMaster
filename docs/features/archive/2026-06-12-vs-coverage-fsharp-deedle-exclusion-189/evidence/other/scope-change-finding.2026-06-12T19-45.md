# Scope-Change Finding — Plan Assumption Invalidated (Halt at P2-T2)

Timestamp: 2026-06-12T19-45

Status: HALTED — scope-change finding reported to caller; awaiting plan revision.

## What was completed before the halt

- Phase 0 (P0-T1..T5): complete, evidence on disk.
- Phase 1 (P1-T1, P1-T2): complete — `TaskMaster.runsettings` edited with the seven-entry `<DataCollectionRunSettings>` Exclude block, well-formed XML, `<MSTest>` block preserved, no `enabled="true"`.
- Phase 2 P2-T1 (WITH-`/collect`): complete — 42/42 Deedle tests pass, no VerificationException, exit 0.

## The finding (at P2-T2, NO-`/collect`)

The plan and AC3/AC5 assume that adding a `<DataCollector friendlyName="Code Coverage">` block WITHOUT `enabled="true"` keeps coverage opt-in (no coverage on a normal `Run Tests` run). Empirically, in this environment (VSTest 18.7.0, VS 18 Community), that assumption is false:

- Edited runsettings, NO `/collect`: a `.coverage` attachment IS produced -> AC3/AC5 violated.
- Baseline runsettings (no collector block), NO `/collect`: NO attachment -> confirms the block is the cause.
- Edited runsettings + `enabled="false"`, NO `/collect`: NO attachment (AC3/AC5 would pass).
- Edited runsettings + `enabled="false"`, WITH `/collect`: collector throws in `DynamicCoverageDataCollector.Initialize` -> run exits 1 -> AC4 violated.

A declared `<DataCollector>` defaults to enabled when the `enabled` attribute is absent. There is no single-attribute setting on this one block that satisfies both AC4 and AC3/AC5 via the CLI `/collect` path.

## Why this is a scope-change finding, not an improvisation

The executor directive states: "If you hit any NEW finding outside the plan (e.g., the WITH-exclusion run does NOT clear the VerificationException, or the collector forces coverage on a normal run), STOP and report it as a scope-change finding rather than improvising." The observed behavior is the second listed trigger. The fix that would reconcile the AC (for example, gating the collector so VS coverage uses it while CLI/normal runs do not, or accepting that the VS IDE coverage path enables the collector by design while the CLI proxy cannot represent AC3/AC5 simultaneously) is a new independent design decision not described by the plan's single mechanical edit. Resolving it requires a plan revision from atomic-planner.

## Important nuance for the planner

The AC are written against the Visual Studio "Analyze Code Coverage" path, where the user explicitly invokes coverage — so within VS the collector being active is the intended path, and the exclusion block is exactly what is needed. The AC3/AC5 "no coverage on a normal run / opt-in" requirement was being validated through the CLI `/collect` vs no-`/collect` proxy. In VS, "Run Tests" (no coverage) vs "Analyze Code Coverage" (coverage) are different commands; the runsettings `<DataCollector>` being present does not force the non-coverage "Run Tests" command to collect coverage in the IDE. The CLI proxy diverges here: declaring the collector in runsettings makes the CLI activate it even without `/collect`.

Open questions for the planner to resolve:
1. Is AC3/AC5 intended to be validated only via the VS IDE "Run Tests" vs "Analyze Code Coverage" distinction (where the present edit is correct), with the CLI no-`/collect` attachment treated as a known CLI-only artifact and not a failure?
2. Or must the runsettings edit be changed so the CLI no-`/collect` run also produces no attachment — in which case the `/collect` path must be re-validated because `enabled="false"` breaks it?

Recommendation: revise P2-T2 acceptance to reflect the VS IDE command distinction (Run Tests vs Analyze Code Coverage) for AC3/AC5, OR add a task that determines a runsettings configuration satisfying both the CLI `/collect` pass and CLI no-`/collect` no-attachment, before continuing.

## Current state of TaskMaster.runsettings

The edited file (additive seven-entry Exclude block, no `enabled` attribute) remains in the working tree as produced by P1-T1. No out-of-scope file was modified. The two out-of-repo temp files used for diagnosis were created under `$TEMP` and deleted immediately after use (not in the repo, not test artifacts).
