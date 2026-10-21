# Remediation Inputs — cycle 1

Entry timestamp: 2026-08-26T21-00
Issue: #614
Branch: `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
Branch head at entry: `0661c9fe`
Source review: `code-review.2026-08-26T16-55.md`, `feature-audit.2026-08-26T16-55.md`,
`policy-audit.2026-08-26T16-55.md`

## Orchestrator disposition override

The feature review returned **GO — PASS with 0 blocking findings**, classifying CR-1 and CR-2 as
**Major but non-blocking**. The orchestrator does not accept that disposition for delivery and
promotes both to **blocking** for this cycle.

Reasoning. Issue #614 is a filing-correctness defect. Its scope, as set by the reporting user, is
"every confirmed defect on the path-representation chain that contributes to this crash or to a
silently-wrong filing destination on the same chain." CR-1 and CR-2 are defects this change
*introduces* on exactly that chain, and their effect is stronger than a wrong destination: they make
a correct destination unreachable. Shipping a fix for "the store root leaks into the filing
boundary" that simultaneously breaks "file into the HR folder" is not an end-to-end solve of the
reported problem. Both were verified independently by the orchestrator against the pre-change tree
before this cycle was opened; neither is a matter of interpretation.

The review's own severity assignment (Major) is not disputed. Only the blocking/non-blocking
disposition is overridden, and only for these two findings. CR-3, CR-4 and the Minor findings retain
the reviewer's non-blocking disposition and are NOT in scope for this cycle.

## Blocking finding 1 — CR-1: filing to any folder named with fewer than three characters now fails

**Regression introduced by this change.** Verified against merge-base `c279d40b`.

Pre-change, `EfcFormController.ActionOkAsync` guarded only:

```csharp
if (selectedFolder is null || selectedFolder.StartsWith("===="))
```

There was no length rule on the filing path. A separate property, `IsValidSelection`, carried
`selectedFolder.Length < 3`, and that property gated **folder creation**, not filing.

Post-change, both call sites delegate to `EfcSelectionGuard.IsValidFilingSelection`, which includes:

```csharp
return value.Length >= 3
```

Consolidating the two guards silently applied a folder-creation rule to the filing path. Filing to an
archive subfolder named `HR`, `IT`, `PR`, `QA`, `Q1`, `AP`, `AR` or any other one- or two-character
name now fails with the dialog "Please select a valid folder." That worked before this change.

Two-character folder names are ordinary in a filing hierarchy, so the blast radius is real rather
than theoretical.

Spec AC16 requires the OK path to reject an empty selection and a full Outlook path. It does not ask
for a minimum length, so the length rule is not traceable to any acceptance criterion.

Required outcome: the filing guard must not reject a selection solely because it is short. Whatever
minimum-length behaviour the folder-creation path legitimately needs must remain confined to the
folder-creation path. The two call sites may still share a predicate, but the shared predicate must
express only rules that are true of both, or the paths must take differently-scoped predicates.

## Blocking finding 2 — CR-2: the router and the filing guard disagree about rooted targets

**Internal inconsistency introduced by this change.**

`BreadcrumbBridgeRouter.SelectRow` was deliberately scope-pinned during plan delta E1 so that a
rooted filing target that is at or under the bound archive root passes through verbatim. That
behaviour is required by the untouched test
`Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch`, which asserts a rooted
under-root value survives selection.

`EfcSelectionGuard.IsValidFilingSelection` rejects **every** value for which
`ArchiveStemContract.IsFullOutlookPath` is true, and that predicate is true for any single-backslash
leading value — including a rooted target that is legitimately under the archive root.

The result: that class of value is selectable in the breadcrumb surface but unfilable at the OK
button. One guard admits it, the other refuses it.

Required outcome: the two guards must agree. The filing guard should reject a rooted value only when
it is genuinely not resolvable against the archive root — the same `TryMakeArchiveRelative`-based
test the router already applies — rather than rejecting rootedness as such. This mirrors the
scope-pinning already applied to the router in P3-T2 and keeps the D1/D4/D9 protection intact: a
store-root or cross-store value still fails, because it fails `TryMakeArchiveRelative`.

## Explicitly out of scope for this cycle

- CR-3 (the AC11/D5f fix repairs code with no production entry point). Real observation, no user
  impact, and reverting it would be worse than keeping it. Reviewer disposition retained.
- CR-4 (`ArchiveRootPathGuard` and `LoadFolders` now throw). Intended by AC13/AC14; the spec chose
  fail-fast over silent misfiling deliberately.
- All Minor findings, including the dead `Func<string,string>` seam, `SortEmail.ResolvePaths` not
  being migrated, and the validator rule-set asymmetry.
- The 84.8696% repo-wide coverage FAIL. Pre-existing, improved by this change, and below no
  remediation trigger.
- AC26 manual validation NOT EXECUTED. Live-Outlook steps cannot run headless; each has a passing
  headless counterpart.

## Constraints carried into this cycle

- Do not weaken D1, D4 or D9. A store-root, cross-store, or above-archive value must still be
  rejected at both the router and the filing boundary. The regression fix must narrow the guard's
  over-rejection, not its under-rejection.
- Do not regress issue #609 or the #439 scenarios. `Issue439ArchiveRootBoundarySelectionAndHostEvent
  RemainDeterministic` remains in its P3-T4-corrected form.
- Do not absorb or regress open issue #499.
- Redaction per issue #602: fabricated placeholders only.
- Every new or changed behaviour needs a test that fails before the fix and passes after.
- File-size gates remain: `EfcFormController.cs` <= 1084, `BreadcrumbBridgeRouter.cs` <= 596,
  `BreadcrumbBridgeRouterIssue439Tests.cs` <= 694.
