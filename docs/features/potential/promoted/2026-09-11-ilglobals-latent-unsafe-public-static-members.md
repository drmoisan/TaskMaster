# ilglobals-latent-unsafe-public-static-members (Issue #863)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ilglobals-latent-unsafe-public-static-members/ (Issue #863)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #863
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/863
- Last Updated: 2026-09-11
## Summary

`ILGlobals` retains two unsynchronised public mutable static members that issue #824 identified but deliberately left out of scope. One is referenced only by a test assertion; the other is referenced nowhere at all. Issue #824 fixed the two sibling fields with the same shape, so these are the remaining untreated surface on the same type.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# production source)
- Command/flags used: `git grep -n 'ILGlobals\.<member>' main -- '*.cs'`
- Data source or fixture: `main` at 3cb9744228f93f9d909511ad9a2cc9aeed8a9340

## Steps to Reproduce

1. Open `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` on `main`.
2. Observe `public static Dictionary<int, object> Cache = new Dictionary<int, object>();` at line 113 and `public static Module[]? modules = null;` at line 131.
3. Compare them to `multiByteOpCodes` (line 122) and `singleByteOpCodes` (line 130), which issue #824 converted to `public static readonly` with controlled publication.

## Expected Behavior

A public static member on a type reached from multiple threads is either immutable and safely published, or removed if it is dead. After #824, all four static fields on `ILGlobals` should hold to that rule.

## Actual Behavior

Two of the four remain publicly mutable and unsynchronised:

| Member | Line on `main` | Declaration | References across all `*.cs` |
|---|---|---|---|
| `ILGlobals.Cache` | 113 | `public static Dictionary<int, object> Cache = new Dictionary<int, object>();` | 1, a test assertion at `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs:267` |
| `ILGlobals.modules` | 131 | `public static Module[]? modules = null;` | 0 |

Neither is written after its field initializer, so neither is a live race today. Both are dormant rather than safe: each has the same shape as the two fields #824 repaired, and `Cache` becomes a live race the moment any caller writes to it.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:

```
$ git grep -n 'ILGlobals\.Cache' main -- '*.cs'
main:UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs:267:            ILGlobals.Cache.Should().NotBeNull();

$ git grep -n 'ILGlobals\.modules' main -- '*.cs'
(no output)
```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: no current runtime defect, because neither member is written after initialization. The exposure is latent — an unsafe shape left on a public API surface that a future caller can turn into the exact race #824 was opened to fix.

## Suspected Cause / Notes

Both members predate the SDIL reader's current use. `spec.md` for issue #824 lists them under "Out of scope / non-goals — latent items identified by the research but not fixed here".

This entry discharges an explicitly owed filing. The #824 evidence artifact `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/other/follow-up-latent-statics.2026-09-09T16-21.md` states: "This artifact records them so the epic orchestration layer can promote them into a GitHub issue. Filing that issue is owned by the epic layer and is not a task in this plan." No such issue existed as of 2026-09-11; the gap was found during worktree-cleanup triage.

Two details from that artifact are worth carrying forward:

- Its re-derived line numbers (113 and 131) differ from the numbers in `spec.md` (112 and 119). The artifact's numbers are the correct ones and match `main` today; the shift came from #824's own edits above the declarations, not from any change to the declarations themselves.
- An unqualified search for `modules` returns local `Module[] modules` variables inside `GetRefferencedOperand` in `MethodBodyReader.cs` that are unrelated to this field. Qualify the search as `ILGlobals.modules` or the conclusion will be wrong.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` — the single `Cache` assertion at line 267 must be updated or removed alongside whichever disposition is chosen.
- [ ] Integration scenario to retest: n/a.
- [x] Manual verification notes: `modules` is entirely unreferenced and is a straight deletion. `Cache` is referenced only by the test that asserts it is non-null, so deleting it requires deleting that assertion too; the alternative is to give it the same `readonly` plus controlled-publication treatment the opcode tables received in #824. Prefer deletion for both unless a caller is planned, since a dead public mutable static is the shape the repository has now fixed twice on this type.

Edit `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` and `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs`.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
