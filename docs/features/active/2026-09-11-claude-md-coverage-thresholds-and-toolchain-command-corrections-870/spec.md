# 2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections (Spec)

- **Issue:** #870
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug (sole acceptance-criteria source is this file; see `## Acceptance Criteria` below)

## Path Notation Convention

This document deliberately wraps only the paths this change actually writes in single backticks; every other file, switch, command name, and configuration file is named in plain prose. The convention is load-bearing, not cosmetic: the blast-radius extraction module shipped under the dot-claude library tree harvests backtick-delimited inline-code tokens from the spec as well as the plan and classifies the path-shaped ones into this item's write set, with no notion of negation, so a backticked comparison or exclusion path would be recorded as a write claim and could serialise this item against unrelated concurrent work. Do not "fix" the formatting of a prose-named path in this file.

Note for the run scheduler: the repository-root instructions file this item writes carries no directory separator, and the extractor admits a separator-free token only when it is an exact member of the configured shared-surface set, which does not list it. It therefore cannot be derived from this document and must be hand-appended to this item's radius. It is named as a backticked path below regardless.

## Context

`CLAUDE.md` contains three documentation defects, all corrected in this single repository-root file. No other file is a write target.

1. Two toolchain-command entries name a command this repository deliberately never runs. The CUT3 "C# Toolchain Command Selection" step 4 and the later "C# Toolchain (run in this exact order)" step 4 both state a bare vstest invocation carrying the built-in Code Coverage data collector switch. The route actually run is a PowerShell script under scripts/vscode named Invoke-MSTestWithCoverage.ps1, wrapped by the VS Code task labelled test: MSTest with Coverage (Koverage). That script runs an outer dotnet-coverage collect process around an inner vstest invocation, and deliberately withholds the built-in Code Coverage data collector from that inner call because the built-in collector conflicts with the outer instrumentation. Instrumentation and exclusions are sourced from the outer settings path, which reads the repository-root coverage.config file. Output is Cobertura XML written under the coverage directory.

2. The coverage-threshold figures in the UT2 "Coverage and Scenarios" block do not state the figures the maintainer settled on issue 563 (decision recorded 2026-09-11): C# line coverage floor 80 percent, C# branch coverage floor 75 percent, PowerShell line coverage floor 80 percent with no PowerShell branch floor because Pester does not measure branch coverage, and new code 90 percent. The three exemption classes already stated in that block are unchanged: (a) VSTO add-in lifecycle classes, (b) WinForms form-derived classes and Designer-generated code, and (c) Outlook Interop event-handler classes with no injectable seam.

3. Two analyzer-severity citations name a configuration file that does not exist anywhere in this repository. Both citations currently name that nonexistent file alongside the file that does exist as the analyzer-severity source. Because both lines already pair the real file with the nonexistent one, the correction at each site is to delete the nonexistent name and leave the real, existing file as the sole named severity source — not to substitute one name for the other, which would instead duplicate the real file's name on the C#1 item 2 line.

All three corrections are documentation-only. This change modifies no source, build, or test file, so no format, lint, type-check, or test toolchain command is warranted or required to validate it.

## Repro & Evidence

Steps to reproduce each defect by direct reading of the current `CLAUDE.md`:

1. Read the CUT3 step 4 entry and the final "C# Toolchain" step 4 entry: both state the same bare vstest command carrying the built-in coverage-collector switch. Read the coverage script referenced in the research record (scripts/vscode/Invoke-MSTestWithCoverage.ps1): the implemented route is an outer dotnet-coverage collect call wrapping an inner vstest call, and the inner call is never given the built-in collector switch, by design.
2. Read the UT2 block: it states an undifferentiated "repository-wide line coverage must remain >= 80%" with no branch figure and no PowerShell-specific figure. The maintainer decision on issue 563, dated 2026-09-11, adds the missing C# branch figure (75 percent) and the missing PowerShell line figure (80 percent, no branch floor).
3. Search `CLAUDE.md` for the nonexistent analyzer-configuration file name: two occurrences, at the C#1 item 2 line and the C#7 line. That file does not exist anywhere in this repository; the file that does exist and is the actual analyzer-severity source is named alongside it at both sites already.

Impact / Severity: Medium. Reviewers citing the CUT3 step 4 entry issue false verdicts against a command that is not actually run, and the undifferentiated coverage figure creates a mismatch against the settled maintainer decision. Neither defect blocks a release, but both cause recurring, avoidable review friction.

## Scope & Non-Goals

In scope:
- Correcting the two toolchain step-4 entries in `CLAUDE.md` to name the actual coverage route and note the deliberate omission of the built-in collector switch from the inner invocation.
- Correcting the UT2 block in `CLAUDE.md` to state the four settled coverage figures, the decision date, and the issue-563 reference, while preserving the three exemption classes verbatim.
- Removing the two citations of the nonexistent analyzer-configuration file name from `CLAUDE.md`, leaving the real, existing file as the sole named source at each site.

Out of scope / non-goals:

The governance rules files published into this repository under the dot-claude tree (the general unit test rule, the quality tiers rule, and the C# rule) and the feature-review coverage validation hook state a stricter pair of figures, eighty five percent line and seventy five percent branch, applied uniformly across module rigor tiers. Those files arrive by an automated push-down process from an upstream governance repository with zero templating applied, so any local edit made to one of them here is overwritten the next time that push-down runs. They are out of scope for this item, must not be edited, must not appear as a task in any implementation plan, and must not be named as a path wrapped in single backticks anywhere in this document. The enforcement half of the issue-563 divergence — reconciling the eighty/seventy-five figures this fix records in the repository's own instructions against the eighty-five/seventy-five figures actually enforced by the rules tree and the review hook — is tracked upstream in the governance repository, not resolved by this item.

Explicitly excluded systems, integrations, or datasets: no source code, build configuration, test assembly, or CI workflow file is touched; no coverage script behavior changes; no analyzer or nullable configuration changes.

## Root Cause Analysis

- The two toolchain step-4 entries were written to describe a direct vstest invocation with the built-in coverage collector before the outer dotnet-coverage-based script existed as the actual entry point; the entries were never updated once the script and its wrapping VS Code task became the real route.
- The UT2 block's undifferentiated 80 percent line figure predates the issue-563 decision process; the decision added a branch figure and a PowerShell-specific figure that were never folded back into `CLAUDE.md`.
- Both analyzer-severity citations already name the real, existing analyzer-configuration file (the one ending in editorconfig) alongside the nonexistent one (the one ending in globalconfig, which this repository has never contained). A literal instruction to "replace one name with the other" would either delete the only correct name or duplicate it; the correct fix at each site is deletion of the nonexistent name only, leaving the real name as the sole citation. This reasoning is recorded here explicitly because a naive replacement pass would otherwise produce a duplicated name on the C#1 item 2 line.
- Only `CLAUDE.md` is in scope. The push-down-owned rules files under the dot-claude tree repeat some of the same figures and command text but are not edited here, per the Scope & Non-Goals section above.

## Proposed Fix

### Design summary (what changes where)

All three corrections are text edits confined to the single repository-root file `CLAUDE.md`. No design alternatives were evaluated: the fix is a determinate prose correction with one file and no architectural choice.

### Boundaries and invariants to preserve

- The three UT2 exemption classes remain verbatim.
- The unchanged figures (C# line at 80 percent, new-code at 90 percent) are preserved; only the missing branch and PowerShell figures are added.
- No file outside `CLAUDE.md` is modified.

### Dependencies or blocked work

None. This is a standalone documentation correction.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

`CLAUDE.md` only, at four prose locations: the two toolchain step-4 entries, the UT2 coverage-figures block, and the two analyzer-severity citation lines (edited as one correction type applied at two sites).

#### Functions/classes/CLI commands impacted

None; this is a prose-only change with no executable surface.

#### Data flow and validation changes

None.

#### Error handling and logging updates

None.

#### Rollback/feature-flag considerations (if applicable)

None; a plain revert of the text edit fully rolls back this change.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

Not applicable; no interface or data format changes.

#### Required configuration keys and defaults

Not applicable.

#### Backward-compatibility expectations

The corrected instructions describe the toolchain route developers and reviewers already run in practice; no behavioral compatibility question arises because no executable behavior changes.

#### Performance constraints (latency/throughput/memory)

Not applicable.

## Assumptions, Constraints, Dependencies

- Assumptions: the settled coverage figures and the decision date recorded in this feature folder's issue.md (issue 563, decided 2026-09-11) are accepted as given, per the research record's documented corroboration path.
- Constraints: `CLAUDE.md` is the only file this item's production diff may touch.
- External dependencies: none.

## Data / API / Config Impact

- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates: none.
- Compatibility notes: the corrected text realigns repository instructions with the toolchain route and coverage figures already in effect; no config schema or CLI flag changes.

## Test Strategy

This change touches no source, build, or test file, so no regression test, unit test, or toolchain pass applies. Validation is by direct text inspection of the corrected `CLAUDE.md` against the Acceptance Criteria below.

- Unit coverage areas: none (documentation-only change).
- Integration scenario to retest: none.
- Manual verification notes: search the corrected `CLAUDE.md` for each literal named in the Acceptance Criteria section and confirm the stated presence or absence.

## Write Set

- `CLAUDE.md`
- `docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/`
- `docs/features/potential/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md`
- `docs/features/potential/promoted/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md`

The first entry is the only production file. The second is this item's own feature folder and every artifact beneath it. The third and fourth are the promotion lifecycle records: the promotion step deletes the first of them and creates the second, so both appear in this item's diff. All four filenames carry this item's own slug, so none can collide with another concurrently scheduled item.

## Acceptance Criteria

- [ ] A search of `CLAUDE.md` for the literal text /EnableCodeCoverage returns zero matching lines, and the same search over the same file returns a non-zero count for the literal text dotnet-coverage, so the zero result is evidence of the correction rather than of a broken search.
- [ ] A search of `CLAUDE.md` for the literal text .globalconfig returns zero matching lines, and the literal text .editorconfig occurs at least twice in the same file.
- [ ] The CUT3 "C# Toolchain Command Selection" step 4 entry names the coverage script scripts/vscode/Invoke-MSTestWithCoverage.ps1 and the VS Code task labelled test: MSTest with Coverage (Koverage), and states that the built-in Code Coverage data collector is deliberately withheld from the inner vstest invocation because it conflicts with the outer dotnet-coverage instrumentation.
- [ ] The final "C# Toolchain (run in this exact order)" step 4 entry states the same script, task label, and withheld-collector note as the CUT3 step 4 entry, so the two toolchain restatements remain consistent with each other.
- [ ] The UT2 coverage block states each of the four settled figures — C# line 80 percent, C# branch 75 percent, PowerShell line 80 percent, new code 90 percent — together with the decision date 2026-09-11 and a reference to issue 563.
- [ ] The UT2 block states explicitly that Pester does not measure branch coverage, as the reason no PowerShell branch floor is stated.
- [ ] The three UT2 exemption classes (VSTO add-in lifecycle classes; WinForms form-derived classes and Designer-generated code; Outlook Interop event-handler classes with no injectable seam) are byte-identical between the pre-change and post-change text of `CLAUDE.md`.
- [ ] A name-only diff anchored on the base commit recorded in Phase 0, paired with a porcelain status listing in the same check, reports exactly one changed path once three exclusions are applied, and that remaining path is `CLAUDE.md`. The three excluded entries are the feature folder `docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/` and its contents, the promotion lifecycle record `docs/features/potential/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md` that the promotion step deleted, and the promotion lifecycle record `docs/features/potential/promoted/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md` that the promotion step created. Those two promotion records are committed by the preparation step that precedes execution, so they are present in the diff against the base commit and an exclusion naming only the feature folder makes this criterion impossible to satisfy.

## Risks & Mitigations

- Risk: a literal "replace name A with name B" edit at the C#1 item 2 site would duplicate the real analyzer-configuration file's name instead of removing only the nonexistent one. Mitigation: the Root Cause Analysis section above states the deletion-only correction explicitly, and the acceptance criteria checks for zero occurrences of the nonexistent name rather than for a specific replacement string.
- Risk: an implementation plan could be tempted to also edit the push-down-owned rules files to close the 80-versus-85 divergence. Mitigation: the Scope & Non-Goals section states those files are out of scope and must not appear in any task or write set for this item.

## Rollout & Follow-up

- Release/rollout steps: merge the corrected `CLAUDE.md` directly; no phased rollout applies to a documentation-only change.
- Post-fix monitoring or clean-up tasks: none beyond confirming reviewers no longer cite the corrected step-4 entries or the nonexistent analyzer-configuration file name in future reviews.
- Links: issue #870; related issues #828 (toolchain command), #563 (coverage threshold divergence, decision recorded 2026-09-11), and #727 sub-finding 5 (the analyzer-configuration citation); research record at docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/research/2026-09-12T10-45-claude-md-coverage-toolchain-corrections-research.md.
