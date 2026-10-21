# QA Gate — CSharpier Format (P2-T1)

Timestamp: 2026-09-01T12-52

This task ran twice. Pass 1 rewrote a tracked file, which under the Phase 2 rule ("if P2-T1
rewrites any tracked file, restart the loop from P2-T1") required the loop to restart. Pass
2 reached a fixpoint. Both passes are recorded below.

---

## Pass 1

### Pre-format snapshot

Command: `git status --porcelain`
EXIT_CODE: 0
Output: *(empty — working tree clean)*

The Phase 1 edits to the two owned files were already committed at `2b633230`, so the
pre-format tree was clean. This makes the comparison sharper than a dirty-tree snapshot
would: any path the formatter rewrites appears immediately, with nothing to disentangle it
from.

### Format

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
Verbatim output: `Formatted 1566 files in 6244ms.`

### Post-format snapshot

Command: `git status --porcelain`
EXIT_CODE: 0
Output:

```
 M QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs
```

Command: `git diff --stat HEAD`
Output: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | 4 +---`
(1 insertion, 3 deletions)

### Difference from the pre-format snapshot

**Yes — the formatter rewrote one tracked file.** One newly-modified path appeared that was
not present before the format run. The rewrite:

```
-            invoked
-                .Should()
-                .BeFalse("an empty filtered array must not reach the writer at all");
+            invoked.Should().BeFalse("an empty filtered array must not reach the writer at all");
```

CSharpier collapsed the chained assertion in the new test onto one line. The test was
written by modelling `WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter`, whose
equivalent assertion CSharpier keeps broken across three lines. The two differ only in the
length of the `because` reason string: the template's reason is long enough to push the
chain past the print width, and this one's is not, so the same formatter produces a
different shape for the same construct. Formatter output wins over the hand-written form
(`CLAUDE.md` C#1.1: "Do not hand-format; if a diff disagrees with `csharpier`, formatter
output wins"), and the collapsed line was kept.

No file outside `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` was touched.
In particular the production file `QuickFiler/Controllers/QfcHomeController.Metrics.cs` was
not rewritten, so the four-line guard as written in P1-T5 is already formatter-canonical.

**Loop restarted from P2-T1.**

---

## Pass 2

### Pre-format snapshot

Command: `git status --porcelain`
EXIT_CODE: 0
Output:

```
 M QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs
```

Command: `git diff --stat HEAD`
Output: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | 4 +---`
(1 insertion, 3 deletions — the uncommitted pass-1 rewrite)

### Format

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
Verbatim output: `Formatted 1566 files in 2151ms.`

### Post-format snapshot

Command: `git status --porcelain`
EXIT_CODE: 0
Output:

```
 M QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs
```

Command: `git diff --stat HEAD`
Output: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | 4 +---`
(1 insertion, 3 deletions)

### Difference from the pre-format snapshot

**No difference.** The modified-path set is identical before and after, and the changed-line
count against `HEAD` is identical before and after (1 insertion, 3 deletions in both
snapshots). No newly-modified path appeared and no changed-line count grew. The single
modified path carried into this pass is pass 1's rewrite, not a pass-2 rewrite.

The tree was therefore already formatter-compliant when pass 2 began. This is the fixpoint
the restart rule exists to establish.

## Acceptance

| Condition | Observed | Met |
|---|---|---|
| `EXIT_CODE 0` recorded | Pass 1: 0. Pass 2: 0. | Yes |
| Task records whether the second `git status --porcelain` shows any additional changed-line count for the owned files, or any newly-modified path, beyond the pre-format snapshot | Pass 1: yes, it did (one newly-modified path, detailed above). Pass 2: no, it did not. | Yes |

ACCEPTANCE: MET on pass 2, with pass 1 recorded as the rewrite that triggered the restart.

## Output Summary

CSharpier 1.2.6 processed 1566 files on each pass, exit 0 both times. Pass 1 collapsed one
chained FluentAssertions call in the new regression test onto a single line, which triggered
the mandated loop restart. Pass 2 changed nothing, confirming the tree is at the formatter's
fixpoint. The test file is 477 lines after formatting (down from 479 before it), still
within the 500-line cap verified in P1-T15; the two-line reduction is the collapsed
assertion and does not affect that task's acceptance.
