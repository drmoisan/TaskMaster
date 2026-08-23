# P11-T1 review-successor baseline

Timestamp: 2026-08-04T10-02

Commands:

```powershell
git rev-parse origin/main
git rev-parse HEAD
git merge-base origin/main HEAD
cmd.exe /d /s /c "git diff --check 050f7cd52a3b13ec2786c9dafbe9f99620ebf9e8 HEAD 2>&1"
git cat-file -e "HEAD:<each required path>"
```

EXIT_CODE: 0 for the baseline collection command; `git diff --check` returned 2, as expected when whitespace diagnostics are present.

Output Summary:

- `origin/main` is `050f7cd52a3b13ec2786c9dafbe9f99620ebf9e8`.
- `HEAD` is `62c4eb1c2b99ae6e9fa7742a31d283ec4a8d7151`.
- The merge base is `050f7cd52a3b13ec2786c9dafbe9f99620ebf9e8`.
- The raw `git diff --check` output contains 1,396 diagnostic headers (2,792 output lines because Git prints a source-content line after each header), all within the required fifteen committed paths.
- `git cat-file -e` succeeded for each required path at `HEAD`.

## Per-file byte inventory

| Path | SHA-256 | Bytes | UTF-8 BOM | CRLF | LF | CR | TRX total-count fields |
| --- | --- | ---: | --- | ---: | ---: | ---: | --- |
| evidence/regression-testing/member-coverage-all-eight-determinism-attempt-2.2026-07-27T04-31.trx | 4D95E611B057DE22903BB9C416F331418546D5EAE59A814CC2E17A8A10474D8F | 8133762 | yes | 41308 | 0 | 0 | total=6056; executed=6056; passed=6056; failed=0 |
| evidence/regression-testing/member-coverage-all-eight-determinism-run-1.2026-07-27T04-43.trx | 37039067BA6E354AB2C8D3E0AD012EA14CCCD8708EE914B7E86E08B59ABFB0F4 | 8133780 | yes | 41311 | 0 | 0 | total=6056; executed=6056; passed=6056; failed=0 |
| evidence/regression-testing/member-coverage-all-eight-determinism-run-2.2026-07-27T04-44.trx | B194EB9938B76FBE48AB20DC4E9E398825954B73509CDBDEBB8C32820C3BEDBF | 8133763 | yes | 41308 | 0 | 0 | total=6056; executed=6056; passed=6056; failed=0 |
| evidence/regression-testing/member-coverage-bridge-stale-aggregate-blame.2026-07-27T05-34.trx | B9DB777DA5B528CB29C54F5EF4C6FEA69EF53F10D4D30276C9898708418C7909 | 8134739 | yes | 41321 | 0 | 0 | total=6056; executed=6056; passed=6055; failed=1 |
| evidence/regression-testing/member-coverage-isolation-TaskMaster.Test.2026-07-27T04-34.trx | F8B4B8339A455E1037AFC1F28DD3128D0B364FF1A03FADFBC3BDB673C280F33B | 400232 | yes | 2172 | 0 | 0 | total=250; executed=250; passed=250; failed=0 |
| evidence/regression-testing/member-coverage-isolation-UtilitiesCS.Test.2026-07-27T04-35.trx | 100822059866281AF1446A0E327DF0C4EEA27B2EC6E3109D99EF3C66D776E1C1 | 6151483 | yes | 31238 | 0 | 0 | total=4608; executed=4608; passed=4608; failed=0 |
| evidence/regression-testing/member-coverage-selector-transition-determinism-run-1.2026-07-27T06-04.trx | 38F46BB43E41BF7D8CD44BDBF5D5554D2363D27A462ED01DD7456B047546EE9A | 8133774 | yes | 41311 | 0 | 0 | total=6056; executed=6056; passed=6056; failed=0 |
| evidence/regression-testing/member-coverage-selector-transition-determinism-run-2.2026-07-27T06-05.trx | F208EA98C355C35A6086B2C73E4624F3CCE59E832C464EE1414174592C32D505 | 8133777 | yes | 41311 | 0 | 0 | total=6056; executed=6056; passed=6056; failed=0 |
| evidence/regression-testing/nonnumeric-adapter-coverage-failure-classification-unbuffered.2026-07-27T09-23.stdout.txt | 873E8F989C30E3E316E105F8A55C4289A516BBFEF7C67118B89BA26981346C32 | 648382 | no | 11881 | 0 | 0 | n/a |
| evidence/regression-testing/nonnumeric-adapter-coverage-failure-classification-unbuffered.2026-07-27T09-23.trx | 435D38D656CD63582003B8F961F9933380B7BC6D8D4A1F99DC83213AA0670111 | 8160968 | yes | 41498 | 0 | 0 | total=6066; executed=6066; passed=6058; failed=8 |
| evidence/regression-testing/p9-t4-all-assembly-spambayes-diagnostic.2026-07-27T02-59.trx | 470139F604B78B68E98ED837C2A8EA4F580626067AA6ED9851F1FE4B17132134 | 8128872 | yes | 41376 | 0 | 0 | total=6047; executed=6047; passed=6043; failed=4 |
| evidence/regression-testing/spambayes-all-assembly-pass-after.2026-07-27T03-28.trx | 99F099B4390F1AC4AD416213DA38D4A00B0338240D23824F7B22FB5333DECFA9 | 8124490 | yes | 41269 | 0 | 0 | total=6049; executed=6049; passed=6049; failed=0 |
| code-review.2026-07-27T12-02.md | AE747426DAF73FDD61DBEC57580E608C2EA4D9AEB7E34E87B817E69CA0B10C69 | 7580 | no | 0 | 101 | 0 | n/a |
| feature-audit.2026-07-27T12-02.md | 769B6C49D1EA97D73063D3B9104242949E52339D66AC76209ECA3EAACEDF358E | 6421 | no | 0 | 80 | 0 | n/a |
| policy-audit.2026-07-27T12-02.md | 197CC89ED33B3D2755771C40FDF0D5DA51A08CE0554639F6BDE9AE57A3E8911B | 11082 | no | 0 | 163 | 0 | n/a |
