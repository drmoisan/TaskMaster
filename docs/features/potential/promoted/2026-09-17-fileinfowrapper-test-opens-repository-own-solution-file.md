# Bug: FileInfoWrapper_Tests.OpenRead opens the repository's own TaskMaster.sln (Issue #906)

- Work Mode: full-bug
- Reported: 2026-09-17
- Source: run `bugs-2026-09-17`, found during item 900's delivery (PR #904)

- Issue: #906
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/906
- Last Updated: 2026-09-17
- Status: Promoted -> docs/features/active/Bug_FileInfoWrapper_TestsOpenRead_opens_the_repositorys_own_TaskMastersln/ (Issue #906)
## Summary

`FileInfoWrapper_Tests.OpenRead_...` opens **`TaskMaster.sln` — the repository's own solution file** —
as its test fixture.

That file is not inert. Resident MSBuild node-reuse worker processes hold solution and project files
open between builds, so the test's outcome depends on whether a build ran recently, whether node
reuse is enabled, and how long those workers linger. None of that is under the test's control.

## Why this is the same class as #900

The test asserts a property about file access while depending on ambient process state it never
establishes. Like the `Task.Run` thread-affinity assumption, it will usually pass, and when it fails
it will look like flakiness rather than a defect in the test's own setup.

It also violates the repository's determinism requirements directly: tests must not depend on mutable
external state that can change between runs, and must not rely on the environment happening to be in
a particular condition.

## Additional concern

Using a real, large, repository-owned file as a fixture couples the test to that file's continued
existence, location and size. A solution restructure would break a test that has nothing to do with
solution structure.

## Proposed fix / validation ideas

1. Use a fixture the test controls rather than a repository artifact. **Note:** the repository
   prohibits creating temporary files in tests, so an in-memory stream or an injectable seam is the
   right shape here — not a scratch file. The `FileInfoWrapper` seam pattern used elsewhere in
   `UtilitiesCS` is the precedent to follow.
2. If the test genuinely needs a real file on disk, it must own that file's lifecycle explicitly and
   must not select one that another process is expected to hold open.

**Acceptance must be demonstrated, not argued:** per the lesson recorded on #895, any criterion
adopted must be observed FAILING before it is accepted. Reproducing this one may require a warm
MSBuild node-reuse worker holding the solution open — state that reproduction condition explicitly
rather than asserting the test is safe once changed.

## Related

- **#900** — same class: a test depending on ambient state it does not control.
- **#905** — sibling breadcrumb tests carrying the `Task.Run` distinct-thread assumption.
- Local vstest on this machine already needs `/InIsolation` and a TestCaseFilter excluding four
  shell-icon classes that hang; this is another instance of environment-coupled test behaviour.

## Next step

Triage and schedule.
