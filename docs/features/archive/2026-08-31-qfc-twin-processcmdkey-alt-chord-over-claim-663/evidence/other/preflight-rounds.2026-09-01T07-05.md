# Preflight clearance record — issue #663

Timestamp: 2026-09-01T07-05
Command: DIRECTIVE: PREFLIGHT VALIDATION ONLY (atomic-executor), repeated until clearance
EXIT_CODE: 0
Output Summary: Six preflight rounds ran against the canonical atomic plan. Defect counts fell
12 -> 5 -> 11 -> 10 -> 3 -> 1 -> 0. Round 7 returned `PREFLIGHT: ALL CLEAR` with
`CONVERGENCE: NO FURTHER ROUNDS EXPECTED`. The MCP plan validator returns `ok:true` on the cleared
plan with no warnings.

## Why this record exists

The interrupted preparation run left no `PREFLIGHT:` signal anywhere in this feature folder, which
made an uncleared plan indistinguishable from a cleared one. This artifact is the durable clearance
evidence. Rounds 1 and 2 predate this artifact and are reconstructed from the commit messages of the
two delta commits that applied them, which are cited per round below.

## Scope of this record

- Canonical plan: `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/plan.2026-08-31T20-16.md`
- Acceptance-criteria source (work mode `full-bug`): `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md`
- Every revision was applied IN PLACE at the canonical plan path. No timestamped sibling plan file
  was created at any point, per the Plan-Path Continuity Contract.
- Branch: `bug/qfc-twin-processcmdkey-alt-chord-over-claim-663`. `origin/main` (`2b85134b`) is an
  ancestor of HEAD throughout, so every citation was re-derived against a tree containing all of
  current `main`.

## Round ledger

| Round | Signal | Defects | Blocking | Delta applied |
|---|---|---|---|---|
| 1 | `PREFLIGHT: REVISIONS REQUIRED` | 12 | not recorded | commit `68d0c76b` |
| 2 | `PREFLIGHT: REVISIONS REQUIRED` | 5 (D13-D17) | not recorded | commit `39d0a15c` |
| 3 | `PREFLIGHT: REVISIONS REQUIRED` | 11 (B1-B4, W1-W3, M1-M4) | 4 | this commit |
| 4 | `PREFLIGHT: REVISIONS REQUIRED` | 10 (B1-B2, W3-W4, M1-M6) | 2 | this commit |
| 5 | `PREFLIGHT: REVISIONS REQUIRED` | 3 (F1-F3) | 0 | this commit |
| 6 | `PREFLIGHT: REVISIONS REQUIRED` | 1 (D1) | 0 | this commit |
| 7 | `PREFLIGHT: ALL CLEAR` | 0 | 0 | n/a |

Plan size across the resume: 837 lines at round 3 entry, 896 after round 3, 998 after round 4,
1006 after round 5, 1008 at clearance. `spec.md`: 646 lines at round 3 entry, 653 at clearance.

### Round 1 — 12 defects (interrupted run, commit `68d0c76b`)

The two most consequential propagated into `spec.md` as well as the plan:

- A gate reading "no console line containing `error`" cannot pass. A successful MSBuild run prints the
  `/errorreport:prompt` token on every Csc command line and prints its own `0 Error(s)` summary.
  Regated on the MSBuild diagnostic form `: error [A-Z]+[0-9]+:`.
- Two of the seven new test method names already exist in `QuickFiler.Test`, on the Email Filer
  fixture at `EfcViewerTests.cs` lines 134 and 156. A bare method name is therefore not a unique
  identifier in a test run's output, so every named-test reading now carries the declaring type.

Also corrected: an unscoped diff already matched the coverage-exclusion attribute twenty times before
any source edit; the instrumented coverage run was gated against an uninstrumented baseline failure
set; the terminal clean-tree gate asserted over its own outputs; the formatter's distinguishing
observation did not distinguish; the `Keys.Control` row of the behavior table had no test, so AC-1 was
unestablishable; and the restore step had no fallback for the known MSBuild-restore shortfall.

### Round 2 — 5 defects D13-D17 (interrupted run, commit `39d0a15c`)

Two of the five were incomplete applications of round 1's deltas rather than new planner defects:
D15, where a task edit landed but the reading-guide paragraph restating the same rule was missed; and
D14, where a plan-side diff scoping fix was not mirrored into the matching `spec.md` verification.

D13 was the most serious genuine defect: both analyzer-concordance searches in `[P0-T5]` failed to
execute, because a doubled backslash de-doubles when handed to the native `pwsh` executable and an
escaped double quote is not an escape sequence inside a PowerShell double-quoted string. Both now
spell those characters as the regex hex escapes `\x5C` and `\x22`, which are inert to every
intervening quoting layer.

### Round 3 — 11 defects, 4 blocking

Blocking:

- **B1** `[P0-T3]`: the `dotnet --list-sdks` acceptance clause is unsatisfiable. That command
  enumerates the host muxer's own root and does not consult `global.json`, so it prints the host line
  whether or not the repo-local SDK was installed. Regated on `dotnet --version` plus a
  `Test-Path .dotnet-sdk/sdk/8.0.205` marker check.
- **B2** `[P4-T3]`: an absolute-zero analyzer-warning clause contradicted the baseline-relative clause
  `[P1-T3]` applies to the same command, and is unsatisfiable if any such warning already exists on
  `origin/main`. Made baseline-relative.
- **B3** `[P6-T19]`: the terminal clean-tree sequence had no fixpoint, because recording the
  post-amend observation into `end-state.md` re-dirties the file the amend just committed.
- **B4** `[P4-T2]`: the acceptance is internally contradictory when the CSharpier baseline is
  non-zero, and `[P0-T8]` recorded that baseline without a failure branch.

Warning: **W1** a vacuous warning clause on the `/p:TreatWarningsAsErrors=true` gate; **W2** the
artifact AC-11 names as its evidence is deleted by `[P4-T7]`; **W3** `[P0-T13]` had no branch if the
target `class` element is absent from the post-processed document.

Minor: **M1** over-strict CSharpier version equality; **M2** a latent trap where widening the test
file's class-level XML summary fails `[P5-T3]` for an unrelated reason; **M3** a line-range
imprecision in `[P0-T11]`; **M4** a `[P3-T3]` conclusion not carried by an acceptance clause.

The planner corrected two of the orchestrator-supplied citations while applying this delta:
`_build-analyzers.yml` "line 52" became "lines 50 through 52" because the command is a
backtick-continued block, and a composite `Helpers.ps1` range was split into three precise citations.

### Round 4 — 10 defects, 2 blocking

Blocking:

- **B1** `[P0-T13]` required recording that the coverage document contains no `method` element named
  `ClaimsAltChord`. That observation is false as stated: `QuickFiler/Viewers/EfcViewer.cs` line 96
  declares a delivered predicate of the same name and that file is a compile item at
  `QuickFiler/QuickFiler.csproj` line 389. The qualifier had been dropped in exactly one place. This
  is a shared-qualifier-detachment defect class; the class sweep found three instances.
- **B2** `[P6-T19]`'s permitted-residue list omitted `final-commit.md`, which `[P6-T18]` necessarily
  writes after its own commit because that artifact records the commit's own `EXIT_CODE:`. Both the
  at-most clause and the post-amend "prints nothing" statement were therefore unsatisfiable.

Warning: **W3** the `_build-nullable.yml` citation carried the same incomplete-span defect corrected
for the sibling workflow file in round 3; **W4** `[P0-T8]`'s carry-forward disposition named a scoped
check `[P4-T2]` supplied no command for.

Minor: **M1** the reported plan length was wrong (896 reported, 900 measured); **M2** `[P4-T7]` dropped
the class scoping its own acceptance clause carries; **M3** the traceability table omitted `[P1-T3]`
from the AC-10 row; **M4** the named-test derivation admitted a skipped test; **M5** the two msbuild
baselines carried no non-zero disposition; **M6** a wording imprecision about `_format-check.yml`.

Two orchestrator-supplied delta texts were refused by the planner on evidence, and both refusals were
independently verified:

- The B1 delta text required asserting that a `method` element named `ClaimsAltChord` **does** exist
  under the `EfcViewer.cs` class. `EfcViewer.cs` line 20 carries the coverage-exclusion attribute on
  the class declared at line 21, and `coverage.config` overrides only `ModulePaths/Exclude`, leaving
  the collector's default attribute-based exclusion in force. The planner applied the scoping fix and
  required a measured reading instead. Applying the supplied text would have introduced a new false
  assertion.
- The M4 `Skipped:` label could not be observed without running the wrapper, which preparation mode
  forbids. The planner made `[P0-T12]` transcribe the summary block verbatim with a documented
  fallback rather than assert an unobserved label. Round 5 later confirmed the label from recorded
  runs already in the repository.

The planner's own workflow-citation sweep additionally found a defect no round had reported:
`spec.md` claimed the type-check command is "character-for-character the one in
`.github/workflows/ci.yml`", but `ci.yml` contains no msbuild command at all.

### Round 5 — 3 defects, 0 blocking

- **F1** `[P0-T13]`: the stated purpose of the `EfcViewer.cs` probe did not follow from the mechanism
  the same paragraph enumerates. The probe is informative in one direction only, because the
  class-level exclusion attribute and package pruning produce the same absent reading.
- **F2** `[P0-T13]`: `Get-KoverageProjectAllowlist` was cited at `Helpers.ps1` lines 4 through 34. The
  function opens at 4 and closes at 48; the `.Test` skip that makes the sentence's qualifier true is
  at 40-42 and the add is at 44, all outside the cited span.
- **F3** `[P6-T19]`: the amend had no staging span. `git commit --amend --no-edit` commits only the
  index, so with nothing staged it produces an identical tree and leaves all three residues in place.

Applying F2 prompted a script-file citation-span sweep the earlier workflow-file sweep had not
covered: 25 citations checked, 2 corrected. The second was `Merge-CoberturaClassesByFilename`, cited
at 262-301 when the function spans 262-391, excluding the `ReplaceChild` at 382 on which the
surrounding contrast rests.

Two further `spec.md` factual corrections were authorised as their own rounds after this one. The
first: `spec.md` asserted that `scripts/vscode/Invoke-MSTestWithCoverage.ps1` carries the same
single-assembly discovery defect as `Invoke-MSTest.ps1`. That is false — its discovery pipeline at
lines 296-302 is wrapped in `@( ... )`, so the collection is array-valued at any match count and the
`.Count` reads at 304 and 315 cannot throw. The second: the issue-#713 follow-up item generalised the
same defect to "the MSTest wrapper scripts" in the plural, contradicting the correction just made.

### Round 6 — 1 defect, 0 blocking

- **D1** plan lines 202-203 and 209 asserted that the plan's use of `-SearchRoot .` is "a deliberate
  deviation from the `-SearchRoot QuickFiler.Test` form named in the spec's toolchain section". The
  spec's toolchain subsection uses `-SearchRoot .` for both wrappers, and
  `-SearchRoot QuickFiler.Test` appears only inside the Known-tooling-defect subsection as a root that
  throws. The plan agrees with the spec, and the sentence also contradicted the spec's own AC-10 row.
  Corrected to state the agreement.

Round 6 also adjudicated a residual the round-5 sweep had flagged in the `spec.md` AC-10 cell and
returned no edit required: the clause names no script, is true of the only wrapper invoked in that
form, forwards to a subsection that is now correct, and states explicitly that no acceptance depends
on it.

### Round 7 — clearance

`PREFLIGHT: ALL CLEAR` / `CONVERGENCE: NO FURTHER ROUNDS EXPECTED`, with no defects found.

The clearing pass independently re-derived the D1 correction against the spec, the surrounding prose
region, the `[P0-T11]` rationale and residue item 8, and confirmed every count rather than accepting
the reported figures. It additionally re-derived clean: 10 distinct `BASELINE_*` tokens each defined
in Phase 0 before first use; the Phase 6 check-off, commit and amend fixpoint; gate classes G6, G7, G8,
G8b and G9; task-ordering satisfiability at every gate; evidence-path canonicality with no `artifacts/`
evidence path and no absolute host path in either document; the `- Work Mode: full-bug` marker
matching the spec-only AC source; and tonality.

## Cleared-state measurements

| Measure | Value |
|---|---|
| Plan lines | 1008, CRLF throughout, no BOM |
| Phases | 7, all `### Phase N — <Title>` with em dash |
| Tasks | 57 — P0 14, P1 3, P2 3, P3 3, P4 7, P5 8, P6 19; sequential and phase-matched |
| Dangling task IDs | 0; the 57 referenced IDs are exactly the 57 defined |
| Spec lines | 653 |
| Spec acceptance criteria | 15 table rows and 15 checklist entries, all `- [ ]` unchecked |
| MCP plan validator | `ok:true`, no warnings |

## MCP plan validator

Tool: `mcp__drm-copilot__validate_orchestration_artifacts`
Inputs: `artifact_type: "plan"`, `artifact_path: docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/plan.2026-08-31T20-16.md`
Result: `{"ok": true, "summary": "Validated plan artifact at 'docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/plan.2026-08-31T20-16.md'."}`

Run by the orchestrator after every delta application in this resume, and last after the round-6
delta. The plan validator is not exposed to the `atomic-planner` or `atomic-executor` tool surfaces in
this environment, so each of those agents reported `VALIDATOR NOT RUN` and the orchestrator ran the
gate itself.

## Acceptance Criteria Status

- Source: `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md`
- Total AC items: 15
- Checked off (delivered): 0
- Remaining (unchecked): 15
- Items remaining: AC-1 through AC-15, all of them.

No acceptance criterion is checked off, and none should be. This run is preparation only: the plan was
authored, reviewed and cleared, and no plan task was executed. Every AC is delivered by plan execution,
which is out of scope here and is checked off by `[P6-T2]` through `[P6-T16]` during that execution.

## Scope boundary of this run

Performed: preflight rounds 3 through 7, the corresponding plan and spec revisions applied in place,
the MCP plan validator gate, this evidence artifact, and the commit and push to the item branch.

Not performed, and deliberately so: no plan task was executed; no production or test source file was
modified; no toolchain command was run against the solution; no pull request, merge, or issue mutation.
The `git status --porcelain` footprint of this run is confined to `docs/features/`.
