# 2026-09-28-evidence-and-identity-hygiene-sweep (Spec)

- **Issue:** #927
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-29T00-20
- **Status:** Draft (ready for atomic planning; rule 8 and Resolved tensions item 7 added from plan revision round 2)
- **Version:** 0.3
- **Work mode:** full-bug. This file is the sole authoritative acceptance-criteria source; no user-story.md exists for this item.

> Formatting contract (do not "fix"): a downstream tool derives the change footprint from backticked repository paths in this document. Backticked paths appear only in the `## Write Set` section and in the evidence-artifact paths listed under this feature folder in the Test Strategy (which fall under that section's feature-folder glob). Acceptance-criteria lines carry no digits and no code spans: each names its evidence artifact by file stem, and the Test Strategy evidence list carries the full path. Every other path, including paths that this change deliberately does not touch, is written as plain prose. Identifier hygiene: the developer account name, the host name, the eight-dot-three short form of the account and every absolute host path are referred to by category or by the placeholders `<repo-root>`, `<user-profile>`, `<user>` and `<host>`; no drive-rooted profile path is written contiguously anywhere in this file, so this file passes the guard it specifies.

## Context

This item consolidates the unresolved remainder of #602, #671 and #884, and the plan-file exclusion gap from #727 sub-finding 5. The common root cause is that identity-bearing test artifacts and absolute host paths were committed before the committed-evidence convention existed, and nothing rejects them at commit or CI time. PR #881 (issue #873) fixed prevention in the scripts/vscode test tooling: explicit results-directory and log-file-name arguments, and projections instead of raw documents. It did not remove existing content and did not add an enforcement guard, so the counts kept growing.

Environment:

- OS/version: Windows 11 Pro 10.0.26200 for the local sweep; the CI guard runs on the Ubuntu hosted runner, whose image ships PowerShell and Git (research section 1.1).
- Python version: not applicable (repository hygiene; tracked Markdown, TRX and Cobertura files, PowerShell and C# test fixtures).
- Command/flags used at filing: git ls-files and git grep with the identifiers read from the environment, measured on main at 177b6d78e on 2026-09-28.
- Data source or fixture: tracked files under docs/features, docs/research, tests, the five C# test projects, and the stray root-level run log.

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The repository is out of compliance with its own committed-evidence policy (CLAUDE.md, "Committed Test Evidence Format") across hundreds of files, and the profile-path leak surface grew roughly twenty-five-fold after the prevention tooling landed. Prevention by convention alone has been shown not to hold; this item removes the existing content, redacts the identifiers, and adds a CI gate so the cleanup cannot regress.

Authoritative inputs for this specification, in precedence order:

1. issue.md in this folder (scope and expected behaviour).
2. The orchestrator-measured write-set inventory at `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/other/write-set-inventory.2026-09-28T20-30.md` (figures; supersedes the research where the two differ).
3. The research record at `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/research/2026-09-28T19-55-evidence-and-identity-hygiene-sweep-research.md` (design; adopted in full except where a "Resolved tensions" entry below records a reason).

## Repro & Evidence

Steps to reproduce (on main at 177b6d78e, from the repository root, PowerShell):

1. Enumerate tracked test-platform documents under docs/features by name: the listing returns 332 paths.
2. Enumerate tracked cobertura-named XML documents under docs/features: the listing returns 248 paths (240 by file name plus 8 that match only through a directory segment; the latter are raw Pester JaCoCo documents).
3. Search tracked text files for the developer account name, the host name and the generic Windows user-profile path pattern, with the identifier values read from the USERNAME, COMPUTERNAME and USERPROFILE environment variables rather than typed.
4. Run the new guard (once it exists) over the same tree: it exits non-zero with a findings count equal to the raw-document population plus the profile-path file population.

Expected:

- No tracked raw test-platform document and no raw coverage-collector document remain, in any location including a feature folder's evidence tree (CLAUDE.md, "Committed Test Evidence Format").
- No tracked file outside the push-down-owned governance directory contains an absolute user-profile path, the bare account name, the bare host name, or the eight-dot-three short form. Committed text uses the placeholders `<repo-root>`, `<user-profile>`, `<user>` and `<host>`.
- A CI check rejects new violations, so the cleanup does not regress.

Actual (orchestrator measurement on 2026-09-28 at 177b6d78e; identifiers never written):

| Condition | Tracked files | At filing of the consolidated issue |
|---|---|---|
| Raw test-platform documents (trx extension) under docs/features | 332 | 333 (#884) |
| Cobertura collector documents (root element coverage) under docs/features | 243 | 248 by name (#671) |
| dotnet-coverage native documents (root element results) | 23 | not counted |
| Raw Pester JaCoCo documents (root element report with class, sourcefile, method or line children) | 27 | not counted |
| Package-level JaCoCo projections (root element report with only package and counter children) | 18 retained | not counted |
| Contains the account name, outside the governance directory | 1,215 | 991 (#602) |
| Contains a generic user-profile path, outside the governance directory | 1,213 | 45 (#602) |
| Contains the host name, outside the governance directory | 186 | 146 (#602) |
| Contains the eight-dot-three short form, outside the governance directory | 5 | not counted |
| Contains a legacy redaction token (three forms, research section 3.4) | 88 | not counted |

Population of the write set (inventory): 1,691 in-scope tracked files; 625 raw evidence documents (332 trx, 243 Cobertura, 23 dotnet-coverage results, 27 raw Pester JaCoCo); 18 projections retained; 184 feature-folder scopes (64 active, 116 archive, 3 epics, 1 parallel); 6 single promoted records; 16 files outside docs/features (13 C# test fixture files, 1 PowerShell test file, 1 research note, 1 stray root run log). The research record's phase description names 1,661 files and 181 folders; the inventory figures above govern.

Other observations that constrain the fix:

- Two trx file names follow the default vstest naming of account, host and timestamp, so the path itself carries both identifiers and removal is the only remedy for those (research section 3.1).
- Three Cobertura documents carry no cobertura marker in their name, and two put the root tag on its own line with attributes on the next line, so both the removal list and the guard must classify by content, not by name (research section 3.1).
- The two MCP configuration files at the repository root (the JSON MCP manifest and the Codex TOML configuration) match the account search only through the npm package scope of the governance MCP server; that is a package identifier and is out of scope.
- About eight files under the governance directory are affected; they are push-down owned from the upstream governance repository and are tracked there as companion issue 932.
- The root-level run log is a captured vstest console run that nothing reads (research section 5).

Logs / Screenshots:
- [x] Attached minimal logs or snippet
- Snippet: the counts above. The identifiers themselves are deliberately not reproduced, and the baseline artifact required by AC1 records counts only.

## Scope & Non-Goals

In scope:

- Removal (git rm) of every tracked raw evidence document, selected by content classification, plus the stray root-level run log.
- Ignore rules for the raw-document name patterns.
- Redaction of the account name, host name, eight-dot-three form, absolute profile paths and legacy redaction tokens across all tracked text files outside the governance directory, using a throwaway helper kept outside the repository.
- Individual triage and rewrite of the thirteen C# test fixture files and the one PowerShell helper test that carry a drive-rooted profile path, and redaction of the one research note.
- A repository hygiene guard: three PowerShell production files under scripts/hygiene, three Pester test files under tests/scripts/hygiene, a reusable callee workflow, its job in the CI orchestrator, the Pester callee's path-array extension, and the workflow README rows.
- Evidence for this item in the permitted projection and summary forms only.

Out of scope / non-goals (paths in this list are deliberately unbackticked):

- The push-down-owned governance directory, .claude, including .claude/settings.json. It is excluded from both the cleanup and the guard; the companion upstream issue in drm-copilot is 932.
- The npm package scope in the two MCP configuration files, .mcp.json and .codex/config.toml. It is a package identifier, not a host identifier.
- History rewriting of any kind. Removed files remain in history; no force push, filter, or rebase of main.
- Adding the new CI context to the main branch ruleset. That is an operator follow-up after the first green run (see Rollout).
- The other items of the parallel run bugs-2026-09-28. They are prepared on their own branches and are not edited by this item; their evidence must pass the guard once they update past main (see Risks).
- Retroactive generation of JaCoCo projections for removed Cobertura documents (research section 6.2): the projection writer is specified over the post-processed document and would not reconcile against a raw collector document.
- Rewriting of prose links in historical evidence Markdown that will dangle after the removal (research section 6.1); a reviewer must not read a dangling link as an incomplete task.
- Any change to production (non-test) C# code, to TaskMaster.sln, any csproj, packages.config or app.config.
- A committed redaction tool. The orchestrator constraint is a throwaway helper; a committed tool would need test fixtures that are exactly the strings the guard forbids.
- A token allowlist for fixture user segments inside the guard (rejected in research section 4).

Explicitly excluded systems, integrations, or datasets: the upstream governance repository (drm-copilot) and its push-down mechanism; the GitHub branch ruleset (read-only reference here); the ignored working-tree coverage directory, which the guard never reads because enumeration is by git ls-files.

## Root Cause Analysis

- Evidence Markdown written by agents quotes absolute paths and tool banners verbatim; nothing in the commit path or CI inspects committed text for a drive-rooted profile path.
- Raw vstest and coverage-collector output embeds the run user, the computer name and a lowercased storage path; the tooling fix in #873 stopped new raw documents from the standard route but left every already-tracked document in place, and a raw document can still be added by hand or by a non-standard route.
- No gate exists for either class. The ignore file already ignores the binary coverage formats and the working-tree coverage directory but has no entry for trx or for any Cobertura name, so an accidental add is not stopped either.
- Past redaction sweeps excluded the plan file from their own residual scan (#727 sub-finding 5, from item #662), so the plan file re-introduced identifiers after each sweep.
- Legacy sweeps used inconsistent placeholder tokens (three forms), so any guard would need an allowlist unless the tokens are normalised.

## Resolved tensions (scope decisions made by this spec)

1. **Coverage floor for PowerShell.** CLAUDE.md UT2 states an eighty percent line floor and a ninety percent new-code target, settled by the maintainer on 2026-09-11 (#563); the push-down-owned rules under .claude/rules state eighty-five. CLAUDE.md outranks the rules directory (policy compliance order) and is what the Pester callee enforces (research section 2.4). The floor is eighty in CI and the new-code target is ninety.
2. **Guard coverage versus identifier secrecy.** The issue's expected behaviour names the bare account and host name as forbidden, but the guard must not embed the literals it searches for. Resolution: the guard enforces the two identifier-free rules (raw document by content; generic profile-path pattern). The bare account, host and eight-dot-three tokens are enforced once, in this item, by the environment-derived gates in the Test Strategy. A future leak of a bare token outside a path is therefore not caught by CI; this residual is recorded in Risks and is accepted because the alternative embeds the identifier in a tracked file.
3. **Legacy redaction tokens versus this feature's own folder.** The research record and the inventory in this folder necessarily name the legacy tokens they normalise. The legacy-token gate (AC10) therefore excludes this feature folder by name; the identifier gates (AC9) do not exclude it, per #727 sub-finding 5.
4. **PoshQC settings file.** The PowerShell rule names a Pester settings file under scripts/powershell/PoshQC/settings that does not exist in this repository. The local Pester reference for this item is the configuration block in the Pester callee workflow (Run.Path, CodeCoverage.Path, JaCoCo output, eighty percent line floor), extended with the hygiene folders.
5. **Inventory versus research figures.** Where the two differ (1,691 versus 1,661 files; 184 versus 181 folders), the inventory governs; it was produced by git ls-files and git grep over the tracked tree, whereas the research used working-tree content search.
6. **Fixture token retention.** The three C# NotContain assertions on the token testuser, the one on OneDrive and the one on fsAncestor keep their discriminating power only if the token survives the rewrite; the rewrite therefore changes the root of each fixture path and retains the user-segment token.
7. **Legacy-token case.** Gate nine and rule 7 are case-sensitive as specified. The upper-case user-token forms are outside rule 7 and remain. They are not identifiers and do not trip the guard. The plan records their case-insensitive file count as a residual next to the gate nine result (plan decision D18); the count is recorded, not gated.

## Proposed Fix

### Design summary (what changes where):

**Invariant established by this change.** After merge, every tracked file outside the governance directory satisfies both guard rules: it is not a raw test-platform or raw coverage-collector document by content (root element coverage, results, TestRun or CoverageSession, a trx, coverage or coveragexml extension, or a JaCoCo report carrying class, sourcefile, method or line elements), and it contains no line matching the generic Windows user-profile path pattern; the CI guard job fails on the first violation of either rule and prints the path and line only, never the matched text.

Four parts:

1. **Guard** (PowerShell, scripts/hygiene, Pester tests under tests/scripts/hygiene). Entry point dot-sources two part files; the pure rule functions take text and return classifications or line-numbered matches; the git seam wraps the executable and parses the NUL-separated eol listing; the content reader is an injectable delegate whose default decodes by byte-order mark. Governance-directory exclusion is a path-prefix test on the listing, not a pathspec, so it is unit-testable in memory.
2. **Workflow wiring.** A reusable callee copied from the shape of the format-check callee (workflow_call plus workflow_dispatch, contents read, one job on the Ubuntu runner, checkout with depth one, one pwsh step that runs the guard). The CI orchestrator gains one uses-job with no needs edge. The Pester callee's two path arrays gain the hygiene folders. The workflow README gains the callee row, corrects the Pester row to name all three test folders, and adds the seventh predicted context.
3. **Removal and ignore rules.** git rm of the content-classified raw-document list and the stray run log; two ignore patterns (trx and cobertura-named XML) with a comment stating the content guard is the real protection.
4. **Redaction.** Throwaway helper outside the repository; identifiers from USERNAME, COMPUTERNAME and USERPROFILE (plus the profile leaf and the eight-dot-three leaf derived from USERPROFILE); eight ordered rules (the seven in the table below and rule 8, added by the atomic plan's decision D17 for the reason recorded in the table), longest first, idempotent; bytes in, bytes out with the same encoding, byte-order mark and line endings; residual scan over every tracked file including the plan file and this folder. The fourteen fixture files are rewritten by hand per the triage table, not by the helper, because their roots change rather than their tokens.

**Trace of one violating value of each kind, today and after the change.**

Rule A (a raw document). A trx document is written by a non-standard vstest route and staged under a feature evidence tree. (1) Accept point today: git add succeeds because the ignore file has no trx entry. (2) Every existing gate passes over it: the formatter ignores evidence trees and trx (csharpierignore), the two msbuild gates compile nothing from it, the MSTest gate reads only the working-tree coverage directory, the Pester gate runs only the two listed test folders. (3) Absorption: the pull request merges with the document, its run user and computer name in the tree; nothing reports. (4) After the change: the ignore pattern stops the accidental add without a force flag; if forced, the guard enumerates the path through git ls-files, the extension gate classifies it as trx, one finding line names the path, the findings total is non-zero, the script exits one, the pwsh step propagates the exit, the hygiene context is red.

Rule B (a profile path). An agent quotes a tool banner containing a drive-rooted profile path into an evidence Markdown file. (1) Accept point today: no gate reads Markdown content. (2) through (3) as above; the identifier merges. (4) After: the guard reads the file through the content adapter, the generic pattern matches the drive letter, separator run, profile parent and the first character of the user segment, the finding line reports path and line number only, exit one, context red. A placeholder such as `<user-profile>` never matches because the first character after the separator run is not in the permitted set.

Why neither half suffices alone: the guard cannot go green on CI until the removal and the redaction have landed on the same ref, and the removal and redaction without the guard leave the tree in the state that produced the twenty-five-fold growth after #873. The two ship in one pull request (research section 10).

### Boundaries and invariants to preserve:

- The CI orchestrator remains a pure orchestrator: no inline steps, zero needs edges, caller-owned concurrency group, callees declare no concurrency block.
- The gate step's non-zero exit is the signal; no exit-code reset is added, because the step has no deliberately failing nested command (ci-workflows rule).
- Pure rule functions never touch disk or environment; the only executable reached is git, through the wrapper seam; tests mock the wrapper, never the executable.
- The guard contains no identifier, no token allowlist and no path allowlist.
- The removed documents are not replaced by retroactive projections; the committed Markdown summaries remain the record.
- The redaction helper changes only matched spans: encoding, byte-order mark, line-ending style and trailing-newline state of each rewritten file are unchanged; XML-family files are not rewritten (the eighteen projections contain no identifier).
- Every test method in the fourteen touched test files keeps its name and its assertions; only string literals change.
- The user-segment tokens testuser, test, Test and user survive in the fixtures; the three-letter real-account prefix in the three Store tests does not.

### Dependencies or blocked work:

- The PowerShell change-budget router applies: three production files exceed the direct-mode overall scope of two, so the PowerShell work routes through the PowerShell orchestrator, in one batch of three production plus three test files (exactly the per-batch cap).
- The C# change-budget router applies to the thirteen test files; they are literal-only edits but the full C# toolchain runs.
- The workflow change requires a green run against the branch head before merge (feature-review rule modified-workflow-needs-green-run).
- The ruleset update (operator) follows the first green run and is not blocked on by this item.
- Sibling items of bugs-2026-09-28 are not blocked by this item, but their evidence must pass the guard once their branches update past main.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

See `## Write Set` for the complete backticked enumeration. By role:

- New guard production files: the entry point, the rules part file and the git part file under scripts/hygiene.
- New Pester test files: rules tests, git tests and orchestration tests under tests/scripts/hygiene.
- New reusable callee workflow; modified CI orchestrator, Pester callee and workflow README.
- Modified ignore file.
- Thirteen modified C# test fixture files across TaskMaster.Test, ToDoModel.Test, QuickFiler.Test and UtilitiesCS.Test; one modified PowerShell helper test; one modified research note.
- Deleted: the stray root run log and 625 raw documents inside the feature-folder scopes.
- Redacted: the remaining Markdown and text files inside the 184 feature-folder scopes and the 6 promoted records.
- This feature folder: spec, plan, issue and evidence artifacts.

#### Functions/classes/CLI commands impacted:

New functions (names are the contract the tests bind to):

| File (prose) | Function | Contract |
|---|---|---|
| scripts/hygiene entry point | `Invoke-RepositoryHygieneMain` | Enumerates tracked records through the git seam, drops records whose path starts with the governance-directory prefix, applies rule A and rule B, emits one `HYGIENE <rule> <path>[:<line>]` line per finding and a `HYGIENE Findings=<n>` summary, returns the exit decision; the script calls exit one when the count is non-zero. Body runs only when the invocation name is not the dot operator. |
| rules part file | `Get-UserProfilePathPattern` | Returns the regex string below; no parameters. |
| rules part file | `Find-UserProfilePathMatch -Text` | Returns line-numbered match records (line number only, never the matched text) for every line matching the pattern, case-insensitive. |
| rules part file | `Get-RawEvidenceDocumentKind -RelativePath -Content` | Extension gate first (trx; coverage or coveragexml; anything other than xml returns none), then for xml: strip byte-order mark, skip declaration, comments, processing instructions and DOCTYPE, take the first element name terminated by whitespace, greater-than or slash; map coverage to cobertura, results to dotnet-coverage, TestRun to trx, CoverageSession to opencover, report to jacoco-raw when the document contains a class, sourcefile, method or line element, else jacoco-projection; otherwise none. A finding is any kind other than none and jacoco-projection. |
| git part file | `Invoke-GitExe -GitArgs` | Splats into git with stderr merged; throws on non-zero exit. The only place git is invoked. |
| git part file | `Get-TrackedFileRecord` | Parses the NUL-separated eol listing into records with Path and IsBinaryInIndex. |
| git part file | `Read-TrackedFileText -Path -ReadContent` | Adapter; default reads bytes and decodes by byte-order mark (UTF-8, UTF-16 little- and big-endian), otherwise UTF-8, so UTF-16 files are scanned rather than skipped. |

Generic profile-path pattern (case-insensitive; valid in both the .NET and POSIX ERE dialects; no digit class, no word boundary, no lookaround):

```text
[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]
```

The last class requires the first character of the user segment to be alphanumeric, underscore, dot, tilde or hyphen; every canonical placeholder begins with a less-than sign and never matches; the eight-dot-three form begins with a letter and matches. There is no allowlist: legacy tokens are normalised instead (research section 3.4).

Redaction helper (outside the repository; never committed). Ordered rules, each a .NET regex built at run time from the environment; SEP is a run of one or two separators, ACCT is the alternation of the escaped account token, the escaped eight-dot-three leaf and the three legacy user tokens, PROFILE is drive letter, colon, SEP, the profile parent, SEP, ACCT:

| # | Match | Replacement |
|---|---|---|
| 1 | PROFILE, SEP, repos, SEP, TaskMaster, followed by a separator, end, or a non-alphanumeric non-hyphen character | `<repo-root>` (a worktree suffix under the repository root is kept; a hyphenated sibling worktree root falls to rule 2) |
| 2 | PROFILE not followed by an alphanumeric, underscore, dot or hyphen | `<user-profile>` |
| 3 | bare account token bounded by non-alphanumerics (hyphen is a boundary, so the session-key form inside scratch paths is covered) | `<user>` |
| 4 | bare eight-dot-three leaf, same bounds | `<user>` |
| 5 | bare host token, bounded by non-alphanumerics and not followed by a hyphen | `<host>` |
| 6 | the legacy upper-case host token | `<host>` |
| 7 | the two legacy hyphenated user tokens outside a path, not adjacent to angle brackets | `<user>` |
| 8 | a drive letter, a colon, a separator run, the profile parent (case-insensitive), a separator run and a user segment of one or more characters from the class letter, digit, underscore, dot, tilde and hyphen; applied after rule 7, case-insensitive (added by the atomic plan, decision D17) | `<user-profile>` (reason: the in-scope list names absolute profile paths and AC9 requires gate four to reach zero; rules 1 to 7 cover only account-segment paths, and 19 tracked files carry fixture or ellipsis user segments) |

Rule 3 is skipped by path for the two MCP configuration files. Bare lower-case "host" and bare "redacted" are left unchanged (ordinary words). Every replacement string contains none of the eight match targets, so a second run performs zero substitutions; the helper proves this by re-running its match phase in dry-run mode after writing.

Removal command: git rm driven by the classifier's list (the entry point may expose a list-only switch, or the executor may drive git rm from the three content searches in the Test Strategy), plus git rm of the root run log. Ignore rules are added after the removal; ignore rules do not affect already-tracked files.

Fixture rewrites (research section 4, adopted in full). Root replacement: a drive-rooted fixture root whose second segment is Fixtures, which keeps drive-rooted semantics and removes the profile parent. Per file:

| File (prose) | Change |
|---|---|
| TaskMaster.Test AppFileSystemFolderPathsOneDriveResolutionTests | three constants rebased onto the fixtures root under a testuser segment; the NotContain testuser assertion is unchanged |
| TaskMaster.Test AppFileSystemFolderPathsMatchBestSpecialFolderTests | roots rebased under a Test segment; the root-only candidate becomes the bare fixtures root; the upper-case literal becomes an upper-case fixtures root; the lower-case form becomes a lower-case fixtures path |
| TaskMaster.Test AppAutoFileObjectsFolderPredictorTests | AppData mock value rebased under a test segment |
| ToDoModel.Test PeopleScoDictionaryNewTests | the doubled-backslash profile prefix rebased onto the escaped-string fixtures root under a user segment |
| QuickFiler.Test EfcSelectionGuardTests | two literals rebased under a testuser segment with the OneDrive suffix retained |
| UtilitiesCS.Test FilePathHelperConverterTests | four literals rebased under a Test segment with the AppData suffix |
| UtilitiesCS.Test StoresWrapperTests, StoresWrapperDisableTests, StoreFilterAttributionTests | the three-letter real-account prefix replaced by testuser and the root rebased; the Google Workspace Sync and Google Apps Sync tokens retained at all six sites |
| UtilitiesCS.Test LcppnFolderPredictorStore_Tests | AppData constant rebased under a test segment |
| UtilitiesCS.Test EmailFilerConfig_Tests | two literals rebased under a testuser segment; the NotContain fsAncestor assertion unchanged |
| UtilitiesCS.Test ArchiveStemContractTests | two literals rebased under a testuser segment with the OneDrive suffix; both NotContain assertions unchanged |
| UtilitiesCS.Test FolderConverterIssue614Tests | the OneDriveRoot constant rebased under a testuser segment with the OneDrive suffix |
| tests/scripts/vscode helper test | the worktree root and the sibling repository root rebased onto the existing fixture root used elsewhere in the same file (the bare drive-rooted repo segment), keeping the stamped worktree suffix |

The exact fixture literals, for the executor (fenced so they are not harvested as paths):

```text
C:\Fixtures\testuser\OneDrive - Contoso
C:\Fixtures\testuser\OneDrive
C:\Fixtures\testuser\OneDrive - Personal
C:\Fixtures\Test\...            (MatchBestSpecialFolder; root-only candidate: C:\Fixtures)
C:\FIXTURES\TEST                (upper-case mismatch case)
c:\fixtures\test\file.txt       (lower-case mismatch case)
C:\Fixtures\test\AppData
C:\\Fixtures\\user\\            (escaped-string form, PeopleScoDictionaryNewTests)
C:\Fixtures\testuser\Google\Google Workspace Sync\sync.ost
C:\Fixtures\testuser\GOOGLE\Google Apps Sync\sync.ost
C:\repo\TaskMaster-wt-2026-07-04-12-57
C:\repo\TaskMaster
C:\repo\TaskMaster\ToDoModel\Data Model\ToDo\ToDoItem.cs
```

#### Data flow and validation changes:

- Guard: git ls-files (eol, NUL-separated) to records; prefix filter; per record, extension gate then content read only for xml, trx and text; rule B over decoded text; findings to stdout; exit decision.
- Removal: classifier list to git rm; ignore rules; check-ignore verification.
- Redaction: bytes to decoded string to eight ordered substitutions to bytes with the original encoding; dry-run second pass; residual scan; parse check over any XML-family file rewritten (expected none).

#### Error handling and logging updates:

- The git wrapper throws on non-zero exit with the argument list in the message; the entry point lets the throw propagate (fail fast) so a broken git invocation cannot read as a clean tree.
- A file whose bytes cannot be decoded by the adapter is reported as a finding with a distinct rule name (unreadable), never skipped silently.
- Finding lines never include the matched text; the CI log therefore cannot echo an identifier.
- The redaction helper writes its own log (which echoes matched paths) to the scratch directory only, never under the feature folder.

#### Rollback/feature-flag considerations (if applicable):

- The guard is reported but not required until the operator ruleset update; reverting this pull request before that update removes the context with no merge-policy effect. After the update, a revert must be paired with a ruleset PUT restoring the previous context set (workflow README, rollback paragraph).
- The removal and redaction are ordinary commits; history is untouched, so any removed document is recoverable by git show from a pre-merge ref.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- Guard input: the tracked tree of the checked-out ref; no parameters required. Output: zero or more `HYGIENE <rule> <path>[:<line>]` lines, one `HYGIENE Findings=<n>` line, exit zero or one.
- Rule names: raw-document, profile-path, unreadable.
- Committed evidence (this feature folder): Markdown only, schema fields Timestamp, Command, EXIT_CODE, optional ExpectedExitCode, Output Summary; counts only; identifiers never written.

#### Required configuration keys and defaults:

- Pester callee: Run.Path gains tests/scripts/hygiene; CodeCoverage.Path gains scripts/hygiene; both arrays keep alphabetical order (dependencies, hygiene, vscode). No other key changes.
- Ignore file: two new patterns after the existing coverage patterns: the trx extension and the cobertura-marker XML pattern (marker anywhere in the name, because at least one tracked name places a timestamp after the marker).
- Guard: no configuration keys; the governance-directory prefix and the rule set are constants in the script.

#### Backward-compatibility expectations:

- No script, test, workflow or task reads a tracked raw document (research section 6.1); removal breaks no executable consumer. Markdown links into removed documents dangle and are tolerated.
- The Pester callee's gate semantics (fail on any failed test; fail below eighty percent line) are unchanged; only its path arrays widen.
- The MSTest and Pester CI contexts keep their names; one context is added.
- The fixture rewrites do not change any production behaviour; the only production reference to the profile parent reads the environment and parses nothing (research section 4).

#### Performance constraints (latency/throughput/memory):

- The guard's content read is extension-gated, so most tracked files are never opened; the run is expected to stay well inside the callee's ten-minute timeout on the Ubuntu runner. No numeric latency gate is asserted.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): the executor has a shell with git and PowerShell; USERNAME, COMPUTERNAME and USERPROFILE are set; the eight-dot-three leaf is obtainable from the file-system object; one tracked UTF-16 file, docs/features/archive/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/2026-05-14T12-41-05Z/msbuild-analyzers.txt, contains a profile path on the pre-sweep tree, and the helper handles it through the byte-order-mark branch; the pre-cleanup and post-cleanup census of that file class are recorded in docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/identifier-residual-scan.md (P4-T8) (note, 2026-09-29: the brief's recorded zero binary-only hits is superseded by this measurement); the Ubuntu hosted runner image ships PowerShell and Git (research, verified against the image page).
- Constraints (budget, performance, compatibility): three production PowerShell files plus three test files, exactly the per-batch cap; each new file under the five-hundred-line ceiling; PowerShell seven compatibility; MSTest, Moq and FluentAssertions only in C# tests (no new tests are added in C#); no temporary files in any test; no identifier written into any tracked file or CI log.
- External dependencies (services, libraries, releases): Pester at the version pinned in the Pester callee; actionlint at the version pinned in the actionlint callee; GitHub check-runs API for the outcome verification; the operator for the ruleset update.

## Data / API / Config Impact

- User-facing or API changes: none in the add-in. New CI context reported on every pull request.
- Data or migration considerations: 625 tracked documents and one run log removed; about 1,691 files rewritten by substitution; history retained.
- Logging/telemetry updates (if any): guard output as specified; no add-in logging change.
- Compatibility notes (CLI flags, config schemas, versioning): the guard has no flags in its CI form; a list-only switch, if added, must not change the default behaviour; the ignore file gains two patterns; the Pester callee's arrays widen.

## Test Strategy

Seeded from the issue (retained for traceability; the authoritative criteria are in `## Acceptance Criteria`):

- git rm every tracked raw evidence document, relying on committed summaries; no history rewrite.
- Ignore rules for the raw evidence document types.
- Placeholder replacement across all tracked non-governance text files, longest first, every file type including plan files, the run log deleted, the fixture hits triaged individually.
- A CI guard following the callee convention, failing on a tracked raw document or a profile-path pattern outside the governance directory, embedding no identifier.
- Coordination with the sibling items so their evidence passes the guard before it becomes required.
- Validation: the counts return zero outside the governance directory and the guard fails on a violation.

Bugfix workflow mapping (CLAUDE.md): the failing regression observations are (a) the guard exiting non-zero over the pre-sweep tree with the baseline findings count, and (b) the three Pester test files failing when the rule functions do not yet exist; the fix is the guard plus the sweep; the pass observations are the guard exiting zero over the final tree and the Pester suite green.

Regression tests to add or update (Pester, in-memory; the It names below are the names the acceptance criteria bind to):

Rules tests (tests/scripts/hygiene, rules test file):

- "matches a backslash-separated profile path assembled at run time"
- "matches a forward-slash-separated profile path"
- "matches a doubled-backslash profile path"
- "matches a lower-case drive letter and profile parent"
- "matches an upper-case profile parent"
- "matches an eight-dot-three user segment"
- "does not match a user-profile placeholder path"
- "does not match a repo-root placeholder path"
- "does not match a bare drive root"
- "does not match a fixtures root"
- "reports the line number and not the matched text"
- "classifies a trx extension as trx"
- "classifies a coverage root on its own line as cobertura"
- "classifies a results root as dotnet-coverage"
- "classifies a report root with class elements as jacoco-raw"
- "classifies a package-only report root as jacoco-projection"
- "classifies a report root behind a DOCTYPE"
- "classifies a byte-order-mark prefixed document"
- "classifies a ps1 file containing a TestRun element as none"

Git tests (git test file):

- "parses a NUL-separated eol listing into path records"
- "flags an index-binary record"
- "decodes UTF-16 little-endian bytes by byte-order mark"
- "decodes UTF-8 bytes without a byte-order mark"
- "throws when the git wrapper reports a non-zero exit"

Orchestration tests (orchestration test file; `Invoke-GitExe` mocked with a body declaring a string-array GitArgs parameter; content injected through the ReadContent delegate; no file created):

- "excludes governance-directory records and reports the remaining violation"
- "reports a raw document record as a finding"
- "retains a package-level projection record"
- "returns a non-zero exit decision when findings exist"
- "returns a zero exit decision over clean content"
- "prints path and line only and never the matched text"

Fixture rule for all three test files: a violating fixture is assembled by string concatenation at run time (for example a drive letter, a colon, a separator, the profile parent and a segment as separate literals) so the tracked test file never contains a contiguous profile path; XML fixtures are here-strings, which the extension gate keeps out of scope for the file itself.

Unit tests (pytest) for the fixed behavior and boundaries: not applicable; no Python is in scope.

Edge cases and negative scenarios (invalid inputs, missing data, boundary values): root tag on its own line; DOCTYPE before the root; byte-order-mark prefixed XML; UTF-16 content; an index-binary record; an empty tracked file; a placeholder path; a fixtures root; the git wrapper failing; a ps1 or Markdown file that quotes XML.

Error handling and logging verification: the wrapper-throw test; the unreadable rule; the assertion that no finding line contains the fixture's user segment.

Coverage impact and targets for changed lines/modules: each of the three new production files at or above ninety percent line coverage in the Pester JaCoCo output; the aggregate over the three listed script folders at or above eighty percent (the CI floor); C# coverage unchanged in principle (literal-only edits) and not below the pre-change first-party figures for line and branch.

Toolchain commands to run (format, lint, type-check, test), in order, restarting from the first on any failure or auto-fix:

PowerShell (for the six new files and the one modified helper test):

1. PoshQC format through the MCP command run_poshqc_format.
2. PoshQC analyze through the MCP command run_poshqc_analyze (zero findings).
3. Type checking: not applicable.
4. Pester through the MCP command run_poshqc_test, or directly with the pinned Pester version and a configuration identical to the Pester callee's block with the hygiene folders added; JaCoCo output stays under the ignored working-tree coverage directory and only a Markdown projection is committed.

C# (for the thirteen fixture files):

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. The MSTest-with-coverage route under scripts/vscode (the VS Code task or the script directly); its trx and Cobertura output stay under the ignored coverage directory; the committed evidence is the test-result summary and the package-level projection.

Both msbuild logs must be captured and shown to contain no line skipping the CoreCompile target, so the Rebuild is proven non-vacuous.

Workflow files: the actionlint runner script under scripts/dev-tools locally, then the CI actionlint context on the pull request.

Manual gates (PowerShell, repository root; identifiers from the environment; counts only are recorded):

```powershell
$account  = $env:USERNAME
$leaf     = Split-Path -Leaf $env:USERPROFILE          # add as a second token when it differs from $account
$hostName = $env:COMPUTERNAME
$short    = (New-Object -ComObject Scripting.FileSystemObject).GetFolder($env:USERPROFILE).ShortName
$scope    = @('--', '.', ':!.claude/', ':!.mcp.json', ':!.codex/config.toml')

# Gate 1  account name (text files, case-insensitive, fixed string). Baseline 1,215. Expected 0.
@(git grep -I -l -i -F -e $account -e $leaf @scope).Count
# Gate 2  host name. Baseline 186. Expected 0.
@(git grep -I -l -i -F -e $hostName -- . ':!.claude/').Count
# Gate 3  eight-dot-three form. Baseline 5. Expected 0.
@(git grep -I -l -i -F -e $short -- . ':!.claude/').Count
# Gate 4  generic profile path. Baseline 1,213. Expected 0.
@(git grep -I -l -i -E -e '[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]' -- . ':!.claude/').Count
# Gate 5  raw documents by name and by content. Baselines 332 / 240 named; 625 by content. Expected 0 / 0 / 0.
@(git ls-files -- '*.trx' '*cobertura*.xml' '*.coverage' '*.coveragexml').Count
@(git grep -l -E '^<(coverage|results|TestRun|CoverageSession)' -- '*.xml' '*.trx').Count
@(git grep -l -E '<(class|sourcefile|method|line)[ >]' -- 'docs/features/*.xml').Count
# Gate 6  the guard over the real tree. Expected exit 1 before the sweep, 0 after.
pwsh -NoProfile -File ./scripts/hygiene/Test-RepositoryHygiene.ps1; $LASTEXITCODE
# Gate 7  the Pester suite for the guard. Expected 0 failed after; non-zero failed before the rule functions exist.
Invoke-Pester -Path tests/scripts/hygiene -Output Detailed
# Gate 8  ignore rules live. Expected exit 0.
git check-ignore -q docs/features/x/a.trx docs/features/x/b.cobertura.xml; $LASTEXITCODE
# Gate 9  legacy tokens outside the governance directory and outside this feature folder. Baseline 88. Expected 0.
@(git grep -I -l -e redacted-account -e redacted-user -e REDACTED-HOST -- . ':!.claude/' ':!docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/').Count
# Gate 10 encoding survey of the write set (before), and eol parity (after).
git ls-files --eol -- <write-set> | Select-String 'i/-text'
```

Gate 5's third command is scoped to docs/features because class and line elements occur legitimately in other tracked XML; the guard itself is root-element gated and needs no such scoping. Gate 1 excludes the two MCP configuration files by pathspec because of the npm package scope; gates 2 to 4 exclude only the governance directory. All gates run before any change (baselines observed failing) and again after (expected values).

Manual validation steps (if required): after pushing, query the check runs on the pull request head and capture the exact hygiene context string; confirm every previously required context is also green; do not perform the ruleset PUT as part of this item.

Evidence artifacts (Markdown only; fixed names; feature-relative paths):

- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/baseline/identifier-and-raw-document-baseline.md` — gates one to five and nine before any change, the tracked-file total, and the encoding survey; counts only.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/regression-testing/guard-pre-sweep-run.md` — the guard over the pre-sweep tree; declares an expected exit code of one; records the findings total.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/regression-testing/pester-hygiene-fail-before.md` — the three test files run before the production files exist; declares an expected exit code of one; records the failed count.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/regression-testing/pester-hygiene-pass-after.md` — the same suite green; passed, failed, skipped totals.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/pester-coverage-projection.md` — package-level projection of the JaCoCo output with per-file line figures for the three new production files and the aggregate.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/raw-document-removal.md` — gate five and gate eight after the change; the count of documents removed by class and the count of projections retained.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/identifier-residual-scan.md` — gates one to four and nine after the change; scope statement naming the plan file and this folder as included; the parse check over rewritten XML-family files (expected none rewritten).
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/redaction-fidelity.md` — the eol parity listing before and after, the per-file "only substituted lines changed" check, and the second-run zero-substitution result.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/csharp-toolchain-pass.md` — the four C# commands with exit codes, the CoreCompile non-vacuity check, and the test-result summary derived from the trx (passed, failed, skipped, and the baseline passed total for comparison).
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/csharp-coverage-projection.md` — the package-level projection and the one-line first-party summary.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/powershell-toolchain-pass.md` — PoshQC format and analyze results for the final pass.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/guard-post-sweep-run.md` — gate six over the final tree; exit zero; findings zero.
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/ci-hygiene-context.md` — the check-runs query output for the pull request head, the captured hygiene context string, and the conclusion of every context; states that the ruleset was not modified.

## Acceptance Criteria

Note (2026-09-29, adjacent to AC1; not a criterion): AC1's profile-path reference figure was amended after the defect "Phase 0 backslash-collapse undercount" was found on the first execution. The P0-T17 UTF-16 census typed its .NET separator class with a doubled backslash that the Bash-to-pwsh channel collapsed to a single backslash, so the census matched only forward-slash paths, recorded zero UTF-16 profile-path files, and the baseline artifact recorded 1213 where the same population re-measured on the same tree with the intact pattern is 1214. The evidence is `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/regression-testing/guard-pre-sweep-run.md`, whose MEASUREMENT-CORRECTION record carries the re-measured figure AC1 now names. The equality stays exact; only its reference figure changed. The amendment was authorised by the run coordinator with the plan's revision 1.12; no other criterion changed.

Note (2026-09-30, adjacent to AC19; not a criterion): AC19's clause "the merge base with main is unchanged" was replaced by three ancestry checks with the plan's revision 1.15 (preflight round 11 finding F2, Option B, ruled by the run coordinator). The run directs the branch to merge origin/main before the Phase 6 checks run, and a merge of main necessarily moves the merge base, so equality with the Phase 0 merge base (177b6d78e, recorded in the baseline artifact p0-t2-tree-state) cannot hold on a correctly executed run. The three checks preserve the clause's intent (no history rewrite, no force push, the branch still contains its own starting point): the Phase 0 merge base is an ancestor of the current merge base with main; the branch's recorded base commit (cfbb2bd61, the P0-T2 BASE-SHA) is an ancestor of the branch head; and the pushed tip of the branch, read after a fetch at the time of the check, is an ancestor of the branch head. Each check exits zero, and a negative control (the branch head is not an ancestor of the Phase 0 merge base) exits one to show the check can fail; the evidence is the qa-gates artifact p6-t14-scope-containment. The scope-containment and no-force-push clauses are unchanged; no other criterion changed.

- [x] AC1 Regression observation, guard: before any removal or redaction, the guard run over the tracked tree exits one and its findings total equals the raw-document population recorded in the baseline artifact plus the profile-path file population (the baseline artifact's figure or, where the regression-testing artifact guard-pre-sweep-run records a MEASUREMENT-CORRECTION re-measuring that population on the same tree with the intact pattern, the re-measured figure); recorded in the regression-testing artifact guard-pre-sweep-run with an expected exit code of one, and the baseline counts (gates one to five and nine, the tracked-file total, the encoding survey) recorded as counts only in the baseline artifact identifier-and-raw-document-baseline (full paths in the Test Strategy evidence list).
- [x] AC2 Regression observation, Pester: the three new test files, run before the three production files exist, report a non-zero failed count and a non-zero exit; recorded in the regression-testing artifact pester-hygiene-fail-before with an expected exit code of one.
- [x] AC3 Pester green: with the production files present, the suite under the hygiene test folder reports zero failed and zero skipped, and every It name listed in the Test Strategy (nineteen rules tests, five git tests, six orchestration tests) is present and passes; recorded in the regression-testing artifact pester-hygiene-pass-after.
- [ ] AC4 Pester coverage: line coverage for each of the three new production files is at or above ninety percent, and the aggregate line figure over the three listed script folders is at or above eighty percent, measured with the pinned Pester version and the extended path arrays and recorded as a Markdown projection in the qa-gates artifact pester-coverage-projection.
- [x] AC5 Guard hygiene: none of the six new PowerShell files or the new callee workflow contains a contiguous drive-rooted profile path, the account name, the host name, the eight-dot-three form, or a token or path allowlist; every violating fixture is assembled by concatenation at run time; no test creates, writes or deletes a file on disk; each of the six PowerShell files is under the five-hundred-line ceiling; the guard reads no environment variable and no wall clock.
- [x] AC6 Guard output contract: every finding line consists of the marker, the rule name, the path and an optional line number and never the matched text, and the exit decision is one when the findings total is non-zero and zero otherwise; pinned by the orchestration tests "prints path and line only and never the matched text", "returns a non-zero exit decision when findings exist" and "returns a zero exit decision over clean content".
- [x] AC7 Raw documents removed: after the change, no tracked file has the trx, coverage or coveragexml extension; no tracked XML file has a root element of coverage, results, TestRun or CoverageSession; no tracked XML file under docs/features has a report root carrying class, sourcefile, method or line elements; all eighteen package-level projections that the baseline classified as jacoco-projection remain tracked; and the stray root-level run log is no longer tracked; recorded in the qa-gates artifact raw-document-removal.
- [x] AC8 Ignore rules: the ignore file carries a pattern for the trx extension and a pattern for cobertura-marked XML names, placed after the existing coverage patterns with a comment naming the content guard as the real protection; git check-ignore on a hypothetical path of each shape exits zero; the existing coverage and coveragexml patterns are unchanged.
- [x] AC9 Identifier gates: gate one (account and profile leaf, excluding the governance directory and the two MCP configuration files), gate two (host), gate three (eight-dot-three form) and gate four (generic profile path), the last three excluding only the governance directory, each return zero files over the whole tracked tree with the plan file and this feature folder included in scope; recorded as counts only in the qa-gates artifact identifier-residual-scan.
- [x] AC10 Legacy tokens normalised: gate nine returns zero files, that is, no tracked file outside the governance directory and outside this feature folder contains any of the three legacy redaction tokens named in the research record's legacy-token section; the canonical placeholders are used in their place.
- [x] AC11 Redaction fidelity: for every file the helper rewrote, the git eol classification is identical before and after, the byte-order mark is preserved or absent as before, the diff for that file touches only lines that carried a replaced identifier or legacy token, no XML-family file was rewritten (or, if one was, it re-parses), and a second run of the helper over the final tree performs zero substitutions and leaves the working tree unchanged; recorded in the qa-gates artifact redaction-fidelity.
- [x] AC12 Fixtures triaged: the thirteen C# test fixture files and the PowerShell helper test contain no drive-rooted profile path; the three Store tests no longer contain the three-letter real-account prefix and still contain the Google Workspace Sync and Google Apps Sync tokens; the NotContain assertions on testuser, OneDrive and fsAncestor are unchanged; the one research note's citations of the upstream checkout path use the user-profile placeholder with the repository suffix retained; and every test method in the thirteen rewritten C# test classes and the two named Pester tests in the helper test file pass.
- [ ] AC13 C# toolchain: the csharpier check exits zero; both msbuild Rebuild passes exit zero and their captured logs contain no line reporting the CoreCompile target as skipped; the MSTest-with-coverage route reports zero failed and a passed total not lower than the baseline passed total, with first-party line and branch coverage not below the pre-change figures; the evidence is the test-result summary in the qa-gates artifact csharp-toolchain-pass and the projection in the qa-gates artifact csharp-coverage-projection; the CI mstest-coverage context on the pull request head is the authoritative pass.
- [x] AC14 PowerShell toolchain: the PoshQC format pass makes no change on the final iteration and the PoshQC analyzer reports zero findings over the six new files and the modified helper test; recorded in the qa-gates artifact powershell-toolchain-pass.
- [x] AC15 Workflow wiring: the new callee declares workflow_call and workflow_dispatch, contents read permission, no concurrency block, and one job on the Ubuntu runner whose single pwsh step runs the guard with no exit-code reset; the CI orchestrator gains one uses-job for it with no needs edge and no inline steps; the Pester callee's Run.Path and CodeCoverage.Path arrays include the hygiene test and script folders; the workflow README table has a row for the callee, its Pester row names all three test folders, and its context list has the seventh entry marked predicted; the local actionlint run exits zero.
- [ ] AC16 CI outcome: on the pull request head, the check-runs query lists exactly one context whose name begins with the hygiene caller job id and the separator, its conclusion is success, every previously required context also reports success, and the captured string (not a typed one) is recorded in the qa-gates artifact ci-hygiene-context together with a statement that the branch ruleset was not modified by this change.
- [x] AC17 Guard green over the final tree: gate six exits zero with a findings total of zero, with this feature folder, the plan file and every evidence artifact included in the enumeration; recorded in the qa-gates artifact guard-post-sweep-run.
- [x] AC18 Evidence form: every artifact this item adds under its evidence tree is Markdown carrying the schema fields and placeholders only; the diff adds no file with an xml, trx or coverage extension anywhere in the repository; the helper's log and the raw tool outputs remain outside the tree.
- [x] AC19 Scope containment: the diff touches nothing under the governance directory, neither MCP configuration file, no solution, project, packages or app configuration file, no production (non-test) C# file, and no active feature folder of a sibling item of the same parallel run; three ancestry checks each exit zero (the phase-zero merge base recorded in the tree-state baseline is an ancestor of the current merge base with main; the branch's recorded base commit is an ancestor of the branch head; the pushed tip of the branch, read after a fetch at the time of the check, is an ancestor of the branch head), a negative control against a commit that is not an ancestor exits one, and no force push occurred.
- [x] AC20 Invariant and trace: the delivered guard matches the two traces in the Proposed Fix, in that a raw document record and a profile-path line each produce exactly one finding and a non-zero exit decision in the orchestration tests, and a package-level projection record produces none; the guard contains no exemption mechanism of any kind.

## Risks & Mitigations

- Technical or operational risks:
  - Sibling items of bugs-2026-09-28 commit evidence Markdown quoting absolute paths; once their branches update past main the guard is red on them. Mitigation: the orchestrator tells each sibling to run the guard (or gate four before this item merges) and to use the four placeholders; merge this item first, then the operator ruleset update, then siblings.
  - A future bare account or host token outside a path is not caught by the guard (Resolved tensions, item 2). Mitigation: recorded here; the profile-path rule covers the dominant leak shape (tool banners and absolute paths); any later leak is a new sweep item.
  - The helper mis-decodes a UTF-16 or byte-order-marked file. Mitigation: bytes-in bytes-out with byte-order-mark detection; the encoding survey in the baseline; the eol parity check in AC11.
  - The bare host token appears in a non-host context. Mitigation: the executor lists the distinct match contexts before applying rule five (research section 14, item 2).
  - CSharpier re-flows lines near the wrap width after the literal changes. Mitigation: the format step runs first and the loop restarts.
  - The Pester callee runs on the Windows runner while the guard runs on Ubuntu. Mitigation: the rule and parser functions operate on strings only; git ls-files output is separator-normalised on both.
  - The pull request diff is very large (625 deletions of multi-megabyte files and about 1,691 substituted files). Mitigation: the pull request body points reviewers at the gate artifacts rather than the diff.
  - A required-context gap between the ruleset update and sibling merges over-blocks siblings (fail-closed). Mitigation: the README procedure; keep the interval short.
- Mitigations and rollbacks: ordinary revert for the workflow and content changes; a ruleset PUT restoring the previous context set if the operator update has already been applied; removed documents recoverable from history.

## Rollout & Follow-up

- Release/rollout steps: one pull request, phased so the guard's tests are green before the guard is run over the tree (baselines; guard code and tests with the Pester callee arrays; removal and ignore rules; fixture rewrites with the C# toolchain; redaction and residual scan; callee, orchestrator and README wiring; push and confirm the context). If a split is forced for review size, the only safe cut is between the fixture phase and the redaction phase, and the first pull request must not wire the callee into the orchestrator (research section 10).
- Post-fix monitoring or clean-up tasks: operator follow-up, not part of this change: after the first green run, capture the hygiene context from the live head, and add it to the main ruleset with one atomic PUT per the workflow README procedure, then verify by GET. Confirm each sibling of bugs-2026-09-28 is green on the hygiene context before it merges. Companion upstream issue 932 handles the governance directory.
- Links: issue #927; consolidated items #602, #671, #884, #727 sub-finding 5; prior tooling #873 (PR #881); companion upstream issue 932; CI split #553 and Pester callee #869 for the workflow conventions.

## Write Set

Created:

- `scripts/hygiene/Test-RepositoryHygiene.ps1`
- `scripts/hygiene/Test-RepositoryHygiene.Rules.ps1`
- `scripts/hygiene/Test-RepositoryHygiene.Git.ps1`
- `tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1`
- `tests/scripts/hygiene/Test-RepositoryHygiene.Git.Tests.ps1`
- `tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1`
- `.github/workflows/_hygiene.yml`

Modified:

- `.github/workflows/ci.yml`
- `.github/workflows/_pester.yml`
- `.github/workflows/README.md`
- `.gitignore`
- `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs`
- `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs`
- `TaskMaster.Test/AppGlobals/AppAutoFileObjectsFolderPredictorTests.cs`
- `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs`
- `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`
- `UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs`
- `UtilitiesCS.Test/EmailIntelligence/LcppnFolderPredictorStore_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterIssue614Tests.cs`
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`
- `docs/research/2026-08-10-parallel-bug-flighting-and-surface-blockers.md`
- `docs/features/potential/promoted/2026-08-10-mstest-coverage-discovery-claude-worktree-exclusion.md`
- `docs/features/potential/promoted/2026-08-11-research-doc-cohort-library-false-negative.md`
- `docs/features/potential/promoted/2026-08-14-orchestrator-hooks-reference-absent-python-validators.md`
- `docs/features/potential/promoted/2026-08-14-potential-to-issue-promoted-copy-not-written.md`
- `docs/features/potential/promoted/2026-08-29-parallel-run-merge-gate-misparses-pr-number.md`
- `docs/features/potential/promoted/2026-09-02-committed-host-identity-leaks.md`
- `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/**`

Deleted:

- `test-output.txt`
- the 625 raw documents inside the feature-folder scopes below (content-classified list produced at execution time)

Feature-folder scopes (redaction and removal; files affected, of which raw documents; copied from the inventory):

- `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/**` (1, 0)
- `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/**` (1, 0)
- `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/**` (9, 6)
- `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/**` (9, 0)
- `docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/**` (1, 0)
- `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/**` (6, 0)
- `docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/**` (2, 0)
- `docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437/**` (7, 0)
- `docs/features/active/2026-08-07-quickfiler-helper-classes-coverage-434/**` (1, 0)
- `docs/features/active/2026-08-07-quickfiler-item-controller-coverage-453/**` (12, 0)
- `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/**` (10, 0)
- `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/**` (6, 0)
- `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/**` (10, 0)
- `docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/**` (6, 0)
- `docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/**` (3, 0)
- `docs/features/active/2026-08-21-winformspumphost-suite-determinism-511/**` (1, 0)
- `docs/features/active/2026-08-24-breadcrumb-coordinator-hub-defects-501/**` (20, 20)
- `docs/features/active/2026-08-24-breadcrumb-router-navigation-defects-498/**` (35, 34)
- `docs/features/active/2026-08-24-qfc-collection-controller-defects-468/**` (52, 51)
- `docs/features/active/2026-08-24-qfc-item-controller-defects-484/**` (1, 0)
- `docs/features/active/2026-08-24-quickfiler-bug-family-446/**` (37, 36)
- `docs/features/active/2026-08-24-quickfiler-keyboard-action-defects-444/**` (1, 1)
- `docs/features/active/2026-08-24-webview2-host-initializer-defects-476/**` (20, 19)
- `docs/features/active/2026-08-25-efc-controller-surface-defects-464/**` (23, 22)
- `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/**` (6, 6)
- `docs/features/active/2026-08-25-itemviewer-breadcrumb-lifecycle-defects-488/**` (22, 21)
- `docs/features/active/2026-08-25-itemviewer-surface-defects-489/**` (25, 24)
- `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/**` (25, 11)
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/**` (11, 0)
- `docs/features/active/2026-08-26-qfc-remove-stackmoveditems-parameter-629/**` (8, 2)
- `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/**` (8, 8)
- `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/**` (7, 6)
- `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/**` (6, 6)
- `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/**` (12, 12)
- `docs/features/active/2026-08-31-efcselectionguard-banner-prefix-arity-and-stale-comment-662/**` (9, 8)
- `docs/features/active/2026-08-31-narrow-fileio2-retryable-exception-set-707/**` (2, 0)
- `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/**` (2, 0)
- `docs/features/active/2026-09-02-claude-md-cites-ciyml-for-moved-toolchain-commands-564/**` (1, 0)
- `docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/**` (3, 3)
- `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/**` (24, 18)
- `docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/**` (13, 2)
- `docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565/**` (3, 2)
- `docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/**` (3, 0)
- `docs/features/active/2026-09-02-quickfiler-session-metrics-twelve-hour-time-format-645/**` (21, 2)
- `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/**` (13, 11)
- `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/**` (3, 2)
- `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/**` (3, 3)
- `docs/features/active/2026-09-03-terminal-notification-hook-test-lacks-sync-barrier-751/**` (1, 0)
- `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/**` (5, 0)
- `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/**` (1, 0)
- `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/**` (1, 0)
- `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/**` (2, 2)
- `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/**` (1, 0)
- `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/**` (2, 0)
- `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/**` (6, 5)
- `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/**` (1, 0)
- `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/**` (2, 2)
- `docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/**` (4, 2)
- `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/**` (2, 0)
- `docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/**` (2, 2)
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/**` (1, 0)
- `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/**` (3, 2)
- `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/**` (3, 0)
- `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/**` (17, 0)
- `docs/features/archive/2026-03-13-utilities-coverage-65/**` (4, 0)
- `docs/features/archive/2026-03-19-outlook-folder-wrapper-tests-82/**` (2, 0)
- `docs/features/archive/2026-03-19-utilities-coverage-part-three-87/**` (42, 0)
- `docs/features/archive/2026-03-25-getmovediagnostics-null-guard-97/**` (2, 0)
- `docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/**` (8, 0)
- `docs/features/archive/2026-03-26-conversation-info-updateui-ordering-103/**` (4, 0)
- `docs/features/archive/2026-03-27-qfc-queue-remove-item-cancellation-106/**` (1, 0)
- `docs/features/archive/2026-03-27-quickfiler-navigation-key-collision-111/**` (4, 0)
- `docs/features/archive/2026-04-05-select-junk-folders-119/**` (1, 0)
- `docs/features/archive/2026-04-08-outlook-recipient-com-cross-thread-crash-124/**` (6, 0)
- `docs/features/archive/2026-04-13-outlook-com-sta-materialization-128/**` (7, 0)
- `docs/features/archive/2026-04-14-bayesian-staging-asynclazy-null-guard-131/**` (13, 0)
- `docs/features/archive/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/**` (4, 2)
- `docs/features/archive/2026-04-21-triage-trains-entire-conversation-137/**` (2, 0)
- `docs/features/archive/2026-05-05-outlook-startup-ui-thread-deblock-141/**` (13, 0)
- `docs/features/archive/2026-05-07-outlook-startup-ui-lockup-followup-148/**` (23, 0)
- `docs/features/archive/2026-05-14-ci-format-and-vs-test-failures-155/**` (3, 0)
- `docs/features/archive/2026-05-26-actionable-classifier-not-serialized-164/**` (8, 0)
- `docs/features/archive/2026-05-27-worktrees-missing-claude-dir-166/**` (5, 0)
- `docs/features/archive/2026-06-01-quickfiler-high-confidence-filter-169/**` (11, 0)
- `docs/features/archive/2026-06-02-quickfiler-high-confidence-prefilter-171/**` (5, 0)
- `docs/features/archive/2026-06-08-ci-flaky-test-isolation-176/**` (8, 8)
- `docs/features/archive/2026-06-08-csharp-analyzer-stack-hardening-181/**` (26, 12)
- `docs/features/archive/2026-06-08-hierarchical-lcppn-folder-prediction-177/**` (9, 4)
- `docs/features/archive/2026-06-10-triage-multiselect-only-first-183/**` (4, 3)
- `docs/features/archive/2026-06-12-global-json-sdk-pin-regressed-to-10-194/**` (2, 0)
- `docs/features/archive/2026-06-12-taskmaster-ribbon-tab-185/**` (3, 0)
- `docs/features/archive/2026-06-12-timeout-task-flaky-timing-191/**` (4, 1)
- `docs/features/archive/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/**` (6, 0)
- `docs/features/archive/2026-06-12-vscode-test-runner-parity-188/**` (5, 0)
- `docs/features/archive/2026-06-14-coverage-increments-1-3-testable-seams-199/**` (2, 0)
- `docs/features/archive/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/**` (8, 7)
- `docs/features/archive/2026-06-19-log4net-startup-log-directory-not-created-208/**` (3, 2)
- `docs/features/archive/2026-06-19-tesseract-engine-initialization-failure-209/**` (5, 4)
- `docs/features/archive/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/**` (13, 10)
- `docs/features/archive/2026-06-24-folder-tree-cache-and-refresh-214/**` (27, 13)
- `docs/features/archive/2026-06-26-qfc-high-confidence-queue-filter-218/**` (9, 3)
- `docs/features/archive/2026-06-28-non-deterministic-createasync-task-wait-tests-219/**` (1, 0)
- `docs/features/archive/2026-06-28-qfc-banned-api-time-delay-seams-222/**` (2, 0)
- `docs/features/archive/2026-06-28-qfc-form-viewer-testability-223/**` (4, 0)
- `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/**` (11, 2)
- `docs/features/archive/2026-06-30-emailmovemonitor-cross-thread-com-228/**` (1, 0)
- `docs/features/archive/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/**` (26, 10)
- `docs/features/archive/2026-07-03-quickfiler-navigation-key-collision-232/**` (9, 1)
- `docs/features/archive/2026-07-04-coverage-gaps-test-seams-236/**` (47, 18)
- `docs/features/archive/2026-07-06-app-events-readiness-comexception-242/**` (11, 0)
- `docs/features/archive/2026-07-06-appevents-loadasync-inbox-gating-243/**` (4, 4)
- `docs/features/archive/2026-07-06-bayesian-email-sorter-unit-tests-248/**` (4, 0)
- `docs/features/archive/2026-07-06-qfc-high-confidence-empty-batch-crash-244/**` (1, 0)
- `docs/features/archive/2026-07-06-quickfiler-darkmode-stale-subscription-251/**` (5, 2)
- `docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/**` (3, 0)
- `docs/features/archive/2026-07-07-disabled-stores-settings-ui-265/**` (2, 0)
- `docs/features/archive/2026-07-07-folder-settings-store-model-null-262/**` (2, 0)
- `docs/features/archive/2026-07-07-onedrive-writer-timeout-test-determinism-253/**` (1, 0)
- `docs/features/archive/2026-07-07-outlook-crash-async-void-sectiongroupname-270/**` (2, 0)
- `docs/features/archive/2026-07-07-store-disable-service-261/**` (4, 0)
- `docs/features/archive/2026-07-07-store-lockup-detect-notify-264/**` (2, 0)
- `docs/features/archive/2026-07-07-store-runtime-reenable-263/**` (2, 0)
- `docs/features/archive/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/**` (1, 0)
- `docs/features/archive/2026-07-08-liveoutlook-harness-construction-scoped-skip-283/**` (4, 0)
- `docs/features/archive/2026-07-09-tagcontroller-testability-refactor-293/**` (1, 0)
- `docs/features/archive/2026-07-09-taskvisualization-core-testability-refactor-297/**` (1, 0)
- `docs/features/archive/2026-07-09-taskvisualization-secondary-testability-298/**` (10, 0)
- `docs/features/archive/2026-07-10-swordfish-collection-stack-lineage-307/**` (2, 0)
- `docs/features/archive/2026-07-10-swordfish-dictionary-lineage-306/**` (4, 0)
- `docs/features/archive/2026-07-10-swordfish-interface-project-teardown-308/**` (2, 0)
- `docs/features/archive/2026-07-10-swordfish-raw-usage-cleanup-310/**` (3, 2)
- `docs/features/archive/2026-07-10-swordfish-scosorteddictionary-removal-309/**` (6, 2)
- `docs/features/archive/2026-07-11-collection-lock-recursion-coverage-317/**` (5, 0)
- `docs/features/archive/2026-07-11-legacy-scodictionary-removal-315/**` (7, 0)
- `docs/features/archive/2026-07-12-people-tag-window-autotag-322/**` (3, 2)
- `docs/features/archive/2026-07-15-efcviewer-folder-tree-percentage-327/**` (2, 0)
- `docs/features/archive/2026-07-15-folder-probability-plumbing-324/**` (1, 0)
- `docs/features/archive/2026-07-15-outlook-store-exclusion-328/**` (2, 2)
- `docs/features/archive/2026-07-15-quickfiler-folder-tree-percentage-325/**` (2, 0)
- `docs/features/archive/2026-07-15-quickfiler-inline-image-cid-fix-326/**` (6, 0)
- `docs/features/archive/2026-07-16-dependabot-net481-support-340/**` (1, 0)
- `docs/features/archive/2026-07-16-efcviewer-breadcrumb-webview2-349/**` (4, 0)
- `docs/features/archive/2026-07-16-folder-hierarchy-live-provider-350/**` (4, 0)
- `docs/features/archive/2026-07-16-progress-viewer-cancel-button-339/**` (7, 3)
- `docs/features/archive/2026-07-16-quickfiler-breadcrumb-webview2-351/**` (9, 0)
- `docs/features/archive/2026-07-18-stale-app-config-binding-redirects-354/**` (8, 0)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-dialogs-misc-374/**` (9, 8)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-email-classifier-372/**` (9, 9)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-email-parsing-370/**` (10, 9)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-extensions-363/**` (8, 7)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-helperclasses-364/**` (2, 2)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/**` (2, 2)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-outlook-folder-store-365/**` (2, 0)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-outlook-mailitem-item-371/**` (3, 3)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-residuals-375/**` (3, 2)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-reusabletypes-366/**` (5, 4)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-svgcontrol-368/**` (8, 7)
- `docs/features/archive/2026-07-18-utilitiescs-nullable-threading-369/**` (1, 0)
- `docs/features/archive/2026-07-19-utilitiescs-nullable-ci-capstone-376/**` (4, 2)
- `docs/features/archive/2026-07-20-breadcrumb-suggestions-upgrade-race-398/**` (3, 0)
- `docs/features/archive/2026-07-20-folder-combobox-fallback-index-out-of-range-392/**` (1, 0)
- `docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/**` (185, 65)
- `docs/features/archive/2026-08-04-folder-tree-dispatcher-thread-affinity-420/**` (9, 8)
- `docs/features/archive/2026-08-04-svg-renderer-null-document-nre-418/**` (30, 0)
- `docs/features/archive/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/**` (6, 2)
- `docs/features/archive/2026-08-07-quickfiler-explorer-controller-latent-defects-449/**` (44, 0)
- `docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/**` (14, 0)
- `docs/features/archive/2026-08-07-quickfiler-search-keystroke-focus-steal-438/**` (4, 4)
- `docs/features/archive/2026-08-07-quickfiler-test-form1-live-form-491/**` (3, 2)
- `docs/features/archive/2026-08-07-winforms-message-pump-test-seam-230/**` (3, 2)
- `docs/features/archive/2026-08-08-ribbon-controller-engines-null-unsafe-507/**` (8, 0)
- `docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/**` (73, 0)
- `docs/features/archive/2026-08-08-ribbon-engine-toggle-state-guards-505/**` (9, 0)
- `docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/**` (12, 0)
- `docs/features/archive/2026-08-10-cobertura-coverage-arithmetic-441/**` (19, 5)
- `docs/features/archive/2026-08-10-coverage-threshold-policy-reconciliation-494/**` (31, 10)
- `docs/features/archive/2026-08-10-csharp-toolchain-gate-fidelity-512/**` (17, 0)
- `docs/features/archive/2026-08-10-excludefromcodecoverage-nested-lambdas-457/**` (22, 4)
- `docs/features/archive/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/**` (3, 0)
- `docs/features/archive/2026-08-14-ci-parallel-job-split-553/**` (4, 0)
- `docs/features/epics/build-ci-coverage-gate-fidelity/**` (1, 0)
- `docs/features/epics/quickfiler-bug-family/**` (1, 0)
- `docs/features/epics/review-residuals-2026-09-08/**` (1, 0)
- `docs/features/parallel/bugs-2026-09-17/**` (1, 0)

Write Set notes: the ToDoModel.Test fixture path contains a space in its directory name and is therefore not harvestable by the footprint tool; it is listed above for completeness and the planner must carry it explicitly in the declared blast radius. Not in the Write Set, deliberately: anything under .claude, .mcp.json, .codex/config.toml, TaskMaster.sln, any csproj, packages.config, app.config, and the sibling active feature folders of bugs-2026-09-28.
