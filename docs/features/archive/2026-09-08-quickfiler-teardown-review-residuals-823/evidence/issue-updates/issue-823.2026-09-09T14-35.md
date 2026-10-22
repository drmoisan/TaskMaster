# Issue update mirror — issue #823

Timestamp: 2026-09-09T14-35

PostedAs: unknown

This mirror records the exact text intended for issue #823. It was not posted from this execution:
the executing agent has no confirmed authority to write to the issue tracker in this run, and the
plan authorises no posting step. The orchestrator or a later step may post it verbatim.

---

## Exact text intended for issue #823

Issue #823, the standing residuals record for the `bugs-2026-09-06` reviews of items 810 and 812,
is delivered. Five of its six entries are addressed and the sixth is deferred upstream with a
recorded reason.

**R1 — per-controller SMTP retry latch shared across stores. Fixed.**
`StoreWrapperController` bounded the AC6 SMTP retry with a single `private bool
_userEmailRetryAttempted;`. One controller observes many distinct `StoreWrapper` instances, so that
flag was consumed by whichever store was displayed first with a null address and every later
store's lookup was suppressed. The flag is replaced by a non-static, readonly
`HashSet<StoreWrapper>` keyed by reference identity, and the retry gate's third conjunct becomes a
membership test with the corresponding `Add`. The first two conjuncts keep their text and order, the
`Add` keeps its position ahead of the lookup so a throwing lookup still consumes that store's
attempt, and the set is never reset. Reference identity rather than `StoreId` is deliberate:
`StoreId` is documented as possibly unreadable at four existing fail-safe sites, so keying on it
would collapse every store with an unreadable id onto one budget, which is this defect reintroduced
in the worst place. Accepted cost: the worst case rises from one blocking UI-thread SMTP lookup per
dialog open to N, bounded by the number of null-address stores in `Model.Stores` — a figure set by
configuration rather than by user gestures, which is the distinction issue #812 drew. Two tests were
added and four prose sites corrected.

**R2 — the issue-818 throw relocated rather than disappeared. Decision recorded, no code change.**
The relocation is an intermediate state. The exception type is unchanged on both sides,
`InvalidOperationException` raised by `ArchiveRootPathGuard.RequireResolvedArchiveRoot`, and the
throw still reaches a UI-dispatcher boundary unhandled. The remaining work is owned by sibling issue
#813, which owns `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, so that file is off
limits here and no scope was widened. Correction for a later reader: the first `FolderArray` read in
`AssignFolderComboBox` occurs at `:200`, so `:212` is the second read rather than the unique
pre-change throw site.

**R3 — `Register` signature and contract disagreed. Reconciled.**
`BreadcrumbPopupOwnerRegistry.Register` declared non-nullable parameters and silently returned on
null, and its XML doc justified the tolerance with the claim that the registration hop runs from a
form lookup that can legitimately find no form. That claim is false: the sole production call site
consumes the lookup with `?.`, so a failed lookup produces a null receiver and skips the invocation
entirely, and the two arguments are `this` and a lambda literal. The signature was right and the
contract was wrong. The silent return is replaced by two explicit `ArgumentNullException` throws,
both XML docs now state rejection, and the ignore test was rewritten into a rejection test asserting
the parameter names and that the registry is provably unchanged. The test file deliberately does not
gain `#nullable enable`, which would raise CS8625 on the two literal null arguments and break the
nullable gate.

**R4 — stale line-count comment. Corrected.**
`BreadcrumbDropDownHost.Open.cs:11` claimed `BreadcrumbDropDownHost.cs` measures 480 lines. It
measures 459, re-measured at delivery and again after the final formatter pass. The token now reads
`(459 lines)`.

**R5 — intermittent dispatcher-transaction test. Observation record only, as scoped.**
`Transaction_SecondCallerCannotInstallUntilTheFirstRestores` is probabilistic by construction, which
its own class doc already records. An append-only observation log was seeded at
`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md`
with the issue-810 observation set of one failure and three passes, each row carrying date, command,
assembly set, parallelism setting and failure text, and each citing the issue-810 evidence path it
was read from. The test method's XML doc now points at that log. No executable statement changed: no
sleep, no retry attribute, no timing tolerance. The test passed in every run of this delivery.

**R6 — plan-authoring guidance amendment. Out of scope, deferred upstream.**
Everything under `.claude/` in this repository is pushed down from the separate `drm-copilot`
governance repository with no templating, so a fix applied here is overwritten by the next
push-down. No file under `.claude/` was changed.

### Acceptance criteria

All 29 criteria in this feature's `spec.md` are met and checked off. AC1 through AC10 cover R1,
AC11 covers R2, AC12 through AC16 cover R3, AC17 and AC18 cover R4, AC19 through AC21 cover R5,
AC22 through AC25 are the toolchain gates and AC26 through AC29 are the footprint and work-mode
gates.

### Toolchain

Format, analyzers, nullable, tests, in that order, with one clean pass and no restart.

- `dotnet tool run csharpier format .` — exit 0, `Formatted 1622 files`, with identical porcelain
  path sets and identical anchored diffstats on both sides of the invocation, so it rewrote nothing.
- `dotnet tool run csharpier check .` — exit 0, `Checked 1622 files`.
- `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  — exit 0, 0 warnings, 0 errors, 18 projects compiled, 0 skipped `CoreCompile` targets.
- `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  — exit 0, 0 warnings, 0 errors, 18 projects compiled, 0 skipped `CoreCompile` targets.
- `vstest.console.exe UtilitiesCS.Test QuickFiler.Test /EnableCodeCoverage` — exit 0, 6286 total,
  6286 passed, 0 failed.

### Coverage

Measured with `dotnet-coverage collect --output-format cobertura` over the nine first-party test
assemblies, aggregating the nine first-party packages, identically on both sides.

- Line coverage: 84.67 percent baseline, 84.68 percent post-change, delta +0.01.
- Branch coverage: 79.45 percent baseline, 79.46 percent post-change, delta +0.01.
- Changed-line coverage: 100 percent over nine measurable added production lines, none uncovered.
- Test totals: 7185 baseline, 7187 post-change, 0 failed on both sides. The difference is exactly
  the two added tests.

The two policy sources state different line floors and both are reported rather than reconciled: the
75 percent branch floor and the 80 percent `CLAUDE.md` line floor are met; the 85 percent line floor
stated by `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` is not met at
84.68 percent, a pre-existing repository-wide condition that this change moved toward rather than
away from. No threshold was lowered, weakened or deleted, and no production file was added to any
coverage exclusion list.

### Reported-only observations, recorded so they are not lost at merge

Three sibling doc comments in `QuickFiler/Viewers/` carry the same class of stale line-count figure
that R4 corrects. They are outside R4's named scope and none was changed; a fence proves it.

1. `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs:11` claims 487 for
   `BreadcrumbBridgeCoordinator.cs`, which measures 437.
2. `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs:10` claims 481 for
   `BreadcrumbItemViewerLifecycleCoordinator.cs`, which measures 497. This is the one worth a later
   follow-up, because it understates a file that now sits three lines under the 500-line ceiling.
3. `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs:8` claims 477; the referenced file
   was not measured.

---

ENTRY-DISPOSITIONS: 6

1. R1 — in scope, code change. Delivered: production change plus two new tests plus four corrected
   prose sites plus one dated correction to the issue-812 living spec.
2. R2 — in scope, decision only. Delivered: decision recorded, no code change, fence proves the
   owning file untouched.
3. R3 — in scope, contract reconciliation. Delivered: production change plus one rewritten test plus
   two rewritten XML docs.
4. R4 — in scope, comment correction. Delivered: one token, `(480 lines)` to `(459 lines)`.
5. R5 — in scope, observation only. Delivered: seeded observation log plus one XML-doc pointer, no
   executable statement changed.
6. R6 — out of scope, deferred upstream to the `drm-copilot` governance repository. Nothing changed
   in this repository.

REPORT-ONLY-ITEMS: 3

1. `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs:11` — claims 487, measures 437.
2. `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs:10` — claims 481, measures
   497.
3. `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs:8` — claims 477, not measured.

Output Summary: Issue-update text mirrored locally for issue #823. Six entry dispositions and three
report-only observations enumerated. Not posted from this execution; `PostedAs: unknown`.
