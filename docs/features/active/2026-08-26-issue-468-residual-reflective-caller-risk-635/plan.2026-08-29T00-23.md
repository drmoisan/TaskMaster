# 2026-08-26-issue-468-residual-reflective-caller-risk (Plan)

- **Issue:** #635
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29T04-55
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug
- **Requirements source:** `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md`, `## Acceptance Criteria`, AC-1 through AC-15.

## Preamble

**No production or test source file is modified by this plan.** This item is an evidence-producing
audit. Its entire change set is Markdown under
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/`. Any change to a
file with a `.cs`, `.csproj`, `.props`, `.targets`, `.resx`, `.config`, `.settings`, `.xaml`, or
`.ps1` extension is out of scope and is a defect in the execution of this plan. The QuickFiler
production tree and the QuickFiler test tree are read and searched only; they are never written.

**The thirteen identifiers**, in the order the specification's Context table lists them, which is not
the order in which the removal commit's diff declares them:
`WireUpKeyboardHandler`, `AnyOpenDropDownsAsync`, `LoadGroups_02cAsync`, `LoadGroups_02bAsync`,
`LoadGroup_03bAsync`, `LoadConversationsAndFoldersAsync`, `LoadItemGroup`, `LoadSequentialAsync`,
`LoadGroupSequential`, `CacheTlpForMove`, `SwapTlp`, `CaptureTlpTemplate`, `_templateTlp`.

**Filename-stamp convention.** Every artifact filename in this plan carries the single fixed stamp
`2026-08-29T04-55`. That stamp is the planning stamp. Each artifact's own `Timestamp:` field records
the artifact's actual execution time, which will differ from the filename stamp. The difference is
this stated convention, not an inconsistency, and no artifact filename is to be renamed to match its
`Timestamp:` value.

**Evidence location.** Every artifact is written under
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/` in one of
`baseline/`, `other/`, `regression-testing/`, or `qa-gates/`. No other location is permitted.

**Artifact schema.** Each command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, and
`Output Summary:`. Where the expected exit code is non-zero the artifact additionally carries
`ExpectedExitCode:` with that value. `ExpectedExitCode:` is per file, so a gate expecting a non-zero
code lives in its own artifact file and is not combined with a gate expecting zero.

**No helper script files.** Do not create any `.ps1`, `.py`, `.sh`, or `.cs` file anywhere under the
evidence subtree or anywhere else in this repository for this item. Every command belongs inline in
the artifact body. A retained script under the evidence subtree is matched by extension alone by the
downstream review agent and produces a spurious coverage failure.

**Host-identity hygiene.** No artifact contains an absolute host path, a user account name, or a
machine name. Refer to the repository root generically.

**Executor tool constraint.** Every command in this plan leads with `git` or `pwsh`. Bare `grep`,
`rg`, `find`, `wc`, `sed`, `awk`, `cat`, and `ls` are not available to the executor. Counting and
filtering are done in `pwsh`, never by piping `git` output into `wc` or `grep`.

**PowerShell quoting form.** Every `pwsh -NoProfile -Command` invocation in this plan is written with
a bash **single-quoted** outer string and PowerShell **double-quoted** inner strings. The
double-quoted outer form is not usable: a `$` inside it is expanded by bash before `pwsh` is reached,
and the worktree-isolation guard refuses the invocation outright. Record the command in its
single-quoted form verbatim in the artifact.

**Exit-code handling.** A bare `git grep` that selects no line exits `1`. The one task whose success
is a zero-hit result declares `ExpectedExitCode: 1` in its artifact; an artifact that omits that
expectation records a correct zero-hit run as a failure. A `pwsh -NoProfile -Command` wrapper exits
`0` regardless of the exit code of any command inside it unless the command string re-raises it, so
every `pwsh` task in this plan asserts the printed value and never the wrapper's exit code.

**No repository-wide zero-hit condition is written anywhere in this plan.** `LoadSequentialAsync`
names three live and unrelated members in the TaskMaster startup assembly that must keep working,
and the docs tree and the .claude tree quote every one of the identifiers thousands of times. The
acceptance shape used throughout is a total classification with one empty class: every hit is
assigned to exactly one category, the per-category counts sum to the recorded total, and the category
"genuine name-based caller of a removed member" is empty. Only Partition A carries a zero-hit
assertion, and only because its pathspec excludes both prose trees.

**The production tree is not free of `System.Reflection`.** It carries occurrences that are the
log4net logger-declaration idiom calling `MethodBase.GetCurrentMethod`, `using` directives, comments,
and — because the production pathspec also reaches tracked non-`.cs` files — project-file assembly
references and package-manifest entries. None of them takes a member-name argument, so none can
resolve a member by name. No task in this plan asserts that the production tree has zero
`System.Reflection` occurrences; the assertion is that it has zero name-based member-lookup call
sites, and the inventory records the `System.Reflection` occurrences with their classification so the
distinction is visible rather than hidden.

**No coverage task appears in this plan, by design.** Nothing executable changes, so there is no
coverage delta to measure, and emitting a repository coverage artifact from this item would trip an
unrelated downstream threshold check. No unit-test task appears either: the Phase 3 fail-before
exception dossier stands in for the normally-required failing regression test, because no
reproducible defect exists. `spec.md` records that the applicable toolchain gates follow the branch
diff's language composition, which makes the Phase 4 conditional gate an approved conditional rather
than an unauthorized skip.

**Path-citation convention in this document.** A file this plan creates or modifies appears as a
backtick-delimited full repository-relative path. A file the plan only reads or searches is named in
bare prose without backticks, so the downstream blast-radius extractor does not mark it a write
target. Pathspecs inside fenced command blocks are unavoidable and are not read as write targets.

**Fail-closed evidence rule.** If any required baseline artifact, evidence artifact, or QA artifact
is missing or has incomplete fields, the outcome is BLOCKED or INCOMPLETE, never PASS. Do not mark a
plan task complete without its artifact.

---

### Phase 0 — Policy reads and baseline capture

- [x] [P0-T1] Read the repository policy files in the mandated order and record the read in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/phase0-instructions-read.2026-08-29T04-55.md`.
  - Read, in this order: CLAUDE.md at the repository root; the general-code-change rule file, the general-unit-test rule file, the quality-tiers rule file, the tonality rule file, and the csharp rule file, all under the .claude rules directory; and the atomic-plan-contract, evidence-and-timestamp-conventions, and acceptance-criteria-tracking skill files under the .claude skills directory.
  - Acceptance: the artifact carries `Timestamp:`, a `Policy Order:` line, and an explicit list naming all nine files read, one per line.

- [x] [P0-T2] Record the requirements inputs for this item in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t2-requirements-inputs.2026-08-29T04-55.md`, reading `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` in full, then the issue document and the reflective-caller-closure research document that sit beside it in the same feature folder, also in full.
  - Acceptance: the artifact carries `Timestamp:` and the three single-line tokens `AC_COUNT: 15`, `WORK_MODE: full-bug`, and `AC_SOURCE: spec.md`, and lists the thirteen identifiers in the preamble order given in this plan, one per line, numbered 1 through 13.

- [x] [P0-T3] Capture the worktree baseline in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t3-worktree-baseline.2026-08-29T04-55.md`.
  ```
  git rev-parse HEAD
  git status --porcelain
  ```
  - Acceptance: the artifact records the printed HEAD object name verbatim and the verbatim porcelain output, and states explicitly `(no output)` where the porcelain output is empty. No path with a `.cs`, `.csproj`, `.props`, `.targets`, `.resx`, `.config`, `.settings`, `.xaml`, or `.ps1` extension appears in the porcelain output; if one does, stop and report a dirty-baseline blocker.
  - The recorded HEAD object name is recorded, not asserted against any fixed value. Do not write a fixed commit identifier as a plan expectation for HEAD.

- [x] [P0-T4] Derive the thirteen-identifier search set at commit level from `63eebd47` and record it in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t4-identifier-derivation.2026-08-29T04-55.md`. Discharges AC-1.
  ```
  git show --stat 63eebd47
  git show 63eebd47 -- QuickFiler/Controllers/QfcCollectionController.cs
  ```
  - Acceptance: the artifact carries a thirteen-row table, one row per identifier in the preamble order given in this plan, each row naming the identifier and quoting the removed-line text from the second command's diff that declares it; twelve rows are method declarations and the row for `_templateTlp` is a field declaration. The artifact states `IDENTIFIER_ROWS: 13`.
  - If `63eebd47` does not resolve in this worktree, record `BLOCKER: removal commit unresolved` in the artifact, leave the task unchecked, and continue with the remaining phases; do not halt the plan.

- [x] [P0-T5] Measure the search scope and the tracked-file census and record them in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t5-scope-census.2026-08-29T04-55.md`. Discharges AC-3.
  ```
  pwsh -NoProfile -Command '$f = git ls-files -- ":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"; Write-Output ("SCOPE_FILES=" + $f.Count); $f | Group-Object { [System.IO.Path]::GetExtension($_) } | Sort-Object Count -Descending | Select-Object -First 12 | ForEach-Object { Write-Output ((($_.Name -replace "^$","(none)")) + " " + $_.Count) }'
  ```
  ```
  pwsh -NoProfile -Command 'Write-Output ("TRACKED_TOTAL=" + (git ls-files).Count); Write-Output ("TRACKED_CS=" + (git ls-files -- "*.cs").Count); Write-Output ("TRACKED_NON_CS=" + (git ls-files -- ":(exclude)*.cs").Count); Write-Output ("AC16_SIX_EXTENSION_SCOPE=" + (git ls-files -- "*.csproj" "*.resx" "*.config" "*.xaml" "*.json" "*.settings" ":(exclude)docs/*" ":(exclude).claude/*").Count)'
  ```
  - Acceptance: the artifact records both commands verbatim with their full printed output, and the first command's printed first line is `SCOPE_FILES=683`. The printed extension census carries twelve rows and includes the line `.md 190`. The artifact records `TRACKED_TOTAL`, `TRACKED_CS`, and `TRACKED_NON_CS` verbatim as printed, and records `AC16_SIX_EXTENSION_SCOPE` verbatim together with a computed `WIDENING_DELTA` line equal to the printed `SCOPE_FILES` value minus the printed `AC16_SIX_EXTENSION_SCOPE` value. The printed `AC16_SIX_EXTENSION_SCOPE` value is greater than zero and less than the printed `SCOPE_FILES` value.
  - Expected values at the base commit, recorded for reference and not asserted except where stated above: `TRACKED_TOTAL=11866`, `TRACKED_CS=1599`, `TRACKED_NON_CS=10267`, `AC16_SIX_EXTENSION_SCOPE=153`, `WIDENING_DELTA=530`. The first command's expected census rows are `.md 190`, `.toml 96`, `.svg 77`, `.resx 62`, `.ps1 51`, `.config 38`, `.png 28`, `.json 28`, `.csproj 18`, `.bak 11`, `.txt 9`, `.sh 9`.
  - Assert the printed values only. Do not assert the exit code of either `pwsh` wrapper: the wrapper exits `0` regardless of what runs inside it.
  - `SCOPE_FILES` and `AC16_SIX_EXTENSION_SCOPE` are both measured over a pathspec that excludes the docs tree and the .claude tree, so neither value can be moved by this item's own artifact writes.

- [x] [P0-T6] Check off AC-1 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P0-T4 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-1 criterion text is unchanged.

- [x] [P0-T7] Check off AC-3 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P0-T5 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-3 criterion text is unchanged.

---

### Phase 1 — Identifier sweep partitions and the untracked pass

- [x] [P1-T1] Run the Partition A sweep over tracked non-source files outside the docs tree and the .claude tree and record it in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t1-partition-a-sweep.2026-08-29T04-55.md`. Discharges AC-2.
  ```
  git grep -n -I -F -e WireUpKeyboardHandler -e AnyOpenDropDownsAsync -e LoadGroups_02cAsync -e LoadGroups_02bAsync -e LoadGroup_03bAsync -e LoadConversationsAndFoldersAsync -e LoadItemGroup -e LoadSequentialAsync -e LoadGroupSequential -e CacheTlpForMove -e SwapTlp -e CaptureTlpTemplate -e _templateTlp -- ":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"
  ```
  - Acceptance: the artifact carries `ExpectedExitCode: 1` and `EXIT_CODE: 1`, records the command verbatim, and records the output as the literal line `(no output)`. `git grep` exits `1` when it selects no line, so an artifact that omits the expectation records this pass as a failure.
  - This artifact contains this one gate only. Do not combine it with any gate whose expected exit code is `0`.
  - Acceptance also requires the artifact to carry `SearchScope:`, `SearchPatterns:`, and `SearchResult:` lines, with `SearchScope:` naming the pathspec and citing the `SCOPE_FILES` value recorded by P0-T5, so the zero result is auditable and demonstrably non-vacuous.

- [x] [P1-T2] Run the Partition A non-vacuity control and record it in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t2-partition-a-control.2026-08-29T04-55.md`.
  ```
  git grep -n -I -F -e QfcCollectionController -- ":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"
  ```
  - Acceptance: the artifact carries `EXIT_CODE: 0` and enumerates the printed hits one row per line. The artifact states `CONTROL_HITS: 13` and `CONTROL_FILES: 4`, and the four files are the QuickFiler project file with 2 hits, its tracked backup project file with 2 hits, the QuickFiler test project file with 8 hits, and an extensionless tracked notes file under the QuickFiler production tree with 1 hit. The per-file counts sum to 13.
  - Acceptance also requires the artifact to state, in one sentence, why the extensionless file is the decisive element of this control: it is a file type that the six build-input extensions of the earlier AC-16 search could never reach, so it proves the widened pathspec reaches real content the narrower scope did not.
  - This control is the non-vacuity proof for P1-T1: the same pathspec that returns nothing for the thirteen identifiers returns thirteen hits for a token that is genuinely present.

- [x] [P1-T3] Run the Partition B sweep including the docs tree and the .claude tree, classify every hit by path prefix, and record it in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t3-partition-b-classification.2026-08-29T04-55.md`. Discharges AC-4 and AC-5.
  ```
  pwsh -NoProfile -Command '$h = git grep -n -I -F -e WireUpKeyboardHandler -e AnyOpenDropDownsAsync -e LoadGroups_02cAsync -e LoadGroups_02bAsync -e LoadGroup_03bAsync -e LoadConversationsAndFoldersAsync -e LoadItemGroup -e LoadSequentialAsync -e LoadGroupSequential -e CacheTlpForMove -e SwapTlp -e CaptureTlpTemplate -e _templateTlp -- ":(exclude)*.cs"; Write-Output ("TOTAL=" + $h.Count); Write-Output ("CAT_D_DOCS=" + @($h | Where-Object { $_ -like "docs/*" }).Count); Write-Output ("CAT_E_CLAUDE=" + @($h | Where-Object { $_ -like ".claude/*" }).Count); Write-Output ("CAT_G_OTHER=" + @($h | Where-Object { -not ($_ -like "docs/*") -and -not ($_ -like ".claude/*") }).Count)'
  ```
  - Acceptance: the artifact records the command verbatim and the four printed values verbatim, and asserts both of the following arithmetic identities against the printed numbers: the printed `CAT_D_DOCS` value plus the printed `CAT_E_CLAUDE` value equals the printed `TOTAL` value, and the printed `CAT_G_OTHER` value is `0`.
  - Acceptance also requires the artifact to state the mechanical test by which each hit is assigned its category, in exactly these terms: category D is any hit whose path begins docs/ ; category E is any hit whose path begins .claude/ ; category G, "genuine name-based caller of a removed member", is any hit matched by neither, and it must be empty. The tests are applied in that order and are derived from the path alone, with no reading of hit text.
  - Acceptance also requires the artifact to state that the printed `CAT_G_OTHER` value of `0` is the same population that P1-T1 measured directly, so the two tasks corroborate each other by independent routes.
  - Do not assert a fixed value for `TOTAL`. The base-commit measurement was 2,229 hits, of which 2,216 are under the docs tree and 13 under the .claude tree. `git grep` searches tracked files only, so this plan file and this item's evidence artifacts are outside this sweep's search set at the moment this task runs: they are untracked until P4-T1, which runs in Phase 4. The value can still move, because the agent-memory tree beneath the .claude directory is tracked and is written by the agents executing this plan. Record the printed value and this reason in the artifact.
  - Assert the printed values only. Do not assert the exit code of the `pwsh` wrapper.

- [x] [P1-T4] Run the Partition C sweep over tracked source files, enumerate every hit, and record it in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t4-partition-c-enumeration.2026-08-29T04-55.md`. Discharges AC-6.
  ```
  git grep -n -I -F -e WireUpKeyboardHandler -e AnyOpenDropDownsAsync -e LoadGroups_02cAsync -e LoadGroups_02bAsync -e LoadGroup_03bAsync -e LoadConversationsAndFoldersAsync -e LoadItemGroup -e LoadSequentialAsync -e LoadGroupSequential -e CacheTlpForMove -e SwapTlp -e CaptureTlpTemplate -e _templateTlp -- "*.cs"
  ```
  - Acceptance: the artifact carries `EXIT_CODE: 0`, states `PARTITION_C_HITS: 31`, and carries exactly 31 enumerated rows, one per printed line, each row naming the file, the line number, the matched identifier, and the assigned category.
  - Acceptance also requires the category counts to be `CAT_A: 2`, `CAT_B: 28`, `CAT_C: 1`, `CAT_G: 0`, summing to 31, under these mechanical tests applied in order: category A is a hit whose path ends Controllers/QfcCollectionController.cs, which at base is the two lines of the live preserved member LoadItemGroupsAndViewers_02 matched only because the bare stem `LoadItemGroup` is a strict prefix of it; category B is a hit under the TaskMaster, TaskMaster.Test, or UtilitiesCS trees whose matched identifier is `LoadSequentialAsync`, which at base is 28 lines naming three live and unrelated members in the TaskMaster startup assembly together with their tests and doc comments; category C is a hit whose line's first non-whitespace token is `//` or `///`, which at base is the one triple-slash documentation comment in the QuickFiler test tree naming `WireUpKeyboardHandler`; category G is any hit matched by none of the above and must be empty.
  - Acceptance also requires the artifact to state that no string literal anywhere in the QuickFiler test tree equals one of the thirteen identifiers, and to name the single QuickFiler test-tree hit (the category C row) as the sole occurrence of any of the thirteen in that tree. That statement is the input the Phase 2 closure argument consumes.
  - The 31 figure is stable for this plan's execution because this plan writes no file with a `.cs` extension.

- [x] [P1-T5] Run the supplementary pass over untracked, unignored files and record it in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t5-untracked-pass.2026-08-29T04-55.md`. Discharges AC-7.
  ```
  pwsh -NoProfile -Command '$f = git ls-files --others --exclude-standard; Write-Output ("UNTRACKED_FILES=" + $f.Count); $f | ForEach-Object { Write-Output ("FILE " + $_) }; $outside = 0; foreach ($p in $f) { if (Test-Path -LiteralPath $p -PathType Leaf) { $m = @(Select-String -LiteralPath $p -SimpleMatch -Pattern "WireUpKeyboardHandler","AnyOpenDropDownsAsync","LoadGroups_02cAsync","LoadGroups_02bAsync","LoadGroup_03bAsync","LoadConversationsAndFoldersAsync","LoadItemGroup","LoadSequentialAsync","LoadGroupSequential","CacheTlpForMove","SwapTlp","CaptureTlpTemplate","_templateTlp" -ErrorAction SilentlyContinue); if ($m.Count -gt 0) { Write-Output ("HIT " + $p + " " + $m.Count); if (-not $p.StartsWith("docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/") -and -not $p.StartsWith(".claude/")) { $outside = $outside + 1 } } } }; Write-Output ("UNTRACKED_HIT_FILES_OUTSIDE_SCOPE=" + $outside)'
  ```
  - Acceptance: the artifact records the command verbatim, records the printed `UNTRACKED_FILES` value, records every printed `FILE` line as the enumerated list of files searched, records every printed `HIT` line, and asserts that the printed `UNTRACKED_HIT_FILES_OUTSIDE_SCOPE` value is `0`.
  - Acceptance also requires the artifact to carry `SearchScope:`, `SearchPatterns:`, and `SearchResult:` lines, so the result is auditable whether or not the enumerated list is empty.
  - The two carve-outs from the outside-scope count are stated in the artifact with their reason: a hit under this item's own feature folder is one of this item's own artifacts quoting the identifiers it is auditing, and a hit under the .claude tree is agent-memory prose, which is exactly category E of the Phase 1 classification. Every such hit is still enumerated by a `HIT` line, so neither carve-out hides a hit; each only excludes it from the outside-scope counter.
  - Only the file path from the enumeration variable is printed. Do not print any resolved provider path, because a resolved path carries the host account name.
  - Assert the printed values only. Do not assert the exit code of the `pwsh` wrapper.

- [x] [P1-T6] Check off AC-2 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P1-T1 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-2 criterion text is unchanged.

- [x] [P1-T7] Check off AC-4 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P1-T3 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-4 criterion text is unchanged.

- [x] [P1-T8] Check off AC-5 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P1-T3 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-5 criterion text is unchanged.

- [x] [P1-T9] Check off AC-6 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P1-T4 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-6 criterion text is unchanged.

- [x] [P1-T10] Check off AC-7 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P1-T5 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-7 criterion text is unchanged.

---

### Phase 2 — Reflection entry-point inventory and closure

- [x] [P2-T1] Run the seventeen-pattern reflection entry-point inventory over the QuickFiler production tree and the QuickFiler test tree and record it in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t1-reflection-inventory.2026-08-29T04-55.md`. Discharges AC-8.
  ```
  pwsh -NoProfile -Command 'Write-Output ("QF_PROD_SCOPE_FILES=" + (git ls-files -- "QuickFiler/*").Count); Write-Output ("QF_TEST_SCOPE_FILES=" + (git ls-files -- "QuickFiler.Test/*").Count); @("GetMethod(","GetMethods(","GetMember(","GetMembers(","GetProperty(","GetProperties(","GetField(","GetFields(","GetEvent(","InvokeMember(","Type.GetType(","Activator.CreateInstance","Assembly.CreateInstance","Assembly.Load","Delegate.CreateDelegate","CallByName","System.Reflection") | ForEach-Object { $p = $_; $prod = @(git grep -n -I -F -e $p -- "QuickFiler/*").Count; $test = @(git grep -n -I -F -e $p -- "QuickFiler.Test/*").Count; Write-Output ($p + " prod=" + $prod + " test=" + $test) }'
  ```
  - Acceptance: the artifact records the command verbatim, states `INVENTORY_PATTERNS: 17`, and carries exactly seventeen printed pattern rows in the listed order, one per pattern, each row recorded verbatim with both its production count and its test count. The two scope lines the command prints before those rows are recorded as well, under the clause below, and are not pattern rows.
  - Acceptance also requires that the sixteen name-resolving patterns — every pattern in the list except `System.Reflection` — each print `prod=0`. That set includes the `GetField(` and `GetFields(` family that the earlier AC-16 search omitted entirely, and the artifact must state that omission explicitly.
  - Acceptance also requires that the `System.Reflection` row prints a production value of at least 32, that the value is recorded verbatim, and that it is not asserted to be zero. The base-commit reference values for the test-tree column, recorded and not asserted, are: `GetMethod(` 69, `GetMethods(` 4, `GetMember(` 6, `GetMembers(` 0, `GetProperty(` 24, `GetProperties(` 0, `GetField(` 172, `GetFields(` 2, `GetEvent(` 10, `InvokeMember(` 0, `Type.GetType(` 0, `Activator.CreateInstance` 4, `Assembly.CreateInstance` 0, `Assembly.Load` 0, `Delegate.CreateDelegate` 0, `CallByName` 0, `System.Reflection` 121.
  - Acceptance also requires the artifact to record the printed `QF_PROD_SCOPE_FILES` and `QF_TEST_SCOPE_FILES` values, which are the measured scope sizes P2-T2, P2-T4 and P3-T4 cite for every zero result taken over either QuickFiler tree. The base-commit reference values, recorded and not asserted, are 228 and 151.
  - The production pathspec reaches every tracked file under the QuickFiler production tree, including tracked non-source files, so the printed `System.Reflection` production value is expected to exceed the 32 first-party source-file occurrences. P2-T2 classifies the whole printed population.
  - Assert the printed values only. Do not assert the exit code of the `pwsh` wrapper.

- [x] [P2-T2] Enumerate and classify every production-tree `System.Reflection` occurrence and record it in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t2-production-reflection-classification.2026-08-29T04-55.md`.
  ```
  git grep -n -I -F -e "System.Reflection" -- "QuickFiler/*"
  ```
  - Acceptance: the artifact carries `EXIT_CODE: 0` and enumerates every printed line, one row per line, each row assigned to exactly one of these five classes by a test applied in order: L1, the log4net logger-declaration idiom, being a line containing `MethodBase.GetCurrentMethod()` whose first non-whitespace token is not `//`; L2, a `using System.Reflection;` directive; L3, a comment or commented-out code, being a line whose first non-whitespace token is `//` or that lies inside a block comment; L4, a tracked non-source file entry, being a project-file assembly reference, a hint path, or a package-manifest entry; L5, a call site taking a member-name argument.
  - Acceptance also requires that the five class counts sum to the production value printed by P2-T1 for `System.Reflection`, and that `L5: 0`.
  - Base-commit reference values, recorded and not asserted except for the `L5: 0` clause and the summation clause: L1 26, L2 3, L3 3, L4 7.
  - Acceptance also requires the artifact to state, in one sentence, why `L5: 0` is the operative finding: `MethodBase.GetCurrentMethod()` takes no member-name argument, a `using` directive resolves no member, a comment is not compiled, and a project or package manifest entry names an assembly rather than a member, so none of the occurrences in L1 through L4 can resolve a member of any type by name.

- [x] [P2-T3] Enumerate the receiver-scoped reflection call sites in the QuickFiler test tree, state the closure argument for the variable-argument sites, and record both in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t3-variable-argument-closure.2026-08-29T04-55.md`. Discharges AC-9.
  ```
  git grep -n -I -F -e "typeof(QfcCollectionController)" -- "QuickFiler.Test/*"
  ```
  ```
  git grep -n -I -F -e "GetField(" -e "GetMethod(" -- "QuickFiler.Test/Controllers/*"
  ```
  - Derivation of the set to enumerate, stated mechanically so a third party re-running it obtains the same set: a site belongs to the set when its reflection receiver is the expression `typeof(QfcCollectionController)` and its member-name argument is neither a string literal nor an identifier declared as a `const string` in the same file. The receiver expression may sit on the same printed line as the call or on the immediately preceding printed line; the member-name argument may sit on the same printed line as the call or on the immediately following printed line. A site whose argument is a `const string` identifier is enumerated separately by the named-constant clause below and is not counted toward `VARIABLE_ARGUMENT_SITES`.
  - Acceptance: the artifact enumerates at least these eight sites, each named individually by file and line, each carrying the API called, the form of its member-name argument, and its closure statement — in QfcCollectionController.TestSupport.cs, `GetField` at lines 38, 51, 65, 80 and 95 and `GetMethod` at line 118, each taking the variable `name`; in QfcCollectionControllerNavigationDigitsTests.cs, `GetField` at line 34 taking the variable `name` supplied on line 35; and in QfcCollectionControllerTests.cs, the receiver at line 381 with `GetField` at line 382 taking the variable `name`. The artifact states `VARIABLE_ARGUMENT_SITES: 8`.
  - Acceptance also requires the artifact to record the three named-constant sites in QfcCollectionControllerDefects468Tests.cs at lines 44, 66 and 86, which pass the constant `ReentrancyCounterField`, together with that constant's declared value `"removespecificcontrolgroupcounter"` at line 30 of the same file, and to state that the resolved value is not one of the thirteen identifiers. A named constant is literal-equivalent and is closed by naming its value, not by the variable-argument argument.
  - Acceptance also requires the artifact to state the closure argument in full: for each variable-argument site, the set of values the member-name variable can take is bounded by the string literals present in the source text of the assemblies that call it; P1-T4 established that the thirteen identifiers occur in the QuickFiler test tree exactly once, inside a triple-slash documentation comment, and occur nowhere in the QuickFiler production tree except the two lines of a live preserved member; therefore no call site can supply one of the thirteen.
  - Acceptance also requires the artifact to record the stated limit of that argument rather than omit it: the argument does not cover a member name assembled at runtime by string concatenation or interpolation. No such construction was observed at any site enumerated here, but its absence in general was not proved.
  - Reconciliation note the artifact must carry: AC-9 in the specification names six variable-argument sites; the mechanical derivation above yields eight. The eight are a superset of any six the specification could mean, so enumerating all eight individually discharges AC-9. The specification's baseline section describes AC-9's six as `GetField(` sites, but the measured set contains seven variable-argument `GetField(` sites — QfcCollectionController.TestSupport.cs lines 38, 51, 65, 80 and 95, QfcCollectionControllerNavigationDigitsTests.cs line 34, and QfcCollectionControllerTests.cs line 382 — together with one variable-argument `GetMethod(` site at QfcCollectionController.TestSupport.cs line 118. No six-element subset can be identified with the specification's six, so the artifact records the full eight and does not claim a subset identity. The count difference is recorded here as an evidence note; the approved specification is not edited to change the figure.

- [x] [P2-T4] Check the data-binding, serialization, and COM-visibility surface of the QuickFiler production tree and record the result in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t4-binding-serialization-surface.2026-08-29T04-55.md`.
  ```
  pwsh -NoProfile -Command '@("DataBindings.Add","DisplayMember","ValueMember","DataPropertyName","[Serializable","DataContract","JsonProperty","XmlElement") | ForEach-Object { $p = $_; $prod = @(git grep -n -I -F -e $p -- "QuickFiler/*").Count; Write-Output ($p + " prod=" + $prod) }'
  ```
  ```
  git grep -n -I -F -e "[assembly: ComVisible(false)]" -- "QuickFiler/*"
  ```
  - Acceptance: the artifact records both commands verbatim; the first command prints eight rows and every row prints `prod=0`; the second command exits `0` and its printed output names one line in the QuickFiler production tree's assembly-information source file. The artifact states `BINDING_SERIALIZATION_PATTERNS: 8`.
  - The literal `[assembly: ComVisible(false)]` is present in the tracked tree at the base commit; this task neither creates nor modifies it.
  - Acceptance also requires the artifact to state the affirmative conclusion this evidence supports: the affected type carries no property-name string binding surface and no serialization surface, and the assembly is not COM-visible, so no host-side late-binding path — a VBA `CallByName`, an `Application.Run`, or an Outlook macro — can reach a member of that type by name.
  - Assert the printed values only for the first command. Do not assert the exit code of the `pwsh` wrapper.

- [x] [P2-T5] Check off AC-8 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P2-T1 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-8 criterion text is unchanged.

- [x] [P2-T6] Check off AC-9 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P2-T3 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-9 criterion text is unchanged.

---

### Phase 3 — AC-16 corrections, fail-before dossier, and decision record

- [x] [P3-T1] Record both corrections to the AC-16 record in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p3-t1-ac16-corrections.2026-08-29T04-55.md`. Discharges AC-10.
  - Acceptance: the artifact carries exactly two numbered corrections and states `AC16_CORRECTIONS: 2`.
  - Correction 1: the AC-16 build-input file-type search covered twelve identifiers and omitted the thirteenth, the private field `_templateTlp`. The artifact states the omission, cites the P0-T4 commit-level derivation as the evidence that the removal was twelve methods plus one field, and states why the omitted identifier is the one for which the search mattered most: field reflection is the only name-based mechanism that demonstrably exists anywhere near the affected type, as P2-T3 enumerates.
  - Correction 2: the AC-16 claim of zero occurrences of any removed identifier anywhere in the QuickFiler test tree no longer holds. The artifact identifies the superseding occurrence by file, line, and category, taking all three from the category C row of the P1-T4 enumeration, and states that the occurrence is a triple-slash documentation comment naming `WireUpKeyboardHandler`, which is not a string literal, is not emitted as a member name into assembly metadata, and cannot be passed to any reflection API.
  - Acceptance also requires the artifact to state that the AC-16 artifact in the issue #468 feature folder is a time-stamped historical record and is not edited by this item; these corrections are recorded here instead.
  - This task writes no command output. It carries `Timestamp:` and `Output Summary:` and omits `Command:` and `EXIT_CODE:`, because it runs no command.

- [x] [P3-T2] Write the fail-before exception dossier at `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/regression-testing/fail-before-exception.2026-08-29T04-55.md`. Discharges AC-14.
  - Acceptance: the dossier carries `Timestamp:` and a `WhyFailingRunImpossible:` field of one to three sentences stating that this item changes no executable code, that a test asserting a repository search finds no genuine name-based caller is a tautology both before and after the work, and that no reproducible defect exists to redden such a test.
  - Acceptance also requires an alternative-proof section supplying the non-vacuity measurement as the substitute proof, citing by path the P0-T5 scope census, the P1-T1 Partition A zero-hit result, the P1-T2 control that proves the same pathspec reaches real content, the P1-T3 total classification with its empty category G, and the P1-T4 fully enumerated 31-row hit set with its empty category G.
  - Acceptance also requires the dossier to state that no unit test is added and no existing test is modified by this item, and why: a search-based test would encode a point-in-time measurement as a permanent gate over prose files that legitimately accrete these identifiers, and would fail on the next evidence artifact that quotes one of them.
  - This task's artifact records no command output. It carries `Timestamp:` and `Output Summary:` and omits `Command:` and `EXIT_CODE:`, because it runs no command.

- [x] [P3-T3] Write the decision record at `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p3-t3-decision-record.2026-08-29T04-55.md`. Discharges AC-13.
  - Acceptance: the artifact carries exactly one of the two single-line tokens `DECISION: RESIDUAL RISK CLOSED` or `DECISION: CALLER FOUND`, and carries neither token more than once.
  - If the token is `DECISION: RESIDUAL RISK CLOSED`, the artifact cites, by path, every Phase 1 and Phase 2 artifact the closure rests on, and states which classes of caller were proved absent and which were not, naming the runtime-assembled member name as the one class not proved absent.
  - If the token is `DECISION: CALLER FOUND`, the artifact names the caller by file, line, and the mechanism by which it resolves the member, and records the number of the separate issue raised to address it. Do not repair the caller inside this item: the repository bugfix workflow directs a deeper problem uncovered during a fix to a new issue rather than to a widened scope, and a repair would additionally require its own reproducible failing test.
  - A hit under the docs tree or the .claude tree is a category D or E hit, never a caller, and does not trigger the `DECISION: CALLER FOUND` branch.
  - This task's artifact records no command output. It carries `Timestamp:` and `Output Summary:` and omits `Command:` and `EXIT_CODE:`, because it runs no command.

- [x] [P3-T4] Audit every zero-result search in this item for auditable-absence fields and record the audit in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p3-t4-zero-result-audit.2026-08-29T04-55.md`. Discharges AC-11.
  - Acceptance: the artifact enumerates every search in this item whose recorded result is zero — the P1-T1 Partition A sweep, the P1-T3 category G count, the P1-T4 category G count, the P1-T5 outside-scope count, the sixteen name-resolving production rows of the P2-T1 inventory, the eight test-tree rows of the P2-T1 inventory that print zero — GetMembers(, GetProperties(, InvokeMember(, Type.GetType(, Assembly.CreateInstance, Assembly.Load, Delegate.CreateDelegate and CallByName — the P2-T2 class L5 count, and the eight production rows of the P2-T4 surface check — and for each one records `SearchScope:`, `SearchPatterns:`, `SearchResult:`, and a measured scope size.
  - Acceptance also requires the artifact to state `ZERO_RESULT_SEARCHES: 37` and to show that every enumerated row carries all four fields, so that no zero result in this item rests on an unstated or empty search set.
  - The count of 37 is the sum of the enumerated rows: 1 Partition A sweep, 1 Partition B category G count, 1 Partition C category G count, 1 untracked outside-scope count, 16 production inventory rows, 8 zero test-tree inventory rows, 1 class L5 count, and 8 surface-check rows. The measured scope size for every row scoped to a QuickFiler tree is the corresponding value recorded by P2-T1. If any enumerated row is absent because its producing task recorded a blocker, record the reduced count together with the reason and leave this task unchecked.

- [x] [P3-T5] Check off AC-10 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P3-T1 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-10 criterion text is unchanged.

- [x] [P3-T6] Check off AC-11 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P3-T4 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-11 criterion text is unchanged.

- [x] [P3-T7] Check off AC-13 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P3-T3 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-13 criterion text is unchanged.

- [x] [P3-T8] Check off AC-14 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P3-T2 dossier path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-14 criterion text is unchanged.

---

### Phase 4 — Final QC, no-modification proof, and toolchain gate

- [ ] [P4-T1] Stage and commit every artifact produced so far under `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/`, so the anchored diff that P4-T2 runs can observe them.
  ```
  git add -- docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635
  git commit -m "docs(635): record widened reflective-caller sweep, reflection inventory, and decision"
  git status --porcelain -- docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635
  ```
  - Acceptance: the third command prints no output.
  - Stage with the explicit feature-folder pathspec shown above. Do not stage with an all-paths flag: an all-paths stage sweeps unrelated untracked files, including queued promotion entries belonging to other items, onto this branch.
  - This task writes no artifact. Any artifact it wrote would re-dirty the tree it asserts is clean.

- [ ] [P4-T2] Produce the no-modification proof and record it in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t2-no-modification-proof.2026-08-29T04-55.md`. Discharges AC-12.
  ```
  git diff --name-only origin/main...HEAD
  git status --porcelain
  ```
  - Run both commands and capture their output before writing the artifact, so the artifact file does not appear in its own porcelain listing.
  - Acceptance: the artifact records both commands verbatim with their full output, states `(no output)` explicitly where output is empty, and asserts both of the following over the union of the two listings: no path in the union has a `.cs`, `.csproj`, `.props`, `.targets`, `.resx`, `.config`, `.settings`, `.xaml`, or `.ps1` extension; and every path in the union either begins `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/` or lies under the tracked agent-memory tree beneath the .claude directory.
  - Both commands are required and neither alone is sufficient. The anchored diff cannot see an untracked file, and the porcelain status goes empty once a change is committed, so each is wrong in exactly one state. The three-dot form diffs HEAD against the merge base with the base branch, which is what the acceptance criterion requires; the two-dot form would report unrelated commits on the base branch as reversed changes if that branch advances during execution.
  - The agent-memory carve-out is stated in the artifact with its reason: that tree is tracked and is written by the agents executing this plan as their own bookkeeping, not by this item's change set. Every path of that kind is enumerated individually in the artifact and marked as such, so the carve-out hides nothing.
  - Acceptance also requires the artifact to record a `LANGUAGE_COMPOSITION:` line derived from the union's file extensions, which P4-T3 consumes as its branch condition.

- [ ] [P4-T3] Record the toolchain gates applicable to the branch diff in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t3-toolchain-gate.2026-08-29T04-55.md`. Discharges AC-15.
  - Branch on the `LANGUAGE_COMPOSITION:` line recorded by P4-T2, and record which branch was taken.
  - Branch one, taken when the P4-T2 union lists any path outside `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/` whose extension is `.cs`, `.csproj`, `.props`, `.targets`, `.resx`, `.config`, `.settings` or `.xaml`: run the four mandated C# toolchain commands in the repository's mandated order — the CSharpier format command, then the analyzer rebuild, then the nullable rebuild, then the test run — and record each command verbatim with its own `EXIT_CODE:`. Restart the loop from the format step if any step fails or changes a file. If the union additionally lists a `.ps1` path outside that folder, run the PowerShell gates and record them the same way.
  - Branch two, taken when the P4-T2 union lists no such path: record the two single-line tokens `CSHARP_GATE: NOT APPLICABLE` and `POWERSHELL_GATE: NOT APPLICABLE`, and for each one cite the P4-T2 artifact path as the evidence that the gate has no in-scope file. Record the reason as no in-scope file rather than as a skip.
    - This task's artifact records no command output. It carries `Timestamp:` and `Output Summary:` and omits `Command:` and `EXIT_CODE:`, because it runs no command.
  - Acceptance: the artifact records the `LANGUAGE_COMPOSITION:` value it branched on, states `TOOLCHAIN_BRANCH: 1` or `TOOLCHAIN_BRANCH: 2`, and the branch recorded matches the P4-T2 union. Branch one additionally requires four recorded commands each with an `EXIT_CODE:` of `0` in the final pass; branch two additionally requires both not-applicable tokens with their citation.
  - This gate can fail. A source-extension path outside the feature folder forces branch one, and a branch-one toolchain failure fails the task. A bare skip could not fail.
  - No coverage command is run and no coverage artifact is emitted by either branch, because no executable line changes in this item.

- [ ] [P4-T4] Scan every artifact under `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/` for host-identity leaks and record the scan in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t4-host-identity-scan.2026-08-29T04-55.md`.
  ```
  pwsh -NoProfile -Command '$root = "docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635"; $f = Get-ChildItem -Path $root -Recurse -File -Name | Where-Object { $_ -notlike "*p4-t4-host-identity-scan*" -and $_ -notlike "*plan.2026-08-29T00-23.md" }; Write-Output ("SCANNED_FILES=" + $f.Count); $n = 0; foreach ($p in $f) { $m = @(Select-String -LiteralPath (Join-Path $root $p) -SimpleMatch -Pattern "C:\","c:\","C:/","c:/","\Users\","/Users/" -ErrorAction SilentlyContinue); if ($m.Count -gt 0) { Write-Output ("LEAK " + $p + " " + $m.Count); $n = $n + $m.Count } }; Write-Output ("HOST_IDENTITY_HITS=" + $n)'
  ```
  - Acceptance: the artifact records the command verbatim, records the printed `SCANNED_FILES` value, records every printed `LEAK` line, and asserts that the printed `HOST_IDENTITY_HITS` value is `0`. The printed `SCANNED_FILES` value is greater than zero, which makes the zero-hit result non-vacuous.
  - The exclusion filter names two files, and both are stated in the artifact with their reason: this task's own artifact file, which does not yet exist when the command runs and is filtered defensively so that a re-run is idempotent, and `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/plan.2026-08-29T00-23.md`. Each of those two quotes this scan's pattern list verbatim, so including either would make the gate report a hit on a line that is the gate's own pattern list rather than a leak, and the gate could never pass. No other file is excluded. Both excluded files were checked by hand at planning time and carry no absolute host path, account name, or machine name outside that pattern list.
  - The file list is enumerated from disk rather than from the tracked index, so the scan covers the Phase 4 artifacts that are still untracked at this point as well as the artifacts committed by P4-T1. The scan cannot cover an artifact written after it runs; the only such artifact is the P4-T7 reconciliation record, whose sole command is a repository-relative read of the specification file.
  - Run the command before writing the artifact. Assert the printed values only; do not assert the exit code of the `pwsh` wrapper.

- [ ] [P4-T5] Check off AC-12 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P4-T2 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-12 criterion text is unchanged.

- [ ] [P4-T6] Check off AC-15 in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md` by marking that criterion's checkbox `[x]`, citing the P4-T3 artifact path as its evidence pointer.
  - Acceptance: exactly one checkbox changes in this task; the AC-15 criterion text is unchanged.

- [ ] [P4-T7] Reconcile the acceptance-criteria checklist and record the reconciliation in `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t7-ac-reconciliation.2026-08-29T04-55.md`.
  ```
  pwsh -NoProfile -Command '$l = Get-Content -LiteralPath "docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md"; Write-Output ("AC_CHECKED=" + @($l | Where-Object { $_ -match "^- \[x\] \*\*AC-" }).Count); Write-Output ("AC_UNCHECKED=" + @($l | Where-Object { $_ -match "^- \[ \] \*\*AC-" }).Count)'
  ```
  - Acceptance: the artifact records the command verbatim and asserts that the printed values are `AC_CHECKED=15` and `AC_UNCHECKED=0`.
  - Acceptance also requires the artifact to carry the acceptance-criteria status summary: the source file path, a total of 15 items, the checked count, the remaining count, and the criterion text of any remaining item.
  - If any acceptance criterion could not be verified, leave its checkbox unchecked, record the gap and its reason here, and leave this task unchecked. Do not check a box that its evidence does not support.
  - Assert the printed values only; do not assert the exit code of the `pwsh` wrapper.

- [ ] [P4-T8] Commit the remaining artifacts and the checked-off specification under `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/` and confirm the feature folder is fully committed.
  ```
  git add -- docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635
  git commit -m "docs(635): record QA gates, no-modification proof, and acceptance reconciliation"
  git status --porcelain -- docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635
  git status --porcelain
  ```
  - Acceptance: the third command prints no output.
  - Acceptance: the fourth command's every printed path either begins `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/` or lies under the agent-memory tree beneath the .claude directory, and no printed path has a `.cs`, `.csproj`, `.props`, `.targets`, `.resx`, `.config`, `.settings`, `.xaml`, or `.ps1` extension. This closes the interval between P4-T2 and the end of the plan, which P4-T2 cannot observe.
  - Stage with the explicit feature-folder pathspec shown above, for the same reason stated in P4-T1.
  - This task writes no artifact, for the same reason stated in P4-T1.
