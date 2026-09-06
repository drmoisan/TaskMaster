# Code Review — Issue #782 (pr-778-post-merge-review-residuals)

- **Date:** 2026-09-06
- **Reviewer:** feature-review agent (re-audit, cycle 2)
- **Base:** `main` -> `origin/main` @ `77c6d31404e2bc2291aec7eb9561e393c20cdcae`
- **Head:** `refactor/pr-778-post-merge-review-residuals-782` @ `e053a4f2305502adb09afe6bcc9a26351804f6fe`
- **Scope:** the full branch diff, 126 paths, of which 15 `.cs`, 1 `.csproj`, and 110 `.md`
- **Companion artifacts:** `policy-audit.2026-09-06T02-18.md`, `feature-audit.2026-09-06T02-18.md`, `remediation-inputs.2026-09-06T02-18.md`

## Executive Summary

The delivery is sound and the remediation improved it. Zero blocking findings. Three new non-blocking
accuracy nits are raised (N1, N2, N3), one of which corrects this reviewer's own cycle-1 record.

The remediation's central problem was hard, and the delivery got it right. R3 asked for an assertion
that genuinely pins the message. The naive fix — replace a wildcard with a constant reference — is
close to tautological, because an assertion written against the same constant the production code
throws moves with that constant and pins nothing about its text. The delivery states that limitation
explicitly rather than claiming more than the change buys, identifies the one test that does hold the
literal (`WpfDispatcherYieldTests.cs:196`, `Message.Should().Contain("UiThread.Init()")`), and then
proves the property it does claim by observation rather than derivation: with the removed tail
appended at the `WpfDispatcherYield` throw site, `YieldAsync_WithoutDispatcher_RemainsStrict` fails
and its sibling passes. This reviewer read the TRX at `TestResults/782-r1-p1t7` directly and confirms
`outcome="Failed"`, 2 total, 1 passed, 1 failed. That is the standard of proof this reviewer asks for
and rarely receives.

### Verified correct — re-derived at the new head, not carried forward from cycle 1

- **Toolchain.** This reviewer re-ran `dotnet tool run csharpier check .` (`Checked 1583 files`, exit
  0), the analyzer `msbuild /t:Rebuild` (`0 Warning(s) 0 Error(s)`, exit 0), and the nullable
  `msbuild /t:Rebuild` (exit 0, 19 projects recompiled). `/t:Rebuild` means neither gate was vacuous.
- **Coverage held exactly.** Aggregating `782-r1-baseline.cobertura.xml` and
  `782-r1-final.cobertura.xml` independently returns identical counters on both sides:
  112351/132961 lines and 26498/33480 branches under the delivery's selection, 55683/65896 and
  13249/16740 under a class-level selection that does not double-count.
- **R4 is confirmed by measurement.** The amended `evidence/baseline/p0-t7-coverage.md` claims the
  retained document aggregates to 112359 and 26496. It does, exactly. The amendment's discriminating
  observation — `Total tests: 6992` in the companion log versus 6997 in `p0-t6-vstest.md` — is the
  right kind of evidence, because it is a value the run itself wrote rather than mutable filesystem
  metadata. The amendment also declines to assert a mechanism for the missing document, which is the
  correct posture when no record supports one.
- **The reflection consolidation actually consolidated.** A grep for the single-line token
  `"_dispatcher"` across every `*.cs` in the repository returns exactly two hits:
  `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:117` and the unchanged
  `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136`. Six sites became
  one per assembly, and the cross-assembly split is forced, not accidental: `UtilitiesCS` grants
  `InternalsVisibleTo("UtilitiesCS.Test")` but not to `QuickFiler.Test`.
- **The split preserved every name.** Extracting method names from the pre-split file and from the
  union of the two post-split files and comparing the sets: zero names lost, one name added (the C26
  synchronous sibling). Both parts declare `public partial class ProgressTracker_Tests` in namespace
  `UtilitiesCS.Test`, so every fully-qualified name is preserved. `[TestClass]` and
  `[DoNotParallelize]` sit on separate lines in exactly one part. Each file has exactly one
  `<Compile Include>` entry.
- **The seam's documented invariant holds.** `UiThreadDispatcherScope` declares in its `<remarks>`
  that it is deliberately not synchronized and that serialization is supplied by `[DoNotParallelize]`
  on every installing class. This reviewer enumerated all five files that call `Install` or
  `InstallNull` and confirmed each carries the attribute, including the new partial part, which
  inherits it from the other part of the same type. A documented invariant that is actually true is
  worth more than a lock.
- **`RibbonViewer` dead-guard removal is behavior-preserving.** `var dispatcher = UiThread.Dispatcher;`
  throws when the static is unset — on `origin/main` as well as at head — so `dispatcher != null` was
  already unreachable-false before the branch. Removing it changes nothing. This matters because the
  file is invisible to coverage, so inspection is the only available check.
- **`ProgressTracker` / `ProgressTrackerAsync` C02 fix is a genuine no-op on value.** Line 33 assigns
  `UiDispatcher = UiThread.Dispatcher`; line 39 previously re-read the static and now reads the
  already-captured property. Same value, one fewer opportunity for a differing read.

## Findings Table

| ID | Severity | Blocking | File | Summary |
|---|---|---|---|---|
| N1 | Nit | No | `plan.2026-09-05T15-47.md:42`, `research/research.2026-09-05T16-10.md:6` | Absolute host path including the account name is embedded in two committed artifacts. Corrects this reviewer's cycle-1 policy-audit row 2.11, which recorded PASS. |
| N2 | Nit | No | `evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md` | Titled a maintainer disposition; no maintainer ratification record exists in any committed artifact or in the orchestrator state. |
| N3 | Nit | No | `user-story.md` AC-U2 | Names the withdrawn C03 retry behavior as a delivered production behavior change. |
| N4 | Informational | No | `evidence/baseline/p0-t7-coverage.md`, SD22 selection | The pinned `.//line` aggregation double-counts method-level rows. Impact 0.0021 points. |
| N5 | Informational | No | `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | Changed lines are unmeasurable under a pre-existing type-level `[ExcludeFromCodeCoverage]`. |
| N6 | Informational | No | `artifacts/pr_context.summary.txt` | `Core logic changes: 0 files` against 16 changed code files. Generator defect. |
| N7 | Informational | No | `artifacts/pr_context.summary.txt` | `Close candidates` author-asserted list is 22 entries scraped from prose. |
| N8 | Informational | No | `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` | `#nullable enable annotations` rather than `#nullable enable`; no `CS86xx` flow analysis over the file. |
| R1 | Procedural | No | `artifacts/csharp/` | Canonical C# coverage artifact absent. Recurs from cycle 1; dispositioned. |
| R2 | Should-fix | No | `UtilitiesCS/Threading/UiThread.cs` | Modified-file line coverage 76.83%. Recurs from cycle 1; waived on the identical-uncovered-set evidence. |

## N1 — absolute host path in two committed artifacts, and a correction to cycle 1

Two committed artifacts embed the reviewer's own workstation path and account name:

```text
plan.2026-09-05T15-47.md:42
**Worktree root.** All other paths are relative to `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-05T10-47`.

research/research.2026-09-05T16-10.md:6
- Research root (worktree): `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-05T10-47`
```

Cycle 1's policy audit recorded row 2.11 as PASS with the evidence "Evidence artifacts substitute
`<worktree>` for host paths and explicitly decline to reproduce vstest-generated TRX filenames." That
evidence sentence is true — the substitution is complete across all 90 changed files under
`evidence/` — but the criterion is stated over artifacts generally, and the plan and research
documents are artifacts of this delivery. This cycle records the row as FAIL and states the correction
explicitly rather than silently re-scoping the criterion to match the evidence.

**Why it is nonetheless not blocking.** The prohibition is a reviewer convention, not repository
policy: no file under `.claude/rules/` and no section of `CLAUDE.md` states it. Its factual footing is
also weak — `git grep -l` over `docs/**` at the base commit returns **827 committed documents** already
carrying the same path. Two more occurrences change nothing about the repository's exposure. Raising
this to blocking would apply a standard to this branch that `origin/main` does not meet.

Recommendation: substitute `<worktree-root>` in the two lines if the delivery wants internal
consistency with its own evidence tree. Do not gate the pull request on it.

## N2 — the disposition record asserts an authority no record supports

`evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md` is titled "Maintainer disposition of
findings R1 and R2". Its body is accurate and, notably, reproduces in full this reviewer's cycle-1
qualification that "would force a FAIL verdict" is not a legitimate reason to omit an artifact —
recorded rather than paraphrased away, which is the right call.

The title, however, asserts that a maintainer made the decision. This reviewer looked for the record
and did not find it:

- `artifacts/orchestration/orchestrator-state.json` carries a `remediation_disposition` object with
  `decided_at: "2026-09-06T00:20:00Z"` and no actor field. Its `rationale_for_fixing_two_non_blocking_findings`
  value is written in the orchestrator's voice.
- The same file's `human_interaction` key is `null`.
- The document's own body attributes itself correctly: "It is written by task [P3-T7] of the
  remediation plan."

So the recorded decider is the orchestrator, not the maintainer. It is entirely possible a human
ratified this in session and it simply was not logged; this finding states what the artifacts
support, not what did or did not happen. The concern is specific rather than procedural: `CLAUDE.md`
UT2 requires maintainer ratification for coverage exemptions, and R2 is adjacent enough to that class
that a title implying maintainer authority could later be cited as the ratification it is not.

Recommendation: retitle to name the actual decider, or add a one-line maintainer ratification record.
Either resolves it.

## N3 — AC-U2 names a behavior change that was withdrawn

`user-story.md` AC-U2, checked `[x]`:

> The delivery introduces no production behavior change other than the text of the
> `InvalidOperationException` message and the retry-after-failed-initialization behavior of
> `UiThread.Init()`, both of which are stated in the specification's Behavioral Contract.

C03 — the latch re-arm that would produce that retry behavior — was withdrawn at commit `92c43665`
after a measured regression, and `UiThread.Init()` is byte-identical to its `pre-782-base` form. The
`spec.md` Behavioral Contract handles this correctly and at length: it states the method is unchanged,
gives the bisected regression, names the mechanism (the two lazy accessors retrying WinForms
construction and starving the thread pool against a 500 ms `CancelAfter`), and records the promotion
to #788. `spec.md` AC2 likewise routes C03 through its omission branch explicitly.

AC-U2 alone was not updated. As a proposition it still holds — "no change other than A and B" is
satisfied by delivering only A — and the clause "both of which are stated in the specification's
Behavioral Contract" is literally true, since B is stated there as withdrawn. So this is staleness
rather than falsehood, and a reader who follows the pointer finds the full explanation. That is why it
is a nit and not a Should-fix.

Recommendation: reword AC-U2 to name only the message text, or add "the latter withdrawn under SD18".

## Design and Architecture Notes

**The seam design is the right shape.** `UiThreadDispatcherScope` is 126 lines, `internal sealed`,
`IDisposable`, with a private constructor and three static entry points. The `Dispose` contract is
documented with the reasoning that matters: the captured prior is written back unconditionally and is
never tested for null first, because a null prior is a real state and skipping the write for it would
leak an installed dispatcher into every later test. That is precisely the bug the six ad-hoc sites
were prone to.

**The failure mode is the improvement, not the deduplication.** The old sites used
`DispatcherField?.GetValue(null)` with a null-conditional, so a rename of `_dispatcher` would have
turned an order-independence guard into a silent no-op that still passed. The new resolution asserts
non-null in the static initializer, so a rename raises `TypeInitializationException` on first use and
fails every consuming test. `EmailMoveMonitorTests` had exactly the null-conditional shape and now
reads through `UiThreadDispatcherFixture.Current`. This is a real robustness gain, not a cosmetic one.

**Two seams, two synchronization disciplines.** `UiThreadDispatcherScope` (UtilitiesCS.Test) is
explicitly unsynchronized and relies on `[DoNotParallelize]`; `UiThreadDispatcherFixture`
(QuickFiler.Test) takes a `FieldLock` on every read and write. Both write the same process-global
static. Within each assembly the discipline is sound and the invariant was verified to hold. Whether
the two assemblies can ever share a test host process, and therefore race across the seam boundary,
was not established by this review and is not asserted either way. It is pre-existing in structure —
the QuickFiler fixture is unchanged by this branch except for its new consumer — and is noted only so
a future reader does not assume the two seams are interchangeable.

**The C21 test's cross-thread read is correctly synchronized.** `observed` is written on a worker and
read on the test thread after `worker.Join()`. `Thread.Join` supplies the happens-before edge, so no
`volatile` or memory barrier is needed. The test uses no sleep and no polling.

**The `WpfDispatcherYield` comment correction is the most valuable prose change on the branch.** The
old comment asserted "UiThread.Dispatcher is set-once state populated by UiThread.Init() and is null
outside a live host", which PR #778 made false. The replacement states that the production fallback
provider throws directly and that the local guard is therefore unreachable on the production path,
covering only injected providers typed `Func<Dispatcher?>` that exist only in tests. A reader can now
tell why the guard is there without reconstructing the history.

## Policy Compliance Notes

- **File size.** Every changed `.cs` file is under 500 lines; the maximum is 397. The branch removes a
  pre-existing violation: `ProgressTracker_Tests.cs` was 514 and is now 271 plus a 288-line sibling.
- **Determinism.** No `Thread.Sleep`, `Task.Delay`, or wall-clock wait is added. The two pre-existing
  `DateTime.Now` call sites are unchanged in count from the base commit.
- **Temp files.** None. The only added line containing the substring `Temp` is the word "Temporarily"
  in an XML-doc summary.
- **Coverage exclusions.** `coverage.config` excludes only third-party module paths. The derived run
  configuration appends `.*\.Test\.dll$`, which excludes test assemblies as the policy requires. No
  `exclude` entry matches a production source path, so the Blocking condition in
  `.claude/rules/general-unit-test.md` does not arise.
- **Evidence locations.** All 90 changed evidence files are under `<FEATURE>/evidence/<kind>/` across
  six canonical kinds. Zero paths under any forbidden `artifacts/` sub-path.
- **`.claude/**` untouched.** `git diff --name-only 77c6d314..HEAD -- .claude` returns zero paths. The
  delivery's own gate at `evidence/qa-gates/r-p5-t1-dotclaude-untouched.md` is independently confirmed.
- **Line endings.** `git ls-files --eol` reports `i/lf` for every committed file under the feature
  folder; `.gitattributes` sets `* text=auto`. The executor's observation that three edited markdown
  files are LF-only describes working-tree state only. Committed content is uniformly normalized, so
  there is nothing to fix.
- **`REMEDIATION-BASE-SHA` occurrence count.** Confirmed: one occurrence at line start (line 23), two
  under a containment reading (line 27 is a backticked mention naming the consumer). The executor's
  line-start convention is the correct one and matches how the plan defines its other field keys.
  Recording both measurements rather than picking one silently was the right disclosure.

## Recommendation

**GO for pull request.** Zero blocking findings, zero code defects requiring a fix before merge.

The three new nits (N1, N2, N3) are all in documentation, all one-line edits, and all optional. If the
delivery elects to fix them — and the same reasoning that justified fixing R3 and R4 applies, since
this delivery exists to remove accuracy defects from audit artifacts — they can be handled in a single
commit without a toolchain pass, because none touches a `.cs` file.

Two constraints on the pull request body:

1. Close **#782 only**. #787 and #788 are follow-ups that must stay open, and the PR context
   `Close candidates` author-asserted list is unusable: 22 entries scraped from prose, including
   `#ISO-8601`, `#S2-1`, `#S3-1` through `#S4-2`, and eight unrelated real issues.
2. Do not restate `Core logic changes: 0 files` from the PR context summary. Fifteen `.cs` files and
   one `.csproj` file changed. Take the changed-file set from `git diff`, not from that section.
