Timestamp: 2026-09-09T10-24
Baseline: 85.5936% line / 79.7998% branch (source: <FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml,
line-rate="0.855936" branch-rate="0.797998", lines-covered=55950, lines-valid=65367; measured at
P0-T8, this branch/worktree, not the plan-wide-conventions figure from the prior sibling audit)
PostChange: 85.6063% line / 79.7939% branch (source: <FEATURE>/evidence/qa-gates/coverage-post-change.cobertura.xml,
line-rate="0.856063" branch-rate="0.797939", lines-covered=55966, lines-valid=65376,
branches-covered=13474, branches-valid=16886)

Delta: line +0.0127 percentage points (improvement, not a regression); branch -0.0059 percentage
points (a change of 0.0059 points, far inside the 0.5-point no-further-regression allowance, and
still far above the 75% floor).

NewCodeCoverage: every executable line inside the try/catch (InvalidOperationException) block added
in Phase 3 (QuickFiler/Controllers/QfcItemController.FolderHandling.cs, lines 231-245 in the current
file: 231 declaration with no initializer emits no separate sequence point; 232-237 the try block;
238-245 the catch (InvalidOperationException) clause) shows hits > 0 in the post-change Cobertura
XML, class node filename="QuickFiler\Controllers\QfcItemController.FolderHandling.cs" (backslash
form, per the Koverage post-processing rewrite), method "AssignFolderComboBox":
  line 233 hits=1 (try block open brace sequence point)
  line 234 hits=1, branch condition-coverage 100% (6/6) (ternary: _globals is null ? null : ...)
  line 235 hits=1
  line 236 hits=1 (?? string.Empty)
  line 237 hits=1 (try block close brace)
  line 238 hits=1 (catch (InvalidOperationException) clause line)
  line 239 hits=1 (catch block open brace)
  line 244 hits=1 (archiveRootPath = string.Empty; -- the catch handler body, exercised by the new
    #813 regression test's Ol.ArchiveRootPath-throws setup)
  line 245 hits=1 (catch block close brace)
9/9 emitted sequence-point lines hit (100% new-code coverage), exceeding the 90% new-code floor.

Acceptance evaluation:
- Post-change repo-wide line coverage 85.6063% >= 85% floor: PASS.
- Post-change line coverage is 0.0127 points ABOVE baseline (not a regression, well inside the
  0.5-point allowance): PASS.
- Post-change repo-wide branch coverage is 0.0059 points below baseline (well inside the 0.5-point
  no-further-regression allowance; per the plan-wide "Coverage floor resolution" note, the
  pre-existing repo-wide branch-coverage shortfall against the 75% floor cited there is a
  dispositioned, unrelated gap this narrow fix is not responsible for closing -- and this
  worktree's actual measured branch-rate, 79.7939%/79.7998%, is in fact already above the 75%
  floor): PASS.
- Every line inside the added try/catch (InvalidOperationException) block shows hits > 0 (100% new-
  code coverage): PASS.
