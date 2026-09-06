# pr-778-post-merge-review-residuals (User Story)

- **Issue:** #782
- **Owner:** drmoisan
- **Last Updated:** 2026-09-05
- **Status:** Draft
- **Version:** 1.0

## Story

As the repository maintainer who owns the code review backlog, I want the twenty-six actionable
residuals from the three-phase post-merge review of PR #778 delivered as one Refactor, so that the
review's output is fully discharged before the #584 feature folder is archived and I do not have to
carry an untracked backlog of small items in my head.

## Who Benefits

- **The maintainer.** One issue, one branch, one review cycle instead of twenty-six open threads.
- **The next agent or engineer touching `UtilitiesCS/Threading/UiThread.cs`.** The accessor's
  contract becomes self-describing: XML documentation, one shared message that names the correct
  entry point, and a comment stating why the accessor deliberately does not self-heal.
- **Anyone who later audits the #584 delivery.** Its audit and evidence artifacts become internally
  consistent, so a reader can trust the recorded commands, counts, and ordering claims without
  re-deriving them.

## Outcome

The review's residuals stop being latent. Specifically:

- Two latent defects introduced or left in place by PR #778 — a leaked, never-shut dispatcher on a
  pooled test worker thread and a torn double read of a non-volatile static — are closed.
- Reflection against the private `UiThread._dispatcher` static is consolidated from six independently
  written sites into one install scope with one failure mode, so a rename of that field fails loudly
  instead of degrading a guard to a silent no-op.
- The one test file over the repository's 500-line limit is split, removing disclosed policy debt
  rather than carrying it forward.
- Comments and assertion reasons describe the mechanism the code actually has today.

## Why One Consolidated Refactor

Twenty-six separate follow-ups would cost far more than the work itself. The findings are heavily
coupled: the message-text change touches the same lines as the shared-constant change and the XML
documentation; the reflection consolidation and the file split touch the same file, so their ordering
has to be decided once rather than negotiated across two branches; and eight of the findings are
corrections to audit artifacts that only make sense as one internally consistent edit. Each item
individually is too small to justify a branch, a plan, a review cycle, and a toolchain pass, which is
exactly why items of this size are normally lost. Consolidating them makes the fixed cost payable
once and gives one reviewable diff whose scope is bounded by an explicit finding-to-file mapping.

The consolidation is deliberately not unlimited. Two findings that require a production behavior
change or an edit to a push-down-owned tree are excluded and tracked separately, so the delivery
stays a Refactor with no new behavior beyond the exception message text.

## Done When

Observable, in this order:

1. `git diff` against the merge base lists exactly the files named in the specification's Write Set
   sections, and nothing under .claude/.
2. Every finding identifier in the specification's traceability table is either present in that diff
   or recorded as an omission with a stated reason in the delivery's code-review artifact.
3. The full C# toolchain — CSharpier format then check, analyzer build, nullable build, and the test
   run with coverage — passes in a single final pass, with one evidence artifact per step recording
   its exact command and exit code.
4. No test file in the touched set exceeds 500 lines, and every test that existed before the split is
   still discovered and passing under its original fully-qualified name.
5. Every `EXIT_CODE:` field in the #584 evidence tree is a single integer.
6. The C09 behavioral follow-up exists as its own promoted entry with a GitHub issue number, and the
   two push-down-owned items are recorded as upstream follow-ups for drm-copilot.

## Acceptance Criteria

- [ ] AC-U1: One branch and one pull request deliver all in-scope findings; the pull request body
      maps every finding identifier to the file that changed or to the recorded reason it did not.
- [x] AC-U2: The delivery introduces no production behavior change other than the text of the
      `InvalidOperationException` message and the retry-after-failed-initialization behavior of
      `UiThread.Init()`, both of which are stated in the specification's Behavioral Contract.
- [x] AC-U3: The #584 feature folder can be archived with no unrecorded residual: every review
      finding is resolved, promoted, recorded as an upstream follow-up, or recorded as needing no
      action.
- [x] AC-U4: A reader of the #584 audit artifacts can verify every command, count, and ordering claim
      they contain against the committed evidence without re-deriving it.
- [x] AC-U5: The full C# toolchain passes in a single final pass and changed-line coverage does not
      decrease.
