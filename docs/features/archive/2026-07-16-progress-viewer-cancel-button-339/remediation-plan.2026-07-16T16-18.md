# Remediation Plan: 2026-07-16-progress-viewer-cancel-button-339 (2026-07-16T16-18)

- **Issue:** #339
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/339
- **Last Updated:** 2026-07-16T16-18
- **Status:** Planned
- **Version:** 1.0
- **Work Mode:** minor-audit
- **Requirements Source:** `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/remediation-inputs.2026-07-16T16-18.md`
- **Feature Folder:** `docs/features/active/2026-07-16-progress-viewer-cancel-button-339`
- **Original Feature Plan:** `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/plan.2026-07-16T12-39.md`
- **Remediation Scope:** `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx` only; plan and evidence status artifacts may also change as required by execution.

## Requirements and Scope Boundary

This plan treats `remediation-inputs.2026-07-16T16-18.md` as the authoritative remediation source. The persisted `- Work Mode: minor-audit` marker and the three checkbox items under the exact `## Acceptance Criteria` heading in `issue.md` remain the feature requirements boundary. `spec.md`, `user-story.md`, and `research.md` are not required and must not be introduced.

The initial feature review verified all three acceptance criteria, the C# implementation, 5,468 passing tests, 83.46% repository line coverage, 100% `ProgressViewer.cs` line coverage, and 100% changed production-line coverage. Remediation is limited to six trailing-space instances in one retained diagnostic TRX. Existing C# QA evidence remains authoritative because this plan prohibits changes to C# source, tests, projects, solution files, runsettings, coverage XML, requirements, and policies.

Before a remediation commit, effective-tree whitespace verification must use `git diff --check bump-release` and `git diff --check`. The orchestrator must run `git diff --check bump-release...HEAD` after committing the remediation because the three-dot command otherwise continues to inspect the pre-remediation `HEAD` tree.

All new execution evidence must be written under this feature folder's `evidence/remediation-baseline/` or `evidence/qa-gates/` hierarchy. Every command-evidence Markdown artifact must contain `Timestamp:`, the exact `Command:`, numeric `EXIT_CODE:`, and `Output Summary:`. No manual task, temporary file, source edit, policy edit, dependency change, suppression, coverage rewrite, or acceptance-criteria rewrite is authorized.

---

### Phase 0 — Remediation Baseline Capture and Status Synchronization

- [x] [P0-T1] Read `AGENTS.md` in full as the standing repository instruction source.
  - Acceptance: the read covers professional tone, generated-policy authority, filesystem editing constraints, and repository policy precedence; evidence is consolidated in P0-T5.

- [x] [P0-T2] Read the cross-language code-change policy in `AGENTS.md` after the standing instructions.
  - Acceptance: the read covers targeted defect remediation, scope containment, deterministic verification, documentation updates, and completion reporting; evidence is consolidated in P0-T5.

- [x] [P0-T3] Read the cross-language unit-test policy in `AGENTS.md` after the code-change policy.
  - Acceptance: the read confirms that existing C# tests and coverage remain authoritative only if no C# file changes during this evidence-only remediation; evidence is consolidated in P0-T5.

- [x] [P0-T4] Read `.agents/skills/evidence-and-timestamp-conventions/SKILL.md` after the cross-language policies.
  - Acceptance: the read covers canonical feature evidence locations, `yyyy-MM-ddTHH-mm` timestamps, and required command-evidence fields; evidence is consolidated in P0-T5.

- [x] [P0-T5] Record the completed ordered policy reads in `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/remediation-baseline/phase0-instructions-read.2026-07-16T16-18.md`.
  - Acceptance: the artifact contains `Timestamp:`, `Policy Order:`, and an explicit ordered list of P0-T1 through P0-T4; it states that no policy file was modified.

- [x] [P0-T6] Verify and record the minor-audit boundary, original-plan state, and acceptance-criteria state before remediation.
  - Command: `pwsh -NoProfile -Command '& { $f="docs/features/active/2026-07-16-progress-viewer-cancel-button-339"; $issue=Join-Path $f "issue.md"; $plan=Join-Path $f "plan.2026-07-16T12-39.md"; $text=Get-Content -Raw $issue; if($text -notmatch "(?m)^- Work Mode: minor-audit$" -or $text -notmatch "(?m)^## Acceptance Criteria$" -or (Test-Path (Join-Path $f "spec.md")) -or (Test-Path (Join-Path $f "user-story.md"))){exit 1}; $acSection=($text -split "(?m)^## Acceptance Criteria\s*$")[1] -split "(?m)^## " | Select-Object -First 1; $acChecked=([regex]::Matches($acSection,"(?m)^- \[x\]")).Count; $acUnchecked=([regex]::Matches($acSection,"(?m)^- \[ \]")).Count; $planChecked=(Select-String -Path $plan -Pattern "^- \[x\] \[P\d+-T\d+\]").Count; $planUnchecked=(Select-String -Path $plan -Pattern "^- \[ \] \[P\d+-T\d+\]").Count; "WORK_MODE=minor-audit"; "AC_CHECKED=$acChecked"; "AC_UNCHECKED=$acUnchecked"; "ORIGINAL_PLAN_CHECKED=$planChecked"; "ORIGINAL_PLAN_UNCHECKED=$planUnchecked"; if($acChecked -ne 3 -or $acUnchecked -ne 0 -or $planChecked -ne 29 -or $planUnchecked -ne 0){exit 1} }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/remediation-baseline/status-baseline.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` records `minor-audit`, 3 checked and 0 unchecked AC items, 29 checked and 0 unchecked original-plan tasks, and absence of `spec.md` and `user-story.md`.

- [x] [P0-T7] Capture the diagnostic TRX XML and whitespace baseline without changing the file.
  - Command: `pwsh -NoProfile -Command '& { $p="docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx"; [xml]$x=Get-Content -Raw $p; $sha=(Get-FileHash -Algorithm SHA256 $p).Hash.ToLowerInvariant(); $c=$x.SelectSingleNode("//*[local-name()=''Counters'']"); $o=@(git diff --check bump-release...HEAD -- $p 2>&1); $e=$LASTEXITCODE; $findings=@($o | Where-Object { $_ -match ("^"+[regex]::Escape($p)+":(981|3844|3904|4014|4029|5897): trailing whitespace\.$") }); "XML_ROOT=$($x.DocumentElement.LocalName)"; "TRX_SHA256=$sha"; "TOTAL=$($c.GetAttribute(''total''))"; "EXECUTED=$($c.GetAttribute(''executed''))"; "PASSED=$($c.GetAttribute(''passed''))"; "FAILED=$($c.GetAttribute(''failed''))"; "DIFF_CHECK_EXIT_CODE=$e"; "WHITESPACE_FINDING_COUNT=$($findings.Count)"; $findings; if($x.DocumentElement.LocalName -ne "TestRun" -or $e -ne 2 -or $findings.Count -ne 6){exit 1} }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/remediation-baseline/trx-whitespace-baseline.2026-07-16T16-18.md`.
  - Acceptance: the command exits 0 after proving the TRX is well-formed, recording its SHA-256 and counters, and detecting exactly the six expected whitespace diagnostics at lines 981, 3844, 3904, 4014, 4029, and 5897; the artifact includes all required evidence fields.

- [x] [P0-T8] Capture the immutable C# and coverage scope before remediation.
  - Command: `pwsh -NoProfile -Command '& { $expectedHead="a22530c11dd9d2f3c94c74531840d889268b8d53"; $expectedMergeBase="0eb0b39abd206d8347f84d7fe438944a8d4d788e"; $head=(git rev-parse HEAD).Trim(); $mergeBase=(git merge-base bump-release HEAD).Trim(); if($head -ne $expectedHead -or $mergeBase -ne $expectedMergeBase){"HEAD=$head"; "MERGE_BASE=$mergeBase"; exit 1}; $expected=@("UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs","UtilitiesCS/Threading/ProgressViewer.cs"); $branchCs=@(git diff --name-only bump-release...HEAD -- "*.cs" | Sort-Object); if(Compare-Object $expected $branchCs){$branchCs; exit 1}; "BASE=bump-release"; "HEAD=$head"; "MERGE_BASE=$mergeBase"; foreach($p in $expected){"CS_SHA256=$p|$((Get-FileHash -Algorithm SHA256 $p).Hash.ToLowerInvariant())"}; foreach($p in @("docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/csharp-coverage-baseline.2026-07-16T12-39.cobertura.xml","docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/csharp-coverage-final.2026-07-16T12-39.cobertura.xml")){"COVERAGE_SHA256=$p|$((Get-FileHash -Algorithm SHA256 $p).Hash.ToLowerInvariant())"}; "BRANCH_CSHARP_COUNT=$($branchCs.Count)"; $branchCs }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/remediation-baseline/immutable-scope-baseline.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`; the artifact records pre-remediation `HEAD`, SHA-256 values for both C# files and both authoritative coverage XML files, and exactly the two expected C# paths in the branch diff.

- [x] [P0-T9] Verify the branch contains no evidence under a forbidden non-canonical `artifacts/` hierarchy.
  - Command: `pwsh -NoProfile -Command '& { $patterns="^artifacts/(baselines?|qa|qa-gates|evidence|coverage|regression-testing|post-change)/"; $bad=@(git diff --name-only bump-release...HEAD | Where-Object { $_ -match $patterns }); $validator=@(Get-ChildItem -Recurse -File -Filter validate_evidence_locations.py -ErrorAction SilentlyContinue); "VALIDATOR_SCRIPT_COUNT=$($validator.Count)"; "FORBIDDEN_EVIDENCE_PATH_COUNT=$($bad.Count)"; $bad; if($bad.Count -ne 0){exit 1}; if($validator.Count -gt 0){python $validator[0].FullName --root .; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}} }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/remediation-baseline/evidence-location-baseline.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`; the artifact records zero forbidden paths and the count of available `validate_evidence_locations.py` scripts; if a script exists, execution must also run it with `--root .` and require exit code 0 before checking off this task.

---

### Phase 1 — Bounded Diagnostic TRX Normalization

- [x] [P1-T1] Remove only the six reviewed trailing-space instances from `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx` using `apply_patch`.
  - Required edits: remove the final space from both `Integration test of GetMatchProbability method which ` lines, both `3) Some of the tokens found in those lists do not meet the minimum threshhold for inclusion and are excluded from the list. ` lines, and both `Input Tokens: ` lines identified by P0-T7.
  - Acceptance: exactly six line endings change; no character before the final trailing space changes; no other file is edited by this task; `git diff --numstat HEAD -- docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx` reports six added and six deleted lines.

- [x] [P1-T2] Verify the normalized TRX remains well-formed and preserves its recorded test counters.
  - Command: `pwsh -NoProfile -Command '& { $p="docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx"; $baseline=Get-Content -Raw "docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/remediation-baseline/trx-whitespace-baseline.2026-07-16T16-18.md"; [xml]$x=Get-Content -Raw $p; $c=$x.SelectSingleNode("//*[local-name()=''Counters'']"); "XML_ROOT=$($x.DocumentElement.LocalName)"; if($x.DocumentElement.LocalName -ne "TestRun"){exit 1}; foreach($n in @("total","executed","passed","failed")){$v=$c.GetAttribute($n); "$($n.ToUpper())=$v"; if($baseline -notmatch ("(?m)^"+$n.ToUpper()+"="+[regex]::Escape($v)+"$")){throw "Counter mismatch: $n"}} }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/trx-normalization.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`; XML root and all recorded counter values match P0-T7; the artifact contains the required evidence fields.

- [x] [P1-T3] Prove that remediation did not change any C# file, requirement source, coverage XML, policy file, or repository configuration.
  - Command: `pwsh -NoProfile -Command '& { $forbidden=@(git diff --name-only HEAD -- "*.cs" "*.csproj" "*.sln" "*.runsettings" "*.cobertura.xml" "AGENTS.md" ".agents/skills/**" "docs/features/active/2026-07-16-progress-viewer-cancel-button-339/issue.md"); "FORBIDDEN_REMEDIATION_DELTA_COUNT=$($forbidden.Count)"; $forbidden; if($forbidden.Count -ne 0){exit 1} }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/remediation-scope.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`; the forbidden remediation delta count is 0; review/plan/evidence artifact additions are allowed only inside the active feature folder.

---

### Phase 2 — Final QA, Status Synchronization, and Post-Commit Handoff

- [x] [P2-T1] Verify the effective feature branch plus remediation working tree is whitespace-clean relative to `bump-release`.
  - Command: `git diff --check bump-release`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/remediation-effective-tree-diff-check.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`, no command output, and the artifact contains `Timestamp:`, exact `Command:`, numeric `EXIT_CODE:`, and `Output Summary:`.

- [x] [P2-T2] Verify the unstaged remediation delta itself is whitespace-clean.
  - Command: `git diff --check`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/remediation-working-tree-diff-check.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`, no command output, and the artifact contains all required evidence fields.

- [x] [P2-T3] Re-parse the TRX and compare its final counters with the P0-T7 baseline.
  - Command: `pwsh -NoProfile -Command '& { $p="docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx"; [xml]$x=Get-Content -Raw $p; $c=$x.SelectSingleNode("//*[local-name()=''Counters'']"); $text=Get-Content -Raw "docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/remediation-baseline/trx-whitespace-baseline.2026-07-16T16-18.md"; if($x.DocumentElement.LocalName -ne "TestRun"){exit 1}; foreach($n in @("total","executed","passed","failed")){ $v=$c.GetAttribute($n); if($text -notmatch ("(?m)^"+$n.ToUpper()+"="+[regex]::Escape($v)+"$")){throw "Counter mismatch: $n"}; "$($n.ToUpper())=$v" }; "XML_ROOT=$($x.DocumentElement.LocalName)" }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/trx-integrity-final.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`; the XML root is `TestRun`; total, executed, passed, and failed counters exactly match the baseline; the artifact contains all required evidence fields.

- [x] [P2-T4] Verify the two C# files and two authoritative coverage XML files retain their P0-T8 SHA-256 values.
  - Command: `pwsh -NoProfile -Command '& { $b=Get-Content -Raw "docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/remediation-baseline/immutable-scope-baseline.2026-07-16T16-18.md"; $paths=@("UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs","UtilitiesCS/Threading/ProgressViewer.cs","docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/csharp-coverage-baseline.2026-07-16T12-39.cobertura.xml","docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/csharp-coverage-final.2026-07-16T12-39.cobertura.xml"); foreach($p in $paths){$h=(Get-FileHash -Algorithm SHA256 $p).Hash.ToLowerInvariant(); "$p|$h"; if($b -notmatch [regex]::Escape("$p|$h")){throw "Immutable file changed: $p"}} }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/immutable-scope-final.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`; all four hashes match P0-T8, proving existing C# QA and coverage values remain applicable without rerunning the C# toolchain.

- [x] [P2-T5] Re-run canonical evidence-location verification for the effective branch.
  - Command: `pwsh -NoProfile -Command '& { $patterns="^artifacts/(baselines?|qa|qa-gates|evidence|coverage|regression-testing|post-change)/"; $bad=@(git diff --name-only bump-release | Where-Object { $_ -match $patterns }); "FORBIDDEN_EVIDENCE_PATH_COUNT=$($bad.Count)"; $bad; if($bad.Count -ne 0){exit 1}; $validator=@(Get-ChildItem -Recurse -File -Filter validate_evidence_locations.py -ErrorAction SilentlyContinue); "VALIDATOR_SCRIPT_COUNT=$($validator.Count)"; if($validator.Count -gt 0){python $validator[0].FullName --root .; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}} }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/evidence-location-final.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`; zero forbidden evidence paths; any available canonical validator script also exits 0; the artifact contains all required evidence fields.

- [x] [P2-T6] Synchronize and verify the original feature plan and minor-audit acceptance-criteria status after remediation.
  - Command: `pwsh -NoProfile -Command '& { $f="docs/features/active/2026-07-16-progress-viewer-cancel-button-339"; $issue=Get-Content -Raw (Join-Path $f "issue.md"); $section=($issue -split "(?m)^## Acceptance Criteria\s*$")[1] -split "(?m)^## " | Select-Object -First 1; $acChecked=([regex]::Matches($section,"(?m)^- \[x\]")).Count; $acUnchecked=([regex]::Matches($section,"(?m)^- \[ \]")).Count; $plan=Join-Path $f "plan.2026-07-16T12-39.md"; $planChecked=(Select-String -Path $plan -Pattern "^- \[x\] \[P\d+-T\d+\]").Count; $planUnchecked=(Select-String -Path $plan -Pattern "^- \[ \] \[P\d+-T\d+\]").Count; "AC_CHECKED=$acChecked"; "AC_UNCHECKED=$acUnchecked"; "ORIGINAL_PLAN_CHECKED=$planChecked"; "ORIGINAL_PLAN_UNCHECKED=$planUnchecked"; if($acChecked -ne 3 -or $acUnchecked -ne 0 -or $planChecked -ne 29 -or $planUnchecked -ne 0){exit 1} }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/status-final.2026-07-16T16-18.md`.
  - Acceptance: `EXIT_CODE: 0`; issue AC remain 3/3 checked and the original feature plan remains 29/29 checked; the command is read-only and criterion/task text and checkbox state remain unchanged.

- [x] [P2-T7] Record the automated remediation-readiness handoff for the orchestrator.
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/remediation-readiness.2026-07-16T16-18.md`.
  - Acceptance: the artifact contains `Timestamp:`, `Command: evidence review of P0-T6 through P2-T6`, `EXIT_CODE: 0`, and an `Output Summary:` with `REMEDIATION_READINESS=PASS`, `TRX_XML_VALID=True`, `EFFECTIVE_TREE_DIFF_CHECK=PASS`, `WORKING_TREE_DIFF_CHECK=PASS`, `IMMUTABLE_CSHARP_AND_COVERAGE_HASHES=PASS`, `FORBIDDEN_EVIDENCE_PATH_COUNT=0`, `AC_STATUS=3/3`, and `ORIGINAL_PLAN_STATUS=29/29`; it also states the mandatory next orchestrator gate: after committing these remediation changes, run `git diff --check bump-release...HEAD` and repeat feature review before PR creation.

## Executor Completion Contract

The executor may report this remediation plan complete only when every task is checked, every named artifact exists with its required schema, no unauthorized path changed, and P2-T7 records `REMEDIATION_READINESS=PASS`. The executor must return the plan path, modified path list, evidence path list, and the exact signal `REMEDIATION_EXECUTION: PASS`. Any mismatch returns `REMEDIATION_EXECUTION: BLOCKED` with the exact failed task and evidence.
