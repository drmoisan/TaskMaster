---
name: project-614-store-root-leak-plan-seams
description: "#614 plan seams: AC25 net non-growth (3 over-limit files incl. Issue439Tests 695); E1 spec-corrects the D1-codifying Issue439 boundary test (P3-T4, run task now P3-T5); SelectRow guard scope pinned (out-of-root only); evidence-path normalization; host-identifying raw TRX stays out of evidence"
metadata:
  type: project
---

Plan `plan.2026-08-26T09-59.md` for issue #614 (store-root path leak, work mode full-bug, 26 ACs, 11 phases / 70 tasks).

Key seams a revision loop must not undo:

- **AC25 is unsatisfiable as written.** `QuickFiler/Controllers/EfcFormController.cs` (1084 lines) and `BreadcrumbBridgeRouter.cs` (596) exceed 500 pre-change. Plan encodes net non-growth for those two + <=500 for everything else, and places the D9 shared predicate in a NEW file `QuickFiler/Controllers/EfcSelectionGuard.cs` so EfcFormController does not grow. Flagged for orchestrator review in P10-T25.
- **Near-limit test files** (FolderConverterTests 446, BreadcrumbBridgeRouterTests 435, EfcDataModelTests 409): new matrices go to `*Issue614Tests.cs` companion files; spec-pinned placements kept (AC17 test in EmailFilerConfig_Tests.cs, AC18 test in BreadcrumbBridgeRouterTests.cs, AC11 `:329` update in place).
- **Spec names non-canonical evidence paths** `evidence/coverage/` and `evidence/qa/`; plan records two EVIDENCE_LOCATION_OVERRIDE_REJECTED lines normalizing to `evidence/baseline/` + `evidence/qa-gates/`.
- **Raw TRX and raw coverage output embed account/host names** — AC21 (redaction, issue #602) forbids them under evidence/. Plan sends raw output to gitignored `coverage/trx/<task-id>/` and puts hand-authored redacted Markdown summaries under evidence/. See [[terminal-phase-planner-traps]] and the shared no-absolute-host-paths rule.
- **Baseline vstest exit code is 1** (pre-existing #594 Console.Out race, 6481/6482): baseline and final test artifacts must declare `ExpectedExitCode: 1` when the flake fires; a suite-wide Failed:0 gate is forbidden by the caller.
- **#499 boundary:** router rejection leaves SelectedFolderPath unchanged, never null; BindRowsAsync clearing semantics untouched. Empty-archive-root binding mode pass-through preserved (public overload consumers outside EFC chain).
- **Planner decisions recorded in plan:** IsFullOutlookPath also rejects drive-rooted input; `ask` parameter removed (not Obsolete) after a call-site search task.

R1 preflight revision (2026-08-26, applied in place; 11 phases / 80 tasks unchanged):

- **AC25 ratified.** Orchestrator APPROVED the net-non-growth reading (EfcFormController <= 1084, BreadcrumbBridgeRouter <= 596), persisted in `artifacts/orchestration/orchestrator-state.json` under `orchestrator_adjudications` dated 2026-08-26T10:38:00Z. P10-T25 now cites the ratification; no longer "flagged for review".
- **CSharpier wrapped the chain**: `fsPath.Substring(3)` is NOT on one line (:157-159); the P5-T2 token is `.Substring(3)` (1 hit at :158). Companion gates: `olBranchPath.Replace(olAncestorPath, fsAncestorEquivalent)` (:155), `has a value of {fsPathExDividers}` (:164), full `:329` assertion line + `illegalFolderName.Replace(illegalFolderName, "")` (:112) for P5-T4's production half.
- **Scope diff form**: `git diff --name-only $(git merge-base HEAD origin/main)..HEAD` fails under pwsh (usage block, exit 0) AND is blind pre-commit. Canonical form: separate pwsh statements, `git diff --name-only "$base"` (working tree vs merge-base, no `..HEAD`), used in P8-T2 and P9-T6.
- **Coverage figure derivation**: runner throws on vstest exit 1 BEFORE its in-place Koverage rewrite, so with the #594 flake the on-disk `coverage\coverage.cobertura.xml` stays raw; plan copies it to `coverage.cobertura.raw.p#-t#.xml` and saves out-of-band filtered output as `coverage.cobertura.filtered.p#-t#.xml` (P9-T5 inputs). Fully-green run consumes the raw in place; unfiltered figure then recorded unavailable (informational only).
- **Invoke-MSTestWithCoverage.ps1 accepts no /Logger** — Global rule 6 carve-out for P0-T9/P9-T4 (no TRX emitted, no redaction hazard).
- **Nullable split** (Global rule 11): BreadcrumbBridgeRouter/EmailFilerConfig/FolderConverter carry `#nullable enable`; EfcDataModel/EfcFormController/AppOlObjects/AppFileSystemFolderPaths do not.
- **P10-T28 commits `.claude/agent-memory/**`** (tracked, agent-written; clean porcelain unachievable otherwise). AC22 check-off has a real qa-gates artifact with six banned-API zero-hit searches (no "check-off comment" — the tracking skill permits only checkbox flips).
- **IOlObjects.ArchiveRootPath has 19 production call sites**; getter-throw is verified at P9-T4, not assumed. P2-T2 uses no Moq (pure static class).

R2 preflight revision (2026-08-26, B7-B8 + NB1-NB6; 11 phases / 80 tasks unchanged):

- **Six pre-plan branch paths are allowlisted in P8-T2/P9-T6** (`.gitignore` + five `docs/features/potential/promoted/2026-08-26-*.md`): commits `b8776b58`/`34350f45`/`a8a96561`/`aec3f18f`/`5a429486` sit ahead of merge-base `c279d40b`, so `git diff --name-only "$base"` necessarily reports them. Allowlisted only as pre-existing state; a fourth statement `git diff --name-only HEAD` proves the change did not modify them. General lesson: a working-tree-vs-merge-base diff gate on a branch with pre-plan commits needs a pre-existing-paths allowlist plus a `diff HEAD` non-modification proof.
- **P8-T3 redaction sweep carries a recorded exception**: `FolderConverterTests.cs:22-23` contains the fabricated `first.last@company.com` (untouched lines; only `:329` is edited). `@`-string negative claim scoped to changed hunks; account-name claim scoped to whole changed files.
- **Invoke-MSTestWithCoverage.ps1 has NO `\.claude\` exclusion** — its filter is `\bin\<Config>\` minus `\obj\`/`\ref\` only. Global rule 6 and P9-T4 now state this truthfully (this worktree has no nested agent worktrees, so no exclusion is needed).
- **P5-T7 filter needs a third alternation** `FullyQualifiedName~FolderConverter_Tests`: `UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs` exercises `ToFsFolderpath` but `FolderConverter_Tests` does not contain the substring `FolderConverterTests`.
- **P5-T3 must keep `paramName` = `nameof(fsPath)`** — `FolderConverterTests.cs:63` asserts `.WithParameterName("fsPath")`.
- **P1-T3 Helpers extraction requires `public partial class` at `:23`** ([TestClass] stays only on the original file).
- **P9-T5 `>= 84.80` miss is re-measured once** before being read as a regression (dotnet-coverage denominator nondeterminism).
- **P10-T24 restates the AC24 fourth-command substitution** (literal `vstest.console.exe ... /EnableCodeCoverage` executed via the canonical coverage runner).

E1 execution delta (2026-08-26, v1.3; Phase 3 now 5 tasks, 81 total; P0 + P1-T1/T2 committed and frozen):

- **`Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` (:542-616) codified D1/D9** (asserted selecting out-of-root `\External\Clients` and a `string.Empty` root-exact selection). New P3-T4 updates it: post-fix both activations are non-selections — `selected` empty, `SelectedFolderPath` null, `PostMessageJson` `Times.Exactly(2)` → `Times.Never` (rejection returns before the render post). Former P3-T4 run task renumbered P3-T5 (trx/artifact names follow). Carve-outs added in P3-T5, P9-T4, P10-T19 (AC19's enumerated scenarios do NOT include this test), P8-T1 item (h).
- **The Issue439 test file is itself a THIRD pre-existing over-limit file (695 lines)** — added to the AC25 net-non-growth handling (P9-T6 gate <= 695, P10-T25 flags it: the 10:38Z adjudication predates E1).
- **`Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch:165` asserts the rooted-but-UNDER-root `\aRcHiVe\Clients\North`** — stays green only if the P3-T2 SelectRow guard rejects out-of-root full paths ONLY (relative stems and under-root rooted targets pass verbatim; empty-root mode unguarded). P3-T2 now pins this scope; also pins that ToHierarchyPath keeps root-prefixing RELATIVE targets (the #609 mock contracts depend on it) and that the surviving prefix branch drops the dead `TrimStart` so the zero-hit literal gate stays satisfiable.
- **Corrected-line gates count occurrences**: `router.SelectedFolderPath.Should().BeNull();` already exists at :537, so the gate is "exactly two hits", not one. Always count pre-existing occurrences of a corrected assertion line before writing an exactly-one-hit gate.
- Verified green (do not re-open): `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` (binds `\` → trims to empty → preserved pass-through mode) and the three `Issue609_*` tests (under-root stems only).

**Why:** these were all judgment calls resolving spec-vs-reality conflicts; a naive revision pass re-reading spec.md literally would reintroduce unsatisfiable gates.
**How to apply:** on any #614 preflight revision, keep these seams unless the orchestrator explicitly overrules a recorded decision.
