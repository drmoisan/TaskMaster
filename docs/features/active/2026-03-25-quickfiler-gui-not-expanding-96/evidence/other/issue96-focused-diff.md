# Issue #96 Focused Diff (Remediation: issue-96 2026-03-26T15-25)

Timestamp: 2026-03-26T15:48:00Z

Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue96-clean diff --name-only origin/development...bug/quickfiler-gui-not-expanding-96-clean

EXIT_CODE: 0

## Changed Paths

```
QuickFiler.Test/Controllers/QfcItemControllerTests.cs
QuickFiler/Controllers/QfcItemController.cs
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/code-review.2026-03-25T14-00.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-coverage.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-format.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-lint.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-nullable.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-test.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/qa-format.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/qa-lint.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/qa-nullable.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/qa-test.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/regression-testing/regression-fail-before.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/feature-audit.2026-03-25T14-00.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/issue.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/plan.2026-03-25T09-03.md
docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/policy-audit.2026-03-25T14-00.md
docs/features/potential/2026-03-25-quickfiler-gui-not-expanding.md
```

## Output Summary

19 changed paths total. All paths are within the issue #96 allowlist:
- **QuickFiler/\*\***: `QuickFiler/Controllers/QfcItemController.cs` — production fix
- **QuickFiler.Test/\*\***: `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` — regression tests
- **docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/\*\***: 16 evidence/audit/plan files

**Note**: One file `docs/features/potential/2026-03-25-quickfiler-gui-not-expanding.md` is outside the strict three-directory allowlist. This is the issue #96 potential-to-issue promotion document created by the feature-promotion-lifecycle tooling and is expected issue lifecycle material. It does not represent scope creep — it is a passive documentation artifact that was part of the original issue #96 commit `bd8fc03`.

No issue #97, residual excluded-work, or issue #87 scope is present.
