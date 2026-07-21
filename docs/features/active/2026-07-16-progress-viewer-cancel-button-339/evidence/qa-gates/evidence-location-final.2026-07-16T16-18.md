# Final Evidence Location Verification

Timestamp: 2026-07-16T16-18

Command: `pwsh -NoProfile -Command '& { $patterns="^artifacts/(baselines?|qa|qa-gates|evidence|coverage|regression-testing|post-change)/"; $bad=@(git diff --name-only bump-release | Where-Object { $_ -match $patterns }); "FORBIDDEN_EVIDENCE_PATH_COUNT=$($bad.Count)"; $bad; if($bad.Count -ne 0){exit 1}; $validator=@(Get-ChildItem -Recurse -File -Filter validate_evidence_locations.py -ErrorAction SilentlyContinue); "VALIDATOR_SCRIPT_COUNT=$($validator.Count)"; if($validator.Count -gt 0){python $validator[0].FullName --root .; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}} }'`

EXIT_CODE: 0

Output Summary:

FORBIDDEN_EVIDENCE_PATH_COUNT=0
VALIDATOR_SCRIPT_COUNT=0
No canonical evidence-location validator script was present, so no additional script execution was applicable.
