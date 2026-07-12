---
name: lock-recursion-coverage-317
description: Issue #317 root-cause research — deleted ConcurrentObservableCollectionLockRecursionTests.cs is a restoration, not new authoring; git/shell tool absence in this agent session
metadata:
  type: project
---

Issue #317 (child of epic swordfish-removal, follows F5 #308) asks to re-express lock-recursion
regression coverage for `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection.ConcurrentObservableCollection<T>`
after F5's WI-4 deleted `ConcurrentObservableCollectionLockRecursionTests.cs`.

**Root cause confirmed (four independent already-committed sources, not just the orchestrator's
claim):** the deleted file was F2's (#307) own clean-base regression coverage — bound to the clean
namespace, not `Swordfish.NET.Collections` — deliberately re-expressed by F2 at plan task P4-T7
against the lock-free `ObservableCollection<T>`-based clean type. F5 knowingly deleted it anyway
(spec named it as one of "three direct-Swordfish test files" to remove per WI-4) and, per its own
AC-12 evidence (`f2-regression-coverage-confirmation.md`), correctly flagged the gap and raised #317
rather than authoring replacement coverage itself (F5 does not own F2's scope). This is
process-compliant sequencing, not a misclassification bug — see also
`.claude/agent-memory/atomic-executor/project_swordfish_f5_test_misclassification.md` which records
the same fact from a different agent's session. **Conclusion: #317's fix is a restoration
(`git show <deletion-commit>~1:<path>` + one csproj `<Compile Include>` line), not new test
authoring** — the current `ConcurrentObservableCollection<T>` API is unchanged from what the deleted
file targeted (verified: `Add`/`Count`/`CollectionChanged` all still plain-inherited, no lock is
used at all per the class's own XML doc comment).

**Namespace inconsistency found (uncovered independently of the orchestrator's brief):** literal
namespace `ConcurrentObservableCollection.Tests` already exists in three surviving Dictionary-side
test files (older, non-folder-mirroring convention). Both surviving Collection-side siblings in the
exact target folder use the newer convention `UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection`
instead. If the restored file's original namespace doesn't match its living siblings, normalize it
— no class-name collision either way, but CLAUDE.md §7 favors matching the existing local (sibling)
style.

**Tool-access limitation to record:** this research session (task-researcher persona) had no
Bash/git-execution tool available — only Read/Grep/Glob/Write/Edit/WebFetch. Could not literally run
`git show` to reproduce the deleted file's exact bytes despite the orchestrator's brief asking for
it. Worked around this by cross-referencing already-committed markdown evidence/plan/memory
artifacts from the F5 and F2 features instead, which is a reasonable substitute when git access is
unavailable but is not a substitute for literally re-reading the historical blob — flagged this
explicitly in the research artifact rather than asserting false certainty. Full research:
`docs/features/active/collection-lock-recursion-coverage-317/research/research.2026-07-11T21-15.md`.
