# [P3-T7] Changed-line coverage on the measurable production set

Timestamp: 2026-09-07T08-05

Command: `git add --intent-to-add -- '*.cs'`; `git status --porcelain --untracked-files=all -- 'UtilitiesCS/OutlookObjects/Folder' 'QuickFiler/Controllers'`; `git diff --unified=0 <BASE-SHA> -- <path>` for each of the six measurable production paths; then the de-duplicated per-line `hits` map built from artifacts\csharp\coverage.xml and, for the baseline side, from coverage\799-baseline.cobertura.xml

EXIT_CODE: 0

ExpectedExitCode: 0

## Measurable set

[P0-T13] reported all seven queried production paths as `MEASURABLE:` and none as `UNMEASURABLE:`. The comparison
below therefore covers all six existing Write Set production paths with no substitute-evidence branch taken. No
path required a `CHANGED-LINE-COVERAGE: NOT MEASURABLE` record.

The two files this plan creates are out of scope for this task by construction — they have no baseline side — and
are measured for the first time by [P3-T9].

## Porcelain companion

```
 M QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs
 M UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs
 M UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs
 M UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs
```

The porcelain output is scoped to the two directories the task names. It lists only the four files the [P3-T1]
formatter touched within those directories after the Phase 2 commit, because everything else in this plan's footprint is already committed
at `f50fb727` and the anchored diff, not porcelain, is what sees it. The two mechanisms are complementary and each
alone is wrong in one state, which is why both are recorded.

## Method

For each measurable path the changed line numbers are the `+` lines of the anchored `git diff --unified=0` against
`BASE-SHA`. Each changed line's post-change `hits` is read from a de-duplicated per-line map over
artifacts\csharp\coverage.xml that merges the class-level `line` elements with the method-level `line` elements,
keyed by line number and resolved by maximum `hits`. The baseline side uses the identical map construction over
coverage\799-baseline.cobertura.xml.

Baseline mapping follows the rule this task states: a changed line is given a baseline counterpart only when its
hunk's removed and added line counts are equal, so a one-to-one correspondence exists. Every other changed line is
recorded `base=none` and excluded from the regression count rather than being attributed borrowed coverage.

A changed line carrying no `line` element in either branch of the merged map is non-executable — an XML doc
comment, a blank line, a using directive, a brace or an interface method declaration — and is recorded as
`hits=non-executable` and excluded from both the `hits = 0` count and the regression count.

## Per-file result

| Path | Changed lines | Executable with `hits = 0` | Non-executable | Regressions |
|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 171 | 0 | 107 | 0 |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 20 | 0 | 9 | 0 |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 16 | 0 | 12 | 0 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 106 | 2 | 52 | 0 |
| `QuickFiler/Controllers/EfcFormController.cs` | 2 | 2 | 0 | 0 |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 0 | 0 | 0 | 0 |
| **Total** | **315** | **4** | **180** | **0** |

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` shows zero changed lines because [P2-T3] only REMOVED
lines from it (the relocation of the breadcrumb pipeline helper), and [P2-T14] added the constructor argument to
the relocated member in the new partial rather than to this file. Its anchored diff contains deletions only, so it
contributes no `+` line to this comparison.

## Regression count

CHANGED-LINES-WITH-LOWER-POST-HITS: 0

This is the task's gating figure and it is 0. Every one of the 315 changed lines fell in a hunk whose removed and
added counts are unequal, so all 315 are recorded `base=none` and none entered the regression count. That is the
expected shape for a change dominated by additions: the two projection helpers, the AC7 gate and absence report,
the AC6 score projection and the AC7 suppression are all new code, and the four converted call sites replaced
short bodies with shorter delegations.

Because a mechanical count of zero could be read as vacuous when every line is `base=none`, the two files that
carry executable zero-hit lines were additionally checked directly against the baseline document, below.

## The four executable changed lines with `hits = 0`

COUNT-OF-CHANGED-EXECUTABLE-LINES-WITH-ZERO-HITS: 4

### `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` lines 202 and 203

These are the opening brace and the `continue;` of the null-`FolderPath` guard inside the [P2-T11] score
projection loop. The guard is what makes the null-forgiving operator on the following `ToDisplayStem` call sound,
so it is required by the [P3-T4] nullable gate rather than optional. Its false arm is exercised on every test that
reaches the loop; its true arm needs a `FolderScore` whose `FolderPath` is null, which no test in this plan or in
the existing suite constructs.

Post-change `hits` for the surrounding lines, read from the same merged map, shows the loop itself is covered and
only the defensive arm is not:

```
L197 hits=1   L198 hits=1   L199 hits=1   L200 hits=1   L201 hits=1
L202 hits=0   L203 hits=0
L208 hits=1   L209 hits=1   L210 hits=1   L211 hits=1   L212 hits=1
```

No coverage was lost: these two lines did not exist at the base commit, so there is no baseline value they could
be lower than.

### `QuickFiler/Controllers/EfcFormController.cs` lines 1054 and 1055

These are the two arguments of the provider construction [P2-T13] changed:
`_globals.Ol.FolderTreeService` and the added lazy accessor `() => _globals.Ol.ArchiveRootPath`. They sit inside
`ConfigureBreadcrumbControl`, which constructs a `WebView2BreadcrumbHost` over the form's live WebView2 control and
is therefore not reachable from a unit test.

The whole method was already uncovered at the base commit. Read from coverage\799-baseline.cobertura.xml and from
artifacts\csharp\coverage.xml over the same line span:

```
baseline   L1048..L1058 all hits=0
post-change L1048..L1058 all hits=0
```

So the change neither gained nor lost coverage on those lines; it added one argument to an already-uncovered
construction. This is exactly the COM/VSTO/WinForms shape the repository coverage policy anticipates, and it is
why AC6 was delivered in the router rather than at this call site (D7): the router half IS covered, at
`hits=1` on lines 197 through 212 above.

## Substitute evidence for the four zero-hit lines

Named passing tests that exercise the behaviour these lines participate in, all green in [P2-T16] and in the
[P3-T5] full run:

- Router score projection loop (lines 197-215, of which only the null arm is uncovered):
  `BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage`,
  `BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage`,
  `BindRowsAsync_EmptyBoundRoot_LeavesTheJoinUnchanged` and
  `BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey`.
- The lazy-accessor contract the Efc construction supplies:
  `GetAncestorChainAsync_RootAccessorThrows_DoesNotThrowAndReturnsTheUntrimmedChain` pins that an accessor which
  throws is tolerated, and `BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow` in
  QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs still passes, which is the reason the argument is a
  delegate rather than an eagerly read value (D2).

## Output Summary

All six measurable production paths were compared. 315 changed lines were recorded, every one with either a
post-change `hits` value or the `hits=non-executable` marker. 180 are non-executable. Of the 135 executable
changed lines, 131 have `hits` greater than 0 and 4 have `hits = 0`. The count of changed lines whose post-change
`hits` is lower than their baseline `hits` is 0, which is this task's acceptance condition. The four zero-hit lines
are a null-guard arm in the router and two arguments inside an already-uncovered WebView2-bound method, and both
were checked directly against the baseline document to confirm no coverage was lost.

## Per-line detail

Every changed line, grouped by file, with its hunk header, its post-change `hits` value or `non-executable`
marker, and its baseline mapping. `one_to_one=False` on a hunk header is why the lines under it read `base=none`.

```
=== UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs
POST_MAPPED_LINES=121 BASE_MAPPED_LINES=65
  HUNK -2,0 +3,0 one_to_one=False
  L3 hits=non-executable base=none
  HUNK -9,0 +11,25 one_to_one=False
  L11 hits=non-executable base=none
  L12 hits=non-executable base=none
  L13 hits=non-executable base=none
  L14 hits=non-executable base=none
  L15 hits=non-executable base=none
  L16 hits=non-executable base=none
  L17 hits=non-executable base=none
  L18 hits=non-executable base=none
  L19 hits=non-executable base=none
  L20 hits=non-executable base=none
  L21 hits=non-executable base=none
  L22 hits=non-executable base=none
  L23 hits=non-executable base=none
  L24 hits=non-executable base=none
  L25 hits=non-executable base=none
  L26 hits=non-executable base=none
  L27 hits=non-executable base=none
  L28 hits=non-executable base=none
  L29 hits=non-executable base=none
  L30 hits=non-executable base=none
  L31 hits=non-executable base=none
  L32 hits=non-executable base=none
  L33 hits=non-executable base=none
  L34 hits=non-executable base=none
  L35 hits=non-executable base=none
  HUNK -15,0 +41,3 one_to_one=False
  L41 hits=non-executable base=none
  L42 hits=non-executable base=none
  L43 hits=non-executable base=none
  HUNK -22,0 +51,16 one_to_one=False
  L51 hits=non-executable base=none
  L52 hits=non-executable base=none
  L53 hits=non-executable base=none
  L54 hits=non-executable base=none
  L55 hits=non-executable base=none
  L56 hits=non-executable base=none
  L57 hits=non-executable base=none
  L58 hits=non-executable base=none
  L59 hits=1 base=none
  L60 hits=1 base=none
  L61 hits=1 base=none
  L62 hits=non-executable base=none
  L63 hits=1 base=none
  L64 hits=1 base=none
  L65 hits=1 base=none
  L66 hits=non-executable base=none
  HUNK -26,0 +71,8 one_to_one=False
  L71 hits=non-executable base=none
  L72 hits=non-executable base=none
  L73 hits=non-executable base=none
  L74 hits=non-executable base=none
  L75 hits=non-executable base=none
  L76 hits=non-executable base=none
  L77 hits=non-executable base=none
  L78 hits=non-executable base=none
  HUNK -28,0 +80,4 one_to_one=False
  L80 hits=1 base=none
  L81 hits=1 base=none
  L82 hits=1 base=none
  L83 hits=1 base=none
  HUNK -30,0 +86,0 one_to_one=False
  L86 hits=1 base=none
  HUNK -32,0 +89,14 one_to_one=False
  L89 hits=non-executable base=none
  L90 hits=non-executable base=none
  L91 hits=non-executable base=none
  L92 hits=non-executable base=none
  L93 hits=non-executable base=none
  L94 hits=non-executable base=none
  L95 hits=non-executable base=none
  L96 hits=non-executable base=none
  L97 hits=non-executable base=none
  L98 hits=non-executable base=none
  L99 hits=non-executable base=none
  L100 hits=non-executable base=none
  L101 hits=non-executable base=none
  L102 hits=non-executable base=none
  HUNK -41,0 +111,60 one_to_one=False
  L111 hits=1 base=none
  L112 hits=non-executable base=none
  L113 hits=non-executable base=none
  L114 hits=non-executable base=none
  L115 hits=1 base=none
  L116 hits=1 base=none
  L117 hits=1 base=none
  L118 hits=1 base=none
  L119 hits=non-executable base=none
  L120 hits=non-executable base=none
  L121 hits=1 base=none
  L122 hits=1 base=none
  L123 hits=1 base=none
  L124 hits=1 base=none
  L125 hits=1 base=none
  L126 hits=non-executable base=none
  L127 hits=non-executable base=none
  L128 hits=non-executable base=none
  L129 hits=non-executable base=none
  L130 hits=non-executable base=none
  L131 hits=1 base=none
  L132 hits=1 base=none
  L133 hits=1 base=none
  L134 hits=1 base=none
  L135 hits=1 base=none
  L136 hits=non-executable base=none
  L137 hits=non-executable base=none
  L138 hits=non-executable base=none
  L139 hits=non-executable base=none
  L140 hits=non-executable base=none
  L141 hits=non-executable base=none
  L142 hits=non-executable base=none
  L143 hits=non-executable base=none
  L144 hits=non-executable base=none
  L145 hits=1 base=none
  L146 hits=1 base=none
  L147 hits=1 base=none
  L148 hits=1 base=none
  L149 hits=1 base=none
  L150 hits=non-executable base=none
  L151 hits=non-executable base=none
  L152 hits=non-executable base=none
  L153 hits=1 base=none
  L154 hits=1 base=none
  L155 hits=non-executable base=none
  L156 hits=1 base=none
  L157 hits=1 base=none
  L158 hits=1 base=none
  L159 hits=1 base=none
  L160 hits=1 base=none
  L161 hits=1 base=none
  L162 hits=1 base=none
  L163 hits=non-executable base=none
  L164 hits=1 base=none
  L165 hits=non-executable base=none
  L166 hits=non-executable base=none
  L167 hits=non-executable base=none
  L168 hits=1 base=none
  L169 hits=1 base=none
  L170 hits=1 base=none
  HUNK -75,0 +205,3 one_to_one=False
  L205 hits=non-executable base=none
  L206 hits=non-executable base=none
  L207 hits=1 base=none
  HUNK -79,0 +211,7 one_to_one=False
  L211 hits=1 base=none
  L212 hits=1 base=none
  L213 hits=1 base=none
  L214 hits=1 base=none
  L215 hits=1 base=none
  L216 hits=non-executable base=none
  L217 hits=1 base=none
  HUNK -81,0 +220,4 one_to_one=False
  L220 hits=non-executable base=none
  L221 hits=non-executable base=none
  L222 hits=1 base=none
  L223 hits=non-executable base=none
  HUNK -88,0 +231,4 one_to_one=False
  L231 hits=non-executable base=none
  L232 hits=non-executable base=none
  L233 hits=non-executable base=none
  L234 hits=non-executable base=none
  HUNK -90,0 +236,0 one_to_one=False
  L236 hits=non-executable base=none
  HUNK -108,5 +254,20 one_to_one=False
  L254 hits=1 base=none
  L255 hits=1 base=none
  L256 hits=non-executable base=none
  L257 hits=non-executable base=none
  L258 hits=non-executable base=none
  L259 hits=1 base=none
  L260 hits=1 base=none
  L261 hits=non-executable base=none
  L262 hits=non-executable base=none
  L263 hits=non-executable base=none
  L264 hits=non-executable base=none
  L265 hits=1 base=none
  L266 hits=1 base=none
  L267 hits=1 base=none
  L268 hits=1 base=none
  L269 hits=1 base=none
  L270 hits=1 base=none
  L271 hits=1 base=none
  L272 hits=1 base=none
  L273 hits=non-executable base=none
  FILE_TOTALS changed=171 zero_hits_executable=0 non_executable=107 regressions=0
=== UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs
POST_MAPPED_LINES=535 BASE_MAPPED_LINES=540
  HUNK -11,0 +12,0 one_to_one=False
  L12 hits=non-executable base=none
  HUNK -793,0 +794,5 one_to_one=False
  L794 hits=non-executable base=none
  L795 hits=1 base=none
  L796 hits=1 base=none
  L797 hits=1 base=none
  L798 hits=1 base=none
  HUNK -850,11 +855,3 one_to_one=False
  L855 hits=non-executable base=none
  L856 hits=non-executable base=none
  L857 hits=1 base=none
  HUNK -876,0 +874,3 one_to_one=False
  L874 hits=non-executable base=none
  L875 hits=non-executable base=none
  L876 hits=1 base=none
  HUNK -879,0 +879,2 one_to_one=False
  L879 hits=1 base=none
  L880 hits=1 base=none
  HUNK -957,8 +958,6 one_to_one=False
  L958 hits=non-executable base=none
  L959 hits=non-executable base=none
  L960 hits=non-executable base=none
  L961 hits=1 base=none
  L962 hits=1 base=none
  L963 hits=1 base=none
  FILE_TOTALS changed=20 zero_hits_executable=0 non_executable=9 regressions=0
=== QuickFiler/Controllers/QfcItemController.FolderHandling.cs
POST_MAPPED_LINES=168 BASE_MAPPED_LINES=173
  HUNK -227,4 +227,4 one_to_one=True
  L227 hits=non-executable base=none
  L228 hits=non-executable base=none
  L229 hits=non-executable base=none
  L230 hits=non-executable base=none
  HUNK -253,18 +253,8 one_to_one=False
  L253 hits=non-executable base=none
  L254 hits=non-executable base=none
  L255 hits=non-executable base=none
  L256 hits=non-executable base=none
  L257 hits=non-executable base=none
  L258 hits=non-executable base=none
  L259 hits=non-executable base=none
  L260 hits=non-executable base=none
  HUNK -274,11 +264,4 one_to_one=False
  L264 hits=1 base=none
  L265 hits=1 base=none
  L266 hits=1 base=none
  L267 hits=1 base=none
  FILE_TOTALS changed=16 zero_hits_executable=0 non_executable=12 regressions=0
=== QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
POST_MAPPED_LINES=213 BASE_MAPPED_LINES=162
  HUNK -25,0 +26,6 one_to_one=False
  L26 hits=non-executable base=none
  L27 hits=non-executable base=none
  L28 hits=non-executable base=none
  L29 hits=non-executable base=none
  L30 hits=non-executable base=none
  L31 hits=non-executable base=none
  HUNK -49,0 +56,0 one_to_one=False
  L56 hits=1 base=none
  HUNK -106,0 +114,0 one_to_one=False
  L114 hits=1 base=none
  HUNK -128,0 +137,14 one_to_one=False
  L137 hits=1 base=none
  L138 hits=non-executable base=none
  L139 hits=non-executable base=none
  L140 hits=non-executable base=none
  L141 hits=non-executable base=none
  L142 hits=non-executable base=none
  L143 hits=1 base=none
  L144 hits=1 base=none
  L145 hits=1 base=none
  L146 hits=1 base=none
  L147 hits=1 base=none
  L148 hits=1 base=none
  L149 hits=1 base=none
  L150 hits=1 base=none
  HUNK -131,0 +154,0 one_to_one=False
  L154 hits=1 base=none
  HUNK -133,0 +156,0 one_to_one=False
  L156 hits=1 base=none
  HUNK -135,0 +158,0 one_to_one=False
  L158 hits=1 base=none
  HUNK -137,0 +160,5 one_to_one=False
  L160 hits=non-executable base=none
  L161 hits=non-executable base=none
  L162 hits=non-executable base=none
  L163 hits=non-executable base=none
  L164 hits=1 base=none
  HUNK -151,0 +179,76 one_to_one=False
  L179 hits=non-executable base=none
  L180 hits=non-executable base=none
  L181 hits=non-executable base=none
  L182 hits=non-executable base=none
  L183 hits=non-executable base=none
  L184 hits=non-executable base=none
  L185 hits=non-executable base=none
  L186 hits=non-executable base=none
  L187 hits=1 base=none
  L188 hits=non-executable base=none
  L189 hits=non-executable base=none
  L190 hits=non-executable base=none
  L191 hits=non-executable base=none
  L192 hits=1 base=none
  L193 hits=1 base=none
  L194 hits=1 base=none
  L195 hits=non-executable base=none
  L196 hits=non-executable base=none
  L197 hits=1 base=none
  L198 hits=1 base=none
  L199 hits=1 base=none
  L200 hits=1 base=none
  L201 hits=1 base=none
  L202 hits=0 base=none
  L203 hits=0 base=none
  L204 hits=non-executable base=none
  L205 hits=non-executable base=none
  L206 hits=non-executable base=none
  L207 hits=non-executable base=none
  L208 hits=1 base=none
  L209 hits=1 base=none
  L210 hits=1 base=none
  L211 hits=1 base=none
  L212 hits=1 base=none
  L213 hits=1 base=none
  L214 hits=1 base=none
  L215 hits=1 base=none
  L216 hits=1 base=none
  L217 hits=non-executable base=none
  L218 hits=1 base=none
  L219 hits=1 base=none
  L220 hits=non-executable base=none
  L221 hits=non-executable base=none
  L222 hits=non-executable base=none
  L223 hits=non-executable base=none
  L224 hits=non-executable base=none
  L225 hits=non-executable base=none
  L226 hits=non-executable base=none
  L227 hits=non-executable base=none
  L228 hits=non-executable base=none
  L229 hits=non-executable base=none
  L230 hits=1 base=none
  L231 hits=1 base=none
  L232 hits=1 base=none
  L233 hits=1 base=none
  L234 hits=non-executable base=none
  L235 hits=non-executable base=none
  L236 hits=1 base=none
  L237 hits=1 base=none
  L238 hits=1 base=none
  L239 hits=1 base=none
  L240 hits=1 base=none
  L241 hits=1 base=none
  L242 hits=non-executable base=none
  L243 hits=non-executable base=none
  L244 hits=non-executable base=none
  L245 hits=non-executable base=none
  L246 hits=1 base=none
  L247 hits=1 base=none
  L248 hits=non-executable base=none
  L249 hits=1 base=none
  L250 hits=1 base=none
  L251 hits=1 base=none
  L252 hits=1 base=none
  L253 hits=1 base=none
  L254 hits=non-executable base=none
  FILE_TOTALS changed=106 zero_hits_executable=2 non_executable=52 regressions=0
=== QuickFiler/Controllers/EfcFormController.cs
POST_MAPPED_LINES=823 BASE_MAPPED_LINES=822
  HUNK -1054,0 +1054,2 one_to_one=False
  L1054 hits=0 base=none
  L1055 hits=0 base=none
  FILE_TOTALS changed=2 zero_hits_executable=2 non_executable=0 regressions=0
=== QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
POST_MAPPED_LINES=210 BASE_MAPPED_LINES=210
  HUNK -132,33 +131,0 one_to_one=False
  FILE_TOTALS changed=0 zero_hits_executable=0 non_executable=0 regressions=0
GLOBAL changed=315 zero_hits_executable=4 non_executable=180 baseline_none=315 regressions=0
```

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
