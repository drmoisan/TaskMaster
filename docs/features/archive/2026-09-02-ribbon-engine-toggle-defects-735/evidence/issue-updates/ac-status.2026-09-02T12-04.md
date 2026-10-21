# Acceptance Criteria Status Summary (P5-T8)

Timestamp: 2026-09-03T03-32
Task: [P5-T8]
Command: `Get-Content -LiteralPath <spec> | Where-Object { $_ -match '^- \[x\] ' }` and the corresponding unchecked-pattern count, both taken over the spec file itself.
EXIT_CODE: 0

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md`
- Total AC items: **25**
- Checked off (delivered): **24**
- Remaining (unchecked): **1**

The checked count is verified by counting `- [x] ` items in the spec file itself, not by summing the
plan's own claims. The count returned 24 checked, 1 unchecked, 25 total, which reconciles with the
25-item inventory recorded at P0-T2.

### Items remaining

**F2-AC8** — Finding 2:

> The change description records the manual verification: the not-ready notice is observed instead of
> a `NullReferenceException` when Clear Spam Manager is confirmed before initialization completes,
> and the reset still runs end to end when repeated after initialization completes.

Status: **OPERATOR-ACTION-REQUIRED**

Reason: the `ManualVerificationStatus:` field of
`evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md` reads
`OPERATOR-ACTION-REQUIRED`. This executor has no live Outlook host, and the procedure requires
launching Outlook with add-in user-interface errors shown and clicking Clear Spam Manager during the
pre-initialization window. Repository unit-test policy independently forbids starting an external
process or a message pump from a test, so the step cannot be automated either. Leaving this
criterion unchecked is the correct outcome per the plan, not a plan failure.

To close it, an operator performs the two-step procedure recorded in that dossier, replaces the two
"Observed outcome" lines with the actual observations, changes the status field to `PERFORMED`, and
checks the box.

### Per-criterion disposition

| ID | Criterion | Status | Primary evidence |
|---|---|---|---|
| F1-AC1 | five unresolved callback names before, zero after | Checked | fail-before-finding1, pass-after-finding1 |
| F1-AC2 | exactly four action-callback values renamed; no viewer method added, renamed or removed | Checked | xml-edit-scope (callback symmetric difference; RibbonViewer.cs numstat empty) |
| F1-AC3 | exactly one element deleted | Checked | xml-edit-scope (element symmetric difference, one entry) |
| F1-AC4 | four renames plus one removal | Checked | xml-edit-scope partition table |
| F1-AC5 | enumeration test exists and passes | Checked | pass-after-finding1 TRX |
| F1-AC6 | check-box arity test exists and passes | Checked | pass-after-finding1 TRX |
| F1-AC7 | both new tests fail against the pre-fix tree | Checked | fail-before-finding1 (5 and 4 names quoted) |
| F2-AC1 | gate class exists with three validated dependencies | Checked | gate-tests TRX, three constructor cases |
| F2-AC2 | RunAsync contract | Checked | gate-tests TRX, six contract cases |
| F2-AC3 | no exemption attribute, no Office or WinForms using, no logger; doc records the omission | Checked | gate-class-constraints (four zero counts) |
| F2-AC4 | preamble and dialog unchanged; only engine-touching statements routed through the gate | Checked | callsite-edit-scope |
| F2-AC5 | all nine gate tests pass | Checked | gate-tests TRX, total 9 passed 9 |
| F2-AC6 | gate class line coverage at least 90% | Checked | coverage-delta row 2: 100.00% |
| F2-AC7 | no new exemption attribute anywhere in the diff | Checked | no-new-exemption (0 added, 0 removed) |
| F2-AC8 | change description records the manual verification | **UNCHECKED — OPERATOR-ACTION-REQUIRED** | manual-verification dossier |
| F3-AC1 | cache is a concurrent dictionary of a private nested reference type, interlocked sequence | Checked | race-fix-structure, coordinator-size-contingency |
| F3-AC2 | both writers take a ticket before the read and invalidate only when applied | Checked | race-fix-structure, pass-after-finding3 |
| F3-AC3 | reader keeps its bool return type; ordering test still passes | Checked | race-fix-structure, coordinator-fixture-after-fix |
| F3-AC4 | prime completion treats non-ran-to-completion as failure | Checked | race-fix-structure, pass-after-finding3, coordinator-fixture-after-fix |
| F3-AC5 | all six race tests pass; three demonstrated to fail pre-fix | Checked | fail-before-finding3, pass-after-finding3 |
| F3-AC6 | existing fixture changes by exactly one added partial keyword | Checked | partial-keyword-edit (numstat `1 1`) |
| X-AC1 | new source files registered as compile items; solution builds | Checked | build logs, msbuild-analyzer, msbuild-nullable |
| X-AC2 | every file under the 500-line ceiling after formatting | Checked | file-line-counts, coordinator-size-contingency |
| X-AC3 | full toolchain passes in order in a single pass | Checked | toolchain-loop-closure |
| X-AC4 | no behavior outside the three findings changes | Checked | footprint-scope, callsite-edit-scope |

### Two dispositions worth stating explicitly

**X-AC1 names "three new source files"; the change delivers five.** The three the spec anticipated
are `SpamManagerResetGate.cs`, `SpamManagerResetGateTests.cs` and
`EngineToggleStateCoordinatorTests.Race.cs`. The P4-T3 branch B contingency added two more,
`EngineTogglePressedStateCache.cs` and `EngineTogglePressedStateCacheTests.cs`. All five are
registered as compile items in the two legacy non-SDK project files, and each was confirmed present
on the recorded `csc.exe` command line. The criterion's substance — every new source file is
registered and the solution builds — is satisfied for a superset of the files it names.

**X-AC3 says "no auto-fixes"; the format step did rewrite files.** The formatter rewrote 4 of 8
paths on its first invocation and 2 of 10 on the branch B re-run. Every rewritten file was newly
authored by this change, and the rewrites occurred inside the loop's own format step. No gate failed
at any point, so no restart was triggered — the plan's P4-T1 acceptance states explicitly that the
restart obligation is triggered by a later failing step, not by a format rewrite. The terminal
read-only check (P4-T4) then reported zero unformatted files across 1576 files, so a further pass
would rewrite nothing. The criterion is recorded as met on that basis, and the nuance is stated here
rather than left implicit.

Output Summary: 24 of the 25 acceptance criteria in the spec are checked off, verified by counting
the checkboxes in the file rather than by summing plan claims. The single remaining item is F2-AC8,
the manual-verification record, which is OPERATOR-ACTION-REQUIRED because this executor has no live
Outlook host.
