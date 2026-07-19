# AC-2 Schema Structure Review

- Timestamp: 2026-07-16T15-56
- Issue: #340

## Key-by-key checklist (single `nuget` `updates:` entry)

| Key | Observed value/type | Present |
|---|---|---|
| `package-ecosystem` | `"nuget"` (string) | Yes |
| `directories` | `["/*"]` (list of one string) | Yes |
| `schedule` | mapping with `interval: "weekly"` | Yes |
| `open-pull-requests-limit` | `10` (integer) | Yes |
| `groups` | mapping with 4 sub-keys, each a mapping with `patterns` (list) + `group-by` (string) | Yes |
| `ignore` | list of 8 mappings, each with `dependency-name` (string) + `update-types` (list of one string) | Yes |

All six required keys are present, correctly typed, and match the Dependabot v2 options reference structure cited in research §2/§4/§5.

Output Summary: all 6 required keys present and correctly typed
