# Remediation Plan — Issue #911, Cycle 1

- Timestamp: 2026-09-20T01-37
- Issue: #911 (`bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`)
- Work Mode: `full-bug`. Acceptance-criteria source is `spec.md` only; no `user-story.md` exists
  and none may exist for this mode.
- Execution worktree: `<execution-worktree-root>` = `$HOME\repos\TaskMaster-wt\dependabot-911`
- Session worktree: `<session-worktree-root>` = `$HOME\repos\TaskMaster-wt\2026-09-12T10-15`
- Feature folder (both worktrees):
  `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`
- Predecessor plan: `plan.2026-09-19T09-44.md`, complete at 128 of 128 tasks. It is **not** re-opened,
  renumbered or re-executed by this cycle. Its **gate rules 1 through 16 remain binding on every task
  below**, cited by number rather than by line so the citations survive the file rewrite Phase 4
  performs on that document.

## What this plan is

A remediation cycle against the review dated 2026-09-20T01-37:
`code-review.2026-09-20T01-37.md`, `feature-audit.2026-09-20T01-37.md`,
`policy-audit.2026-09-20T01-37.md` and `remediation-inputs.2026-09-20T01-37.md`, all in the feature
folder. It discharges four blocking findings, five major findings and four minor findings against
already-delivered work. It adds no feature behaviour.

## Task identifier namespace — a recorded deviation

The delegation brief asked for an `R#-T#` task prefix so remediation tasks could not be confused with
the predecessor plan's `P#-T#` tasks. That prefix is **mechanically rejected** by
`.claude/hooks/validate-planner-output.ps1`, whose task pattern is
`^- \[(?<State>[ xX])\] \[P(?<Phase>\d+)-T(?<Task>\d+)\] (?<Text>.+)$` and whose phase pattern is
`^### Phase (?<Phase>\d+)\s+—\s+(?<Title>.+)$`. A plan using `R1-T1` fails that hook and cannot be
handed off at all. The brief's intent is met a different way and the deviation is recorded here
rather than silently resolved:

- this plan is a **separate file**, so its identifier namespace does not overlap the predecessor's
  on disk;
- **every task below opens with its finding tag** — `R1` through `R9d` — before any other word, so a
  task is attributable to a finding without reading the plan header;
- **every evidence artifact this plan produces carries the `2026-09-20T01-37` timestamp**, where
  every predecessor artifact carries `2026-09-19T09-44`, so no artifact of one cycle can be mistaken
  for the other.

Read `[P3-T7]` in this document as "remediation cycle 1, phase 3, task 7". It is not, and cannot be
confused with, the predecessor's `[P3-T7]`, which lives in a different file under a different
timestamp and is already ticked.

## Findings to phases

| Finding | Severity | Discharged in |
|---|---|---|
| R1 — no green workflow run at head | Blocking | Phase 6 (last, by CI at head) |
| R2 — `Sync-PackageReferences.ps1` negative and error paths untested | Blocking | Phase 1 |
| R3 — push gate discards normalisation and redirect writes | Blocking | Phase 3 |
| R4 — 74 committed occurrences of an absolute host path | Blocking | Phase 4, plus a merge-time instruction in Phase 6 |
| R5 — `Invoke-ProjectConsistencyRepair` cannot be called correctly | Major | Phase 2 |
| R6 — disclosure step unguarded and appending | Major | Phase 3 |
| R7 — binding-redirect class unreachable from the trigger | Major | Phase 3, plus one `spec.md` amendment verified in Phase 0 |
| R8 — repair commit identity matches no account | Major | Phase 3 |
| R9a — two detector false positives in the autoclose list | Minor | Phase 4 |
| R9b — unfiltered `Get-AnalyzerAssemblyPath` call site uncommented | Minor | Phase 2 |
| R9c — non-recursive manifest discovery fails silently | Minor | Phase 2 |
| R9d — two production files at the 500-line cap | Minor | Phases 1, 2, 3 (size audits) and Phase 5 |

---

## Decisions of Record

Each decision below selects one of the discharges the review offered, states why, and states what the
rejected alternative would have cost. An executor does not re-open these.

**D1 — R5 is discharged by making the resolution shared, not by removing the export.** The review
offered two routes: add a parameter and thread a resolved value through, or remove the function from
`Export-ModuleMember` and retarget its tests at the composition root. The second route was measured
and rejected: `Invoke-ProjectConsistencyRepair` is called by four test sites —
`tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1:265` and
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1:333`, `:350` and `:379` — and three of them
carry criteria (`AC16-` twice, `AC21-` once, the last being the #908 regression fixture). Retargeting
those three onto the composition root re-bases delivered criterion evidence for a defect that is a
parameter-threading error, and leaving the function in place while retargeting its tests away from it
would leave 99 uncovered lines in a 493-line module and break the per-file coverage floor.

The discharge is:

1. **Move `Resolve-ReferenceAssemblyVersion` out of the composition root and into the reconciliation
   module.** It sits today at `scripts/dependencies/Repair-PackageManifestConsistency.ps1:143-177`, a
   private function of a 498-line script that the minor finding R9d declares at capacity. It moves
   verbatim to `scripts/dependencies/ProjectConsistency.psm1`, which is 331 lines and imports
   `PackageGraph.psm1` and `AnalyzerItemRepair.psm1` today. It calls `Select-CompatibleAssetFolder`,
   so `ProjectConsistency.psm1` additionally imports
   `scripts/dependencies/PackageCompatibility.psm1`; that module imports nothing, so no cycle is
   created. This is the "extract rather than append" R9d requires, and it moves the composition root
   **down** roughly 35 lines rather than up.
2. **`Invoke-ProjectConsistencyRepair` resolves the assembly version per package and passes it.** With
   no identity evidence available inside the module, `Resolve-ReferenceAssemblyVersion` returns the
   version the project **already declares**, so `Get-RewrittenReferenceVersionLine` writes back what is
   already there and the line is left byte-identical. The failure mode — rewriting every matching
   `<Reference Include="...Version=X..." />` to the package version — is removed for every caller,
   whether or not it supplies anything, which is a stronger property than the parameter the review
   proposed.

This is safe against the detectors: `Find-VersionDisagreement` in
`scripts/dependencies/ConsistencyVerifier.psm1:78-120` states in its own description that a
`<Reference>` is outside the detector, because its Include carries an assembly version that need not
track the package version. Leaving a Reference line unrewritten therefore creates no residual
divergence and cannot turn an `IsSuccess` result into a failure. The two `AC16-` tests assert
`Kind` contains `Import` and `HintPath`, not `Reference`, and `AC21-` asserts `Import`, `Error` and
`Analyzer`, so none of the three depends on the removed rewrite.

No new parameter is added. The plan states the contract instead: the module function reconciles the
folder-segment kinds and **preserves** the declared Reference assembly version; a consumer wanting
evidence-based reference resolution uses the composition root, which supplies the resolved value at
`Repair-PackageManifestConsistency.ps1:265-268`.

**D2 — R7 is discharged as out of scope.** The workflow supplies no `-CandidateUpgrade`, so
`$upgrade.Applied` is always empty and the `app.config` reconciliation block does not execute under
the configured trigger. Deriving the applied upgrade set from the Dependabot commit is new production
behaviour with new untested paths, in a remediation cycle whose purpose is to close a review. The
discharge is therefore: remove the dead `BindingRedirect` clause from the `$beyondKnownWeak` filter in
`.github/workflows/dependabot-repair.yml`, add a comment at that line naming the reachability
decision, and amend the AC14 note in `spec.md`. The binding-redirect **write** path is not removed and
is not dead in the composition root; only the workflow's filter clause, which no record can reach
because the call site keeps `.Text` and discards the `Kind = 'BindingRedirect'` record, is removed.

**D3 — R8 is discharged by correcting the identity, not by restating AC18.** The review offered
either. Correcting the identity keeps AC18 satisfiable as written, which is the better outcome for
#914, and avoids a second `spec.md` amendment. The address is **derived at run time** rather than
written as a literal, because the numeric part of a GitHub App bot's noreply address is the **bot
user's** id and not the app id, and neither is knowable when the workflow is authored. The commit step
reads the app slug from the token step's `app-slug` output, resolves the bot user id through
`gh api "/users/<slug>%5Bbot%5D" --jq .id`, and composes
`<bot-user-id>+<slug>[bot]@users.noreply.github.com`. Both reads are **guarded**: an empty slug or an
empty id fails the step with a named error rather than falling back to a literal that resolves to
null. **Assumption of record:** that `actions/create-github-app-token@v3` publishes an `app-slug`
output. It is not verifiable in this worktree and is not verifiable without a live run; the guard is
what converts a wrong assumption from a silent bad commit identity into a loud step failure, and #914
is where the assumption is settled.

**D4 — R9c is discharged by making the shortfall visible, not by widening discovery.** The review
offered either a recursive walk with the existing prune list or a verbose enumerated-directory count.
A recursive walk changes which manifests the production pass discovers, which is a behaviour change
with no covering test in a cycle that closes a review. The verbose count is added instead. It does not
prevent a nested project from being skipped; it makes the shortfall observable in the run log, which
is what the review asked for as its second option, and the plan says so plainly rather than implying
the silent-skip risk is removed.

**D5 — R2 targets the nine uncovered logic lines, and the file lands below 85 percent.** This is
stated in advance because the arithmetic is fixed and a later reader will otherwise re-open it.
`scripts/vscode/Sync-PackageReferences.ps1` measures 95 covered of 127 instrumented, 74.80 percent. Of
the 32 uncovered lines, 19 are the `Get-PackageSyncSeam` delegate table and 4 are the top-level
invocation; the remaining 9 are pure logic and every one is a negative or an error path. Covering all
nine gives 104 of 127, **81.89 percent**.

Reaching 85 percent requires one of exactly two things, and both are prohibited here:

- **Executing the top-level invocation.** The four uncovered entry lines run only when the file is
  invoked rather than dot-sourced, at which point `Invoke-PackageReferenceSync` runs the production
  seam against a real tree, enumerating and rewriting real project files. `.claude/rules/general-unit-test.md`
  prohibits external dependencies and temporary files in tests, and no injected seam reaches a
  top-level invocation.
- **Excluding the delegate table from measurement.** The Coverage Exclusion Policy in
  `.claude/rules/general-unit-test.md` states that no production file may be excluded, and
  `remediation-inputs.2026-09-20T01-37.md` states in terms that those 19 lines legitimately remain in
  the denominator and must not be excluded.

81.89 percent clears the authoritative floor. Per **gate rule 13** of the predecessor plan, the
authoritative figure is the 80 percent in the execution worktree's `CLAUDE.md`, settled by the project
maintainer on 2026-09-11 under issue #563; the 85 in `.claude/rules/general-unit-test.md` is
push-down-owned upstream boilerplate and the discrepancy is tracked as open issue #668. The plan
asserts the measured per-line outcome rather than a percentage target, because the per-line assertion
is what the finding is actually about: the review recorded a **scenario-completeness** failure and a
coverage figure as two readings of one defect, and the nine named lines discharge both readings. P1-T11
records the reconciliation so the next reviewer reads the arithmetic rather than re-deriving it.

**D6 — the branch has merged `origin/main`, so the C# toolchain baseline is re-taken.** Every C#
figure the review recorded was measured at `794d34f02`, before the merge. The merge brought C# changes
this branch has never built. Phase 0 therefore re-takes all four C# gates as the baseline for this
cycle, and Phase 5 re-runs them. This cycle changes no `.cs`, `.csproj`, `packages.config`,
`app.config` or `.csharpierignore` file, so the two C# coverage figures should agree within run-to-run
noise; a disagreement is a finding about the merge, not about this remediation. Comparing Phase 5
against the delivered `evidence/qa-gates/p9-t7-coverage-projection.2026-09-19T09-44.jacoco.xml` instead
would be exactly the defect **gate rule 14** names: a baseline that an intervening transformation — here,
the merge — invalidated.

---

## Gate-Quality Rules Added by This Cycle

Gate rules 1 through 16 of `plan.2026-09-19T09-44.md` are binding and are not restated. Four more bind
this cycle.

**Gate rule 17 — no artifact this plan writes, and not this plan itself, may contain the account
name in any spelling.** R4 exists because 27 committed documents leaked one. Three obligations follow,
and the third is the one that is easy to miss:

- Every evidence artifact, every command this plan records, and this plan file itself use the
  placeholder tokens `<execution-worktree-root>`, `<session-worktree-root>`, `<repo-root>`,
  `<user-home>` and `<user-home-8dot3>` in prose, and the shell-expanded `$HOME` form in commands.
- **Every path an artifact records is repository-relative**, or placeholder-normalised when it must
  be absolute. Tool output quoted into an artifact — an MSBuild log line, a runner's printed
  destination path, a `gh` response — is normalised through the same map before it is written, and
  the artifact states that it was normalised.
- **The search pattern itself is derived at run time and is never typed.** A task that writes the
  account name in order to search for it makes its own residual assertion unsatisfiable: this plan
  file and every artifact quoting the pattern would match it. Every task below that searches for the
  host path builds the pattern as
  `$acct = Split-Path $HOME -Leaf; $pattern = [regex]::Escape($acct) + '|' + [regex]::Escape($acct.Substring(0, 6) + '~')`,
  which yields the long spelling and the 8.3 spelling, both matched case-insensitively by
  `Select-String`. This is the recurring class in which a prohibition sentence legitimately contains
  the phrase it prohibits, and deriving the pattern is what keeps the assertion satisfiable without
  carving the plan file out of its own scope.

An artifact written under this rule needs no sanitisation pass, which is what lets Phase 4 sanitise
once and Phase 5 verify a residual of zero across artifacts written after it.

**Gate rule 18 — every `pwsh` invocation is `$HOME`-rooted, absolute, and sets its own location.** This
is gate rule 16 restated in the sanitised form the rule above requires. The shape is
`pwsh -NoProfile -Command 'Set-Location "$HOME\repos\TaskMaster-wt\dependabot-911"; & "$HOME\repos\TaskMaster-wt\dependabot-911\<script>" <args>'`,
with outer single quotes so the calling shell expands nothing and inner double quotes so `pwsh`
expands `$HOME`. `-WorkingDirectory` remains prohibited: it does not affect how `-File` resolves, and
a relative script path executes the session worktree's copy against the session worktree's tree,
succeeds, and writes evidence describing a repository this plan is not changing.

**Gate rule 19 — criterion text is amended by a planning agent before execution, never by the
executor.** This cycle requires exactly one `spec.md` amendment, to the AC14 note (D2). Its text is
given verbatim below. The coordinator applies and commits it before execution begins. P0-T4 is a
**read-only verification** with a stop-and-report branch: if the amendment is absent the executor
records the absence and halts rather than making the edit itself. No other criterion is reworded by
this cycle, and P5-T11 asserts that.

**Gate rule 20 — for a component that has never executed, state the verification route and the
residual.** R3, R6, R7 and R8 all sit in `.github/workflows/dependabot-repair.yml`, which has never
run. Every task against them states both (a) what is verified without a live run and how that check
can fail, and (b) what remains unverifiable until the #914 credential exists. A task that verifies
only the static text and does not say so reads as a stronger discharge than it is.

### The AC14 amendment, verbatim

The coordinator appends the following sentence to the AC14 bullet in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`,
after the existing `Evidence: Pester output under evidence/qa.` sentence, changing nothing else in the
file:

> The class is exercised by unit assertion only and is not reachable from the `workflow_run` trigger:
> the workflow invokes the repair entry point with no `-CandidateUpgrade`, so the applied-upgrade set
> is always empty and the `app.config` reconciliation pass does not execute in the configured trigger
> path. Making it reachable is out of scope for issue #911 and is recorded in the code review dated
> 2026-09-20.

The criterion remains ticked and the criterion count remains exactly 26.

---

## Facts Measured for This Cycle

Every figure below was measured in the execution worktree while this plan was written. Figures the
plan's own tasks invalidate are marked, and no task asserts one of those as a literal.

| Fact | Value |
|---|---|
| Uncovered pure-logic lines in `scripts/vscode/Sync-PackageReferences.ps1` | 151 (`Resolve-ManifestPackageId` returns empty), 180 (`Resolve-PackageAssetFolder` returns empty), 248 (the #902 rejection warning), 290 and 293 (`Repair-ProjectReferenceVersion` early returns), 330 (no project file), 336 and 337 (conflict-marker skip), 345 (empty repair set) |
| Function names in the review are wrong for three of those lines | `remediation-inputs.2026-09-20T01-37.md` names `Get-PackageIdentifier` for line 151 and `Set-ReferenceAssemblyVersion` for lines 290 and 293. Neither identifier exists in the file. The real names are `Resolve-ManifestPackageId` and `Repair-ProjectReferenceVersion`, verified by reading the file. The line numbers are correct |
| Line counts of the files this cycle edits | `Repair-PackageManifestConsistency.ps1` 498, `ConsistencyVerifier.psm1` 493, `ProjectConsistency.psm1` 331, `PackageCompatibility.psm1` 172, `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` 185, `tests/scripts/dependencies/DependabotConfig.Tests.ps1` 335, `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` 382, `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` 275. All invalidated by this plan's own edits, so every size clause reads a ceiling and a measured value, never a predicted one |
| Host-path census at planning time | 76 matching **lines** across 29 files under the feature folder for the case-insensitive backslash form, against the review's 74 **occurrences** across 27 files. The two review artifacts added since account for the difference. The metric is ambiguous between lines and occurrences, so P0-T3 records **both** and every later clause names which it reads |
| Distinct host-path variants present | four roots — `<user-home>\repos\TaskMaster-wt\dependabot-911`, `<user-home>\repos\TaskMaster-wt\2026-09-12T10-15`, `<user-home>\repos\TaskMaster`, and bare `<user-home>` — plus forward-slash and 8.3 (`<user-home-8dot3>`) spellings, 9 matching lines across 4 files for the latter two. A single-literal substitution leaves the rest behind |
| Already-sanitised control file | `evidence/qa-gates/p4-t5-actionlint.2026-09-19T09-44.md` carries `<execution-worktree-root>` and **no** host path. It is the census's negative control: a census that lists it has matched something it should not |
| Autoclose list in `artifacts/pr_context.summary.txt` | 13 members in each of two sections; the 11 genuine ones are #181, #563, #668, #895, #898, #902, #903, #907, #908, #909, #911, and the two false positives are `#MEZIANTOU-898` and `#SHA-256`. `artifacts/` is gitignored at `.gitignore:57`, so the file is untracked and no commit lists it |
| `ci.yml` accepts a manual dispatch | `.github/workflows/ci.yml:8` declares `workflow_dispatch` alongside `push` and `pull_request`, so `gh workflow run CI --ref <branch>` is available for R1 |
| Workflow file count | 9 `.yml` files under `.github/workflows/`. This cycle adds none, so the count is 9 before and after, and per **gate rule 10** it is read from a filesystem enumeration because actionlint prints nothing on a clean run |
| CI Pester job scope | `.github/workflows/_pester.yml:41` runs `tests/scripts/dependencies` and `tests/scripts/vscode` and `:45` covers `scripts/dependencies` and `scripts/vscode`, with an aggregate line floor of 80 at `:71`. Every test this cycle adds lands inside an already-scanned directory, so **no workflow change and no spec Write Set amendment is needed for the new tests** |
| Write Set coverage | all eight files this cycle edits are already members of the spec `## Write Set`. No Write Set amendment is required and none is made |
| Reference elements are outside the disagreement detector | `scripts/dependencies/ConsistencyVerifier.psm1:78-120`, stated in the function description. This is what makes D1 safe |
| `Resolve-ReferenceAssemblyVersion` returns the declared version when it has no evidence | `scripts/dependencies/Repair-PackageManifestConsistency.ps1:159-176`: it returns `$declared` on every no-evidence branch, and `''` only when the project declares no matching Reference at all — in which case no line matches and the fallback cannot write anything |

---

## Command Reference

Cited by name from task text. Every block obeys gate rules 17 and 18.

**CMD-PESTER-ALL** — the full suite with coverage

```
pwsh -NoProfile -Command 'Set-Location "$HOME\repos\TaskMaster-wt\dependabot-911"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies","tests/scripts/vscode"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "<OUTPATH>"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

`<OUTPATH>` is replaced per task and **always lies under `coverage/`**, which `.gitignore:144`
ignores. Per **gate rule 12** no collector document is written under the evidence tree or committed;
the figures the task asserts are projected into the `.md` artifact the task names. This run is
unfiltered, so `Total` and the executed population coincide.

**CMD-PESTER-FILTERED** — one file, one name filter

```
pwsh -NoProfile -Command 'Set-Location "$HOME\repos\TaskMaster-wt\dependabot-911"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("<FILE>"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.Filter.FullName = "<FILTER>"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Executed=$($r.PassedCount + $r.FailedCount + $r.SkippedCount) Total=$($r.TotalCount) NotRun=$($r.NotRunCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

**Every `Executed` figure a filtered task asserts is `Passed + Failed + Skipped`, never
`$r.TotalCount`.** Per **gate rule 2**'s sub-case, `TotalCount` counts filtered-out tests as `NotRun`,
so it reports the whole file's `It` count and is invariant under the filter, including a filter that
matches nothing. Every filtered task records `Total` and `NotRun` as context and asserts `Executed`.

**CMD-JACOCO-PERFILE** — read one file's LINE counter and its uncovered line numbers

```
pwsh -NoProfile -Command 'Set-Location "$HOME\repos\TaskMaster-wt\dependabot-911"; [xml]$j = Get-Content "<DOC>" -Raw; $sf = @($j.SelectNodes("//sourcefile") | Where-Object { $_.name -like "*<LEAF>" })[0]; $c = @($sf.SelectNodes("counter") | Where-Object { $_.type -eq "LINE" })[0]; "FILE=<LEAF> covered=$($c.covered) missed=$($c.missed)"; "UNCOVERED=" + ((@($sf.SelectNodes("line") | Where-Object { [int]$_.ci -eq 0 }) | ForEach-Object { $_.nr }) -join ",")'
```

Selection is by XPath rather than dotted property access, because a JaCoCo document declares a
`DOCTYPE` and `$j.report` resolves to a two-element array. A `line` element whose `ci` attribute is
zero is uncovered; if the emitted attribute names differ from that, the executor corrects the
expression to the same semantic rule and records the correction in the artifact.

**CMD-POSHQC-FORMAT**, **CMD-POSHQC-ANALYZE** — MCP tools
`mcp__drm-copilot__run_poshqc_format` and `mcp__drm-copilot__run_poshqc_analyze`, each invoked with
`scan_folders` supplied **explicitly** as
`["scripts/dependencies","scripts/vscode","tests/scripts/dependencies","tests/scripts/vscode"]`. The
tool resolves its scan set from `config/poshqc-scan.json`, which does not exist here, so an omitted
`scan_folders` measures nothing. `MCP Result: ok:true` is **not** an acceptance condition for the
analyzer: pre-existing findings remain on this tree and the tool reports `ok:false` while they do.
The analyzer acceptance is stated per task in terms of the finding set.

**CMD-REVERT-OUT-OF-SCOPE-FORMAT** — `git checkout -- <derived-pathspec>`, run immediately after every
CMD-POSHQC-FORMAT. The pathspec is **derived at run time** as the set of paths whose
`Get-FileHash -Algorithm SHA256` changed across the format invocation, minus every member of the spec
`## Write Set`. `scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
are unformatted on `main`, clean under the ruleset CI runs, and are not touched by this change; they
are reverted if the formatter rewrites them. When the derived set is empty the command is not run and
the task records `REVERT-SET: empty`, which is the truthful observation and not a failure.

**CMD-CSHARPIER-CHECK**

```
pwsh -NoProfile -Command 'Set-Location "$HOME\repos\TaskMaster-wt\dependabot-911"; dotnet tool run csharpier check .; exit $LASTEXITCODE'
```

Success prints one line beginning `Checked ` and ending `ms.`; `N` is the scanned count, not a
finding count, per **gate rule 6**.

**CMD-MSBUILD-ANALYZERS**

```
pwsh -NoProfile -Command 'Set-Location "$HOME\repos\TaskMaster-wt\dependabot-911"; msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\analyzers.msbuild.log;Verbosity=normal"; exit $LASTEXITCODE'
```

**CMD-MSBUILD-NULLABLE**

```
pwsh -NoProfile -Command 'Set-Location "$HOME\repos\TaskMaster-wt\dependabot-911"; msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\nullable.msbuild.log;Verbosity=normal"; exit $LASTEXITCODE'
```

Do not add `/p:Nullable=enable` and do not substitute `/t:Build`; both are load-bearing omissions
recorded in `CLAUDE.md`. Non-vacuity is asserted on the echoed compiler command line carrying
`/out:obj\Debug\`, per **gate rule 7**.

**CMD-MSTEST-COVERAGE**

```
pwsh -NoProfile -Command 'Set-Location "$HOME\repos\TaskMaster-wt\dependabot-911"; & "$HOME\repos\TaskMaster-wt\dependabot-911\scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot .'
```

`-SearchRoot .` is mandatory; the script's single-search-root defect otherwise discovers assemblies
from a sibling worktree. The script enforces its own floors of 0.80 line and 0.75 branch. It prints
`Coverage projection: <path>` unconditionally on a successful run and `Test-result summary: <path>`
only when the trx yielded a summary; per **gate rule 12** the projection copy is mandatory, the
summary copy is mandatory when its line appears, and its absence is recorded verbatim with
`TEST-RESULT-SUMMARY: not produced — <reason>` rather than failing the task.

**CMD-OUTLOOK** — precondition for the two solution-wide `/t:Rebuild` commands and for nothing else.
`Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count`
must return `0` and the calling task records `OUTLOOK-CLOSED: true`. **Outlook must be closed by the
user, never killed.** This binds P0-T10, P0-T11, P5-T5 and P5-T6 only; it binds no per-project build.

**CMD-ACTIONLINT**

```
pwsh -NoProfile -Command 'Set-Location "$HOME\repos\TaskMaster-wt\dependabot-911"; & "$HOME\repos\TaskMaster-wt\dependabot-911\scripts\dev-tools\run-actionlint.ps1"'
```

A clean run prints nothing at all, so no count can be read from its output; the non-vacuity figure
comes from an independent filesystem enumeration, per **gate rule 10**.

---

### Phase 0 — Policy Reads, Anchors and Remediation Baseline Capture

- [x] [P0-T1] R0 — read the policy documents in the order `policy-compliance-order` defines, **from the execution worktree**: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`, `.claude/rules/powershell.md`, `.claude/rules/ci-workflows.md`, and the four review artifacts in `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`. Record into `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/remediation-baseline/phase0-instructions-read.2026-09-20T01-37.md` the `Timestamp:`, the `Policy Order:` as an ordered list, each file's path and SHA-256, and the coverage floors the execution worktree's `CLAUDE.md` states. Acceptance: all eleven files listed with a non-empty 64-character hash each; the recorded C# floors are line 80 and branch 75 and the recorded PowerShell floor is line 80 with no branch floor; and the artifact states that `.claude/rules/general-unit-test.md` says 85, that the conflict is open issue #668, and that gate rule 13 makes the `CLAUDE.md` figure authoritative. Fails if any file is unreadable, if a hash is absent, or if the recorded floors disagree with the file read.

- [x] [P0-T2] R0 — anchor the worktree into `evidence/remediation-baseline/p0-t2-anchor.2026-09-20T01-37.md`: record `git rev-parse --abbrev-ref HEAD`, `git rev-parse HEAD`, `git merge-base HEAD origin/main`, `git rev-list --count <MERGE_BASE>..HEAD`, and `git status --porcelain --untracked-files=all`. Acceptance: the branch is exactly `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`; `git merge-base --is-ancestor da7a6e3a0 HEAD` exits 0, which fails if the branch was reset or rebased away from the reviewed state; the commit count is recorded and is greater than 0; and per **gate rule 9** no `.cs`, `.csproj`, `.sln`, `packages.config` or `app.config` path appears among the modified or untracked entries. The head SHA is recorded, never asserted as a literal, because later commits move it.

- [x] [P0-T3] R4 — census the absolute host path. The scope is the union of `git diff --name-only <MERGE_BASE>..HEAD` and the paths in `git status --porcelain --untracked-files=all`, using the merge base P0-T2 recorded; scoping to the branch's own footprint is required because the repository carries the same literal in more than a thousand historical documents that this cycle must not touch. Build the search pattern at run time per **gate rule 17** and never type it. For each in-scope path, record the count of matching **lines** and the count of matching **occurrences**, and the distinct variants found, each recorded in placeholder-normalised form and never as the raw literal. Also record, per path, the count of pre-existing `<execution-worktree-root>`, `<session-worktree-root>`, `<repo-root>` and `<user-home>` placeholder occurrences, as `PLACEHOLDERS-BEFORE`. **One path is excluded by name from the rewrite and the reason is recorded:** `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` carries 4 occurrences naming a worktree dated 2026-07-04 and a canonical root, as **fixture input strings and expected values** of a path-rewriting test. It predates this branch, so it is expected to be **absent** from the branch diff and therefore out of scope automatically; rewriting it would change assertion inputs and break the suite. Record whether it appears in the branch diff, and record as an observation for the coordinator that a pre-existing host-path disclosure exists on `main` in that file, outside this cycle's remit and worth promoting as its own issue. Write to `evidence/remediation-baseline/p0-t3-hostpath-census.2026-09-20T01-37.md`. Acceptance: the derived pattern is recorded as the expression that built it rather than as its value; the totals are recorded as two distinct integers, labelled lines and occurrences, with the occurrence total at least 74 and the file count at least 27, being the review's measured figures which can only have grown since the review artifacts were added; at least 4 distinct normalised variants are enumerated, which fails if the census matched one spelling only; the enumerated file list contains `plan.2026-09-19T09-44.md`, `evidence/baseline/p0-t5-sdk-bootstrap.2026-09-19T09-44.md` and `evidence/baseline/p0-t8-dotnet-coverage.2026-09-19T09-44.md`; and it does **not** contain `evidence/qa-gates/p4-t5-actionlint.2026-09-19T09-44.md`, which is already sanitised and is the negative control, nor `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/remediation-plan.2026-09-20T01-37.md`, which is written under gate rule 17 and must carry no match. A total of 0, or a census listing either control file, is a failure, not a clean result.

- [x] [P0-T4] R7 — verify, read-only, that the coordinator applied the AC14 amendment given verbatim in the `Gate-Quality Rules Added by This Cycle` section of `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/remediation-plan.2026-09-20T01-37.md` to `spec.md`. Record into `evidence/remediation-baseline/p0-t4-spec-amendment.2026-09-20T01-37.md` the AC14 bullet verbatim, the count of lines matching `^- \[[ xX]\] \*\*AC\d+ ` in `spec.md`, and the last commit that touched `spec.md` from `git log -1 --format=%H -- <path>`. Acceptance: the AC14 bullet contains the exact sentence fragment `is not reachable from the` and the exact fragment `no -CandidateUpgrade`; the criterion count is exactly 26; AC14 remains ticked; and the recorded commit value is a non-empty 40-character hexadecimal string, which is empty for a never-committed path and is therefore falsifiable. **If the amendment is absent the executor records `AC14-AMENDMENT: absent`, halts, and reports to the coordinator.** Per **gate rule 19** the executor does not make the edit.

- [x] [P0-T5] R9d — capture the size and text baseline of every file this cycle edits into `evidence/remediation-baseline/p0-t5-size-and-text-baseline.2026-09-20T01-37.md`: the line count of `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `scripts/dependencies/ConsistencyVerifier.psm1`, `scripts/dependencies/ProjectConsistency.psm1`, `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1`, `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`, `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` and `.github/workflows/dependabot-repair.yml`; and, verbatim, the five lines of `.github/workflows/dependabot-repair.yml` that Phase 3 rewrites, identified by their text rather than their number: the `repair-count=` output line, the `beyond-known-weak=` assignment line, the commit step's `if:` line, the two `git config` lines, and the disclosure step's `name:` line. Acceptance: exactly 8 line counts recorded, every one an integer at most 500; and the six quoted workflow fragments recorded verbatim, each non-empty. Fails if any file already exceeds 500, which would mean the cycle starts over the cap.

- [x] [P0-T6] R0 — run CMD-POSHQC-FORMAT, then CMD-REVERT-OUT-OF-SCOPE-FORMAT against the derived pathspec, recording into `evidence/remediation-baseline/p0-t6-poshqc-format.2026-09-20T01-37.md` the exact `scan_folders` argument, the SHA-256 hash set for every `.ps1`, `.psm1` and `.psd1` under the four folders before and after, the derived pathspec, and the pre-revert and post-revert `git status --porcelain --untracked-files=all -- scripts/vscode` captures. Acceptance: both hash sets recorded; the derived set recorded explicitly, including the empty case as `REVERT-SET: empty`; the rewrite count recorded as the hash-difference count computed **after** the revert and excluding derived-set members; and the post-revert capture listing no derived-set member. `Formatted N files` is not the rewrite count, per **gate rule 6**.

- [x] [P0-T7] R0 — run CMD-POSHQC-ANALYZE and record into `evidence/remediation-baseline/p0-t7-poshqc-analyze.2026-09-20T01-37.md` the exact `scan_folders` argument, the integer finding total `N`, and the full finding list as `(file path, rule name, line)` tuples. Acceptance: `N` is recorded as an integer and is at least 1, and every tuple is enumerated; a total of 0 is a failure, not a clean result, because it is what a run that resolved no files reports. `N` and the tuple set are the baseline every later analyze task compares against; `MCP Result: ok:true` is not asserted and is expected to be false while the pre-existing findings remain.

- [x] [P0-T8] R2 — run CMD-PESTER-ALL with `<OUTPATH>` set to `coverage/p0-t8-pester-coverage.xml`, then CMD-JACOCO-PERFILE with `<DOC>` set to that path and `<LEAF>` set to `Sync-PackageReferences.ps1`. Record into `evidence/remediation-baseline/p0-t8-pester.2026-09-20T01-37.md` the `PESTER` counts line, the report-level LINE covered and missed, the per-file LINE covered and missed for all seven measured files, and the full `UNCOVERED=` list for `scripts/vscode/Sync-PackageReferences.ps1`. Acceptance: `EXIT_CODE: 0`; `Failed=0`; `Total` recorded and at least 302; the aggregate line percentage recorded and at least 80; the Sync file's covered plus missed recorded, expected 127; and the `UNCOVERED=` list **contains all nine of 151, 180, 248, 290, 293, 330, 336, 337 and 345**. That nine-member containment is this cycle's fail-before evidence for R2: it is true today and every one of the nine must be absent from the same list at P1-T10. The artifact states, per **gate rule 12**, that its recorded figures stand in for a permitted evidence form that does not exist for the Pester route, because all three permitted forms are defined against the C# Cobertura pipeline and Pester emits JaCoCo with no Cobertura stage.

- [x] [P0-T9] R0 — run CMD-CSHARPIER-CHECK and record the result in `evidence/remediation-baseline/p0-t9-csharpier-check.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, the verbatim `Checked N files in Xms.` line with `N` an integer greater than 900, and zero files reported with findings. Fails if the merge of `origin/main` brought an unformatted file.

- [x] [P0-T10] R0 — satisfy CMD-OUTLOOK, run CMD-MSBUILD-ANALYZERS and record the result in `evidence/remediation-baseline/p0-t10-msbuild-analyzers.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`; `OUTLOOK-CLOSED: true` recorded; exactly 0 lines containing `CS0006` in `coverage/analyzers.msbuild.log`; and at least 18 lines containing `/out:obj\Debug\` with the exact count recorded. The `/out:` count is the non-vacuity observation: a warm build that skipped every compile reports zero errors and zero such lines.

- [x] [P0-T11] R0 — satisfy CMD-OUTLOOK, run CMD-MSBUILD-NULLABLE and record the result in `evidence/remediation-baseline/p0-t11-msbuild-nullable.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`; `OUTLOOK-CLOSED: true` recorded; and at least 18 lines containing `/out:obj\Debug\` in `coverage/nullable.msbuild.log`, with the exact count recorded.

- [x] [P0-T12] R0 — run CMD-MSTEST-COVERAGE and record into `evidence/remediation-baseline/p0-t12-mstest-numeric-baseline.2026-09-20T01-37.md` the numeric line and branch coverage percentages the runner printed, the passed, failed and skipped counts, and the verbatim one-line first-party coverage report. Copy the file named by the run's `Coverage projection: <path>` line to `evidence/remediation-baseline/p0-t12-coverage-projection.2026-09-20T01-37.jacoco.xml`; that copy is mandatory. When a `Test-result summary: <path>` line appears, copy it to `evidence/remediation-baseline/p0-t12-test-results.2026-09-20T01-37.summary.txt`; when it does not, record the `Test-result summary was not written:` warning verbatim together with `TEST-RESULT-SUMMARY: not produced — <reason>` and do not fail. Acceptance: `EXIT_CODE: 0`; both percentages recorded as numbers, not placeholders; line at least 0.80 and branch at least 0.75; and the artifact states in terms that **this is the post-merge C# baseline for this cycle and supersedes the delivered `p9-t7` figures**, which were measured at `794d34f02` before the `origin/main` merge, per decision D6 and **gate rule 14**.

- [x] [P0-T13] R1 — probe the remote tooling this cycle depends on and record into `evidence/remediation-baseline/p0-t13-remote-probe.2026-09-20T01-37.md`: the verbatim output and exit code of `gh auth status`, of `gh run list --workflow=ci.yml --branch bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911 --limit 5 --json databaseId,headSha,conclusion,status`, of `gh api repos/drmoisan/TaskMaster/actions/secrets --jq ".secrets | length"`, and of `gh pr list --repo drmoisan/TaskMaster --state open --json number,headRefName --jq "length"`. Acceptance: all four invocations recorded with their exit codes and verbatim output; `GH-AVAILABLE: true` or `GH-AVAILABLE: false` recorded explicitly with the deciding output quoted; and the secrets query recorded as an integer count **or** as `403 FORBIDDEN` with `CREDENTIAL-PRESENT: unknown`, never as an empty list, because an empty list and a forbidden query are different states and only the first proves absence. Fails if any invocation's output is summarised rather than quoted.

---

### Phase 1 — R2: Negative and Error-Path Coverage for `Sync-PackageReferences.ps1`

Every test in this phase drives an existing correct behaviour through the script's injected seam. No
production file is edited, so the nine line citations remain valid throughout the phase, which is the
invariance **gate rule 14** requires of P0-T8's uncovered list. Every new `It` name begins `R2- ` and
no `Describe` or `Context` name added by this phase matches the regex `AC\d`, per **gate rule 11**.
The target file is `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, 185 lines at P0-T5, with a
**ceiling of 470 lines** for this phase.

- [x] [P1-T1] R2 — write the fail-before exception dossier to `evidence/regression-testing/fail-before-exception.2026-09-20T01-37.md`. It records `WhyFailingRunImpossible:` — the eight tests assert behaviour the production file already implements correctly, so no test can be shown failing before a fix, because the defect is the **absence** of tests rather than a wrong behaviour — and it supplies the alternative proof: the `UNCOVERED=` list P0-T8 recorded, quoted verbatim, containing all nine of lines 151, 180, 248, 290, 293, 330, 336, 337 and 345. Acceptance: the dossier carries `Timestamp:`, `WhyFailingRunImpossible:` and the quoted nine-member list, cites `evidence/remediation-baseline/p0-t8-pester.2026-09-20T01-37.md` by path, and names P1-T10 as the task that must observe every one of the nine absent from the same list. Fails if the quoted list omits any of the nine.

- [x] [P1-T2] R2 — add to `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` an `It` named exactly `R2- returns no identifier when the restore folder matches no manifest package`, calling `Resolve-ManifestPackageId` with `-FolderName 'Fabrikam.Core.1.0.0'` and a `-VersionMap` declaring only `Contoso.Widgets`, and asserting the result is empty. Run CMD-PESTER-FILTERED with `<FILE>` as that path and `<FILTER>` as `*R2- returns no identifier*`, recording into `evidence/regression-testing/p1-t2-line151.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, `Executed=1`, `Passed=1`, with `Total` and `NotRun` recorded as context. This exercises line 151, the `return ''` that ends `Resolve-ManifestPackageId`. Fails if the function returns a non-empty identifier for a folder no manifest key prefixes, which is the defect the assertion exists to catch.

- [x] [P1-T3] R2 — add an `It` named exactly `R2- returns no asset folder when the library directory is absent` to `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, calling `Resolve-PackageAssetFolder` with a seam whose `TestPath` delegate returns `$false` for every path, and asserting the result is empty and that the seam's `ListAssetFolder` delegate was never invoked. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R2- returns no asset folder*`, recording into `evidence/regression-testing/p1-t3-line180.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, `Executed=1`, `Passed=1`. This exercises line 180. Fails if the function enumerates a directory it has not confirmed exists.

- [x] [P1-T4] R2 — add an `It` named exactly `R2- warns and records no repair when no asset folder the target framework can consume ships the file` to `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, calling `Get-HintPathRepair -WarningVariable` over a project text carrying one stale `<HintPath>`, a version map whose identifier matches the restore folder at a different version, and a seam whose `TestPath` returns `$false` for both the current and the candidate path and whose `ListAssetFolder` offers only `@('netstandard2.1')`. Assert the returned repair set is empty and that the captured warning text contains the exact fragment `no asset folder the target framework can consume ships it`. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R2- warns and records no repair*`, recording into `evidence/regression-testing/p1-t4-line248.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, `Executed=1`, `Passed=1`, and the captured warning quoted verbatim in the artifact. This is the priority case of R2: line 248 is the handler for the exact condition issue #902 introduced, and AC7 asserts only what the selector returns, never what the script does with a rejection. Fails if the script binds an unconsumable asset folder, or emits no warning, or produces a repair record.

- [x] [P1-T5] R2 — add an `It` named exactly `R2- returns the project text unchanged when no Reference names the assembly` to `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, calling `Repair-ProjectReferenceVersion` with an `-AssemblyName` that appears in no `Include` attribute of the supplied text, and asserting the returned string equals the input exactly by `Should -BeExactly`. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R2- returns the project text unchanged when no Reference*`, recording into `evidence/regression-testing/p1-t5-line290.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, `Executed=1`, `Passed=1`. This exercises line 290. Fails if a non-matching assembly name mutates the text.

- [x] [P1-T6] R2 — add an `It` named exactly `R2- returns the project text unchanged when the Reference already names the resolved version` to `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, calling `Repair-ProjectReferenceVersion` with an `-AssemblyVersion` exactly equal to the four-part version the text's `Include` already declares, and asserting the returned string equals the input exactly. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R2- returns the project text unchanged when the Reference already*`, recording into `evidence/regression-testing/p1-t6-line293.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, `Executed=1`, `Passed=1`. This exercises line 293 and pins idempotence: applying the repair twice must be a no-op. Fails if an already-correct version is rewritten, which would make every second run a spurious change.

- [x] [P1-T7] R2 — add an `It` named exactly `R2- skips the manifest directory when no project file sits beside it` to `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, calling `Invoke-ProjectReferenceSync` with a seam whose `ListProjectPath` returns an empty array, and asserting the result's `Skipped` is `$true`, its `FixedCount` is 0, and the seam's `ReadText` delegate was never invoked. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R2- skips the manifest directory*`, recording into `evidence/regression-testing/p1-t7-line330.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, `Executed=1`, `Passed=1`. This exercises line 330. Fails if the function reads a project file it never found.

- [x] [P1-T8] R2 — add an `It` named exactly `R2- skips the project with a warning when merge conflict markers are present` to `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, calling `Invoke-ProjectReferenceSync -WarningVariable` with a seam whose `ReadText` returns a project text containing a seven-character conflict marker, and asserting `Skipped` is `$true`, `FixedCount` is 0, and the captured warning contains the exact fragment `Merge conflict markers detected, skipping`. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R2- skips the project with a warning*`, recording into `evidence/regression-testing/p1-t8-lines336-337.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, `Executed=1`, `Passed=1`, and the captured warning quoted verbatim. This exercises lines 336 and 337, the only pair of the nine discharged by one test, because the warning and its skip return are one behaviour. Fails if a conflicted file is rewritten, which would corrupt an in-progress merge.

- [x] [P1-T9] R2 — add an `It` named exactly `R2- returns an unskipped result with no fix when no hint path needs repair` to `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, calling `Invoke-ProjectReferenceSync` with a seam whose `TestPath` resolves every hint path, and asserting `Skipped` is `$false`, `FixedCount` is 0, and the seam's `WriteText` delegate was never invoked. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R2- returns an unskipped result*`, recording into `evidence/regression-testing/p1-t9-line345.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, `Executed=1`, `Passed=1`. This exercises line 345 and distinguishes the two zero-fix outcomes: a skipped project and an examined project that needed nothing. Fails if a clean project is written back, which would dirty the tree on every run.

- [x] [P1-T10] R2 — run CMD-PESTER-ALL with `<OUTPATH>` set to `coverage/p1-t10-pester-coverage.xml`, then CMD-JACOCO-PERFILE with `<LEAF>` set to `Sync-PackageReferences.ps1`, recording into `evidence/qa-gates/p1-t10-pester-coverage.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`; `Failed=0`; `Total` is exactly the `Total` P0-T8 recorded plus 8; the Sync file's covered plus missed is still 127, which fails if the production file was edited and would invalidate every line citation in this phase; the covered count is at least 104; **none of 151, 180, 248, 290, 293, 330, 336, 337 or 345 appears in the `UNCOVERED=` list**, each checked individually and each recorded as covered or not; the per-file covered count for every other measured file is at least the value P0-T8 recorded for that same file; and the aggregate line percentage is at least 80. The artifact carries the standing-in statement **gate rule 12** requires. Fails if any one of the nine is still listed as uncovered.

- [x] [P1-T11] R2 — write the coverage reconciliation for the file to `evidence/qa-gates/p1-t11-sync-coverage-reconciliation.2026-09-20T01-37.md`: the P0-T8 figure, the P1-T10 figure, the delta, and decision D5 restated with its arithmetic — 95 of 127 is 74.80 percent, the nine logic lines take it to 104 of 127 which is 81.89 percent, and 85 percent requires either executing the top-level invocation against a real tree or excluding the 19-line delegate table from measurement, the first prohibited by the external-dependency and temporary-file rules in `.claude/rules/general-unit-test.md` and the second by the Coverage Exclusion Policy in the same file and by `remediation-inputs.2026-09-20T01-37.md` in terms. Acceptance: all three figures recorded as numbers; the post-change figure is at least 80 and is recorded against both the authoritative 80 and the superseded 85 with issue #668 named; and the artifact states that the scenario-completeness clause is discharged by the eight named tests rather than by the percentage. Fails if the post-change figure is below 80, or if any figure is a placeholder.

- [x] [P1-T12] R2 — run CMD-POSHQC-FORMAT then CMD-REVERT-OUT-OF-SCOPE-FORMAT, recording the before and after hash sets, the derived pathspec and the pre- and post-revert porcelain captures into `evidence/qa-gates/p1-t12-poshqc-format.2026-09-20T01-37.md`. Acceptance: the rewrite count is recorded as the post-revert hash-difference count excluding derived-set members; the post-revert capture lists no derived-set member; and when that count is greater than zero the phase restarts from P1-T2 after the rewritten files are re-read.

- [x] [P1-T13] R2 — run CMD-POSHQC-ANALYZE and record into `evidence/qa-gates/p1-t13-poshqc-analyze.2026-09-20T01-37.md` the exact `scan_folders` argument, the integer total and the full tuple list. Acceptance: the total equals the `N` P0-T7 recorded; the finding count for `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, the one file this phase modified, is exactly 0; and every remaining finding is a member of the P0-T7 tuple set, compared element by element. The equality with `N` is the non-vacuity guard: a run that resolved no files reports 0, an owned count of 0 and a vacuously true subset over the empty set, so a total of 0 is a failure unless `N` is 0, which P0-T7 already forbids.

- [x] [P1-T14] R9d — record the line count of `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` into `evidence/qa-gates/p1-t14-size.2026-09-20T01-37.md`, together with the P0-T5 value and the difference. Acceptance: the count is an integer, is at most 470, and is greater than the P0-T5 value, which fails if the eight `It` blocks were not in fact added to that file. The 470 ceiling leaves headroom below the 500-line cap in `.claude/rules/general-code-change.md`; if the count exceeds 470 the executor halts and reports rather than splitting the file, because a new test file is outside the spec `## Write Set`.

- [x] [P1-T15] R2 — commit Phase 1 with an explicit pathspec covering `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` and `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, recording the head SHA in `evidence/qa-gates/p1-t15-commit.2026-09-20T01-37.md`. Use a single `-m` argument containing no `<`, `>`, `$` or backtick character. Acceptance: `git status --porcelain --untracked-files=all` captured verbatim after the commit and containing no entry outside `coverage/`; `git show --name-only --format= HEAD` captured and listing `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` and at least 12 paths under the feature folder, and listing no path under `scripts/`; and the head SHA differing from the value P0-T2 recorded.

---

### Phase 2 — R5, R9b and R9c: The Reference-Resolution Seam and the Composition Root

- [x] [P2-T1] R5 — move `Resolve-ReferenceAssemblyVersion` verbatim out of `scripts/dependencies/Repair-PackageManifestConsistency.ps1` and into `scripts/dependencies/ProjectConsistency.psm1`: delete the function and its comment-based help from the composition root, insert it unchanged into the module ahead of `Invoke-VersionReconciliation`, add `Import-Module (Join-Path $PSScriptRoot 'PackageCompatibility.psm1')` beside the module's two existing imports with a comment naming `Select-CompatibleAssetFolder` as the reason, add the function name to the module's `Export-ModuleMember` list and to its header `Exported functions:` list. Perform the edit with the `Write` or `Edit` tool, never `sed` through Bash, per **gate rule 15**. Acceptance: `Select-String -SimpleMatch 'function Resolve-ReferenceAssemblyVersion'` returns exactly 1 hit in `scripts/dependencies/ProjectConsistency.psm1` and exactly 0 in `scripts/dependencies/Repair-PackageManifestConsistency.ps1`; the module's exported-name list has exactly 3 members; both files' line counts are recorded in `evidence/qa-gates/p2-t1-extraction.2026-09-20T01-37.md`, with the composition root at most the P0-T5 value minus 30 and the module at most 380; and `git diff --numstat HEAD -- scripts/dependencies/ProjectConsistency.psm1 scripts/dependencies/Repair-PackageManifestConsistency.ps1` is recorded, anchored to `HEAD` because an unanchored diff compares the worktree against the index and passes vacuously once anything is staged, with the composition root's deletions at least 30 and the module's additions at least 30. Fails if the function exists in both files or in neither.

- [x] [P2-T2] R5 — add to `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` an `It` named exactly `R5- preserves a Reference assembly version the package version does not track`, driving `Invoke-ProjectConsistencyRepair` over a project text whose `<Reference Include="Contoso.Widgets, Version=4.5.6.7" />` carries an assembly version unrelated to the manifest's declared package version, and asserting the returned `ProjectText` still contains the exact fragment `Version=4.5.6.7` and that no repair record has `Kind` equal to `Reference`. Reuse the Reference-element fixture shape the existing `Invoke-VersionReconciliation` case in `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` already exercises, which is the proof that the parser classifies such a line as a reconciled `Reference` element and that the rewrite path is live rather than unreachable. Run CMD-PESTER-FILTERED with `<FILE>` as that path and `<FILTER>` as `*R5- preserves a Reference assembly version*`, recording into `evidence/regression-testing/p2-t2-r5-fail-before.2026-09-20T01-37.md`. **[expect-fail]** Acceptance: `Executed=1`, `Failed=1`, `EXIT_CODE: 1`, `ExpectedExitCode: 1`, and the failure message quoted verbatim showing the assembly version rewritten to the package version. Fails if the test passes before the fix, which would mean the defect the review reported is not present and P2-T3 has nothing to repair.

- [x] [P2-T3] R5 — in `scripts/dependencies/ConsistencyVerifier.psm1`, resolve the assembly version per package inside `Invoke-ProjectConsistencyRepair` and pass it: immediately before the `Invoke-VersionReconciliation` call in the package loop, call `Resolve-ReferenceAssemblyVersion -PackageId $entry.Id -PackageVersion $entry.Version -ProjectText $text` and add `-AssemblyVersion` carrying that value to the reconciliation call. Update the function's `.DESCRIPTION` to state the contract: the folder-segment kinds are reconciled to the manifest version, the declared Reference assembly version is **preserved** because this function has no assembly evidence, and a consumer needing evidence-based reference resolution uses the composition root. Acceptance: `Select-String -SimpleMatch '-AssemblyVersion'` returns at least 1 hit inside `scripts/dependencies/ConsistencyVerifier.psm1`; the file's line count is recorded in `evidence/qa-gates/p2-t3-r5-fix.2026-09-20T01-37.md` and is at most 500; and the `.DESCRIPTION` contains the exact fragment `preserves the declared Reference assembly version`. Fails if the line count exceeds 500, in which case the executor halts and reports rather than deleting unrelated content.

- [x] [P2-T4] R5 — re-run CMD-PESTER-FILTERED for `*R5- preserves a Reference assembly version*`, then run both criterion suites unfiltered: CMD-PESTER-FILTERED with `<FILE>` as `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` and `<FILTER>` as `*AC16-*`, and again with `<FILTER>` as `*AC21-*`. Record all three into `evidence/regression-testing/p2-t4-r5-pass-after.2026-09-20T01-37.md`. Acceptance: for the R5 filter, `Executed=1`, `Passed=1`, `EXIT_CODE: 0`, which is the pass-after half of the P2-T2 fail-before; for `*AC16-*`, `Executed=2` and `Failed=0`; for `*AC21-*`, `Executed=1` and `Failed=0`. Every `Executed` figure is `Passed + Failed + Skipped`, and `Total` and `NotRun` are recorded as context. The two criterion filters are the sibling check decision D1 turns on: they fail if removing the Reference rewrite turned a success result into a residual failure, which is the only way this edit could regress a delivered criterion.

- [x] [P2-T5] R9b — add a call-site comment in `scripts/dependencies/Repair-PackageManifestConsistency.ps1` immediately above the unfiltered `Get-AnalyzerAssemblyPath` invocation, stating that the unfiltered result is a **verification membership set** consumed only by the `-contains` test below and must never be written to a project file, and that a caller wanting a writable path supplies `-PreservedSegment`. **The call site is located by its literal text `Get-AnalyzerAssemblyPath -PackageId $identity.Id`, not by line number**: P2-T1 removed roughly 35 lines above it, so the review's citation of line 320 no longer names it, which is **gate rule 14**'s class reached through a positional citation. Acceptance: `Select-String -SimpleMatch 'verification membership set'` returns exactly 1 hit in that file; the hit's line number is recorded and is within 3 lines above the `Get-AnalyzerAssemblyPath -PackageId $identity.Id` line, whose number is recorded alongside; both in `evidence/qa-gates/p2-t5-r9b-comment.2026-09-20T01-37.md`. Fails if the comment lands beside a different call site.

- [x] [P2-T6] R9c — add a `Write-Verbose` inside `$script:DefaultFileLister` in `scripts/dependencies/Repair-PackageManifestConsistency.ps1` emitting the count of enumerated directories and the count of returned files, and add to `tests/scripts/dependencies/DependabotConfig.Tests.ps1` an `It` named exactly `R9c- records the enumerated directory count in the default manifest lister`, which reads the composition root's text and asserts it contains both the exact fragment `Write-Verbose` within the `$script:DefaultFileLister` block and the exact fragment `enumerated director`. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R9c- records the enumerated directory count*`, recording into `evidence/qa-gates/p2-t6-r9c-lister-visibility.2026-09-20T01-37.md`. Acceptance: `Executed=1`, `Passed=1`, `EXIT_CODE: 0`, and the artifact stating plainly, per decision D4, that this makes a shortfall **observable in the run log** and does not prevent a nested project from being skipped, the recursive-walk alternative having been rejected as an unguarded behaviour change. The assertion is a text assertion over production source and is described as such; it fails today, before the edit, which is what makes it a real gate rather than a restatement.

- [x] [P2-T7] R5 — run CMD-PESTER-ALL with `<OUTPATH>` set to `coverage/p2-t7-pester-coverage.xml`, recording into `evidence/qa-gates/p2-t7-pester-coverage.2026-09-20T01-37.md` the counts line, the report-level LINE figures and the per-file LINE covered and missed for all seven measured files. Acceptance: `EXIT_CODE: 0`; `Failed=0`; `Total` is exactly the P1-T10 `Total` plus 2; the aggregate line percentage is at least 80; each of the six files under `scripts/dependencies/` is at least 90, which is this change's own stricter local requirement under AC24 and is unaffected by the floor; and the per-file covered count for `scripts/vscode/Sync-PackageReferences.ps1` is at least the value P1-T10 recorded. The artifact carries the standing-in statement **gate rule 12** requires. Fails if moving a 35-line function between two measured files dropped either below 90.

- [x] [P2-T8] R5 — run CMD-POSHQC-FORMAT then CMD-REVERT-OUT-OF-SCOPE-FORMAT, recording hash sets, the derived pathspec and both porcelain captures into `evidence/qa-gates/p2-t8-poshqc-format.2026-09-20T01-37.md`. Acceptance as at P1-T12, and when the post-revert rewrite count is greater than zero the phase restarts from P2-T7 after the rewritten files are re-read.

- [x] [P2-T9] R5 — run CMD-POSHQC-ANALYZE and record into `evidence/qa-gates/p2-t9-poshqc-analyze.2026-09-20T01-37.md` the exact `scan_folders` argument, the integer total and the full tuple list. Acceptance: the total equals the `N` P0-T7 recorded; the finding count is exactly 0 for each of the five files this phase modified, enumerated as `scripts/dependencies/ProjectConsistency.psm1`, `scripts/dependencies/ConsistencyVerifier.psm1`, `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` and `tests/scripts/dependencies/DependabotConfig.Tests.ps1`, the last being the file P2-T6 added the R9c assertion to; and every remaining finding is a member of the P0-T7 tuple set. A total of 0 is a failure unless `N` is 0.

- [x] [P2-T10] R9d — record into `evidence/qa-gates/p2-t10-size.2026-09-20T01-37.md` the line counts of `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `scripts/dependencies/ConsistencyVerifier.psm1`, `scripts/dependencies/ProjectConsistency.psm1`, `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` and `tests/scripts/dependencies/DependabotConfig.Tests.ps1`, each beside its P0-T5 value. Acceptance: exactly 5 counts recorded, every one at most 500; the composition root's count is **strictly less** than its P0-T5 value, which fails if the extraction did not actually remove the function; and `ProjectConsistency.psm1` is strictly greater than its P0-T5 value, which fails if it did not actually arrive. The two-sided pair is the check: a size audit that only bounds above is satisfied by a move that never happened.

- [x] [P2-T11] R5 — commit Phase 2 with an explicit pathspec covering `scripts/dependencies/ProjectConsistency.psm1`, `scripts/dependencies/ConsistencyVerifier.psm1`, `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1` and `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, recording the head SHA in `evidence/qa-gates/p2-t11-commit.2026-09-20T01-37.md`. Single `-m`, no `<`, `>`, `$` or backtick character. Acceptance: porcelain captured verbatim after the commit with no entry outside `coverage/`; `git show --name-only --format= HEAD` listing all five source paths named above and no path under `.github/`; and the head SHA differing from the value P1-T15 recorded.

---

### Phase 3 — R3, R6, R7 and R8: The Repair Workflow

Per **gate rule 20**, every task in this phase states its verification route and its residual. The
workflow has never executed, so nothing here observes runtime behaviour of the GitHub API steps. What
**is** verified locally: the workflow's own text, by named Pester assertions that fail before the edit
and pass after; the composed marker-strip expression, by applying the workflow's own literal pattern
to a synthetic body; and the write-set quantity R3's new gate reads, by a unit assertion over the
composition root driven through its injected delegates. What **remains unverifiable until the #914
credential exists**: that the token step publishes an `app-slug` output, that the resolved bot user id
produces a commit whose `author.login` ends `[bot]`, that the push causes the required checks to
re-run, and that the disclosure edit produces exactly one block on a real pull-request body. The
target test files are `tests/scripts/dependencies/DependabotConfig.Tests.ps1`, 335 lines at P0-T5,
ceiling **470**, and `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`, 382
lines at P0-T5, ceiling **440**.

- [x] [P3-T1] R3 R6 R7 R8 — add four `It` blocks to `tests/scripts/dependencies/DependabotConfig.Tests.ps1`, each reading `.github/workflows/dependabot-repair.yml` as text: `R3- gates the commit step on the write-set count rather than the repair count` asserting the file contains the exact fragment `written-count=` and that the commit step's `if:` line contains `written-count` and does not contain `repair-count`; `R6- guards the disclosure step and replaces a delimited block` asserting the disclosure step has an `if:` line naming both `written-count` and `skip-count` and that the file contains both marker literals `<!-- dependabot-repair:begin -->` and `<!-- dependabot-repair:end -->`; `R7- counts beyond-known-weak repairs with the analyzer exclusion alone` asserting the file contains the exact single-line fragment `Where-Object { $_ -ne 'Analyzer' }` and contains the exact fragment `not reachable from the workflow_run trigger`; and `R8- derives the commit identity from the token step outputs` asserting the file contains the exact fragment `steps.app-token.outputs.app-slug`, the exact fragment `users/`, and does not contain the literal `dependabot-repair[bot]@users.noreply.github.com`. Run CMD-PESTER-FILTERED with `<FILE>` as that path and `<FILTER>` as `*R3- gates*`, then `*R6- guards*`, then `*R7- counts*`, then `*R8- derives*`, recording all four into `evidence/regression-testing/p3-t1-workflow-fail-before.2026-09-20T01-37.md`. **[expect-fail]** Acceptance: each of the four runs records `Executed=1` and `Failed=1`, `EXIT_CODE: 1` and `ExpectedExitCode: 1`, with each failure message quoted verbatim. Four failures is the fail-before evidence for the whole phase; a passing assertion here would mean the finding it encodes is not present.

- [x] [P3-T2] R3 — in `.github/workflows/dependabot-repair.yml`, add `"written-count=$(@($result.WrittenPath).Count)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append` to the repair step beside the existing outputs, and change the commit step's condition to `if: steps.repair.outputs.written-count != '0'`. Leave `repair-count` published, because the disclosure body and the beyond-known-weak label still read repair records. Add a comment above the new output naming the defect: `RepairCount` counts per-project repair records only, so a run whose only writes are manifest normalisation or binding-redirect reconciliation reports zero, skips the push, and discards the repair; `WrittenPath` already carries every written path. Acceptance: recorded into `evidence/qa-gates/p3-t2-r3-write-gate.2026-09-20T01-37.md`, the new output line and the new `if:` line quoted verbatim, and `git diff --numstat HEAD -- .github/workflows/dependabot-repair.yml` recorded, anchored to `HEAD` rather than left to compare against the index, with at least 1 addition and exactly 1 deletion, the single deletion being the replaced condition line. **Verified without a live run** by the P3-T1 assertion and by P3-T3's unit assertion over the quantity the gate now reads. **Unverifiable until #914:** that a real repair run pushes.

- [x] [P3-T3] R3 — add to `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` an `It` named exactly `R3- reports a non-zero write count for a run whose only change is a normalisation`, driving the composition root through the existing in-memory fixture harness with a `packages.config` in the wrapped multi-line form and a project file already agreeing with it, and asserting the result's `RepairCount` is exactly 0 while `@($result.WrittenPath).Count` is exactly 1 and the single written path is the manifest. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R3- reports a non-zero write count*`, recording into `evidence/regression-testing/p3-t3-r3-write-set.2026-09-20T01-37.md`. Acceptance: `Executed=1`, `Passed=1`, `EXIT_CODE: 0`, and both figures quoted. This is the load-bearing half of R3: it demonstrates a run in which the **old** gate reads 0 and the **new** gate reads 1, which is exactly the silent-discard case the review described, and it fails if `WrittenPath` does not in fact move for a normalisation-only run, in which case `written-count` would be the wrong quantity to gate on.

- [x] [P3-T4] R6 — in `.github/workflows/dependabot-repair.yml`, add `if: steps.repair.outputs.written-count != '0' || steps.repair.outputs.skip-count != '0'` to the disclosure step, and change the body composition to wrap the report between the literal markers `<!-- dependabot-repair:begin -->` and `<!-- dependabot-repair:end -->` and to strip any prior block from the existing body before appending, using the literal pattern `(?s)<!-- dependabot-repair:begin -->.*?<!-- dependabot-repair:end -->` through `[regex]::Replace`. The guard names `skip-count` as well as `written-count` because AC20 requires the skipped block whenever the run recorded a skip, and a run that skipped an incompatible package without writing anything must still disclose. Acceptance: recorded into `evidence/qa-gates/p3-t4-r6-disclosure-guard.2026-09-20T01-37.md`, the new `if:` line and the full rewritten body-composition block quoted verbatim, and the two marker literals and the pattern literal each present exactly once in the file. **Verified without a live run** by P3-T1 and P3-T5. **Unverifiable until #914:** that a second run against a real pull-request body yields exactly one block.

- [x] [P3-T5] R6 — add to `tests/scripts/dependencies/DependabotConfig.Tests.ps1` an `It` named exactly `R6- replaces rather than appends a previously disclosed block`, which assigns the pattern literal `(?s)<!-- dependabot-repair:begin -->.*?<!-- dependabot-repair:end -->` to a variable, **asserts that `.github/workflows/dependabot-repair.yml` contains that exact literal**, then constructs a synthetic body already carrying one delimited block, applies `[regex]::Replace` with that variable and appends a fresh delimited block, and asserts the result contains exactly one occurrence of the begin marker and exactly one of the end marker and still contains the body's original leading text. Run CMD-PESTER-FILTERED with `<FILTER>` as `*R6- replaces rather than appends*`, recording into `evidence/regression-testing/p3-t5-r6-idempotence.2026-09-20T01-37.md`. Acceptance: `Executed=1`, `Passed=1`, `EXIT_CODE: 0`. The containment assertion is what makes this a test of the workflow rather than of a pattern the test invented: it fails if the workflow's expression and the test's expression ever diverge by a character.

- [x] [P3-T6] R7 — in `.github/workflows/dependabot-repair.yml`, change the `$beyondKnownWeak` assignment so its filter reads exactly `Where-Object { $_ -ne 'Analyzer' }`, and add a comment above it stating that the binding-redirect class is **not reachable from the `workflow_run` trigger** because the entry point is invoked with no `-CandidateUpgrade`, that the call site keeps only the reconciled text and discards the `Kind = 'BindingRedirect'` record, and that the decision is recorded in the AC14 note in `spec.md` and in the code review dated 2026-09-20. Acceptance: recorded into `evidence/qa-gates/p3-t6-r7-dead-filter.2026-09-20T01-37.md`, the rewritten single line quoted verbatim and matching the fragment above exactly, the comment quoted verbatim, and the count of `BindingRedirect` occurrences in the file recorded as exactly 1, that one being inside the comment. The exact-1 figure is the positive counterpart **gate rule 2** requires: a bare zero-count assertion is also satisfied by deleting the explanation, and the explanation is the point. **Verified without a live run** by the P3-T1 `R7- counts` assertion, which fails today because the file contains neither the single-clause fragment nor the reachability comment, and by the static reading of the call site: the entry point is invoked with no `-CandidateUpgrade`, and the call site keeps `.Text` and discards the `Kind = 'BindingRedirect'` record, so no record of that kind can reach the filter. **Unverifiable until #914:** that no execution path under the `workflow_run` trigger produces a `BindingRedirect` record. The static reading covers the configured trigger as authored; it cannot cover a trigger or an invocation this cycle does not write. If a later change supplies `-CandidateUpgrade`, the removed clause becomes load-bearing again, and the comment this task adds is what tells that author so.

- [x] [P3-T7] R8 — in `.github/workflows/dependabot-repair.yml`, replace the two hard-coded `git config` lines in the commit step with a run-time derivation: read the slug from `${{ steps.app-token.outputs.app-slug }}` into a variable, fail the step with a named error when it is empty, resolve the bot user id with `gh api "/users/$([uri]::EscapeDataString($slug + '[bot]'))" --jq .id`, fail the step with a named error when that is empty, then set `user.name` to `<slug>[bot]` and `user.email` to `<id>+<slug>[bot]@users.noreply.github.com`. Add a comment stating that the numeric part is the **bot user's** id and not the app id, that GitHub resolves `author.login` by matching the commit author email to an account, and that a hand-written literal resolves to null and makes AC18 unsatisfiable. Acceptance: recorded into `evidence/qa-gates/p3-t7-r8-commit-identity.2026-09-20T01-37.md`, the full rewritten commit-step block quoted verbatim; the file contains exactly 0 occurrences of the literal `dependabot-repair[bot]@users.noreply.github.com`; and it contains exactly 2 explicit empty-value guards, one per derived value, each quoted. **Verified without a live run** by P3-T1 and by P3-T9's actionlint pass. **Unverifiable until #914:** that `actions/create-github-app-token@v3` publishes `app-slug`, which decision D3 records as an assumption of record, and that the resulting `author.login` satisfies AC18. The two guards are what convert a wrong assumption into a loud step failure instead of a silent bad identity.

- [x] [P3-T8] R3 R6 R7 R8 — re-run the four P3-T1 filters and the two behavioural filters `*R3- reports a non-zero write count*` and `*R6- replaces rather than appends*`, recording all six into `evidence/regression-testing/p3-t8-workflow-pass-after.2026-09-20T01-37.md`. Acceptance: each run records `Executed=1`, `Passed=1` and `EXIT_CODE: 0`, six runs in total, each named by its filter; and the artifact tabulates, per finding, the P3-T1 failure message and the corresponding pass, which is the fail-before-and-pass-after pair for all four workflow findings.

- [x] [P3-T9] R3 R6 R7 R8 — run CMD-ACTIONLINT and record into `evidence/qa-gates/p3-t9-actionlint.2026-09-20T01-37.md` the exit code, the verbatim output, and an independent filesystem enumeration `Get-ChildItem .github/workflows -Filter *.yml | Measure-Object`. Acceptance: `EXIT_CODE: 0`; the output recorded verbatim and expected to be empty; and the enumeration count is exactly 9, recorded with the statement that it is a filesystem enumeration and **not** actionlint output, per **gate rule 10**, because actionlint prints nothing at all on a clean run and no count can be read from it. The count is 9 before and after this phase because this cycle adds no workflow file.

- [x] [P3-T10] R3 R6 R7 R8 — record the workflow footprint into `evidence/qa-gates/p3-t10-workflow-footprint.2026-09-20T01-37.md`: `git diff --name-only <MERGE_BASE>..HEAD -- .github/workflows`, `git diff --numstat HEAD -- .github/workflows/dependabot-repair.yml`, and `git status --porcelain --untracked-files=all -- .github`. Acceptance: the name list contains exactly the same 6 paths the review enumerated — `_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml`, `_pester.yml`, `dependabot-repair.yml` and `README.md` — with no seventh, which fails if this phase touched a workflow it had no business touching; and the numstat for `dependabot-repair.yml` records at least 12 additions and at least 6 deletions, which fails if one of the four edits was not in fact applied. The anchored diff and the porcelain capture are paired per **gate rule 8**, each being blind in the state the other covers.

- [x] [P3-T11] R3 R6 R7 R8 — run CMD-PESTER-ALL with `<OUTPATH>` set to `coverage/p3-t11-pester-coverage.xml`, recording into `evidence/qa-gates/p3-t11-pester-coverage.2026-09-20T01-37.md` the counts line, the report-level LINE figures and the per-file LINE figures. Acceptance: `EXIT_CODE: 0`; `Failed=0`; `Total` is exactly the P2-T7 `Total` plus 6; the aggregate line percentage is at least 80; each file under `scripts/dependencies/` is at least 90; and the per-file covered count for every measured file is at least the value P2-T7 recorded. The artifact carries the standing-in statement **gate rule 12** requires.

- [x] [P3-T12] R3 R6 R7 R8 — run CMD-POSHQC-FORMAT then CMD-REVERT-OUT-OF-SCOPE-FORMAT, recording hash sets, the derived pathspec and both porcelain captures into `evidence/qa-gates/p3-t12-poshqc-format.2026-09-20T01-37.md`. Acceptance as at P1-T12; when the post-revert rewrite count is greater than zero the phase restarts from P3-T11 after the rewritten files are re-read.

- [x] [P3-T13] R3 R6 R7 R8 — run CMD-POSHQC-ANALYZE and record into `evidence/qa-gates/p3-t13-poshqc-analyze.2026-09-20T01-37.md` the exact `scan_folders` argument, the integer total and the full tuple list. Acceptance: the total equals the `N` P0-T7 recorded; the finding count is exactly 0 for each of the two files this phase modified, enumerated as `tests/scripts/dependencies/DependabotConfig.Tests.ps1` and `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`; and every remaining finding is a member of the P0-T7 tuple set.

- [x] [P3-T14] R9d — record into `evidence/qa-gates/p3-t14-size.2026-09-20T01-37.md` the line counts of `tests/scripts/dependencies/DependabotConfig.Tests.ps1`, `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` and `.github/workflows/dependabot-repair.yml`, each beside its P0-T5 value. Acceptance: exactly 3 counts recorded; `DependabotConfig.Tests.ps1` at most 470 and strictly greater than its P0-T5 value; `Repair-PackageManifestConsistency.Tests.ps1` at most 440 and strictly greater than its P0-T5 value; and the workflow at most 500. A count over its ceiling halts the phase for a report rather than authorising a new file, which would fall outside the spec `## Write Set`.

- [x] [P3-T15] R3 R6 R7 R8 — commit Phase 3 with an explicit pathspec covering `.github/workflows/dependabot-repair.yml`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1`, `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` and `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, recording the head SHA in `evidence/qa-gates/p3-t15-commit.2026-09-20T01-37.md`. Single `-m`, no `<`, `>`, `$` or backtick character. Acceptance: porcelain captured verbatim after the commit with no entry outside `coverage/`; `git show --name-only --format= HEAD` listing all three source paths and no path under `scripts/`; and the head SHA differing from the value P2-T11 recorded.

---

### Phase 4 — R4 and R9a: Host-Path Sanitisation and the Autoclose List

- [x] [P4-T1] R4 — derive the substitution map from the P0-T3 census and record it into `evidence/qa-gates/p4-t1-substitution-map.2026-09-20T01-37.md`. The map is **ordered longest-first**, because each root is a prefix of the shorter one below it and an unordered pass would leave partial rewrites: `<user-home>\repos\TaskMaster-wt\dependabot-911` to `<execution-worktree-root>`; `<user-home>\repos\TaskMaster-wt\2026-09-12T10-15` to `<session-worktree-root>`; `<user-home>\repos\TaskMaster` to `<repo-root>`; bare `<user-home>` to `<user-home>`; and, for each of the four, its forward-slash spelling and its 8.3 spelling. Every source literal is **built at run time** from `$HOME` and from the 8.3 account name derived as `(Split-Path $HOME -Leaf).Substring(0, 6) + '~1'`, per **gate rule 17**; the artifact records the building expression and the replacement token for each entry, never the source literal's value. Acceptance: the map records at least 8 entries, each with its replacement token and its building expression; the ordering is recorded and each entry's source literal length is at least the next one's, the lengths being recorded as integers; and the artifact states that substitution is performed with `[regex]::Replace` under `IgnoreCase` rather than `String.Replace`, because `String.Replace` is case-sensitive and the census matched case-insensitively, so a mixed-case spelling would survive a case-sensitive pass and fail P4-T3. Fails if fewer than 8 entries are derived, if any census variant has no map entry, or if any source literal's value appears in the artifact.

- [x] [P4-T2] R4 — rewrite every file the P0-T3 census listed, byte-exactly, with PowerShell `[System.IO.File]::ReadAllText` and `WriteAllText` and the P4-T1 map applied in order. **`sed` through the Bash tool is prohibited** per **gate rule 15**: the tool collapses doubled backslashes before `sed` parses them, so a pattern naming a Windows path arrives matching nothing, and `sed -i` then rewrites every file's line endings anyway, producing 29 modified files with no content change. Record into `evidence/qa-gates/p4-t2-sanitisation.2026-09-20T01-37.md` the enumerated file list, and per file the pre and post line counts and the per-map-entry replacement counts. Acceptance: the file list is exactly the P0-T3 list minus the one named exclusion, and the artifact records whether that exclusion was in scope at all; every file's post line count **equals** its pre line count, which fails on any rewrite that inserted or removed a line; and the summed replacement count equals the occurrence total P0-T3 recorded, less any occurrence attributed to the exclusion.

- [x] [P4-T3] R4 — verify the residual and its positive counterpart into `evidence/qa-gates/p4-t3-residual.2026-09-20T01-37.md`. Over the same scope P0-T3 defined, recompute the census with the same run-time-derived pattern, and recompute the placeholder occurrence count for the four tokens. Also record `git diff --numstat <P3-T15-head-sha>` for the rewritten files. Acceptance: the host-path occurrence total is exactly **0** and the matching-file count is exactly **0**; the placeholder occurrence total equals the P0-T3 occurrence total **plus** the `PLACEHOLDERS-BEFORE` total P0-T3 recorded, which is the positive counterpart and fails on a rewrite that deleted text instead of replacing it; and for every rewritten file the numstat additions equal the deletions, which fails on a line-count change a pure substitution cannot produce. A zero residual alone is not acceptance: per **gate rule 2** it is also what an empty scope reports, and the placeholder equality is what distinguishes the two.

- [x] [P4-T4] R9a — strip `#MEZIANTOU-898` and `#SHA-256` from **both** close-candidate sections of `artifacts/pr_context.summary.txt`, and from `artifacts/pr_context.appendix.txt` if the census found them there, recording the before and after lists verbatim into `evidence/qa-gates/p4-t4-autoclose-list.2026-09-20T01-37.md`. Acceptance: the after list in each section is exactly the 11 members `#181`, `#563`, `#668`, `#895`, `#898`, `#902`, `#903`, `#907`, `#908`, `#909`, `#911`, in that order and with no twelfth; the two stripped tokens have 0 occurrences in either file; and the artifact records that `artifacts/` is gitignored at `.gitignore:57`, so the edit is to an untracked working file and no commit lists it. The artifact additionally records the standing instruction that **if `pr_context` is regenerated before the pull-request body is authored, the same two tokens must be stripped again**, because the detector re-derives them from the words `Meziantou.Analyzer` and `SHA-256` in prose and a regenerated file reintroduces them.

- [x] [P4-T5] R4 — verify the rewrite touched documentation only, into `evidence/qa-gates/p4-t5-md-only.2026-09-20T01-37.md`: capture `git status --porcelain --untracked-files=all` and `git diff --name-only <P3-T15-head-sha>`. Acceptance: every path in the union ends `.md`, and the count of such paths equals the P0-T3 file count; a path with any other extension is a failure, which is what catches a map entry that matched inside a script or a workflow. Both captures are recorded because per **gate rule 8** the anchored diff cannot report an untracked file and porcelain goes empty once the change is committed.

- [x] [P4-T6] R4 — commit Phase 4 with an explicit pathspec limited to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, recording the head SHA in `evidence/qa-gates/p4-t6-commit.2026-09-20T01-37.md`. Single `-m`, no `<`, `>`, `$` or backtick character. Acceptance: porcelain captured verbatim after the commit with no entry outside `coverage/` and `artifacts/`; `git show --name-only --format= HEAD` listing at least the P0-T3 file count of paths, every one ending `.md` and every one under the feature folder; and the head SHA differing from the value P3-T15 recorded. `artifacts/` is expected in porcelain only if git tracks it, which `.gitignore:57` says it does not; an entry there is recorded as an observation, not a failure.

---

### Phase 5 — Final QA Loop, Coverage Reconciliation and Footprint

The loop is format, lint, type-check, test, run in that order for each language. **If any step fails
or rewrites a file, the loop restarts from P5-T1** and every artifact of the abandoned iteration is
retained with an `iterN` suffix in its filename. The PowerShell steps run first because they are
cheap; the C# steps follow and are the expensive ones.

- [ ] [P5-T1] R0 — PowerShell QA step 1: run CMD-POSHQC-FORMAT then CMD-REVERT-OUT-OF-SCOPE-FORMAT, recording hash sets, the derived pathspec and both porcelain captures into `evidence/qa-gates/p5-t1-poshqc-format.iter1.2026-09-20T01-37.md`. Acceptance: the post-revert hash-difference count excluding derived-set members is **0**, and the post-revert capture lists no derived-set member. A non-zero count restarts the loop at P5-T1 after the rewritten files are re-read; `Formatted N files` is not that count, per **gate rule 6**.

- [ ] [P5-T2] R0 — PowerShell QA step 2: run CMD-POSHQC-ANALYZE and record into `evidence/qa-gates/p5-t2-poshqc-analyze.iter1.2026-09-20T01-37.md` the exact `scan_folders` argument, the integer total and the full tuple list. Acceptance: the total equals the `N` P0-T7 recorded; the finding count is exactly 0 for each of the **seven** PowerShell files this cycle modified, enumerated as `scripts/dependencies/ProjectConsistency.psm1`, `scripts/dependencies/ConsistencyVerifier.psm1`, `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1` and `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`; and every remaining finding is a member of the P0-T7 tuple set, compared element by element. A total of 0 is a failure unless `N` is 0.

- [ ] [P5-T3] R2 — PowerShell QA step 3: run CMD-PESTER-ALL with `<OUTPATH>` set to `coverage/p5-t3-pester-coverage.iter1.xml`, then CMD-JACOCO-PERFILE with `<LEAF>` set to `Sync-PackageReferences.ps1`, recording into `evidence/qa-gates/p5-t3-pester-coverage.iter1.2026-09-20T01-37.md` the counts line, the report-level LINE figures, the per-file LINE figures for all seven measured files, and the `UNCOVERED=` list for the Sync file. Acceptance: `EXIT_CODE: 0`; `Failed=0`; `Skipped=0`; `Total` equals the P3-T11 `Total`; the aggregate line percentage at least 80; each of the six files under `scripts/dependencies/` at least 90; the Sync file at least 80 with its covered count at least 104; and none of the nine lines 151, 180, 248, 290, 293, 330, 336, 337 or 345 present in the `UNCOVERED=` list. The artifact carries the standing-in statement **gate rule 12** requires.

- [ ] [P5-T4] R0 — C# QA step 1: run CMD-CSHARPIER-CHECK and record into `evidence/qa-gates/p5-t4-csharpier-check.iter1.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`, the verbatim `Checked N files in Xms.` line with `N` an integer greater than 900, and zero files reported with findings.

- [ ] [P5-T5] R0 — C# QA step 2: satisfy CMD-OUTLOOK, run CMD-MSBUILD-ANALYZERS and record into `evidence/qa-gates/p5-t5-msbuild-analyzers.iter1.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`; `OUTLOOK-CLOSED: true`; exactly 0 lines containing `CS0006` in `coverage/analyzers.msbuild.log`; and at least 18 lines containing `/out:obj\Debug\`, with the exact count recorded as the non-vacuity observation per **gate rule 7**.

- [ ] [P5-T6] R0 — C# QA step 3: satisfy CMD-OUTLOOK, run CMD-MSBUILD-NULLABLE and record into `evidence/qa-gates/p5-t6-msbuild-nullable.iter1.2026-09-20T01-37.md`. Acceptance: `EXIT_CODE: 0`; `OUTLOOK-CLOSED: true`; and at least 18 lines containing `/out:obj\Debug\` in `coverage/nullable.msbuild.log`, with the exact count recorded.

- [ ] [P5-T7] R0 — C# QA step 4: run CMD-MSTEST-COVERAGE and record into `evidence/qa-gates/p5-t7-mstest-coverage.iter1.2026-09-20T01-37.md` the numeric line and branch percentages, the passed, failed and skipped counts, and the verbatim one-line first-party report. Copy the projection the run named to `evidence/qa-gates/p5-t7-coverage-projection.2026-09-20T01-37.jacoco.xml`, mandatory; copy the test-result summary to `evidence/qa-gates/p5-t7-test-results.2026-09-20T01-37.summary.txt` when its line appears, and otherwise record the warning verbatim with `TEST-RESULT-SUMMARY: not produced — <reason>` and do not fail. Acceptance: `EXIT_CODE: 0`; both percentages recorded as numbers; line at least 0.80 and branch at least 0.75.

- [ ] [P5-T8] R0 — record the single-pass attestation into `evidence/qa-gates/p5-t8-toolchain-attestation.2026-09-20T01-37.md`, citing the seven artifacts from P5-T1 through P5-T7 by path and recording their `EXIT_CODE` values and timestamps. Acceptance: every cited artifact exists; the PowerShell steps and the four C# steps all recorded a passing outcome as each task defines it; the seven timestamps are strictly increasing, which proves they ran in order within one pass; and no cited artifact belongs to an earlier loop iteration. If any does, the attestation fails and the loop restarts from P5-T1.

- [ ] [P5-T9] R2 — record the coverage reconciliation into `evidence/qa-gates/p5-t9-coverage-reconciliation.2026-09-20T01-37.md`: for C#, the P0-T12 baseline line and branch figures, the P5-T7 post-change figures and the two deltas; for PowerShell, the P0-T8 aggregate, the P5-T3 aggregate, the per-new-module figures and the `Sync-PackageReferences.ps1` before-and-after pair. Acceptance: every figure is a number and not a placeholder; the C# post-change line is at least 0.80 and branch at least 0.75; each C# delta is at least minus 0.005 in the runner's fractional units, a delta between minus 0.005 and 0 being recorded as measurement noise rather than a regression; the PowerShell aggregate is at least 80 and is at least the P0-T8 value; every module under `scripts/dependencies/` is at least 90; and the artifact states that the C# baseline is P0-T12's post-merge measurement and **not** the delivered `p9-t7` figures, naming decision D6 and **gate rule 14** as the reason. It also states that no branch figure exists for PowerShell and names the tooling reason: Pester emits no BRANCH counter in any output format.

- [ ] [P5-T10] R9d — audit file size across the whole footprint into `evidence/qa-gates/p5-t10-file-size-audit.2026-09-20T01-37.md`: the line count of every path in the spec `## Write Set` under "Production PowerShell" and "Tests", plus `.github/workflows/dependabot-repair.yml` and `.github/workflows/_pester.yml`. Acceptance: exactly 17 files listed — 7 production PowerShell, 8 test PowerShell and 2 workflows — each with an integer count, and every count at most 500. Markdown under the feature folder is exempt from the cap per `.claude/rules/general-code-change.md` and is deliberately not listed.

- [ ] [P5-T11] R0 — verify the change footprint into `evidence/qa-gates/p5-t11-footprint.2026-09-20T01-37.md`: capture `git diff --name-only <P0-T2-head-sha>..HEAD` and `git status --porcelain --untracked-files=all`. Acceptance: the union contains exactly 8 non-documentation paths, enumerated as `scripts/dependencies/ProjectConsistency.psm1`, `scripts/dependencies/ConsistencyVerifier.psm1`, `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `.github/workflows/dependabot-repair.yml`, `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1` and `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`; every one of those 8 is a member of the spec `## Write Set`, so this cycle requires no Write Set amendment; `spec.md` appears in the union only if the coordinator's AC14 amendment was committed after the P0-T2 anchor, and the artifact records which; the union contains exactly 0 paths under `.claude/rules/` or `.github/instructions/`, which policy prohibits this change from touching; and the union contains exactly 0 paths matching `scripts/vscode/Invoke-MSTest.ps1` or `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which every format step reverts. The exact 8 is asserted rather than bounded because a bound above is satisfied by a cycle that silently dropped an edit.

- [ ] [P5-T12] R0 — write the remediation finding-status summary to `evidence/qa-gates/p5-t12-finding-status.2026-09-20T01-37.md`, listing each of R1, R2, R3, R4, R5, R6, R7, R8, R9a, R9b, R9c and R9d with its discharging task IDs, its evidence artifact paths and its verdict. The same artifact carries the criterion ledger read from `spec.md`: the total criterion count, the ticked count, the unticked count, and the three unticked criteria named. Acceptance: exactly 12 findings listed; every cited artifact path exists on disk; every finding except R1 carries a discharged verdict with at least one artifact; R1 carries `deferred to Phase 6`; the ledger records exactly 26 criteria, exactly 23 ticked and exactly 3 unticked, the three being AC18, AC19 and AC20, with AC14 ticked and carrying the amendment P0-T4 verified and no other criterion reworded by this cycle; and the summary reproduces, per **gate rule 20**, the four residuals that remain unverifiable until #914 — the `app-slug` output assumption, the resolved `author.login`, the required-check re-run, and the single-block disclosure edit — together with AC18, AC19 and AC20, which stay unticked. Fails if any finding has no discharging task, or if a cited artifact is absent.

- [ ] [P5-T13] R0 — write the review-handoff index to `evidence/other/p5-t13-handoff-index.2026-09-20T01-37.md`, listing every artifact this cycle produced with its path, its discharging task ID and its `EXIT_CODE`, and recording the P0-T2 anchor SHA, the merge base, and the four phase commit SHAs from P1-T15, P2-T11, P3-T15 and P4-T6. Acceptance: the index lists **exactly 74 plus R** `.md` artifacts, where 74 is one per task from P0-T1 through P5-T14, derived from the phase totals 13, 15, 11, 15, 6 and 14 in the `Task Counts` table rather than estimated, and `R` is the number of retained artifacts from abandoned QA-loop iterations, which the index enumerates by path and which is 0 on a single-iteration run; both figures are recorded separately, because a bare 74 fails on a correct run that restarted the loop and a bare floor is satisfied by a run that dropped an artifact; every listed path exists on disk; exactly 13 of them sit under `evidence/remediation-baseline/`, being every Phase 0 task; the index additionally lists the copied non-`.md` evidence forms, of which the two coverage projections from P0-T12 and P5-T7 are mandatory and the two test-result summaries are conditional, with any not-produced case named and its recorded reason quoted; and the index enumerates the **five** artifacts that record a JaCoCo LINE figure read from a coverage document — those of P0-T8, P1-T10, P2-T7, P3-T11 and P5-T3 — and asserts each carries the standing-in statement **gate rule 12** requires, recording the expected count of 5, the five task IDs and the actual count. P1-T11 and P5-T9 record coverage figures too and are deliberately **not** in that set: they are consumers that cite the five by path, and only a task that read a coverage document can stand in for a permitted evidence form. The set is enumerated rather than described so it is not the executor's to choose, and every count is derived from the enumeration rather than the enumeration from the count.

- [ ] [P5-T14] R0 — commit all remaining work with an explicit pathspec covering `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, recording the head SHA as `H1` in `evidence/qa-gates/p5-t14-commit.2026-09-20T01-37.md`. Single `-m`, no `<`, `>`, `$` or backtick character. Acceptance: `git status --porcelain --untracked-files=all` captured verbatim after the commit and containing no entry outside `coverage/` and `artifacts/`; `git show --name-only --format= HEAD` capturing the committed set and listing `evidence/qa-gates/p5-t7-coverage-projection.2026-09-20T01-37.jacoco.xml`, which is the permitted evidence form **gate rule 12** requires be committed in place of the prohibited collector document; and `H1` differing from the value P4-T6 recorded.

---

### Phase 6 — R1: Green CI Toolchain Run at Head, Merge-Time Instructions and Close-Out

R1 is sequenced last and is **discharged by CI on the pull request**. The rule the review cited
demands a green run of the modified gate at the exact commit being merged, and no local substitute
satisfies it: the six changed workflow files are CI's own definition, and only CI executing them
proves they work. The tasks below push the branch and dispatch a run so the result is known before the
pull request is opened, and they record plainly that the authoritative discharge is the PR-context run
at the merge head, which the orchestrator records at pull-request time.

- [ ] [P6-T1] R1 — push the branch and record into `evidence/qa-gates/p6-t1-push.2026-09-20T01-37.md` the verbatim output of `git push origin HEAD`, `git rev-parse HEAD` as `H1`, and `git status --porcelain --untracked-files=all`. Acceptance: the push output is recorded verbatim and its exit code is 0; `H1` matches the value P5-T14 recorded; and porcelain contains no entry outside `coverage/` and `artifacts/`. Fails if the tree is dirty, because a CI run at `H1` would then be testing something other than what is on disk.

- [ ] [P6-T2] R1 — dispatch and record the CI toolchain run, into `evidence/qa-gates/p6-t2-ci-run.2026-09-20T01-37.md`. When P0-T13 recorded `GH-AVAILABLE: true`: run `gh workflow run CI --ref bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`, then poll `gh run list --workflow=ci.yml --branch bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911 --limit 5 --json databaseId,headSha,conclusion,status,event` until the newest run reaches `status` `completed`, and record its `databaseId`, `headSha`, `event`, `conclusion` and per-job conclusions verbatim. Acceptance for that branch: the recorded `headSha` equals `H1`, and the `conclusion` is `success` with all six jobs — actionlint, format-check, build-analyzers, build-nullable, mstest-coverage and pester — recorded individually; a `failure` on any job fails this task and returns the cycle to Phase 5. When P0-T13 recorded `GH-AVAILABLE: false`: record `CI-DISPATCH: unavailable` with the verbatim deciding output from P0-T13 quoted, and record that R1 passes in full to the PR-context run. Acceptance for that branch: the branch taken is named explicitly, the deciding output is quoted rather than summarised, and `EXIT_CODE` is recorded for whichever invocations ran. **Neither branch may record `EXIT_CODE: SKIPPED`.** `ci.yml:8` declares `workflow_dispatch`, so the dispatch route exists; a dispatched run carries `event` `workflow_dispatch` rather than `pull_request`, and the artifact states that difference rather than implying the run is the PR-context one.

- [ ] [P6-T3] R4 R1 — write the merge-time instruction dossier to `evidence/other/p6-t3-merge-time-instructions.2026-09-20T01-37.md`. It records three instructions for whoever merges, each with its reason: **squash-merge the pull request**, because Phase 4 sanitised the working tree but the pre-sanitisation blobs remain reachable in the branch's history and a merge commit preserves them, which is why this leak class recurred on issues #645, #680, #730 and #752 despite being fixed each time; **strip `#MEZIANTOU-898` and `#SHA-256` again if `artifacts/pr_context.summary.txt` is regenerated** before the body is authored, per P4-T4; and **do not represent issue #911 as closed** until #914 discharges AC18, AC19 and AC20 against a live fixture. Acceptance: all three instructions present, each with its reason and its citing artifact path; the four #914 residuals from P5-T12 reproduced; and the dossier naming R1's authoritative discharge as the PR-context CI run at the merge head. Fails if any instruction is recorded without its reason, because an unreasoned instruction is the one a later reader drops.

- [ ] [P6-T4] R0 — tick every checkbox in `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/remediation-plan.2026-09-20T01-37.md` in the execution worktree, including P6-T4, P6-T5 and P6-T6, then commit the Phase 6 artifacts and the plan with an explicit pathspec limited to that feature folder, recording the head SHA as `H2` in `evidence/qa-gates/p6-t4-commit.2026-09-20T01-37.md`. The checkboxes are ticked **before** the commit rather than after, which is what keeps the terminal tree clean. Acceptance: the count of lines matching `^- \[[xX]\] \[P\d+-T\d+\]` in that file is exactly 80 and the count matching `^- \[ \] \[P\d+-T\d+\]` is exactly 0; porcelain captured verbatim after the commit with no entry outside `coverage/` and `artifacts/`; and `H2` differing from `H1`.

- [ ] [P6-T5] R1 — record the post-commit reconciliation into `evidence/qa-gates/p6-t5-head-reconciliation.2026-09-20T01-37.md`: `git diff --name-only <H1>..<H2>`, `git status --porcelain --untracked-files=all`, and the verbatim output of `git push origin HEAD`. Acceptance: **every** path the diff lists is under `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, and the count of such paths is at least 4, which fails if `H2` changed any file the CI toolchain builds or tests and therefore fails if the `H1` run no longer describes the code at head; the porcelain capture lists no entry outside `coverage/` and `artifacts/`, which is the companion the name-listing diff needs because that diff enumerates tracked changes only and is blind to a file left untracked; the push exit code is 0 and its output is recorded verbatim. The artifact states in terms that the `H1` run is evidence about the code at `H2` **only because** the intervening commit touched documentation alone, and that the authoritative R1 discharge remains the PR-context run at whatever SHA is merged.

- [ ] [P6-T6] R0 — record the terminal state into `evidence/qa-gates/p6-t6-terminal.2026-09-20T01-37.md`: `git status --porcelain --untracked-files=all`, `git rev-parse HEAD`, `git rev-list --count <MERGE_BASE>..HEAD` and `git log --oneline <MERGE_BASE>..HEAD -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`. Acceptance: porcelain contains no entry outside `coverage/` and `artifacts/`; the head SHA equals `H2`; the commit count is greater than the value P0-T2 recorded by at least 6, being the five phase commits and the terminal one; and the artifact records the final coverage, format, lint and test outcomes by citing P5-T8 and P5-T9 by path, so the terminal record carries the toolchain result rather than only the tree state.

---

## Task Counts

Derived mechanically from the task lists above, per phase, and recorded so a later revision
re-derives rather than recalls.

| Phase | Title | Tasks | Cumulative |
|---|---|---|---|
| 0 | Policy Reads, Anchors and Remediation Baseline Capture | 13 | 13 |
| 1 | R2: Negative and Error-Path Coverage | 15 | 28 |
| 2 | R5, R9b and R9c: The Reference-Resolution Seam | 11 | 39 |
| 3 | R3, R6, R7 and R8: The Repair Workflow | 15 | 54 |
| 4 | R4 and R9a: Host-Path Sanitisation | 6 | 60 |
| 5 | Final QA Loop, Coverage Reconciliation and Footprint | 14 | 74 |
| 6 | R1: Green CI Toolchain Run at Head and Close-Out | 6 | 80 |

**Total: 80 tasks across 7 phases.** The ticked-count assertion at P6-T4 reads 80, being every task in
the file including itself, and is derived from this table rather than estimated.

## What This Cycle Does Not Do

- It does not re-open, renumber or re-execute `plan.2026-09-19T09-44.md`.
- It does not make the binding-redirect class reachable from the `workflow_run` trigger (decision D2).
- It does not resolve the 80-versus-85 coverage floor conflict, which is open issue #668.
- It does not discharge AC18, AC19 or AC20, which require a GitHub App credential and an open
  Dependabot pull request and are carried by #914.
- It does not change any `.cs`, `.csproj`, `packages.config`, `app.config` or `.csharpierignore` file.
- It does not fix `.claude/hooks/enforce-powershell-batch-budget.ps1`, which is inert against a
  non-session worktree. That is an upstream defect in a push-down-owned file, and the consequence is
  stated plainly: a PowerShell batch overrun in this cycle would be detected by the per-phase commit
  listings rather than prevented by the hook.
