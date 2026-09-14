# P4-T17 — the production row builder, exercised rather than displaced

Timestamp: 2026-09-13T16-32

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t17

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t17\vstest-class-run.trx:
  `total=26 executed=26 passed=26 failed=0`
- New case: `AddAsync_WithSubstitutedViewerSeams_BuildsTheGroupAndPlacesTheViewer`. It leaves the
  item-group seam at its default, substitutes only the viewer factory, the row placer and the
  dispatcher fake, calls the row builder directly, and asserts the returned item group carries the
  supplied mail item, that the viewer factory received the token the queue was constructed with,
  and that the row placer received the panel, the viewer the factory returned and the index.
- This exercises the production body of the row builder rather than displacing it, which is what
  makes the body coverable instead of merely bypassed.
- The viewer stand-in is an uninitialised `ItemViewer` allocated without running its constructor,
  the pattern the test project already uses for viewer types. Nothing reads a member of it, so no
  WinForms handle is created.
