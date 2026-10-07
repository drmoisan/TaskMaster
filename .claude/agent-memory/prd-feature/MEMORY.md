- [push-down command pattern](project_push_down_pattern.md) — 10-file change map for adding a new push-down command; reference impl is pushDownCodexAndAgentsCustomizations
- [Promotion scaffold metadata defects](project_promotion_scaffold_metadata_defects.md) — fix Status folder path and Last Updated date in scaffolded issue.md before filling docs
- [Test disposition: grep for run-time-only bindings](feedback_test_disposition_overload_pins.md) — before marking a test file "unchanged", grep for Setup/Verify of retired overloads AND GetField reflection on renamed private fields
- [AC gates: verify satisfiability + fresh reads](feedback_ac_gates_verify_satisfiability.md) — check baselines before repo-wide floors; digits on AC lines are OK only when the single `research-path` record passes the 11-label hook (trace it by hand; item 10), else enumerate/absence; no "every file in the Write Set" line-ceiling gate; grep asserted tokens for exact casing; re-read spec before tallies
- [Inherited AC from an upstream sibling](feedback_inherited_ac_from_upstream_sibling.md) — when an upstream epic sibling's diff already satisfies a promoted AC, write it inherited-and-verified (confirm the site is ABSENT), don't drop or restate it as this feature's own work
- [full-bug means spec.md is the only AC source](feedback_full_bug_spec_only.md) — no user-story.md by default (Expected Outputs header vs AC-tracking skill); two exceptions (epic-prep route, cross-reference instruction) handled by making it checkbox-free narrative with a banner
- [Backticked paths ARE the change footprint](feedback_backticked_paths_are_the_change_footprint.md) — a harvester reads backticked paths from spec.md/plan; backtick every in-scope file, leave out-of-scope citations unbackticked; strictest form is a `## Write Set`-only section
- [Invariant + accept-to-throw trace in Proposed Fix](feedback_invariant_and_trace_in_proposed_fix.md) — state the contract in one sentence and trace one accepted value through accept/throw/absorb/new-catch, with an AC pinning it
- [#522 nullable type-check — RESOLVED in CLAUDE.md](project_522_nullable_typecheck_deviation.md) — as of 2026-08-26 quote CLAUDE.md's msbuild commands directly; no deviation note; keep the /t:Rebuild non-vacuity reasoning
- [MSBuild non-vacuity assertion](project_msbuild_nonvacuity_assertion.md) — prove a build compiled with zero `Skipping target "CoreCompile"`; csc.exe counts and CoreCompile headers both mislead
- [UiThread seam conversion belongs to #584](project_uithread_static_seam_belongs_to_584.md) — don't open a new issue for the IUiDispatcher seam (~62 refs/29 files); #584 and #493 share the same static root
- [suggestion-severity analyzers are invisible to msbuild](reference_suggestion_severity_invisible_to_msbuild.md) — banned-DocID ACs need positive observation + a control, never an absence check; adding a DocID at `suggestion` enforces nothing
- [C# tests go in <Project>.Test, not tests/](project_csharp_test_location_policy_conflict.md) — CLAUDE.md outranks .claude/rules on test location; legacy csproj needs an explicit <Compile Include> or the tests silently don't exist
- [Enumeration-gap bugs: widen to all sibling sites](feedback_enumeration_gap_bugs_widen_to_all_sibling_sites.md) - put every sibling site the research found in the write set; a narrow fix re-creates the gap the issue exists to close
- [Doc-derived API needs a compile-proof AC](feedback_documentation_derived_api_needs_compile_proof_ac.md) - if research verified a member from shipped XML docs not IL, keep the hedge, demand a captured build log as the AC PASS condition, and name the fallback
- [Promotion scaffold metadata defects](project_promotion_scaffold_metadata_defects.md) — fix Status path, Last Updated date, line-wrap-shredded AC checkboxes, and DoD checkbox inflation before filling docs
- [Test disposition: grep for old-overload pins](feedback_test_disposition_overload_pins.md) — grep test project for Setup/Verify of retired overloads before marking any test file "unchanged"; loose mocks fail at run time
- [Interface files are zero-denominator for coverage](reference_interface_files_zero_coverage_denominator.md) — reusable 3-proof argument (no body / net48 no DIM / no Cobertura class element) + why shape tests are rejected

## Additional entries

- [QuickFiler per-file coverage baseline](reference_quickfiler_perfile_coverage_baseline.md) — grep the #424 Cobertura artifact for indicative per-file rates before scoping an epic #136 child; most files are already above 80%
- [WinForms Designer partial coverage](reference_winforms_designer_partial_coverage.md) — 4-part argument that a *.Designer.cs is `testable`: QfcFormViewer positive control, no permitted exemption, 99.9% line on one construction, branch capped at 75%

## Additional entries

- [Ratified exemption boundaries](reference_ratified_exemption_boundaries.md) — check docs/features/archive/ for a maintainer-decision artifact before planning any [ExcludeFromCodeCoverage] removal; never promise N -> 0
- [ExcludeFromCodeCoverage lambda propagation](reference_exclude_from_code_coverage_lambda_propagation.md) — method-level leaks nested lambdas into the denominator, class-level does not; a partial-class attribute exempts the whole type
- [Repo-walking count tests must exclude .claude](reference_repo_walking_tests_exclude_claude_worktrees.md) — nested agent worktrees hold full csproj copies; put the .git/.claude/packages/bin/obj exclusion list in the AC text, pair count with content assertion
- [Numeric AC without full derivation: phrase as exclusion](feedback_numeric_ac_without_full_derivation_phrase_as_exclusion.md) — caller wants "exactly one remains" but the research derivation block covers another family; write "no file other than the named one", keep counts informational, say why
- [500-line ceiling counts TOTAL lines](feature_line_ceiling_counts_total_lines.md)
- [Negative control must isolate the code fix](feedback_negative_control_must_isolate_the_code_fix.md) — pin the control's config/environment so declarative hardening can't satisfy it; Pester stops an It at the first failing Should, so a two-half fix needs two staged fail-before artifacts
- [Outcome AC when the mechanism is unverified](feedback_outcome_ac_when_mechanism_unverified.md)
- [issue.md scope text vs orchestrator decisions](feedback_issue_md_scope_conflicts_with_orchestrator_decisions.md) — follow the binding orchestrator decisions, flag the conflict in Scope & Non-Goals and the report, never resolve it silently (#959)
- [Issue 671: projections-only evidence](project_671_projections_only_evidence.md) — no new raw TRX/coverage XML in the repo (effective 2026-09-12); name fixed-filename Markdown projections in full feature-relative backticked paths, never bare `evidence/...`
- [Scope amendment: narrow exclusions + log](feedback_scope_amendment_narrow_exclusions_and_log.md) — when related defects are folded in mid-item, narrow every "no X" to "no X other than the named files, each limited to ...", quote old wording in a Scope Amendment Log, and spell out the partial-split rule; second research record goes on `supplemental-research-path:`
