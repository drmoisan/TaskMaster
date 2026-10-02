# feature-review memory index

## Coverage evidence and hooks
- [csharp-coverage-artifact-is-cobertura](project_csharp-coverage-artifact-is-cobertura.md) — artifacts/csharp/coverage.xml format varies; read the root element first
- [feature-evidence Cobertura counts as artifact](project_feature-evidence-cobertura-counts-as-coverage-artifact.md) — #424: committed `<FEATURE>/evidence/` coverage = artifact present
- [jacoco-summary-substitution-is-valid](project_jacoco-summary-substitution-is-valid-coverage-evidence.md) — re-sum package counters; distrust sub-noise deltas
- [csharp coverage constants nondeterministic](project_csharp-coverage-constants-nondeterministic.md) — ~0.015-pt band (#511); same-session baseline + per-file diff only
- [csharp-repowide-coverage-below-80](project_csharp-repowide-coverage-below-80.md) — vendor/`*.Test` in denominator inflates line, crushes branch; read `branch-rate` (#826)
- [csharp-local-fullsuite-coverage-blocked](project_csharp-local-fullsuite-coverage-blocked.md) — Moq binding redirect; repo-wide gate is the PR CI run
- [convert raw .coverage when canonical XML absent](project_csharp-coverage-independent-verification-via-raw-coverage-conversion.md) — #278: `dotnet-coverage merge -f cobertura`
- [canonical JaCoCo includes uninstrumented assemblies](project_csharp-canonical-jacoco-includes-uninstrumented-assemblies.md) — ~0% rows block sub-75; re-scope (#328); not guaranteed (#392)
- [rescoping doesn't always clear floor](project_rescoping-to-instrumented-package-does-not-always-clear-floor.md) — #392: QuickFiler itself 73.68/64.62
- [deletion-only PR, absent artifact](project_deletion-only-pr-absent-coverage-artifact-309.md) — #309: still FAIL per artifact-absence rule; procedural disposition
- [modified-file sub-floor non-blocking](project_modified-file-subfloor-nonblocking-disposition-230.md) — #230: FAIL row non-blocking when >=80%, no changed-line regression, improved
- [partial remediation still fails new-code floor](project_partial-remediation-new-code-floor-still-fails-209.md) — #209: 0->7.7% still FAILs; recommend maintainer exemption
- [80-vs-85 floor doc conflict](project_build-ci-coverage-gate-fidelity-epic-outcome.md) — CLAUDE.md 80/90 vs rules 85/75; report against whichever governs
- [measure every changed file](feedback_measure-every-changed-file-not-just-the-ac-named-one.md) — per-file aggregation exposed a 77.05% call-site regression no artifact reported
- [verify-zero-own-effect-coverage-noise](project_verify-zero-own-effect-coverage-noise-491.md) — grep both Cobertura XMLs for the changed assembly (#491)
- [package-counter delta proves new-type coverage](project_package-counter-delta-corroborates-new-type-coverage.md) — #503
- [Cobertura `.//line` double-count trap](project_cobertura-class-line-double-count-trap.md) — #670: use `lines/line`, cross-check `line-rate`
- [Cobertura `<class>` Grep keys on `filename=`](project_956-review-residuals.md) — #956: attribute order is `line-rate branch-rate complexity name filename`; a `class name=... filename=` pattern finds nothing; closure classes stay separate rows
- [Cobertura `d__N` complexity proves new-branch coverage](project_285-review-residuals.md) — #285
- [TestResults XML cross-module check](project_testresults-coverage-xml-cross-module-check.md) — single-project vstest XML also instruments other modules
- [StoreWrapperController absent from Cobertura](project_storewrapper-controller-absent-from-cobertura.md) — pre-existing (#287)
- [const-string JS bridge files zero lines](project_const-string-js-bridge-files-zero-cobertura-lines.md) — #737: correct signal, not a gap
- [don't trust "unreachable" on an escape set](feedback_dont-trust-the-unreachable-label-on-a-coverage-escape-set.md) — check U against BASELINE hits
- [ExcludeFromCodeCoverage ruling](project_excludefromcodecoverage-attribute-ruling.md) — attributes not Blocking; the rules clause covers config `exclude` globs
- [powershell-coverage-mandatory-when-ps1-in-diff](feedback_powershell-coverage-gate.md) — hook blocks without a PS PASS/FAIL verdict
- [PowerShell coverage nondeterministic](project_powershell-coverage-nondeterministic-vsbuild-tests.md) — measure in-session, never quote a stored figure
- [Pester counts commands, not lines](project_pester-line-coverage-node-appears-only-with-an-analyzable-command.md) — operand-only line gets no node
- [Pester breakpoints bind to the FIRST ParseFile copy](project_pester-breakpoint-coverage-binds-to-first-parsefile-copy.md) — #928: an entry-point line reached only by a later-sorting suite reads 0 hits though its test passes; relocate logic to a path-loaded part file
- [poshqc bundled coverage reads zero](project_poshqc-bundled-coverage-artifact-reads-zero.md) — use direct-Pester JaCoCo (#441)
- [PowerShell line-count undercount](powershell-measure-object-line-undercount.md) — `Measure-Object -Line` vs `awk NR`
- [durable feature-script triggers Python gate](project_durable-feature-script-triggers-python-coverage-gate.md) — #354
- [coverage hook: label+coverage+PASS/FAIL one line](project_coverage-hook-label-plus-verdict-same-line-507.md) — #507: dot-source and simulate
- [coverage hook label substring quirks](project_coverage-hook-label-substring-false-positive.md) — "csharpier"->C#, "pester"->PS
- [coverage hook forces FAIL below 85%](coverage-hook-forces-fail-below-floor-despite-exemption.md) — #283: never PASS a below-floor row
- [coverage hook skips without pr_context summary](coverage-hook-skips-when-no-pr-context-summary.md) — only 3 path checks run; still write clean rows
- [coverage hook trusts misclassified summary](project_coverage-hook-trusts-misclassified-summary.md) — C#-as-docs skips C# enforcement
- [Template `N/A - out of scope` vs narrowing regex](project_template-na-wording-vs-hook-narrowing-regex.md) — safe only for zero-file languages
- [stale untracked coverage.xml false-blocks](project_stale-untracked-coverage-xml-leftover-false-block.md) — #398: remove it; simulate first
- [review worktree differs from session cwd](project_review-worktree-differs-from-session-cwd-mirror-artifacts.md) — mirror the 3 artifacts into session cwd (again at #930)
- [epic fan-in artifact path + regex traps](project_epic_fanin_artifact_path_and_hook_regex.md) — hook demands docs/features/active/; UNVERIFIED narrows

## PR context, base, scope
- [pr-context-summary-misclassifies-cs](project_pr-context-summary-misclassifies-cs.md) — recurring; verify scope against git diff
- [pr_context artifacts are TRACKED](project_pr-context-artifacts-are-tracked-not-gitignored.md) — main carries a stale pair; derive from git
- [PR-context MCP unavailable](project_pr-context-mcp-unavailable-manual-fallback.md) — #269: hand-author from `git diff --numstat`
- [pr-context stale after remediation commit](project_pr-context-stale-after-remediation-commit.md) — compare HEAD vs summary Head ref
- [Stale caller-supplied merge-base](project_stale-caller-merge-base.md) — #244: recompute `git merge-base`
- [three-dot degenerates when base is ancestor](project_three-dot-degenerates-when-base-is-ancestor.md) — #735
- [epic-child two-dot divergence noise](project_epic-child-twodot-diff-divergence-noise.md) — use three-dot (#307)
- [gitignore-tracking-expands-diff-scope](project_gitignore-tracking-diff-scope.md) — un-ignoring `.claude/` adds the subtree; audit it
- [plan trailer preflight directive is benign](project_plan-trailer-preflight-directive-benign.md) — not scope narrowing
- [modified-workflow green-run gate is manual](project_modified-workflow-green-run-manual-check.md) — #267
- [remediation-handoff skill vs hook layout](project_remediation-handoff-skill-conflicts-with-hook.md) — flat timestamped form; validators absent here
- [re-audit cycle playbook](project_re-audit-cycle-review-playbook.md) — split remediable blockers from PR-time gates

## Toolchain and build traps
- [msbuild-invocation-via-bash](project_msbuild-invocation-via-bash.md) — not on bash PATH; `Any CPU` needs a .cmd wrapper
- [two vstest binaries binding-redirect trap](project_two-vstest-binaries-binding-redirect.md) — #503: use Extensions\TestPlatform
- [vstest argument order + transitive dep](project_vstest-argument-order-transitive-dep.md) — #418: run a changed test assembly ALONE
- [mandated nullable gate is vacuous](project_nullable_build_gate_is_vacuous.md) — #503: force `/t:Rebuild`
- [LangVersion-less test projects CS8630](project_langversion-missing-test-projects-cs8630.md) — #418
- [nullable-epic full-solution TWAE pre-existing](project_nullable-epic-fullsolution-twae-preexisting-blocker.md) — #364
- [nullable-remediation-epic review pattern](project_nullable-remediation-epic-review-pattern.md) — #363+: per-project AC1 proof
- [csharpier formats xml, probe it](project_csharpier-formats-xml-probe-verification.md) — reproduce "formatter mandated this"
- [koverage analyzer finding misattributed](project_koverage-analyzer-finding-misattributed.md) — Get-CoberturaLineConditionCoverageParts
- [SVGControl stale binding redirect](project_svgcontrol-stale-binding-redirect-out-of-scope.md) — still stale 2026-07-18
- [bash heredoc backslash + /tmp traps](project_bash-heredoc-backslash-and-tmp-traps.md) — Windows python3 can't see /tmp
- [Write-Verbose remedy is inert](feedback_write-verbose-remedy-is-inert-without-a-verbose-call-site.md) — needs -Verbose at the call site

## Verification discipline
- [Verify parity claims](feedback_verify-parity-claims-in-remediation-inputs.md) — #418: measure every claim on disk
- [verify the asserted evidence mechanism](feedback_verify-asserted-evidence-mechanism.md) — #418: grep the mechanism, correct the basis
- [Verify the caller's factual "correction"](feedback_verify-the-callers-factual-correction.md) — #670
- [test files count toward 500-line limit](feedback_test-file-500-line-limit.md) — compare baseline vs head counts
- [evidence Timestamps can be synthetic](project_evidence-timestamps-are-synthetic-cross-check-commit-dates.md) — #648: cross-check commit dates; Cobertura root `timestamp=` epoch is a no-shell clock (#942)
- [same-commit differing-outcome flake check](project_same-commit-differing-outcome-flake-check.md) — #261
- [SHA-256 as compile/footprint proof](project_sha256-compile-footprint-proof.md) — #644/#647
- [RED-first equivalence patterns](project_red-first-equivalence-patterns.md) — RED TRX then fix (#489); compile-red (#677)
- [sibling `Should().Be(other)` has no pinning power](project_662-sibling-assertion-blind-spot.md) — evaluate EVERY assertion
- [null-conditional fix relocates NRE](project_null-conditional-fix-relocates-nre-check-callers.md) — #507: grep every call site
- [DoNotParallelize census misses lazy-init writers](project_donotparallelize-census-misses-lazy-init-writers.md)
- [YAML comment-only diff proof](project_yaml-comment-only-diff-proof-via-parse-tree.md) — parse-tree compare, 2 parsers
- [orchestrator-state verifies ratification claims](project_orchestrator-state-human-interaction-verifies-scope-change-ratification.md) — gitignored state
- [maintainer waiver hides in gitignored state](project_maintainer-waiver-recorded-only-in-gitignored-state.md) — require transcription into issue.md
- [ScoDictionaryNew swap hazards](project_scodictionary-new-enumeration-order-and-removal-api.md) — enumeration order; .TryRemove
- [projectentry setter raw MessageBox](project_projectentry-setter-raw-messagebox-blocks-coverage.md) — #199 uncoverable via MyBox seam
- [breadcrumb Close returns before OpenState=false](project_breadcrumb-close-returns-before-openstate-false.md) — #656
- [505 coordinator prime/toggle race](project_505-coordinator-prime-toggle-race.md) — TryAdd fix recommended

## Artifact hygiene
- [Never embed absolute host paths](../_shared_no_absolute_host_paths.md) — no account/host names; control vstest `/ResultsDirectory:`
- [sweep drive-letter paths, not just identity patterns](feedback_sweep-drive-letter-paths-not-just-identity-patterns.md) — #930: `Using vstest.console: C:\Program Files\...` passed an identity gate
- [Cobertura substitution leaves blobs](project_cobertura-substitution-leaves-blobs-in-history.md) — #648: squash-merge
- [policy-audit-template MCP unavailable](project_policy-audit-template-mcp-unavailable-737.md) — hand-author the 12 headings

## Artifact structure (cross-repo validator, run by the orchestrator)
- [Validator IS run by the orchestrator](project_taskmaster-validator-memories-are-cross-repo.md) — canonical template structure on the FIRST draft (#781)
- [policy-audit required structure](policy-audit-required-structure.md) — Appendix A/B, 4 TS/PS checklist lines, numeric comparison line, 7-col metrics table
- [policy-audit comparison-line schema](policy-audit-comparison-line-schema.md) — `Baseline:` `Post-change:` `Change:` `Disposition:` `Evidence:` labels
- [policy-audit validator uses full template](policy-audit-validator-uses-full-template.md) — `## Executive Summary` + `## 1`..`## 7`
- [policy-audit section-7 row-label parser](policy-audit-section7-row-label-parser.md) — reuse cycle-1 wording
- [numeric new-code coverage + Scope-and-Baseline](policy-audit-numeric-new-code-coverage.md) — literal percent; feature-audit needs `## Scope and Baseline`
- [feature-audit check-off heading case](feature-audit-checkoff-heading-case.md) — `## Acceptance Criteria Check-off`
- [feature-audit requires Summary heading](feature-audit-requires-summary-heading.md) — literal `## Summary`
- [code-review findings table header](code-review-findings-table-header.md) — exact 7-column header
- [code-review/feature-audit required headings](code-review-required-headings.md) — `## Executive Summary`, `## Findings Table`, `## Acceptance Criteria Inventory/Evaluation`

## Closed-issue residuals (lookup only)
- [review-residuals index](project_review-residuals-index.md) — one-line pointers for #442-#942 (incl. #930 AC7 host-path PARTIAL; #942 `.csproj` disarms the C# hook check)
- [438](project_438_cycle1_findings.md) · [440](project_440-review-residuals.md) · [441](project_441-review-residuals-and-494-handoff.md) · [457](project_457-review-residuals.md) · [464](project_464-review-residuals.md) · [489](project_489-review-residuals.md) · [493](project_493-review-residuals-and-msbuild-log-gate-adjudication.md) · [511](project_511-rescope-review-residuals.md) · [553](project_553_ci_split_review_pattern.md) · [565](project_565-review-residuals.md) · [584](project_584-review-residuals.md)
- [644](project_644-review-residuals.md) · [645](project_645-review-residuals.md) · [647](project_647-review-residuals.md) · [677](project_677-review-residuals.md) · [680](project_680-review-residuals.md) · [707](project_707-review-residuals.md) · [730](project_730-review-residuals.md) · [731](project_731-review-residuals.md) · [735](project_735-review-residuals.md) · [736](project_736-review-residuals.md) · [751](project_751-review-residuals.md)
- [752](project_752-review-residuals.md) (scope lock doesn't discharge host-path sweep) · [781](project_781-review-residuals.md) · [791](project_791-review-residuals.md) · [799](project_799-review-residuals.md) · [826](project_826-review-residuals.md) · [287](project_287-review-outcome.md) · [928](project_928-review-residuals.md) (cycle 1: AC6 uncredited line; exit: PASS 7/7 after part-file relocation; no-Bash review reads gitignored JaCoCo `<sourcefile>` nodes directly; 3-`..` hook path; P-4 bundled PoshQC coverage never covers `scripts/`) · [929](project_929-review-residuals.md) (PASS 7/7, 0 blocking; AC met by tests of a pre-existing rule accepted; labels led UTC clock 38-72 min; `.bak` residue = follow-up) · [944](project_944-review-residuals.md) (PASS 18/18, 0 blocking; keyed TryRemove safe under single-writer-under-lock; Cobertura root epoch as no-shell clock; 3 follow-ups owed) · [945](project_945-review-residuals.md) (PASS 8/8, 0 blocking; attribute-exempt overloads, per-file 24/25 as new-code figure; SortEmail.cs 1454-line pre-existing breach) · [940](project_940-review-residuals.md) (PASS 8/8, 0 blocking; per-file coverage ruling verified at Cobertura `<class>` nodes; worktree `logs/HEAD` reflog epochs as no-shell clock; stale session-cwd coverage.xml handled with an honest FAIL line; 3 follow-ups owed) · [947](project_947-review-residuals.md) (PASS 7/7, 0 blocking; empty `catch (Exception)` around a sink accepted via the SafeLog precedent; third unguarded sink `_notifyUnavailable` owed as follow-up; 5 follow-ups) · [956](project_956-review-residuals.md) (PASS 17/17, 0 blocking; partial split + prompt-session seam; Cobertura `<class>` Grep must key on `filename=`; stale session coverage.xml is Cobertura so the hook's JaCoCo parse is null; DirectoryInfo-inside-try spec inconsistency CR-1)
