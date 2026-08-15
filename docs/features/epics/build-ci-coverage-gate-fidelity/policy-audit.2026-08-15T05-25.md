# Policy Audit — `build-ci-coverage-gate-fidelity` epic fan-in

- **Artifact timestamp:** `2026-08-15T05-25` (UTC, per `evidence-and-timestamp-conventions`)
- **Base branch:** `main`
- **Merge base:** `0569ac0b489f5867f514c00ccb2f65314c19cb16` (recomputed with `git merge-base main HEAD`; matches the caller-supplied SHA)
- **Head:** `epic/build-ci-coverage-gate-fidelity-integration` @ `22b5de02325331b0dcd660222dba33a1f1b66450` (confirmed with `git rev-parse HEAD`)
- **Range audited:** `git diff 0569ac0b..22b5de02` — 404 files, 80 commits
- **Children in scope:** #441, #512, #394, #457, #494
- **Supersedes:** the untracked `policy-audit.2026-08-15T05-11.md` and `epic-audit.2026-08-15T09-30.md` in this folder. Those artifacts were read only after the findings below were derived independently; where they agree, that is corroboration, not inheritance. Their timestamps use a different clock convention; this artifact uses UTC.

---

## Verdict

**PASS with reservations. 0 Blocking, 5 Major, 12 Minor.**

The merge `fb8eff9b` dropped no incoming hunk. The four mechanical lanes (#441 arithmetic, #457
closure filter, #394 CS2002, #512 toolchain fidelity) are delivered and independently verifiable.
The #494 lane did **not** remove the coverage-threshold contradiction; it recorded the
contradiction, deferred the fix to an upstream repository under an authorized scope correction,
and added a third numeric voice in executable form. See section 3.

---

## 0. Scope Determination

### 0.1 Rejected Scope Narrowing

None. The caller supplied the full 404-file range, stated that scope determination is the
reviewer's responsibility, and directed that the composed diff plus the merge-conflict resolution
are the priority. No caller instruction limited the audit to a plan, task, phase, file subset, or
language subset. No instruction asked that any toolchain step be skipped. The "volume note"
regarding six large Cobertura captures was explicitly marked "not a scope instruction" and was
treated as such; those files were parsed for root counters and hashed.

The caller's note that `artifacts/pr_context.summary.txt` misclassifies the diff is confirmed and
was handled by using `git diff --numstat` as the scope authority throughout.

### 0.2 Composition of the diff

| Extension | Files |
|---|---|
| `.md` | 375 |
| `.xml` | 19 (committed coverage evidence) |
| `.ps1` | 8 |
| `.json` | 1 (`.vscode/tasks.json`) |
| `.csproj` | 1 (`UtilitiesCS.Test/UtilitiesCS.Test.csproj`) |

Of the 404 files, 336 are under `docs/features/`. 55 are under `.claude/agent-memory/`, all of
which the PR-context summary silently drops (Minor m-10).

**Changed-language set derived from `git diff --numstat`:** PowerShell (8 `.ps1` files) and C#
(1 `.csproj` build-configuration file governed by the C# policy). Zero `.cs`, `.ts`, `.tsx`, or
`.py` files changed in this range. The `.cs` and `.github/workflows/**` changes visible in the
working tree arrived from `main` through merge `fb8eff9b` and are therefore outside the
three-dot range.

### 0.3 `modified-workflow-needs-green-run`

`git diff --name-only 0569ac0b 22b5de02 -- .github/ scripts/benchmarks/` returns **zero paths**.
The rule does not fire. No Blocking finding on this ground.

---

## 1. Coverage Verification

Coverage was verified against a head-accurate run plus committed evidence. The canonical artifact
paths `artifacts/pester/powershell-coverage.xml` and `artifacts/csharp/coverage.xml` are both
absent from the working tree, so the reviewer produced a head-accurate PowerShell measurement
directly and read the C# figure from committed Cobertura evidence.

### 1.1 Language verdict rows

- **PowerShell repository-wide line coverage: 68.8982% (494 of 717 lines) against the 85% floor — FAIL.** Dispositioned non-blocking; see 1.4.
- **PowerShell branch coverage: the Pester JaCoCo producer emits no BRANCH counter, so no branch figure exists for this language — FAIL.** Dispositioned non-blocking; see 1.5.
- **C# repository-wide line coverage: 85.5451% (53381 of 62401 lines) against the 85% floor — PASS.**
- **C# repository-wide branch coverage: 79.0449% (12546 of 15872 branches) against the 75% floor — PASS.**
- **TypeScript coverage — no TypeScript file changed in this range; no verdict is required.**
- **Python coverage — no Python file changed in this range; no verdict is required.**

### 1.2 Evidence provenance

| Language | Producer | Figure |
|---|---|---|
| PowerShell | Reviewer-run Pester 5.6.1, `tests/scripts/vscode` over all 11 `scripts/**/*.ps1`, JaCoCo output at head `22b5de02` | LINE 494/717; 70 passed / 0 failed / 70 total |
| C# | `.../494/evidence/baseline/coverage-remeasurement-run{1,2,3}.corrected.cobertura.xml` root attributes | `line-rate` 0.855451 / 0.855627 / 0.855547; `branch-rate` 0.790449 / 0.790449 / 0.790323 |

The three C# runs span 0.0176 percentage points, consistent with the known instrumentation
non-determinism band. The 0.545-point margin above the 85% floor exceeds that band.

The C# figure was captured on 2026-08-11, not at head. It remains applicable because the range
contains **zero** `.cs` changes; the single `.csproj` change removes a duplicate
`<Compile Include>` item for a file that is still compiled exactly once (verified: 2 entries at
base, 1 at head, source file present).

### 1.3 Per-file verdicts for changed PowerShell files

| File | Tier | Lines | Line coverage | Verdict |
|---|---|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | new | 93/93 | 100.00% | PASS (>= 85% and >= 90% new-code) |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | modified | 198/218 | 90.83% | PASS |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | modified | 88/98 | 89.80% | PASS |
| `scripts/vscode/Invoke-VSBuild.ps1` | modified | 36/43 | 83.72% | FAIL vs 85% — non-blocking, see m-1 |

Aggregate for the four changed production files: 415/452 = 91.81%.

`Invoke-MSTestWithCoverage.Helpers.ps1` improved from 88.48% (146/165, `441/evidence/baseline/pester-coverage-baseline.2026-08-10T22-30.xml`) to 90.83%. No changed line in
`Invoke-VSBuild.ps1` is uncovered: its seven missed lines are 42, 134, 139, 144, 164, 165, 166 —
all pre-existing guard clauses and the real MSBuild invocation, none of which the diff touches.

### 1.4 Disposition of the PowerShell repository-wide FAIL

Recorded as FAIL and dispositioned **non-blocking**:

1. The entire shortfall sits in seven files, **none of which appear in this branch's diff**:
   `scripts/temp-extract-coverage.ps1` (0/58), `scripts/dev-tools/run-actionlint.ps1` (0/9),
   `scripts/vscode/Invoke-Restore.ps1` (0/16), `scripts/vscode/TestProcessCleanup.ps1` (0/29),
   `scripts/vscode/Install-RepoDotNetSdk.ps1` (3/33), `scripts/vscode/Invoke-MSTest.ps1` (23/36),
   `scripts/vscode/Sync-PackageReferences.ps1` (53/84). Together: 79 covered of 265.
2. The branch's own four files aggregate to 91.81%, and the one previously-existing changed file
   with a prior measurement improved (88.48% -> 90.83%).
3. The branch adds a new 93-line file at 100%, which raises the repository figure rather than
   lowering it.
4. `scripts/temp-extract-coverage.ps1` (0/58, the single largest contributor) is already tracked
   for removal as #494 follow-up FU-C.

No pre-epic repository-wide PowerShell baseline is committed, so "improved repo-wide" cannot be
proven arithmetically — only "improved on every file the branch touches". This is recorded as a
limitation, not asserted as a pass.

### 1.5 Disposition of the PowerShell branch FAIL

Pester's JaCoCo writer emits `INSTRUCTION`, `LINE`, `METHOD` and `CLASS` counters and no `BRANCH`
counter. No branch figure can be produced for PowerShell from the repository's designated
producer. Recorded as FAIL, dispositioned **non-blocking**, because the shortfall is a producer
capability limit and not a property of the code under review. The same limit applies identically
to every committed PowerShell coverage artifact in this diff.

### 1.6 Measurement non-determinism (contributes to Major M-3)

Comparing the reviewer's head run against the committed mid-epic snapshot
`.../494/evidence/baseline/powershell-baseline.jacoco.xml`, two files change coverage with an
unchanged analyzed-line count:

| File | Snapshot | Head run | Analyzed lines |
|---|---|---|---|
| `scripts/vscode/Sync-PackageReferences.ps1` | 71 covered | 53 covered | 84 (both) |
| `scripts/vscode/Invoke-MSTest.ps1` | 27 covered | 23 covered | 36 (both) |

The same test suite over the same source yields different coverage across runs. The cause is
identified in Major M-3: the Pester suite executes real filesystem and external-process work
whose outcome depends on ambient repository state.

---

## 2. Merge-Conflict Resolution Audit — `fb8eff9b`

`fb8eff9b` merges `main` (`^2` = `0569ac0b`, which carries #553's CI parallel job split) into the
integration branch (`^1` = `85ff0c34`). Merge base of the two parents: `a682c7a2`. Three Markdown
conflicts were hand-resolved. Each was compared against `a682c7a2`, `^1`, `^2`, and the
resolution.

### 2.1 `.claude/rules/csharp.md` — PASS

Both sides made convergent, semantically equivalent edits to the same three toolchain bullets.
The resolution is a superset:

- Retains the integration side's per-file-opt-in nullable wording and its explicit
  "do not add `/p:Nullable=enable`" / "do not use `/t:Build`" block.
- Adopts `main`'s added `dotnet tool restore` precondition, which the integration side lacked.
- Improves on both by naming the post-split workflow files `_build-analyzers.yml` and
  `_build-nullable.yml` instead of `ci.yml`.

One `^1` line is absent: a duplicate `/t:Rebuild` rationale bullet under item 3. The identical
sentence remains under item 2 and item 3 retains its own "do not use `/t:Build`" instruction. No
semantic content is lost.

### 2.2 `.claude/skills/csharp-qa-gate/SKILL.md` — PASS

Strict superset of both parents. The `^2`-to-merge diff is `+2 / -0` (the integration side's
`Skipping target "CoreCompile"` evidence requirement). The `^1`-to-merge diff adds `main`'s
`dotnet tool restore` step-1 wording and its `/t:Rebuild /m` rationale paragraph. Nothing from
either side is dropped.

### 2.3 `.claude/agent-memory/feature-review/MEMORY.md` — PASS with Minor

Index entry counts: `^1` 57, `^2` 65, merge 58. Link-target set analysis:

- Present in `^1`, absent from merge: **zero**.
- Present in `^2`, absent from merge: **nine** — `code-review-findings-table-header.md`,
  `code-review-required-headings.md`, `feature-audit-checkoff-heading-case.md`,
  `feature-audit-requires-summary-heading.md`, `policy-audit-comparison-line-schema.md`,
  `policy-audit-numeric-new-code-coverage.md`, `policy-audit-required-structure.md`,
  `policy-audit-section7-row-label-parser.md`, `policy-audit-validator-uses-full-template.md`.
- `main`'s single new entry, `project_553_ci_split_review_pattern.md`, **is present** in the merge.

The nine absent entries are not a dropped incoming hunk. They were consolidated on the
integration side into the single pointer entry
`project_taskmaster-validator-memories-are-cross-repo.md`, which names all nine by slug. All nine
target files were verified present on disk at head, as was the consolidating file. Recorded as
Minor m-8 because the nine are now reachable only indirectly.

### 2.4 Merge interaction not covered by any conflict — Major M-2

`CLAUDE.md` was **not** a conflicted file, so the hand-resolution did not touch it. It therefore
retains three references to `.github/workflows/ci.yml` that `main`'s #553 split invalidated. See
section 4 and Major M-2. This is the one place where the merge left the composed tree internally
inconsistent, and it is precisely the class of defect a three-way conflict resolution cannot
catch.

---

## 3. Central Question — Was the coverage contradiction removed or relocated?

**Answer: neither removed nor merely relocated. It was documented, deferred out of the
repository, and given an additional executable voice.**

### 3.1 State of the three documents at head

`git diff --numstat 0569ac0b 22b5de02` shows:

- `CLAUDE.md`: 19 insertions / 13 deletions, **all in the C# toolchain sections**. The § UT2
  coverage block is untouched.
- `.claude/rules/general-unit-test.md`: **not in the diff**.
- `.claude/rules/quality-tiers.md`: **not in the diff**.

At head the documents therefore still disagree, verbatim:

| Source | Line floor | Branch floor | New code | Denominator |
|---|---|---|---|---|
| `CLAUDE.md:303-310` | `>= 80%` repo-wide | not stated | `>= 90%` for new modules/classes/methods | testable denominator; COM/VSTO/WinForms exempted via `[ExcludeFromCodeCoverage]` and `coverage.config` assembly excludes |
| `.claude/rules/general-unit-test.md` | `>= 85%` all tiers | `>= 75%` all tiers | not stated | every production file; "No production file may be excluded from coverage measurement"; an `exclude` matching a production path is a **Blocking** finding |
| `.claude/rules/quality-tiers.md` | `>= 85%` | `>= 75%` | not stated | not stated |
| `.claude/rules/powershell.md` | `>= 85%` all tiers | `>= 75%` all tiers | not stated | not stated |

The numeric conflict (80 vs 85, 90 vs 85, no branch floor vs 75) and the denominator conflict
(exemption mandated vs exclusion prohibited) both stand unchanged. Because
`policy-compliance-order` ranks `CLAUDE.md` first, an agent applying the stated precedence still
lands on 80/90 with exemptions, which is the position `.claude/rules/general-unit-test.md`
instructs the same agent to treat as Blocking.

### 3.2 Why: the #494 scope correction

`.../494/spec.md` § "User-Authorized Scope Correction" removes `CLAUDE.md`, all non-memory Claude
runtime customization paths (rules, hooks, skills, agents, settings) and `.agents/skills/**` from
the feature's delivery scope, and designates
`evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` as the complete
local deliverable. Eight of #494's ten acceptance criteria (AC1, AC2, AC3, AC5, AC6, AC8, AC10
and part of AC1) are consequently satisfied by the existence and content of that prompt rather
than by any change to the conflicting documents.

This is an authorized narrowing recorded in a committed, reviewable artifact, so it is not a
policy violation. It does mean the epic's own charter goal — "Repository quality gates report
figures that correspond to what they claim to measure" — is only partially met on this lane.

### 3.3 The new executable voice — Major M-1

`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:459-491` adds
`Assert-CoberturaLineCoverageThreshold`, called from
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:341`. It hard-codes `80` at line 487 and in the
throw message at line 489. It performs **no branch-coverage assertion at all**.

The repository now has four live numeric authorities:

| Authority | Line | Branch | Denominator |
|---|---|---|---|
| `CLAUDE.md` § UT2 | 80 / 90-new | none | testable, exemptions permitted |
| `.claude/rules/general-unit-test.md`, `quality-tiers.md`, `powershell.md` | 85 | 75 | every production file |
| `.claude/hooks/validate-feature-review-coverage.ps1:313,323` | 85.0 | 75.0 | per-language artifact |
| `Assert-CoberturaLineCoverageThreshold` (new) | 80 | none | post-filter Cobertura |

A C# run measuring between 80% and 85% now passes the local developer gate and fails the review
hook. No enforcement is weakened — the stricter authority still applies at review time — but the
epic's stated purpose was to eliminate exactly this class of disagreement.

Mitigating: the epic charter NFR requires that no threshold be lowered "without an explicit,
recorded decision", and `.../494/spec.md` decision **D1** is that explicit record. The 80 figure
is charter-compliant. It is the *coexistence* with the un-amended 85/75 rules that is the defect.

### 3.4 The denominator, in the permissive direction

#457 (`Remove-CoberturaExemptClosureCoverage`) removes compiler-generated closure lines whose
declaring member is absent from the instrumented set, i.e. members carrying
`[ExcludeFromCodeCoverage]`. This shrinks the denominator and raises measured coverage. It is
squarely permitted by `CLAUDE.md` § UT2's exemption clause and squarely in tension with
`.claude/rules/general-unit-test.md`'s exclusion prohibition. The implementation resolves the
tension in `CLAUDE.md`'s favour without the rule file being amended.

The implementation itself is defensible: the filter is documented as fail-safe in the
under-exclusion direction (an unresolved declaring member is always retained), operates at member
granularity rather than file granularity, and leaves every production **file** in the
denominator. That distinction is what keeps this a Major rather than a Blocking finding under the
general-unit-test.md exclusion rule.

---

## 4. Toolchain-Fidelity Claim Verification (#512)

Every factual claim the diff adds to an always-loaded policy document was checked against the
tree.

| Claim | Verdict |
|---|---|
| `CLAUDE.md:188` — CSharpier pinned to `1.2.6` by `dotnet-tools.json` | PASS — `dotnet-tools.json` pins `1.2.6` with `rollForward: false` |
| `CLAUDE.md:188` — `.csharpierignore` keeps `*.csproj`/`*.props`/`*.targets` out of the check | PASS — file present with all three patterns |
| Documented analyzer command uses `/t:Rebuild /m` | PASS — `CLAUDE.md:202`, `.claude/rules/csharp.md`, `.claude/skills/csharp-qa-gate/SKILL.md`, and `.vscode/tasks.json` (`-Target Rebuild`) all agree |
| Documented nullable command omits `/p:Nullable=enable` | PASS — removed from `CLAUDE.md`, `csharp.md`, `tasks.json`; `Invoke-VSBuild.ps1:117` makes the switch a warning-emitting no-op |
| `Invoke-VSBuild.ps1` emits `/m` | PASS — line 82, pre-existing, retained |
| CI's analyzer job uses `/t:Build /m` | PASS — `.github/workflows/_build-analyzers.yml:50` |
| CI's nullable job uses `/t:Rebuild /m` without `/p:Nullable=enable` | PASS — `_build-nullable.yml:57` |
| `CLAUDE.md:211` — "character-for-character the command in `.github/workflows/ci.yml` (step 'Build with nullable warnings treated as errors')" | **FAIL — Major M-2.** `ci.yml` at head is a dispatcher containing no `msbuild` step at all; the step lives in `_build-nullable.yml` |
| `CLAUDE.md:202` — "`.github/workflows/ci.yml` uses `/t:Build /m` for its analyzer step" | **FAIL — Major M-2.** Now `_build-analyzers.yml` |
| `CLAUDE.md:190` — ci.yml "runs the pinned version after `dotnet tool restore`" | **FAIL — Major M-2.** Now `_format-check.yml`, which uses the `dotnet csharpier check .` spelling rather than the mandated `dotnet tool run` form (Minor m-9) |
| #512 AC6 — no site still documents the CSharpier v0 bare form or a `/t:Build` nullable command | PASS as written. Eleven such sites remain (`AGENTS.md:469,470,660,487,488,662`; `.github/instructions/csharp-{code-change,unit-test}.instructions.md`; `.github/agents/csharp-{typed-engineer,atomic-executor}.agent.md`; `.agents/skills/csharp{,-qa-gate}/SKILL.md`). AC6's final sentence permits deliberately-unchanged sites when enumerated with rationale; spec decision SD1 enumerates exactly these eleven, and the required follow-up issue **#535** was verified filed at `docs/features/potential/promoted/2026-08-11-codex-copilot-instruction-mirrors-document-defective-csharp-toolchain-commands.md` |
| #512 AC9 — `CLAUDE.md` § UT2, `general-unit-test.md`, `quality-tiers.md` unmodified | PASS — verified by `git diff --numstat`; § UT2 is outside every `CLAUDE.md` hunk |

---

## 5. Evidence Location Compliance

**PASS.**

- `git diff --name-only 0569ac0b 22b5de02 | grep -E '^artifacts/(baselines|qa|evidence|coverage)/'` returns **zero paths**.
- All 260 evidence files in the diff live under `<FEATURE>/evidence/<kind>/`.
- Kinds observed: `baseline/` (89), `qa-gates/` (83), `regression-testing/` (29), `other/` (28),
  `issue-updates/` (19), `remediation-baseline/` (12).
- `scripts/dev_tools/validate_evidence_locations.py` does not exist in this repository, so the
  scan above was performed directly against the diff. Recorded as a method substitution, not a
  skipped check.
- Minor m-10: `issue-updates/` and `remediation-baseline/` are not canonical kind names.

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events occurred; no caller instruction specified a
non-canonical evidence path.

---

## 6. File Size Limit (500 lines)

**PASS.** Measured with `awk 'END{print NR}'`.

| File | Lines | Headroom |
|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 491 | 9 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | 498 | 2 |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | 459 | 41 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` | 443 | 57 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 389 | 111 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 349 | 151 |
| `scripts/vscode/Invoke-VSBuild.ps1` | 167 | 333 |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | 86 | 414 |

The two files within ten lines of the ceiling are recorded as Minor m-7. Extracting
`Assert-CoberturaLineCoverageThreshold` into the existing `ClosureFilter`-style sibling module
would restore headroom without changing behavior.

---

## 7. Policy Compliance Summary

| Policy | Verdict | Basis |
|---|---|---|
| `CLAUDE.md` — General Code Change Policy | PARTIAL | Design and error-handling clean; §1.2 reusability weakened by a deliberately duplicated rate expression (m-4) |
| `CLAUDE.md` — file size limit | PASS | Section 6 |
| `CLAUDE.md` — C# Code Change Policy | PASS | No `.cs` change; the `.csproj` edit removes a duplicate item and is verified |
| `.claude/rules/general-unit-test.md` — external dependencies | **FAIL** | Major M-3: the Pester suite invokes `vswhere.exe` and a script that writes `.csproj` files |
| `.claude/rules/general-unit-test.md` — determinism | **FAIL** | Section 1.6: same suite, same source, different coverage across runs |
| `.claude/rules/general-unit-test.md` — no temporary files in tests | PASS | No `New-TemporaryFile`, `TestDrive`, or `GetTempPath` use in any changed test file |
| `.claude/rules/general-unit-test.md` — coverage floors | FAIL (dispositioned) | Sections 1.1, 1.4, 1.5 |
| `.claude/rules/general-unit-test.md` — exclusion prohibition | PARTIAL | Section 3.4 |
| `.claude/rules/powershell.md` — advanced functions, `CmdletBinding`, mandatory params | PASS | Every new function is an advanced function with typed, validated parameters and comment-based help |
| `.claude/rules/powershell.md` — `ShouldProcess` for state-changing actions | PARTIAL | m-5: declared on a pure in-memory transform, creating a silent-skip path |
| `.claude/rules/powershell.md` — deterministic tests, no live executables | **FAIL** | Major M-3 |
| `.claude/rules/powershell.md` — approved verbs, descriptive nouns | PASS | `Test-`, `Get-`, `Remove-`, `Assert-`, `Merge-`, `ConvertTo-` |
| `.claude/rules/quality-tiers.md` — uniform 85/75 | **FAIL** | Major M-1: a new gate enforces 80 with no branch check |
| `.claude/rules/tonality.md` | PASS | Reviewed the new prose in `CLAUDE.md`, `csharp.md`, `SKILL.md` and the eight `.ps1` files |
| `evidence-and-timestamp-conventions` | PASS with Minor | Section 5, m-10 |
| `acceptance-criteria-tracking` | PASS | See `feature-audit.2026-08-15T05-25.md` |
| `feature-review-workflow` — `modified-workflow-needs-green-run` | Not triggered | Section 0.3 |

---

## 8. Findings Register

### Major

- **M-1 — A new executable coverage gate enforces 80% while the always-loaded rules mandate 85%/75%.**
  `Invoke-MSTestWithCoverage.Helpers.ps1:487`. Adds a fourth numeric authority and no branch
  assertion. See section 3.3.
- **M-2 — `CLAUDE.md` carries three stale `.github/workflows/ci.yml` references after merge `fb8eff9b`.**
  Lines 190, 202 and 211. Line 211's "character-for-character" claim is now false: `ci.yml` at
  head contains no `msbuild` step. The merge resolution corrected the equivalent references in
  `.claude/rules/csharp.md` but `CLAUDE.md` was unconflicted and was missed. See section 2.4.
- **M-3 — `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` executes real external processes and can rewrite tracked `.csproj` files.**
  `BeforeAll` dot-sources `Invoke-VSBuild.ps1`, whose top-level body runs `vswhere.exe` and then
  `Sync-PackageReferences.ps1`, which writes via `[System.IO.File]::WriteAllText` at line 148.
  `-NoExecute` guards only the MSBuild invocation. Demonstrated non-determinism in section 1.6.
- **M-4 — PowerShell repository-wide line coverage 68.8982% against the 85% floor.** FAIL,
  dispositioned non-blocking; see section 1.4.
- **M-5 — A threshold failure discards the post-processed coverage artifact.**
  `Invoke-MSTestWithCoverage.ps1:341` asserts before the `Set-Content` at line 343, so on failure
  the output path retains the raw, un-post-processed document and the processed one is lost.

### Minor

- **m-1** — `Invoke-VSBuild.ps1` at 83.72% modified-file line coverage. Non-blocking: above the
  80% `CLAUDE.md` floor, zero changed-line regression, unchanged from the mid-epic snapshot.
- **m-2** — `Helpers.ps1:483` (`throw 'Cobertura line-rate must be between 0 and 1.'`) is fully
  uncovered (`mi=1 ci=0`) in a function whose #494 AC4 claims deterministic negative-path
  evidence. `:468` and `:371` are partially covered.
- **m-3** — `Helpers.ps1:221`, the max-hits update branch of the new `Get-CoberturaClassLineSummary`,
  is fully uncovered. Already filed as issue **#537**.
- **m-4** — The line-rate and branch-rate expressions are triplicated across
  `Get-CoberturaCoverageSummary`, `Merge-CoberturaClassesByFilename:371-372` and
  `ClosureFilter.ps1:382-383`, justified in-comment by "the spec specifies exactly one new
  helper" — a process constraint offered as an engineering rationale.
- **m-5** — `Remove-CoberturaExemptClosureCoverage` declares `SupportsShouldProcess` for a pure
  in-memory XML transform. A set `$WhatIfPreference` returns early with no signal, after which
  `Assert-CoberturaLineCoverageThreshold` would gate an unfiltered document.
- **m-6** — `.../494/evidence/baseline/coverage-remeasurement-run{1,2,3}.raw.cobertura.xml` are
  SHA-256 identical to their `.corrected.` counterparts. Both are already post-processed
  (relative filenames, `<sources><source>.</source>`, no `.<>c` class names), so the "raw" label
  is inaccurate and AC7's corrected-arithmetic remeasurement demonstrates no delta.
- **m-7** — `Helpers.ps1` (491) and `Helpers.Tests.ps1` (498) sit within 9 and 2 lines of the
  500-line limit.
- **m-8** — Nine memory index entries were consolidated out of
  `.claude/agent-memory/feature-review/MEMORY.md`; the target files remain on disk but are now
  reachable only through `project_taskmaster-validator-memories-are-cross-repo.md`.
- **m-9** — `.github/workflows/_format-check.yml:41` uses `dotnet csharpier check .` while
  `CLAUDE.md` mandates `dotnet tool run csharpier check .`. Equivalent in effect; not the
  documented form. #512 AC5's "consistent with `.github/workflows/ci.yml`" basis is stale after
  the split.
- **m-10** — `issue-updates/` and `remediation-baseline/` are non-canonical evidence kinds;
  `.claude/agent-memory/**` (55 files) is silently dropped by `artifacts/pr_context.summary.txt`.
- **m-11** — #494's "User-Authorized Scope Correction" has no `human_interaction` receipt in
  `artifacts/orchestration/epic-orchestrator-state.json` (the key is absent from the checkpoint
  entirely). The authorization survives only as committed `spec.md` prose. That prose is
  reviewable and durable, which is why this is Minor rather than Major.
- **m-12** — Package-level `line-rate` and `branch-rate` are not recomputed after closure
  filtering and merging; root and class rates are. Confirmed at head: the committed evidence
  shows 16-decimal package rates alongside 6-decimal class rates. Already filed as **#529**.

### Blocking

None.

---

## Appendix A — Assumptions

1. `full-bug` work mode for all five children resolves the acceptance-criteria source to
   `spec.md` only, per `acceptance-criteria-tracking`. All five `issue.md` files carry an
   explicit `- Work Mode: full-bug` marker; none required the fail-closed default.
2. The C# coverage figure captured on 2026-08-11 is applicable at head because the range contains
   zero `.cs` changes. Stated in 1.2 with its basis.
3. The reviewer produced the PowerShell coverage measurement directly with Pester 5.6.1 rather
   than through `mcp__drm-copilot__run_poshqc_test`, which is not exposed in this session. The
   run is check-only and wrote its JaCoCo output outside the repository tree.

## Appendix B — Command Reference

| Purpose | Command |
|---|---|
| Base resolution | `git merge-base main HEAD` |
| Head confirmation | `git rev-parse HEAD` |
| Scope authority | `git diff --numstat 0569ac0b489f5867f514c00ccb2f65314c19cb16 22b5de02325331b0dcd660222dba33a1f1b66450` |
| Workflow-rule trigger | `git diff --name-only 0569ac0b 22b5de02 -- .github/ scripts/benchmarks/` |
| Merge parents | `git log -1 --format='%H %P' fb8eff9b` |
| Merge base of parents | `git merge-base fb8eff9b^1 fb8eff9b^2` |
| Conflict three-way compare | `git show fb8eff9b^1:<path>`, `git show fb8eff9b^2:<path>`, `git show fb8eff9b:<path>`, `git show a682c7a2:<path>` |
| PowerShell tests + coverage | `Invoke-Pester` with `CodeCoverage.Path` = all `scripts/**/*.ps1`, `OutputFormat = 'JaCoCo'`, `Run.Path = 'tests/scripts/vscode'` |
| C# coverage read | root attributes of `.../494/evidence/baseline/coverage-remeasurement-run{1,2,3}.corrected.cobertura.xml` |
| Evidence-location scan | `git diff --name-only 0569ac0b 22b5de02 \| grep -E '^artifacts/(baselines\|qa\|evidence\|coverage)/'` |
| File size | `awk 'END{print NR}' <file>` |
| Evidence integrity | `sha256sum coverage-remeasurement-run*.xml` |

No command in this audit mutated the repository. The only working-tree change during the review
is the untracked scratch output written outside the repository root.
