# P5 collapsed-readiness harness analyzer restart gate

Timestamp: `2026-07-22T08:24:44Z`

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: `0`

Output Summary: `PASS. The ordered restart analyzer build completed with zero errors. Five pre-existing System.Reactive packages.config compatibility warnings remained; no analyzer or compiler diagnostic was introduced.`

- MSBuild: `18.8.2+ce25c0108`
- Configuration: `Debug|Any CPU`
- Errors: `0`
- Existing package-target warnings: `5`
- Elapsed time: `00:00:01.21`
- Authorized test-file SHA-256: `B53E9E091C461F835D900A8FB5DE0DB6B02080645EDFE18ACD0541E6E95E68F6`
