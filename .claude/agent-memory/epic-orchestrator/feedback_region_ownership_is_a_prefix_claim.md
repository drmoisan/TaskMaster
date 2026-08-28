---
name: region-ownership-is-a-prefix-claim
description: Deriving each child's shared-file region from the Compile Include entries its plan declares under-counts, because execution creates files the plan never enumerated; treat a region as a prefix any child may enter
metadata:
  type: feedback
---

When partitioning a shared, alphabetically ordered project file across concurrent children, do NOT
derive each child's region from the `<Compile Include>` entries its committed plan declares. Treat a
region as a **claim over an alphabetical prefix that any child may enter**, and tell every child the
full prefix map plus "if execution makes you need an entry outside your prefix, stop and report."

**Why:** On the quickfiler-bug-family epic I derived regions from plan text and recorded feature 493
as owning *none*, because its plan declared no includes. During execution 493 created two new test
files and added `Controllers\QfcItemController.UiThreadDispatcherFixture.cs` and
`...FixtureTests.cs` to `QuickFiler.Test.csproj` — squarely inside `Controllers\Qfc*`, which I had
assigned to feature 444, live at the time. It merged before I noticed. Harm was bounded (adjacent
hunks, so a recoverable conflict for 444's remediation loop, not silent corruption), but the NFR was
breached and I could not warn 444 mid-run because there is no `SendMessage` tool
([[no-sendmessage-tool-resume-child-in-place]]).

The child disclosed the deviation but justified it as "about 30 lines clear of every sibling region."
**Line distance is not the criterion; the alphabetical prefix is.** Reject that reasoning if offered.

**How to apply — and re-test your own measurements.** My first collision check reported *zero*
collisions and was wrong: I passed `Controllers\\Qfc` to `grep -E`, where `\Q` is not a literal
backslash, so it searched for `ControllersQfc`. Use `grep -F` for Windows paths. That was my second
pattern-matching false negative in one session — the first had me query `/c/Users/...` against git's
`C:/Users/...` output and conclude five worktrees were unregistered. **A negative result from a
hand-written pattern over Windows paths deserves one confirming test before you report it**, because
a false negative here reads as "all clear" and silently ends the investigation.
