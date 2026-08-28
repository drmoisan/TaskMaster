#!/usr/bin/env bash
# cleanup_worktrees_lib.sh: sourceable function library for the cleanup-worktrees
# tool. Provides the full classification ladder (ancestry -> content-neutral ->
# cherry-equivalence -> rename-aware blob fallback -> unique-residual selection) and
# the report driver. The git-binary test seam (cleanup_wt_git), branch/worktree
# enumeration (enumerate_branches, parse_worktree_list), path normalization
# (normalize_wt_path), the current-worktree/branch protection set (compute_protected),
# and the main-freshness warning (check_main_freshness) now live in the sibling file
# cleanup_worktrees_enumerate_lib.sh, which MUST be sourced before this file.
# Consolidation, deletion, and the CLI live in cleanup_worktrees_actions_lib.sh and
# cleanup-worktrees.sh. Splitting the library keeps every file within the 500-line cap.
#
# Sourcing contract: this library defines functions only; it never runs work at
# source time, so the wrapper and the bats suites can source it without side effects.
#
# It depends on cleanup_worktrees_enumerate_lib.sh (source that first). All git
# commands go through cleanup_wt_git so tests can stub the git binary via
# CLEANUP_WT_GIT_BIN.
#
# Git exit-code capture rule: a git command with an EXPECTED non-zero exit in the
# ladder (merge-base --is-ancestor, diff --quiet) is captured at the function-body
# level with `|| rc=$?` and mapped to a verdict token. A git command whose output is
# ITERATED or whose exit code is authoritative (cherry, diff-tree, ls-tree, rev-list)
# is captured in the PARENT shell (`out=$(cmd) || rc=$?`, then iterated over
# `<<<"$out"`) so a non-zero exit is observed rather than lost inside a process
# substitution or a pipeline. Internal hard-error verdict tokens (echo-verdict
# contract, the callee returns 0; classify_branch maps each to the report state):
#   CHERRY_ERROR    - non-zero `git cherry` exit (classify_cherry_equivalent).
#   DIFF_TREE_ERROR - non-zero `git diff-tree` exit while probing a `+` commit's diff
#                     for the empty-residual case (classify_cherry_equivalent).
#   RESIDUAL_ERROR  - non-zero exit of the name-status `git diff-tree` read or the
#                     D-rung `git ls-tree` probe (classify_residual_commit).
#   MINUS_PRESENT   - a `- <sha>` cherry line was seen (partial-merge signal); read by
#                     classify_branch from the captured verdict instead of a second
#                     `git cherry` invocation whose exit code would be discarded.
# A hard failure of any enumeration/protection/cherry/diff-tree/ls-tree/rev-list read
# maps to the BRANCH|<name>|ANCESTRY_ERROR report state and a non-zero return. A hard
# git failure never resolves to a MERGED_* verdict.
#
# Report line contract (LC_ALL=C ordered, pipe-delimited, one record per line):
#   BRANCH|<name>|<state>
#   COMMIT|<branch>|<sha>|<state>|<paths-csv>|<author>|<author-date>
#   WORKTREE|<path>|<branch-or-DETACHED>|<flags>
#   WARN|main-divergence|<local-sha>|<origin-sha>
#   DIRTY|<worktree-path>|<status-porcelain-line>
#   ACTION|<verb>|<target>|<result>   (apply mode only; emitted by the actions lib)
# Branch states: NOT_MERGED | MERGED_CLEAN | MERGED_CONTENT_NEUTRAL |
#   MERGED_EQUIVALENT | HAS_UNIQUE_RESIDUALS | PROTECTED_CURRENT; ANCESTRY_ERROR is a
#   hard failure. Per-commit states: EQUIVALENT | CONTENT_ON_MAIN | EMPTY | UNIQUE |
#   CONFLICT.

classify_ancestry() {
	# First ladder rung: is <tip> an ancestor of main?
	#
	# Wraps `git merge-base --is-ancestor <tip> main`, capturing the exit code with
	# `|| rc=$?` so set -e does not abort on the expected non-zero. Echoes exactly one
	# verdict token and always returns 0:
	#   exit 0   -> MERGED_CLEAN   (delete-eligible; zero residual commits)
	#   exit 1   -> NOT_ANCESTOR   (continue the ladder)
	#   exit > 1 -> ANCESTRY_ERROR (hard failure for the branch; never "not merged")
	#
	# Args: $1 = branch tip (name or sha).
	local tip="$1" rc=0
	cleanup_wt_git merge-base --is-ancestor "$tip" main >/dev/null 2>&1 || rc=$?
	if ((rc == 0)); then
		printf 'MERGED_CLEAN\n'
	elif ((rc == 1)); then
		printf 'NOT_ANCESTOR\n'
	else
		printf 'ANCESTRY_ERROR\n'
	fi
	return 0
}

classify_content_neutral() {
	# Second ladder rung: does the branch add no net content versus main?
	#
	# Runs `git diff --quiet main...<branch>` (three-dot: merge-base(main,branch) ->
	# branch tip) with `|| rc=$?`. This short-circuit runs BEFORE any per-commit
	# `git cherry` analysis and catches revert-pairs (a commit and its later revert
	# on the same branch net to nothing). Echoes exactly one verdict token and always
	# returns 0:
	#   exit 0   -> MERGED_CONTENT_NEUTRAL (delete-eligible)
	#   exit 1   -> NOT_NEUTRAL            (continue the ladder)
	#   exit > 1 -> CONTENT_NEUTRAL_ERROR  (hard failure for the branch)
	#
	# Args: $1 = branch name.
	local branch="$1" rc=0
	cleanup_wt_git diff --quiet "main...$branch" >/dev/null 2>&1 || rc=$?
	if ((rc == 0)); then
		printf 'MERGED_CONTENT_NEUTRAL\n'
	elif ((rc == 1)); then
		printf 'NOT_NEUTRAL\n'
	else
		printf 'CONTENT_NEUTRAL_ERROR\n'
	fi
	return 0
}

classify_cherry_equivalent() {
	# Third ladder rung: patch-id equivalence via `git cherry main <branch>`.
	#
	# `git cherry` emits `- <sha>` when an equivalent patch already exists on main and
	# `+ <sha>` when none does. A `+` commit whose diff is empty (verified with
	# `git diff-tree --no-commit-id -r <sha>` producing no output) is also counted as
	# equivalent (droppable empty commit). When every residual is equivalent the branch
	# is content-equivalent-merged.
	#
	# Output contract (one token per line; always returns 0):
	#   - if all residuals are equivalent: a single line `MERGED_EQUIVALENT`
	#   - otherwise: a `MINUS_PRESENT` line first (only when any `- <sha>` cherry line
	#     was seen), then one `RESIDUAL <sha>` line per remaining `+` commit for the
	#     blob-level tier (classify_residual_commit). classify_branch reads MINUS_PRESENT
	#     from this captured verdict instead of re-invoking `git cherry`.
	#   - on a hard `git cherry` failure (non-zero exit): a single line `CHERRY_ERROR`.
	#     `git cherry` has no expected non-zero in this ladder, so any non-zero exit is
	#     a hard failure; the caller (classify_branch) maps CHERRY_ERROR to the
	#     ANCESTRY_ERROR report state and never to MERGED_EQUIVALENT.
	#   - on a hard `git diff-tree` failure while probing a `+` commit's diff (non-zero
	#     exit): a single line `DIFF_TREE_ERROR`. diff-tree has no expected non-zero
	#     here, so any non-zero exit is a hard failure; the caller maps DIFF_TREE_ERROR
	#     to ANCESTRY_ERROR and never to MERGED_EQUIVALENT. An empty diff-tree output
	#     with exit 0 remains the legitimate droppable empty-commit case.
	#
	# Args: $1 = branch name.
	local branch="$1" rc=0 line marker sha out minus_seen=0
	local -a residuals=()
	# Guarded parent-shell capture: a non-zero `git cherry` exit is observed here and
	# reported as CHERRY_ERROR before any residual processing, so an empty/failed cherry
	# result cannot be mistaken for "all residuals equivalent".
	out=$(cleanup_wt_git cherry main "$branch") || rc=$?
	if ((rc != 0)); then
		printf 'CHERRY_ERROR\n'
		return 0
	fi
	while IFS= read -r line || [[ -n $line ]]; do
		[[ -z $line ]] && continue
		marker=${line%% *}
		sha=${line#* }
		# `-` lines are patch-id equivalent: nothing to carry forward, but record that a
		# partial-merge signal exists so classify_branch need not re-invoke `git cherry`.
		if [[ $marker == "-" ]]; then
			minus_seen=1
			continue
		fi
		if [[ $marker == "+" ]]; then
			local dt="" dtrc=0
			# Guarded parent-shell capture: a non-zero diff-tree exit is a hard failure
			# reported as DIFF_TREE_ERROR, never mistaken for an empty (droppable) diff.
			dt=$(cleanup_wt_git diff-tree --no-commit-id -r "$sha") || dtrc=$?
			if ((dtrc != 0)); then
				printf 'DIFF_TREE_ERROR\n'
				return 0
			fi
			# An empty-diff residual commit is droppable; treat as equivalent.
			[[ -z $dt ]] && continue
			residuals+=("$sha")
		fi
	done <<<"$out"
	if ((${#residuals[@]} == 0)); then
		printf 'MERGED_EQUIVALENT\n'
	else
		((minus_seen == 1)) && printf 'MINUS_PRESENT\n'
		for sha in "${residuals[@]}"; do
			printf 'RESIDUAL %s\n' "$sha"
		done
	fi
	return 0
}

_blob_equal() {
	# Return 0 iff <ref-a>:<path> and <ref-b>:<path> resolve to the same blob OID.
	#
	# A rev-parse failure for either side (e.g. the path is absent on that ref) is
	# treated as "not equal" (return 1) so an added/removed path is never mistaken for
	# equivalent content.
	#
	# Args: $1 = ref-a, $2 = ref-b, $3 = path.
	local ref_a="$1" ref_b="$2" path="$3" oid_a oid_b
	oid_a=$(cleanup_wt_git rev-parse "$ref_a:$path" 2>/dev/null) || return 1
	oid_b=$(cleanup_wt_git rev-parse "$ref_b:$path" 2>/dev/null) || return 1
	[[ -n $oid_a && $oid_a == "$oid_b" ]]
}

classify_residual_commit() {
	# Fourth ladder rung: rename-aware blob-OID comparison for one `+` commit.
	#
	# Enumerates touched paths via
	#   git diff-tree --no-commit-id --name-status -r -M <sha>
	# and decides per path whether the branch's content already exists on main:
	#   A / M : compare blob OIDs of <branch>:<path> vs main:<path>; differ or absent
	#           on main -> unique.
	#   D     : probe `git ls-tree main -- <path>` (guarded capture). Non-empty output
	#           means the path is still present on main -> the branch's deletion is
	#           unique work; empty output (exit 0) means the path is also absent on main
	#           -> droppable. A non-zero ls-tree exit is a hard failure -> RESIDUAL_ERROR.
	#   Rnnn  : compare blob OIDs at the NEW path (rename target).
	# All touched paths equivalent -> CONTENT_ON_MAIN; any unique path -> UNIQUE with
	# the comma-separated unique path list.
	#
	# Output contract (single line; always returns 0):
	#   CONTENT_ON_MAIN
	#   UNIQUE|<path1,path2,...>
	#   RESIDUAL_ERROR   (hard git failure of the name-status diff-tree read or the
	#                     D-rung ls-tree probe; never resolved to CONTENT_ON_MAIN/UNIQUE)
	#
	# Args: $1 = branch name, $2 = commit sha.
	local branch="$1" sha="$2" status p1 p2 relpath out dtrc=0
	local -a unique_paths=()
	# Guarded parent-shell capture: a non-zero name-status diff-tree exit is a hard
	# failure reported as RESIDUAL_ERROR before any path processing, never resolved to
	# CONTENT_ON_MAIN or UNIQUE.
	out=$(cleanup_wt_git diff-tree --no-commit-id --name-status -r -M "$sha") || dtrc=$?
	if ((dtrc != 0)); then
		printf 'RESIDUAL_ERROR\n'
		return 0
	fi
	while IFS=$'\t' read -r status p1 p2 || [[ -n $status ]]; do
		[[ -z $status ]] && continue
		case "$status" in
		A | M)
			relpath="$p1"
			_blob_equal "$branch" main "$relpath" || unique_paths+=("$relpath")
			;;
		D)
			relpath="$p1"
			# Guarded ls-tree probe: a hard git failure (RESIDUAL_ERROR) is distinct from a
			# path legitimately absent on main (droppable). rev-parse's exit code cannot
			# make this distinction, so use ls-tree's stdout: non-empty -> present on main
			# (the deletion is unique work); empty with exit 0 -> absent (droppable).
			local lsout="" lsrc=0
			lsout=$(cleanup_wt_git ls-tree main -- "$relpath") || lsrc=$?
			if ((lsrc != 0)); then
				printf 'RESIDUAL_ERROR\n'
				return 0
			fi
			[[ -n $lsout ]] && unique_paths+=("$relpath")
			;;
		R*)
			relpath="$p2"
			_blob_equal "$branch" main "$relpath" || unique_paths+=("$relpath")
			;;
		esac
	done <<<"$out"
	if ((${#unique_paths[@]} == 0)); then
		printf 'CONTENT_ON_MAIN\n'
	else
		local IFS=,
		printf 'UNIQUE|%s\n' "${unique_paths[*]}"
	fi
	return 0
}

select_cherry_pick_candidates() {
	# Emit one COMMIT record per UNIQUE residual commit, oldest-first.
	#
	# The non-equivalent `+` residual set comes from classify_cherry_equivalent; the
	# oldest-first order and per-commit author/author-date come from
	#   git rev-list --reverse --no-merges --format='%H|%an|%aI' main..<branch>
	# (`--reverse` = application order for cherry-picking; `--no-merges` excludes merge
	# commits; the `commit <sha>` header lines from --format are dropped). Emits, per
	# UNIQUE residual: COMMIT|<branch>|<sha>|UNIQUE|<paths-csv>|<author>|<author-date>.
	#
	# Args: $1 = branch name.
	local branch="$1" line sha author date verdict paths restline rc=0
	local ceout cerc=0 rlout
	local -A is_residual=()
	# Defensive re-check of the cherry verdict: a CHERRY_ERROR (or a non-zero return)
	# must abort with a non-zero status rather than proceed with an empty residual map.
	ceout=$(classify_cherry_equivalent "$branch") || cerc=$?
	if ((cerc != 0)) || [[ $ceout == "CHERRY_ERROR" || $ceout == "DIFF_TREE_ERROR" ]]; then
		return 2
	fi
	while IFS= read -r line; do
		[[ $line == RESIDUAL\ * ]] || continue
		is_residual[${line#RESIDUAL }]=1
	done <<<"$ceout"
	# Guarded parent-shell capture: a rev-list hard failure returns git's exit code
	# with no COMMIT line emitted, so a candidate is never silently fabricated or
	# dropped as a success.
	rlout=$(cleanup_wt_git rev-list --reverse --no-merges --format='%H|%an|%aI' "main..$branch") || rc=$?
	if ((rc != 0)); then
		printf 'cleanup-worktrees: git rev-list failed for %s (rc=%s)\n' "$branch" "$rc" >&2
		return "$rc"
	fi
	while IFS= read -r line || [[ -n $line ]]; do
		[[ $line == commit\ * ]] && continue
		[[ -z $line ]] && continue
		sha=${line%%|*}
		restline=${line#*|}
		author=${restline%%|*}
		date=${restline#*|}
		[[ -n ${is_residual[$sha]:-} ]] || continue
		verdict=$(classify_residual_commit "$branch" "$sha")
		if [[ $verdict == "RESIDUAL_ERROR" ]]; then
			# A residual hard error must abort candidate selection with a non-zero status
			# rather than silently drop or fabricate a COMMIT record.
			return 2
		fi
		if [[ $verdict == UNIQUE\|* ]]; then
			paths=${verdict#UNIQUE|}
			printf 'COMMIT|%s|%s|UNIQUE|%s|%s|%s\n' "$branch" "$sha" "$paths" "$author" "$date"
		fi
	done <<<"$rlout"
	return "$rc"
}

classify_branch() {
	# Orchestrate the full classification ladder for one branch and emit its pinned
	# report lines (BRANCH first, then COMMIT records for unique residuals oldest-first).
	# No commit-message text is ever consulted. Ladder order (spec):
	#   1. PROTECTED_CURRENT exclusion (branch-name OR worktree-path match; main
	#      worktree always protected).
	#   2. ancestry -> MERGED_CLEAN, or ANCESTRY_ERROR (hard fail).
	#   3. content-neutral short-circuit -> MERGED_CONTENT_NEUTRAL.
	#   4. cherry patch-id equivalence -> MERGED_EQUIVALENT when all residuals equiv.
	#   5. rename-aware blob fallback: residuals all CONTENT_ON_MAIN -> MERGED_EQUIVALENT.
	#   6. remaining unique residuals -> HAS_UNIQUE_RESIDUALS when the branch was
	#      partially incorporated on main (a cherry `-` line or a CONTENT_ON_MAIN
	#      residual), else NOT_MERGED (purely unmerged code).
	#
	# Args: $1 = branch name. Returns 0 normally; 2 on any hard error surfaced by the
	# enumeration/protection reads (worktree-list, protected-set), the cherry rung
	# (CHERRY_ERROR), the empty-residual diff-tree probe (DIFF_TREE_ERROR), the
	# name-status/ls-tree residual reads (RESIDUAL_ERROR), or rev-list candidate
	# selection. A hard git failure always maps to the ANCESTRY_ERROR report state and a
	# non-zero return, never a MERGED verdict.
	local name="$1"
	local -A prot_branch=() prot_path=()
	local pline cpout cprc=0
	# Guarded parent-shell capture of the protected set: a compute_protected hard
	# failure (a git worktree-list failure underneath) must map to ANCESTRY_ERROR, not
	# a weakened/empty protected set that could let a real branch reach a MERGED verdict.
	cpout=$(compute_protected) || cprc=$?
	if ((cprc != 0)); then
		printf 'BRANCH|%s|ANCESTRY_ERROR\n' "$name"
		return 2
	fi
	while IFS= read -r pline; do
		case "$pline" in
		protected-branch\|*) prot_branch[${pline#protected-branch|}]=1 ;;
		protected-path\|*) prot_path[${pline#protected-path|}]=1 ;;
		esac
	done <<<"$cpout"
	# Locate this branch's worktree path (normalized), if any, from a guarded capture
	# of the worktree list; a hard failure here likewise maps to ANCESTRY_ERROR.
	local wt_norm="" record wpath wbranch wlout wlrc=0
	wlout=$(parse_worktree_list) || wlrc=$?
	if ((wlrc != 0)); then
		printf 'BRANCH|%s|ANCESTRY_ERROR\n' "$name"
		return 2
	fi
	while IFS= read -r record; do
		[[ -z $record ]] && continue
		IFS='|' read -r wpath _ wbranch _ <<<"$record"
		if [[ $wbranch == "$name" ]]; then
			wt_norm=$(normalize_wt_path "$wpath")
			break
		fi
	done <<<"$wlout"
	if [[ -n ${prot_branch[$name]:-} ]] || { [[ -n $wt_norm && -n ${prot_path[$wt_norm]:-} ]]; }; then
		printf 'BRANCH|%s|PROTECTED_CURRENT\n' "$name"
		return 0
	fi
	local v
	v=$(classify_ancestry "$name")
	case "$v" in
	MERGED_CLEAN)
		printf 'BRANCH|%s|MERGED_CLEAN\n' "$name"
		return 0
		;;
	ANCESTRY_ERROR)
		printf 'BRANCH|%s|ANCESTRY_ERROR\n' "$name"
		return 2
		;;
	esac
	v=$(classify_content_neutral "$name")
	case "$v" in
	MERGED_CONTENT_NEUTRAL)
		printf 'BRANCH|%s|MERGED_CONTENT_NEUTRAL\n' "$name"
		return 0
		;;
	CONTENT_NEUTRAL_ERROR)
		printf 'BRANCH|%s|ANCESTRY_ERROR\n' "$name"
		return 2
		;;
	esac
	local ce
	ce=$(classify_cherry_equivalent "$name")
	if [[ $ce == "CHERRY_ERROR" || $ce == "DIFF_TREE_ERROR" ]]; then
		# Hard `git cherry` or `git diff-tree` failure: same mapping as
		# CONTENT_NEUTRAL_ERROR (ANCESTRY_ERROR report state, hard-fail return 2).
		printf 'BRANCH|%s|ANCESTRY_ERROR\n' "$name"
		return 2
	fi
	if [[ $ce == "MERGED_EQUIVALENT" ]]; then
		printf 'BRANCH|%s|MERGED_EQUIVALENT\n' "$name"
		return 0
	fi
	# Residual `+` commits exist; resolve each via the blob tier.
	local sha verdict unique_count=0 content_count=0
	while IFS= read -r pline; do
		[[ $pline == RESIDUAL\ * ]] || continue
		sha=${pline#RESIDUAL }
		verdict=$(classify_residual_commit "$name" "$sha")
		if [[ $verdict == "RESIDUAL_ERROR" ]]; then
			# Hard failure of the name-status diff-tree read or the D-rung ls-tree probe:
			# map to ANCESTRY_ERROR before any state line is emitted.
			printf 'BRANCH|%s|ANCESTRY_ERROR\n' "$name"
			return 2
		fi
		if [[ $verdict == UNIQUE\|* ]]; then
			unique_count=$((unique_count + 1))
		elif [[ $verdict == "CONTENT_ON_MAIN" ]]; then
			content_count=$((content_count + 1))
		fi
	done < <(printf '%s\n' "$ce")
	if ((unique_count == 0)); then
		# Every residual was content-on-main; the branch is content-equivalent-merged.
		printf 'BRANCH|%s|MERGED_EQUIVALENT\n' "$name"
		return 0
	fi
	# Partial-merge signal: was any part of this branch already incorporated on main?
	# Derived from the already-captured cherry verdict (MINUS_PRESENT token) instead of a
	# second `git cherry` invocation whose exit code would be discarded in a pipeline.
	local minus_present=0
	[[ $ce == *MINUS_PRESENT* ]] && minus_present=1
	local state="NOT_MERGED"
	if ((minus_present == 1)) || ((content_count > 0)); then
		state="HAS_UNIQUE_RESIDUALS"
	fi
	printf 'BRANCH|%s|%s\n' "$name" "$state"
	if [[ $state == "HAS_UNIQUE_RESIDUALS" ]]; then
		# Propagate a rev-list/cherry hard failure: the BRANCH line already names a
		# non-delete-eligible state, and the non-zero return signals the hard error.
		local scrc=0
		select_cherry_pick_candidates "$name" || scrc=$?
		if ((scrc != 0)); then
			return 2
		fi
	fi
	return 0
}

run_report() {
	# Report-mode driver: emit the deterministic report with no mutation of any kind.
	# Emission order: WARN (freshness) first, then WORKTREE registrations, then the
	# per-branch BRANCH/COMMIT lines with branches taken in enumerate_branches'
	# LC_ALL=C order. Returns the maximum classify_branch return code (non-zero when
	# any branch reported ANCESTRY_ERROR). A worktree-list OR enumerate-branches hard
	# failure aborts the report before any line is emitted and returns git's non-zero
	# exit code, so a git failure never resolves to a partial, misleading report.
	local rc=0 crc name record wpath wbranch wflags wlout wlrc=0 ebout ebrc=0
	# Guarded parent-shell captures up front, before any output: a hard failure of either
	# read aborts the report before any WARN/WORKTREE/BRANCH line.
	wlout=$(parse_worktree_list) || wlrc=$?
	if ((wlrc != 0)); then
		return "$wlrc"
	fi
	ebout=$(enumerate_branches) || ebrc=$?
	if ((ebrc != 0)); then
		return "$ebrc"
	fi
	check_main_freshness
	while IFS= read -r record; do
		[[ -z $record ]] && continue
		IFS='|' read -r wpath _ wbranch wflags <<<"$record"
		printf 'WORKTREE|%s|%s|%s\n' "$wpath" "$wbranch" "$wflags"
	done <<<"$wlout"
	while read -r name _; do
		[[ -z $name ]] && continue
		crc=0
		classify_branch "$name" || crc=$?
		if ((crc > rc)); then
			rc=$crc
		fi
	done <<<"$ebout"
	return "$rc"
}
