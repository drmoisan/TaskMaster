Timestamp: 2026-10-18T01-54
Command: node --test --test-isolation=none --test-concurrency=1 --test-reporter=spec SCRATCH\issue-997-order-shuffled.mjs
EXIT_CODE: 0
ExpectedExitCode: 0
Output Summary: 78/78 pass; full individual fixture order: SCRATCH/issue-997-order-shuffled.json. Fresh VM/browser state each test. Shuffled LCG seed997; normal original order; reversed exact full individual order. Outer tooling stages unchanged test bodies with only import routing for controlled registration. No production transform; no test body filesystem/process/service.
