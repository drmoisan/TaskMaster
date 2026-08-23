# P5-T24 green evidence

Timestamp: 2026-08-05T02:12:00-04:00 (derived from the artifact filename)
Command: Multiple recorded VSTest commands — the named AppOlObjects classes and `/TestCaseFilter:"FullyQualifiedName~AppOlObjects" /InIsolation`.
EXIT_CODE: 0
Output Summary: The recorded isolated AppOlObjects execution passed 45/45; the named component suites passed 11/11 and 14/14, respectively.

- `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:AppOlObjectsFolderTreeServiceTests` passed 11/11.
- `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:AppOlObjectsFolderTreeServiceLifecycleTests` passed 14/14.
- `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /TestCaseFilter:"FullyQualifiedName~AppOlObjects" /InIsolation` passed 45/45.
- The retained original class covers the nine required session, disposal, worker composition, queued caller, publication/disposal, retry, and base-dispatcher behaviors.
- Terminal setup and null-factory failures preserve exact exception identity; retries use a fresh factory/thread-check ownership path and one captured dispatcher instance.
- Blocking and queued dispatcher tests verify worker-first composition, captured dispatcher identity/thread, one load, one InvokeAsync, zero BeginInvoke, and stale callback non-publication.
- Candidate publication is atomic for the service and concrete notification sink. Stale candidates cannot overwrite retry state. Candidate sink disposal is attempted when service disposal throws, without replacing the owned ObjectDisposedException terminal.
- The two planned test sources each have one TaskMaster.Test compile entry and are 490 lines. They contain no reflection, global dispatcher mutation, polling, timing, temporary files, or live Outlook UI seams.
- CSharpier, analyzer, nullable, `git diff --check`, compile-entry, forbidden-pattern, and residual-runner checks passed.
- The non-isolated combined AppOlObjects run historically failed six Moq `System.Threading.Tasks.Extensions` host-resolution cases; `/InIsolation` resolves the host and passed 45/45.
