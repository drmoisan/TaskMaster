Timestamp: 2026-08-06T16-11
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/regression-testing/remediation-cycle4-coverage-gap-fail-before.cobertura.xml
EXIT_CODE: 124
Output Summary: The serial coverage command exceeded the executor timeout after 124.2 seconds and did not produce the requested Cobertura report. Process identity at timeout verification: vstest.console PID 87036, started 2026-08-06T16:09:38, CPU 0.81 seconds. This is stale-runner test infrastructure evidence, not a product-code or coverage-gate result. P5-T40 remains unchecked because the required exact unhit-line and failing >=90% coverage proof is unavailable.

## Stale-runner classification

The coverage command was launched after a process inspection found no `vstest` process. The timeout left the recorded serial `vstest.console` process active. Per the approved plan, no successor task or final QA step was executed.
