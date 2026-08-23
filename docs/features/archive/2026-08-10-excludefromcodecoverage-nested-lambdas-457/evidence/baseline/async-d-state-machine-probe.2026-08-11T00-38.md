# [P0-T12] Async `d__` state-machine probe (research §6.2 open question)

Timestamp: 2026-08-11T00-38
Command: read-only analysis via `pwsh -NoProfile -File <scratchpad>/p0t12-probe.ps1` and
`<scratchpad>/p0t12-corpus2.ps1`, plus `git log -1 --format=%cI -- <file>` and `git show <sha>:<file>`
EXIT_CODE: 0

## Question

Does `dotnet-coverage` emit a `Type.<Member>d__<N>` state-machine class for a member that carries
`[ExcludeFromCodeCoverage]` and is `async`?

## Probe Answer: YES

A `d__` class **is** emitted for a member that carries `[ExcludeFromCodeCoverage]` and is `async`.
The residual text in `spec.md` § Risks & Mitigations, residual 1 therefore **stands as written**, and
`[P4-T5]` takes the `YES` branch: no change to `spec.md` residual text.

No C# file was modified by this task. It is read-only.

## Step 1 — attributed async members enumerated

62 `(namespace-qualified declaring type, member name)` pairs were found across first-party C# source,
excluding `\bin\`, `\obj\` and `\packages\`.

Method: for each line matching `^\s*\[\s*(?:System\.Diagnostics\.CodeAnalysis\.)?ExcludeFromCodeCoverage`,
advance to the first following line that is not blank, not a comment and not another attribute — that
is the declaration the attribute applies to — and admit the pair when that declaration contains
`\basync\b`.

Measurement-integrity note: a first attempt reported `ATTRIBUTED_ASYNC_MEMBER_COUNT: 0`. The cause was
a self-defeating exclusion filter, `$_.FullName -notmatch '\\\.claude\\worktrees\\'`, applied to the
FULL path. The executing worktree is itself
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a`, so that predicate
excluded every file in the repository. This is exactly the failure mode the plan warns about in
`[P0-T11]` ("A `\.claude\` substring test over the full path is unsatisfiable when the executing
worktree is itself under `.claude\worktrees\`"). The filter was corrected to test the root-relative
remainder and the count rose from 0 to 62. The zero is recorded here as a corrected measurement
error, not as a finding.

Representative pairs (full list of 62 produced by the probe script):

| Declaring type | Member | Site |
|---|---|---|
| `QuickFiler.Controllers.QfcItemController` | `ToggleExpansionAsync` | `QuickFiler\Controllers\QfcItemController.Navigation.cs:191` (attr), `:192` (decl) |
| `QuickFiler.Controllers.QfcItemController` | `InitializeWebViewAsync` | `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs:41` (attr), `:42` (decl) |
| `QuickFiler.Controllers.QfcItemController` | `BtnPopOut_Click` | `QuickFiler\Controllers\QfcItemController.EventHandlers.cs:60` (attr) |
| `UtilitiesCS.SortEmail` | `SortAsync` (4 overloads) | `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs:42, 76, 112, 303` (attr) |
| `UtilitiesCS.ManagerAsyncLazy` | `ResetLoadManagerAsyncLazy` | `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs:324` (attr) |
| `UtilitiesCS.EmailIntelligence.Bayesian.EmailDataMiner` | `MineEmails` | `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailDataMiner.cs:39` (attr) |
| `TaskVisualization.FlagChangeGroup` | `ProcessGroupAsync` | `TaskVisualization\FlagChangeGroup.cs:75` (attr) |
| `UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyReader` | `ReadFoldersAsync` | `UtilitiesCS\OutlookObjects\Folder\OutlookFolderHierarchyReader.cs:45` (attr) |

## Step 2 — corpus selection and RAW verification

Corpus: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`
(17,473,869 bytes), identified by research §1 as raw.

Verification of the raw identification, before use:

| Criterion | Measurement | Verdict |
|---|---|---|
| A. absolute `filename` attributes (`filename="X:\`) | 2047 matches | raw |
| B. closure classes retained as sibling `<class>` elements (`name="[^"]*&lt;&gt;c`) | 868 matches | raw |
| C. post-processing marker `<sources>` element present | False | raw |

CORPUS_VERDICT: **RAW (verified)**

The post-processed Phase 0 artifact is deliberately not used, because
`Merge-CoberturaClassesByFilename` collapses `d__` classes into the declaring type's class.

Measurement-integrity note: a first attempt reported criterion B as 0 and returned
`CORPUS_VERDICT: NOT RAW`. The cause was the pattern `class name="`, which cannot match this corpus
because it emits `<class line-rate=… branch-rate=… complexity=… name=… filename=…>` — attribute
ORDER, not attribute absence. The pattern was corrected to `name="[^"]*&lt;&gt;c` and the count rose
from 0 to 868, consistent with research §1's recorded figure of 873 occurrences of the escaped token.
The same ordering defect suppressed the declaring-type presence counts in the same first attempt; all
figures below come from the corrected patterns.

CORPUS_TIMESTAMP_EPOCH: 1786069165
CORPUS_CAPTURE_DATE_UTC: **2026-08-07T02:19:25Z**

## Step 3 — search

SearchPatterns (regular expressions applied to the corpus text; `<` and `>` appear XML-escaped):

- Declaring-type presence: `name="<escaped DeclaringType>"`
- State machine: `name="<escaped DeclaringType>.&lt;<escaped Member>&gt;d__`

SearchResult:

| Declaring type | Member | declaring-type `<class>` count | `d__` match count |
|---|---|---|---|
| `QuickFiler.Controllers.QfcItemController` | `ToggleExpansionAsync` | 10 | **1** |
| `QuickFiler.Controllers.QfcItemController` | `InitializeWebViewAsync` | 10 | 0 |
| `UtilitiesCS.SortEmail` | `SortAsync` | 1 | 0 |
| `UtilitiesCS.ManagerAsyncLazy` | `ResetLoadManagerAsyncLazy` | 1 | 0 |
| `UtilitiesCS.EmailIntelligence.Bayesian.EmailDataMiner` | `MineEmails` | 4 | 0 |

The positive match, verbatim:

```
name="QuickFiler.Controllers.QfcItemController.&lt;ToggleExpansionAsync&gt;d__203" filename="C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-04T18-38\Q…
```

`ToggleExpansionAsync` is one of the 62 attributed async members enumerated in step 1. A single
positive instance settles the question in the affirmative; the four zero results are recorded as
observations and do not weaken it. A zero result means only that the member's state machine did not
appear in this particular corpus — most plausibly because the member was never loaded or its assembly
was not exercised by the run that produced it — and cannot, on its own, establish a negative.

## Step 4 — soundness guard

The guard is mandatory before recording `NO`. It is applied here to the positive instance as well,
because a `d__` class present in a corpus captured **before** the attribute was added would be a false
`YES`.

| Fact | Value |
|---|---|
| Corpus capture date (from the corpus's own `<coverage timestamp>`) | 2026-08-07T02:19:25Z (= 2026-08-06T22:19:25-04:00) |
| `git log -1 --format=%cI -- QuickFiler/Controllers/QfcItemController.Navigation.cs` | `2026-07-03T09:16:18-04:00` |
| Commit | `6b821480af9aea37d5801cbd753082cdd2d908ed` "refactor(#227): remediation cycle 5 — reduce residual exemptions 24 -> 19" |

The file's most recent commit of any kind precedes the corpus capture by roughly 35 days, so the file
content at corpus-capture time is exactly the content of that commit. The attribute was verified
present in that commit's content directly:

```
$ git show 6b821480af9aea37d5801cbd753082cdd2d908ed:QuickFiler/Controllers/QfcItemController.Navigation.cs | sed -n '188,194p'

        // Made virtual so tests can override the (TlpCellSnapShot-bound, out-of-scope) state-taking
        // body and verify the parameterless-overload routing without the control-tree collaborator.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public virtual async Task ToggleExpansionAsync(Enums.ToggleState desiredState)
        {
            await _parent.ToggleExpansionStyleAsync(ItemIndex, desiredState);
```

The current working-tree content at the same lines is byte-identical. The
`[ExcludeFromCodeCoverage]` attribute on `ToggleExpansionAsync` therefore **predates the corpus**.

SOUNDNESS GUARD: SATISFIED. The answer is `YES`, not `NOT-DETERMINABLE-FROM-CORPUS`.

## Consequence for the implementation

This confirms that presence-set source (2) — admitting the `<Member>` token of a class named
`Type.<Member>d__<N>` — is genuinely load-bearing in both directions, and confirms residual (a) as
stated: because a `d__` class IS emitted for an attributed async member, that member enters the
presence set and lambdas declared inside it are RETAINED. That under-exclusion is the deliberate
price of not deleting covered lambdas inside non-exempt async members (required direction 2).
`[P4-T2]` opens a follow-up entry for it.

## Output Summary

Probe Answer: **YES**. Corpus verified RAW (2047 absolute filenames, 868 closure classes, no
`<sources>`), captured 2026-08-07T02:19:25Z. 62 attributed async members enumerated. The declaring
type `QuickFiler.Controllers.QfcItemController` appears in the corpus as 10 `<class>` elements and
carries `…&lt;ToggleExpansionAsync&gt;d__203`, whose member is attributed
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`. The soundness guard is satisfied: the
attribute has been present since commit `6b821480` (2026-07-03), 35 days before the corpus was
captured. `spec.md` residual 1 stands unchanged; `[P4-T5]` takes the `YES` branch.
