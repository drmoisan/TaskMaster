# Parallel run bug-families-01 — selection record

Run slug: `bug-families-01`. Plan-home branch: `parallel/bug-families-01-plan`.

Selected 2026-08-23 from the 78 open `bug`-labelled issues on `main @ 988e819b`.

## Selection rule

One issue per blast-radius contention family, preferring the highest-impact bug in each family.
Contention is computed at the project-module level, so any two bugs in the same C# project
conflict on `module_overlap` regardless of which files they touch. The distinct families that
actually carry open bugs are: `QuickFiler`, `QuickFiler.Test`, `UtilitiesCS`, `UtilitiesCS.Test`,
`TaskMaster`, and the module-free `scripts/vscode` coverage tooling. Six families therefore set
the ceiling on a single cohort; items 7 and 8 deliberately double up on the two largest
families so the run uses all 8 concurrency slots across an expected 2 cohorts.

Final cohort assignment is computed from plan-derived blast radii after preparation, not from
this table. The predicted split is an expectation, not a guarantee.

## Selected items (8)

| issue | family | predicted cohort | rationale |
| --- | --- | --- | --- |
| #446 | `QuickFiler` | 0 | IterateQueueAsync closes the queue before its deadline, so queued items are dropped silently. Silent data loss is the most severe class in this family. |
| #504 | `TaskMaster` | 0 | Ribbon callback names referenced by the ribbon XML no longer resolve, so the callbacks are dead. Only selected bug whose radius is TaskMaster/Ribbon/**. |
| #584 | `UtilitiesCS` | 0 | UiThread dispatcher null race in ProgressTrackerAsync. A race in shared threading infrastructure is used by every downstream project. |
| #516 | `UtilitiesCS.Test` | 0 | TimeoutAfter tests race a real wall-clock deadline, violating the determinism-infrastructure rule in general-unit-test.md (banned real wall-clock waits). |
| #531 | `scripts/vscode` | 0 | MSTest coverage discovery does not exclude .claude worktrees, so local coverage runs load stale assemblies. Carries no C# project module, so it is its own contention family. |
| #469 | `QuickFiler.Test` | 0 | QfcCollection move-diagnostics defects. Chosen as the QuickFiler.Test representative; if its plan reaches QuickFiler production sources it will re-cohort with #446. |
| #448 | `QuickFiler` | 1 | UndoConsumer non-terminating loop (hang). Deliberate second QuickFiler item: it conflicts with #446 on module_overlap and is expected to seed cohort 1. |
| #287 | `UtilitiesCS` | 1 | StoreWrapper dialog is imprecise for a genuine failure. Deliberate second UtilitiesCS item: conflicts with #584 on module_overlap and is expected to seed cohort 1. |

### Titles

- #446 — Bug: iteratequeueasync-deadline-closes-queue-early
- #504 — Bug: ribbon-dead-callback-names
- #584 — Bug: uithread-dispatcher-null-race-progresstrackerasync
- #516 — Bug: timeoutafter-tests-race-real-wall-clock-deadline
- #531 — Bug: mstest-coverage-discovery-claude-worktree-exclusion
- #469 — Bug: qfc-collection-move-diagnostics-defects
- #448 — Bug: quickfiler-undoconsumer-nonterminating-loop
- #287 — Bug: storewrapper-dialog-imprecise-for-genuine-failure

## Excluded: not fixable in this checkout (6)

These target `drm-copilot` MCP tools or `scripts/dev_tools/`. Verified 2026-08-23 that
`extensions/` and `scripts/dev_tools/` are both ABSENT from TaskMaster; TaskMaster is a
C#/PowerShell repository. These belong upstream in the drm-copilot repository.

- #589 — Bug: collect-pr-context-shared-path-race-across-concurrent-children
- #555 — Bug: orchestrator-hooks-reference-absent-python-validators
- #554 — Bug: potential-to-issue-promoted-copy-not-written
- #546 — Bug: research-doc-cohort-library-false-negative
- #536 — Bug: poshqc-test-coverage-capture-records-zero
- #513 — Bug: collect-pr-context-misclassifies-csharp-as-documentation

## Excluded: CI / coverage-threshold chain (6)

This cluster carries real ordering (the threshold policy must be reconciled before any gate is
built against it). The parallel surface cannot express ordering: `depends_on` and `wave` are
prohibited keys and cohorts only guarantee non-concurrency, never sequence. Route to
`/epic-plan` + `/epic-orchestrate`.

- #565 — Invoke-MSTestWithCoverage.ps1 asserts coverage threshold before Set-Content, leaving the raw un-post-processed Cobertura on disk when the gate fails
- #564 — CLAUDE.md cites ci.yml for three toolchain commands the #553 split moved into reusable workflows
- #563 — Coverage threshold contradiction remains: CLAUDE.md/csharp.md say 80%, general-unit-test.md/quality-tiers.md say 85%/75%, and two live gates disagree
- #562 — No Pester job in CI: production PowerShell under scripts/vscode has zero CI coverage
- #561 — CI collects coverage but enforces no threshold: _mstest-coverage.yml never converts to Cobertura or compares a floor
- #569 — Bug: ci-nuget-cache-fallback-masks-stale-package-refs

## Deferred to the serial queue (58)

Predominantly the QuickFiler cluster. Every pair conflicts on `module_overlap`, so scheduling
them in a parallel run yields one item per cohort with a full CI cycle and PR merge between
each. A serial queue delivers the same throughput without the cohort-barrier overhead.
Deferred is not dropped: any of these can be admitted later via `/parallel-add` into an
open-mode run, or planned as their own run once the QuickFiler cluster drains.

- #597 — Bug: csproj-analyzer-paths-stale-after-dependabot-bump-breaks-fresh-clone
- #586 — Bug: utilitiescs-test-form1-live-form
- #570 — Bug: system-reactive-7-packages-config-unsupported
- #560 — Bug: overload-name-collision-under-exclusion
- #559 — Bug: local-functions-in-exempt-members-remain-counted
- #537 — Bug: cobertura-max-hits-update-branch-untested
- #532 — Bug: agent-memory-cobertura-dedup-generalization-wrong
- #530 — Bug: cobertura-merged-class-methods-incomplete
- #529 — Bug: cobertura-package-rates-not-recomputed
- #525 — Bug: engine-toggle-prime-last-writer-race
- #524 — Bug: ribbon-controller-intelligence-unguarded-globals-deref
- #520 — Bug: console-setout-races-under-class-parallelism
- #502 — Bug: breadcrumb-suggestions-upgrade-silently-stale-on-superseded-lease
- #501 — Bug: breadcrumb-hub-postjson-caches-before-broadcast-starves-attachments
- #500 — Bug: breadcrumb-webview-post-executes-under-upgrade-lifetime-lock
- #499 — Bug: breadcrumb-router-stale-selectedfolderpath-after-rebind
- #498 — Bug: breadcrumb-router-segment-index-unvalidated-host-crash
- #493 — Bug: uithread-dispatcher-static-swap-no-restore
- #490 — Bug: itemviewer-display-and-folder-contract-defects
- #489 — Bug: itemviewer-ui-thread-marshalling-divergence
- #488 — Bug: itemviewer-breadcrumb-pipeline-lifecycle
- #487 — Bug: itemviewer-parentchanged-console-and-cast
- #486 — Bug: itemviewer-move-option-menu-defects
- #485 — Bug: qfc-item-controller-webview-handler-unguarded-inputs
- #484 — Bug: qfc-item-controller-cleanup-timer-and-stale-field-defects
- #483 — Bug: qfc-item-controller-mailactions-error-handling-defects
- #482 — Bug: qfc-item-controller-expansion-registry-divergence
- #481 — Bug: qfc-item-controller-no-event-unwiring-path
- #480 — Bug: qfc-item-controller-togglenavigation-double-toggle
- #477 — Bug: iwebviewcoreinitializer-contract-defects
- #476 — Bug: webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state
- #475 — Bug: breadcrumb-capturecurrentortests-silently-degrades-in-production
- #474 — Bug: qfc-collection-controller-coupling-and-modal-getter
- #473 — Bug: qfc-collection-background-task-and-catch-defects
- #472 — Bug: qfc-collection-navigation-digits-desync
- #471 — Bug: qfc-collection-eliminate-space-sign-error
- #470 — Bug: qfc-collection-conversation-index-defects
- #468 — Bug: qfc-collection-controller-unreachable-load-paths
- #467 — Bug: efc-viewer-processcmdkey-swallows-alt-mnemonics
- #466 — Bug: efc-dead-code-and-latent-nre-traps
- #465 — Bug: efc-form-controller-lifecycle-and-selection-defects
- #464 — Bug: efc-controllers-null-guard-and-async-void-boundary-defects
- #463 — Bug: quickfiler-webview2-incognito-arg-en-dash
- #462 — Bug: breadcrumb-dropdown-coordinator-stale-closepending-drops-reopen
- #461 — Bug: efc-item-controller-dead-conversation-expanded-handler
- #460 — Bug: efc-item-controller-cleanup-nre-and-timer-leak
- #459 — Bug: efc-item-controller-keyboard-registration-defects
- #458 — Bug: webview2breadcrumbhost-handler-retention-pooled-viewer
- #451 — Bug: efc-home-controller-metrics-inert-duration
- #444 — Bug: kbdactions-enumerable-ctor-bypasses-duplicate-guard
- #443 — Bug: qfc-home-controller-metrics-duration-misread
- #442 — Bug: qfc-home-controller-metrics-never-flushed
- #440 — Bug: breadcrumb-left-right-arrow-parent-child-navigation
- #439 — Bug: efcviewer-missing-lineage-and-segment-navigation
- #427 — Bug: quickfiler-post-show-duplicate-scoring
- #426 — Bug: emailmovemonitor-rejected-item-hook-retention
- #286 — Bug: qfc-collectioncontroller-removespecificcontrolgroup-counter-leak
- #285 — Bug: timeouttask-runwithtimeout-exception-type-mismatch

## Contention visible outside this run

At planning time these branches held in-flight work on QuickFiler and the coverage surfaces,
and are invisible to this run's cohort scheduling:

- `feature/quickfiler-per-file-coverage-capstone-r2`
- `feature/quickfiler-breadcrumb-bridge-coverage-r2`
- `bug/quickfiler-test-form1-live-form-491-exec`
- `bug/winformspumphost-suite-determinism-511-exec`

Items #446, #448, #469 (QuickFiler) and #531 (coverage tooling) are the exposed selections.
Re-check these branches before executing the run.

