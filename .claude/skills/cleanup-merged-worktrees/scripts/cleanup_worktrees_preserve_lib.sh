#!/usr/bin/env bash
# cleanup_worktrees_preserve_lib.sh: sourceable preserve-file consolidation function
# group for the cleanup-worktrees tool. It consumes the `preserved_files[]` array of the
# manifest the cleanup-merged-worktrees skill writes, validates every record fail-closed,
# scans the incoming bytes for host-identifying tokens, re-derives each destination
# index's line-ending convention, and stages the surviving files onto the consolidation
# branch.
#
# Sourcing contract: this library defines functions only; it never runs work at source
# time, so the wrapper and the bats suites can source it without side effects. It MUST be
# sourced AFTER cleanup_worktrees_enumerate_lib.sh, cleanup_worktrees_lib.sh, and
# cleanup_worktrees_actions_lib.sh, because it calls cleanup_wt_git (defined in the
# enumeration library) and consolidation_worktree_path (defined in the actions library).
#
# Two-phase design. preserve_plan performs NO writes: it reads and validates the manifest
# and every record, verifies each source file exists, runs the host-token pre-pass,
# decides each index outcome, and emits the PRESERVE| and ACTION| records plus an internal
# plan stream. preserve_commit_plan is the only function that writes and is entered only
# when the pre-pass reported no match, which is what keeps the writing phase thin enough
# to leave in the coverage denominator rather than exclude from measurement.
#
# Seams. All git commands go through cleanup_wt_git (CLEANUP_WT_GIT_BIN); jq resolves
# through preserve_resolve_jq (CLEANUP_WT_JQ_BIN); the manifest path comes from
# CLEANUP_WT_MANIFEST_PATH, defaulting to
# artifacts/orchestration/cleanup-worktrees-manifest.json; the consolidation worktree
# resolves through consolidation_worktree_path (CLEANUP_WT_CONSOLIDATION_PATH).
#
# Nothing upstream is trusted. line_ending is always re-derived from the target's current
# bytes, and the local content scan runs on every record whose advisory result is `clean`.
# Both advisory values are compared against the local answer and a disagreement is
# reported, never obeyed.
#
# The source file is read with plain file I/O and never through git: the repository-root
# .claude/agent-memory tree is gitignored, so a git-mediated read would need a force flag.
# No cleanup_wt_git invocation in this library passes -f or --force ever.

preserve_resolve_jq() {
	# Echo the path of the jq binary to use, honoring the CLEANUP_WT_JQ_BIN override seam.
	#
	# Mirrors the resolution and 127 contract of cleanup_wt_git exactly: when
	# CLEANUP_WT_JQ_BIN is set to a non-empty value it must point to an existing
	# executable; an empty or nonexistent value is treated as missing and falls back to
	# `command -v jq`. When neither resolves, a diagnostic naming the tool is written to
	# stderr and the function returns 127 with no stdout, so a missing jq is a loud
	# failure and never a silent no-op that stages nothing while reporting success.
	#
	# `command -v` is a bash builtin, so this function needs no external tool on PATH and
	# the unresolvable case is reachable in a test by pointing PATH at a directory that
	# holds no jq.
	local override=${CLEANUP_WT_JQ_BIN:-}
	local jq_bin=""
	if [[ -n $override && -x $override ]]; then
		jq_bin=$override
	else
		jq_bin=$(command -v jq 2>/dev/null) || jq_bin=""
	fi
	if [[ -z $jq_bin ]]; then
		printf 'cleanup-worktrees: no jq binary resolved (CLEANUP_WT_JQ_BIN=%s)\n' "$override" >&2
		return 127
	fi
	printf '%s\n' "$jq_bin"
}

preserve_split_tsv() {
	# Split one tab-separated record into the global array PRESERVE_FIELDS, preserving
	# empty fields exactly.
	#
	# `IFS=$'\t' read -r ...` is deliberately NOT used here. Tab is an IFS *whitespace*
	# character, so read collapses runs of tabs and strips leading and trailing ones; a
	# record whose memory_index_line column is empty would then shift every later column
	# left by one and the evidence field would land in the pattern-set-id variable. This
	# splitter walks the string with parameter expansion instead, which treats an empty
	# field as a field.
	#
	# Args: $1 = the tab-separated record.
	local rest=${1-}
	PRESERVE_FIELDS=()
	while [[ $rest == *$'\t'* ]]; do
		PRESERVE_FIELDS+=("${rest%%$'\t'*}")
		rest=${rest#*$'\t'}
	done
	PRESERVE_FIELDS+=("$rest")
}

preserve_read_manifest() {
	# Emit one tab-separated record per preserved_files[] entry of the named manifest.
	#
	# Exactly one jq invocation does both jobs: it enforces the two top-level checks and
	# projects each record into the fixed 14-column order the rest of this library reads.
	# The columns are worktree_path, source_path, change_class, disposition, verdict,
	# target_path, memory_index_line_present, memory_index_line_is_null,
	# memory_index_line, line_ending, host_token_scan_type, host_token_scan_result,
	# host_token_scan_pattern_set_id, evidence.
	#
	# The key-presence and is-null columns exist because the upstream contract
	# distinguishes "key absent, skip the record" from "null is valid and means there is
	# no index line to carry", and the type column exists because a host_token_scan that
	# is present but is not an object must be refused rather than read. Tab is the
	# transport delimiter precisely because memory_index_line may legitimately contain a
	# pipe character.
	#
	# A jq parse failure and either top-level check failing are the same outcome: emit
	# ACTION|preserve-manifest|<manifest-path>|REJECTED, write a diagnostic to stderr, and
	# return 1 with nothing staged.
	#
	# Args: $1 = manifest path. Returns 0 on success, 1 on rejection, 127 when jq cannot
	# be resolved.
	local manifest=${1-}
	local jq_bin out="" rc=0
	jq_bin=$(preserve_resolve_jq) || return 127
	local filter='
if (.tool != "cleanup-merged-worktrees") or (.schema_version != 1) then
  error("manifest failed top-level validation")
else
  (.preserved_files // [])[]
  | [ (.worktree_path // ""), (.source_path // ""), (.change_class // ""),
      (.disposition // ""), (.verdict // ""), (.target_path // ""),
      (has("memory_index_line") | tostring), ((.memory_index_line == null) | tostring),
      (.memory_index_line // ""), (.line_ending // ""), (.host_token_scan | type),
      (if (.host_token_scan | type) == "object" then (.host_token_scan.result // "") else "" end),
      (if (.host_token_scan | type) == "object" then (.host_token_scan.pattern_set_id // "") else "" end),
      (.evidence // "") ]
  | @tsv
end'
	out=$("$jq_bin" -r "$filter" "$manifest") || rc=$?
	if ((rc != 0)); then
		printf 'ACTION|preserve-manifest|%s|REJECTED\n' "$manifest"
		printf 'cleanup-worktrees: manifest rejected (jq rc=%s): %s\n' "$rc" "$manifest" >&2
		return 1
	fi
	if [[ -n $out ]]; then
		printf '%s\n' "$out"
	fi
}

preserve_report_invalid() {
	# Report one record-validation violation and return 1.
	#
	# The result record travels on the existing ACTION| result channel so this library
	# introduces no second outcome vocabulary. The human-readable reason naming the
	# offending field goes to stderr, keeping the machine-parseable stream on stdout free
	# of prose.
	#
	# Args: $1 = target_path, which is legitimately empty for a record whose target_path
	# is itself the invalid field; $2 = the reason.
	printf 'ACTION|preserve-stage|%s|SKIPPED-INVALID\n' "${1-}"
	printf 'cleanup-worktrees: preserve record skipped (%s)\n' "${2-}" >&2
	return 1
}

preserve_relative_path_reason() {
	# Echo the reason the named value is not an acceptable repo-relative path, or nothing
	# at all when it is acceptable. Both the drive-letter and the leading-slash absolute
	# forms are rejected, because the manifest is authored on a Windows host and its
	# upstream example encodes worktree paths in drive-letter form.
	#
	# Args: $1 = field name, $2 = value.
	local name=${1-} val=${2-}
	if [[ -z $val ]]; then
		printf '%s is empty' "$name"
	elif cleanup_wt_is_absolute_path "$val"; then
		printf '%s is absolute: %s' "$name" "$val"
	elif [[ $val == ".." || $val == "../"* || $val == *"/../"* || $val == *"/.." ]]; then
		printf '%s contains a .. segment: %s' "$name" "$val"
	fi
}

preserve_validate_record() {
	# Validate one tab-separated manifest record fail-closed against the D4 field matrix.
	#
	# Every field is validated at consumption because the manifest is written by the skill's
	# editorial pass rather than by a script, and nothing upstream validates the
	# preserved_files[] array at all. Each violation skips the record and reports
	# ACTION|preserve-stage|<target-path>|SKIPPED-INVALID; the first violation in the chain
	# below wins, because one bad field skips the record whatever the others say.
	#
	# change_class is validated for vocabulary but does NOT change the staging action: an
	# untracked and a modified record are both copied to target_path and staged, stated
	# here so no later change invents a divergent path for one of them.
	#
	# host_token_scan is refused when it is absent, is not a JSON object, is missing either
	# member, or carries a result outside clean/tokens_present: a record whose scan metadata
	# cannot be read is one whose provenance is unknown, and staging it would defeat the
	# pre-pass. line_ending is deliberately NOT a skip condition, because it is advisory, it
	# describes the target rather than the record, and it is re-derived on the write path.
	#
	# Side effect, relied on by preserve_plan: the split fields are left in the global
	# array PRESERVE_FIELDS, so the caller reads the columns without splitting twice.
	#
	# Args: $1 = the tab-separated record. Returns 0 when the record is valid, 1 when it
	# was skipped and reported.
	preserve_split_tsv "${1-}"
	local wt=${PRESERVE_FIELDS[0]-} src=${PRESERVE_FIELDS[1]-} cls=${PRESERVE_FIELDS[2]-}
	local disp=${PRESERVE_FIELDS[3]-} verdict=${PRESERVE_FIELDS[4]-} tgt=${PRESERVE_FIELDS[5]-}
	local present=${PRESERVE_FIELDS[6]-} isnull=${PRESERVE_FIELDS[7]-}
	local scantype=${PRESERVE_FIELDS[10]-} scanresult=${PRESERVE_FIELDS[11]-}
	local scanpsid=${PRESERVE_FIELDS[12]-} ev=${PRESERVE_FIELDS[13]-}
	local verdicts=" DEAD_ONE_OFF ALREADY_SOLVED_ELSEWHERE STALE_OR_CONTRADICTED GENUINELY_NEW STILL_RELEVANT "
	local reason="" psrc ptgt
	psrc=$(preserve_relative_path_reason source_path "$src")
	ptgt=$(preserve_relative_path_reason target_path "$tgt")
	if ((${#PRESERVE_FIELDS[@]} != 14)); then
		reason="record carries ${#PRESERVE_FIELDS[@]} columns, expected 14"
	elif [[ -z $wt ]]; then
		reason="worktree_path is empty"
	elif [[ -n $psrc ]]; then
		reason=$psrc
	elif [[ -n $ptgt ]]; then
		reason=$ptgt
	elif [[ $cls != "untracked" && $cls != "modified" ]]; then
		reason="change_class out of vocabulary: $cls"
	elif [[ $disp != "PRESERVE" ]]; then
		reason="disposition is not PRESERVE: $disp"
	elif [[ $verdicts != *" $verdict "* ]]; then
		reason="verdict out of vocabulary: $verdict"
	elif [[ $present != "true" ]]; then
		reason="memory_index_line key is absent"
	elif [[ $isnull != "true" && $isnull != "false" ]]; then
		reason="memory_index_line is neither a string nor null"
	elif [[ $scantype != "object" ]]; then
		reason="host_token_scan is $scantype, expected an object"
	elif [[ -z $scanresult ]]; then
		reason="host_token_scan.result is absent"
	elif [[ $scanresult != "clean" && $scanresult != "tokens_present" ]]; then
		reason="host_token_scan.result out of vocabulary: $scanresult"
	elif [[ -z $scanpsid ]]; then
		reason="host_token_scan.pattern_set_id is absent"
	elif [[ -z $ev ]]; then
		reason="evidence is empty"
	fi
	if [[ -n $reason ]]; then
		preserve_report_invalid "$tgt" "$reason"
		return 1
	fi
	return 0
}

preserve_plan_one() {
	# Plan one manifest record. Performs no write of any kind.
	#
	# The PRESERVE| report record is emitted for every record that survives validation,
	# before any per-record outcome is known, because that record identifies the finding at
	# its origin exactly as DIRTY|<worktree-path>|<status-line> does. The destination
	# outcome travels separately on the ACTION| result channel.
	#
	# The source file is located by plain file I/O at <worktree_path>/<source_path> and is
	# never read through git: the repository-root .claude/agent-memory tree is gitignored,
	# so a git-mediated read would be refused or would need a force flag.
	#
	# Args: $1 = the tab-separated record. Returns 0 when the record was planned and 1 when
	# it was skipped, refused, or reported.
	preserve_validate_record "${1-}" || return 1
	local wt=${PRESERVE_FIELDS[0]} src=${PRESERVE_FIELDS[1]} verdict=${PRESERVE_FIELDS[4]}
	local tgt=${PRESERVE_FIELDS[5]}
	printf 'PRESERVE|%s|%s|%s\n' "$wt" "$src" "$verdict"
	local srcfile="$wt/$src"
	if [[ ! -f $srcfile ]]; then
		# Skip and report, contributing to exit 1. A missing source is never a hard stop:
		# the operator can still stage everything else and re-run once the file is found.
		printf 'ACTION|preserve-stage|%s|MISSING-SOURCE\n' "$tgt"
		printf 'cleanup-worktrees: source file not found in the named worktree: %s\n' "$srcfile" >&2
		return 1
	fi
	# The pattern-set identifier is advisory. A value this library does not own is reported
	# and changes nothing else, including the exit code: the local scan governs regardless.
	if [[ ${PRESERVE_FIELDS[12]} != "cleanup-wt-host-tokens-v1" ]]; then
		printf 'ACTION|preserve-scan|%s|PATTERN-SET-MISMATCH\n' "$tgt"
	fi
	# The host-token pre-pass. It reads only the source file's bytes and the index line's
	# bytes, and it runs for EVERY valid record before the writing phase is entered for any
	# of them, because a match anywhere refuses the whole pass rather than one record.
	#
	# An upstream result of tokens_present short-circuits to a refusal without scanning, as
	# a cheap fail-closed gate. An upstream result of `clean` does NOT skip the local scan:
	# the manifest is written by an editorial pass rather than by a script, so a `clean`
	# claim is re-derived rather than believed.
	if [[ ${PRESERVE_FIELDS[11]} == "tokens_present" ]]; then
		printf 'ACTION|preserve-stage|%s|HOST-TOKEN-BLOCKED\n' "$tgt"
		printf 'cleanup-worktrees: the manifest reports tokens_present for %s\n' "$srcfile" >&2
		PRESERVE_BLOCKED=1
		return 1
	fi
	if ! preserve_scan_host_tokens "$srcfile" "${PRESERVE_FIELDS[8]}"; then
		printf 'ACTION|preserve-stage|%s|HOST-TOKEN-BLOCKED\n' "$tgt"
		PRESERVE_BLOCKED=1
		return 1
	fi
	if cleanup_wt_git -C "$PRESERVE_CWT" check-ignore -q -- "$tgt"; then
		# Refuse without forcing. Forcing an ignored path would produce a commit that
		# appears to preserve the lesson while push-down never distributes it, because
		# only the tracked bundle mirror is ever distributed. No force flag is passed to
		# the staging call under any condition.
		printf 'ACTION|preserve-stage|%s|IGNORED-TARGET\n' "$tgt"
		printf 'cleanup-worktrees: destination is ignored, refusing to force: %s\n' "$tgt" >&2
		return 1
	fi
	# The index decision, including the line-ending re-derivation it rests on, lives in
	# the line-ending library. A memory_index_line of JSON null means the file is not a
	# memory entry: it is copied and staged, and no index is read, created, or appended to.
	local iaction="none" ipath="" iline="" iterm="lf" ipre="no"
	if [[ ${PRESERVE_FIELDS[7]} == "false" ]]; then
		iline=${PRESERVE_FIELDS[8]}
		preserve_plan_index "$tgt" "$iline" "${PRESERVE_FIELDS[9]}" "$PRESERVE_CWT" || return 1
		iaction=$PRESERVE_INDEX_ACTION
		ipath=$PRESERVE_INDEX_PATH
		iterm=$PRESERVE_INDEX_TERM
		ipre=$PRESERVE_INDEX_PRE
	fi
	local entry
	printf -v entry '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s' \
		"$tgt" "$srcfile" "$PRESERVE_CWT/$tgt" "$iaction" "$ipath" "$iline" "$iterm" "$ipre"
	PRESERVE_PLAN_STREAM+=("$entry")
	return 0
}

preserve_plan() {
	# Phase 1 of the two-phase driver: read, validate, decide, and report. It performs no
	# write of any kind, so a failure anywhere in it leaves the consolidation worktree
	# byte-unchanged and there is nothing to undo.
	#
	# Records are processed in LC_ALL=C order of the pair (worktree_path, source_path)
	# rather than in manifest array order, because the report stream's ordering is
	# normative and the manifest is authored by an editorial pass whose array order is not
	# a contract. The sort is field-aware rather than whole-line, so a worktree_path that
	# is a prefix of another cannot reorder the pair.
	#
	# The decisions are accumulated on two globals rather than returned, because a record's
	# outcome has to survive the loop: PRESERVE_PLAN_STREAM carries one tab-separated entry
	# per record that reached the writing phase, and PRESERVE_EXIT accumulates the
	# per-record skip signal that becomes the pass's exit code.
	#
	# Returns 0 when the pass may proceed to the writing phase, 1 when the manifest itself
	# was rejected, 3 on the host-token hard stop, and 127 when a required tool could not
	# be resolved. Per-record skips do NOT change this return value; they accumulate in
	# PRESERVE_EXIT and are surfaced by run_preserve.
	PRESERVE_PLAN_STREAM=()
	PRESERVE_EXIT=0
	PRESERVE_BLOCKED=0
	PRESERVE_CWT=""
	local manifest records sorted line rc=0
	manifest=${CLEANUP_WT_MANIFEST_PATH:-artifacts/orchestration/cleanup-worktrees-manifest.json}
	PRESERVE_CWT=$(consolidation_worktree_path) || return $?
	records=$(preserve_read_manifest "$manifest") || rc=$?
	if ((rc != 0)); then
		# preserve_read_manifest writes its REJECTED record to stdout, which the capture
		# above swallowed, so it is re-emitted here rather than lost.
		[[ -z $records ]] || printf '%s\n' "$records"
		return "$rc"
	fi
	[[ -n $records ]] || return 0
	sorted=$(printf '%s\n' "$records" | LC_ALL=C sort -t $'\t' -k1,1 -k2,2)
	while IFS= read -r line; do
		[[ -n $line ]] || continue
		preserve_plan_one "$line" || PRESERVE_EXIT=1
	done <<<"$sorted"
	# The hard stop is evaluated only after every record has been examined, so the pass
	# reports every match rather than only the first, and the writing phase is not entered
	# for any record. Nothing has been written at this point, so there is nothing to undo.
	((PRESERVE_BLOCKED == 0)) || return 3
	return 0
}

preserve_commit_plan() {
	# Phase 2: the only function in this library that writes anything.
	#
	# It consumes the plan stream phase 1 built and performs exactly four kinds of
	# operation: mkdir -p of the destination directory, a verbatim byte copy of the source,
	# the destination index append or creation, and the staging call. Every decision was
	# already taken in phase 1, so this function contains no policy and is thin enough to
	# leave in the coverage denominator rather than exclude from measurement.
	#
	# The copy is verbatim. No line-ending transformation is applied to the preserved
	# file's own content: transforming it would alter content the editorial pass judged
	# worth keeping, and the staging call re-normalizes tracked text anyway. The
	# line-ending obligation is scoped to the index append, where the defect occurred.
	#
	# A non-zero result from any of the three operations emits
	# ACTION|preserve-stage|<target-path>|FAILED and contributes to exit 1; it never aborts
	# the remaining records, because each record's write is independent of the others.
	local entry tgt srcfile dstfile iaction ipath iline iterm ipre fail
	((${#PRESERVE_PLAN_STREAM[@]} > 0)) || return 0
	for entry in "${PRESERVE_PLAN_STREAM[@]}"; do
		preserve_split_tsv "$entry"
		tgt=${PRESERVE_FIELDS[0]}
		srcfile=${PRESERVE_FIELDS[1]}
		dstfile=${PRESERVE_FIELDS[2]}
		iaction=${PRESERVE_FIELDS[3]}
		ipath=${PRESERVE_FIELDS[4]}
		iline=${PRESERVE_FIELDS[5]}
		iterm=${PRESERVE_FIELDS[6]}
		ipre=${PRESERVE_FIELDS[7]}
		fail=""
		if ! mkdir -p -- "${dstfile%/*}" 2>/dev/null; then
			fail="cannot create the destination directory"
		elif ! cat -- "$srcfile" >"$dstfile" 2>/dev/null; then
			fail="the verbatim byte copy failed"
		elif [[ $iaction != "none" ]] &&
			! preserve_render_index_append "$ipath" "$iline" "$iterm" "$ipre"; then
			fail="the index append failed"
		elif ! cleanup_wt_git -C "$PRESERVE_CWT" add -- "$tgt"; then
			fail="the staging call failed"
		elif [[ $iaction != "none" ]] && ! cleanup_wt_git -C "$PRESERVE_CWT" add -- "$ipath"; then
			fail="staging the index failed"
		fi
		if [[ -n $fail ]]; then
			printf 'ACTION|preserve-stage|%s|FAILED\n' "$tgt"
			printf 'cleanup-worktrees: %s: %s\n' "$fail" "$tgt" >&2
			PRESERVE_EXIT=1
			continue
		fi
		printf 'ACTION|preserve-stage|%s|OK\n' "$tgt"
	done
	return 0
}

run_preserve() {
	# Drive the preserve pass and return the exit code the arm's contract defines:
	#   0   every valid record staged, nothing skipped, no index created, no token match
	#   1   at least one record skipped, refused, or failed, or an index was created
	#   3   the host-token hard stop; the writing phase was not entered and nothing staged
	#   127 a required tool could not be resolved; nothing staged
	#
	# Two preconditions are checked before any record is read, and both fail closed. The
	# arm never creates the consolidation worktree: creating it would hide an operator
	# mistake behind a directory that looks correct and holds nothing.
	local cwt manifest rc=0
	manifest=${CLEANUP_WT_MANIFEST_PATH:-artifacts/orchestration/cleanup-worktrees-manifest.json}
	cwt=$(consolidation_worktree_path) || return $?
	if [[ ! -d $cwt ]]; then
		printf 'ACTION|preserve-stage||MISSING-WORKTREE\n'
		printf 'cleanup-worktrees: consolidation worktree does not exist: %s\n' "$cwt" >&2
		return 1
	fi
	if [[ ! -f $manifest || ! -r $manifest ]]; then
		printf 'ACTION|preserve-manifest|%s|MISSING\n' "$manifest"
		printf 'cleanup-worktrees: manifest is not an existing readable file: %s\n' "$manifest" >&2
		return 1
	fi
	preserve_plan || rc=$?
	if ((rc != 0)); then
		return "$rc"
	fi
	preserve_commit_plan || rc=$?
	if ((rc != 0)); then
		return "$rc"
	fi
	return "$PRESERVE_EXIT"
}

preserve_scan_host_tokens() {
	# Scan the incoming bytes for host-identifying tokens, implementing the pattern set
	# whose identifier this library owns: cleanup-wt-host-tokens-v1.
	#
	# Scope, narrow on purpose. Exactly two things are read: the bytes of the file named by
	# the (worktree_path, source_path) pair, and the bytes of the memory_index_line string
	# that will be appended. The manifest is NOT scanned, because its own worktree_path
	# values legitimately carry the very tokens refused in staged content; the destination
	# index, the consolidation branch, the repository tree, the push-down bundle, the
	# environment, and the worktree path are excluded for the same reason, since a
	# repository-wide scan would flag tracked files that must contain those strings.
	#
	# Matching is POSIX extended regular expression under LC_ALL=C, case-insensitive, with
	# the input treated as text, evaluated one pattern at a time in ascending identifier
	# order so the first match can be named in the diagnostic.
	#
	# Args: $1 = the source file path, $2 = the index line. Returns 0 when clean and 1 on a
	# match, with the matched identifier written to stderr.
	local srcfile=${1-} iline=${2-}
	local -a ids=(HT1 HT2 HT3 HT4 HT5 HT6)
	local -a patterns=(
		$'[A-Za-z]:[\\\\/]+users[\\\\/]+[^\\\\/[:space:]"\']+'
		$'/mnt/[a-z]/users/[^/[:space:]"\']+'
		'/home/[a-z][a-z0-9._-]*'
		$'[\\\\/][a-z0-9]{1,6}~[0-9][\\\\/]'
		'[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}'
		'%(userprofile|username|computername|homepath)%'
	)
	local i id pattern
	for i in "${!ids[@]}"; do
		id=${ids[$i]}
		pattern=${patterns[$i]}
		if printf '%s' "$iline" | LC_ALL=C grep -a -i -E -q -e "$pattern"; then
			printf 'cleanup-worktrees: host token matched (%s) in the index line\n' "$id" >&2
			return 1
		fi
		if [[ -f $srcfile ]] && LC_ALL=C grep -a -i -E -q -e "$pattern" -- "$srcfile"; then
			printf 'cleanup-worktrees: host token matched (%s) in %s\n' "$id" "$srcfile" >&2
			return 1
		fi
	done
	return 0
}
