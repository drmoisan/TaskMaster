#!/usr/bin/env bash
# cleanup_worktrees_dirt_lib.sh: sourceable dirt-classification function group for the
# cleanup-worktrees tool. Provides the bounded staged-tree probe
# (dirt_staged_tree_commit), the per-entry precedence ladder (classify_dirt_entry), the
# per-worktree driver that emits the DIRTFILE|/DIRTSUM| records
# (classify_worktree_dirt), and the opt-in clearing sequence (clear_disposable_dirt).
#
# Sourcing contract: this library defines functions and constants only; it never runs
# work at source time, so the wrapper and the bats suites can source it without side
# effects. It depends on cleanup_wt_git from cleanup_worktrees_enumerate_lib.sh, which
# MUST be sourced before this file. All git commands go through cleanup_wt_git so tests
# can stub the git binary via CLEANUP_WT_GIT_BIN.
#
# READ-ONLY GUARANTEE. Classification performs no index write and no object-database
# write. It reaches its staged-tree answer by comparing the existing index against
# candidate commits with `diff-index --cached --quiet`, never by materialising a tree,
# and it never redirects the index to an alternate file through the environment. Report
# mode is therefore non-mutating, which is the property
# tests/shell/test_cleanup_worktrees_dirt_clear.bats asserts by searching the stub's
# argv and environment log. Do not add a probe that writes.
#
# ONE-SIDED BOUND ASYMMETRY. Every bound and every match in this file fails in the safe
# direction. A missed match degrades an entry to UNIQUE, which makes the worktree
# HAS_UNIQUE and REFUSES the clear; the worktree is then left in place for manual
# triage, which is the pre-existing behavior. A spurious match would do the opposite and
# clear content that exists nowhere else. Every non-zero exit that carries no defined
# negative answer is therefore mapped to UNIQUE rather than advanced past.
#
# NO --ignored ON THE STATUS READ. The status read must not be given --ignored. Ignored
# files are never classified, so they are never cleared, and `clean -fd` without -x
# leaves them in place. Adding --ignored would bring build output and local tooling
# state into the classified set and, for any entry that then matched a disposable rung,
# into the cleared set. Do not add it.
#
# Git exit-code capture rule, following the sibling libraries: every git-backed read
# whose output or exit code is authoritative is captured in the PARENT shell
# (`out=$(cmd) || rc=$?`), never inside a pipeline or a process substitution, so a
# non-zero exit is observed rather than lost.
#
# TWO-WAY CLASSIFICATION OF NON-ZERO EXITS. A probe whose non-zero exit is its DEFINED
# NEGATIVE ANSWER advances the ladder: `rev-parse main:<path>` exiting non-zero means
# the path is absent from main, and `diff --quiet main -- <path>` exiting 1 means the
# contents differ. Both are rung-4 misses. A probe whose non-zero exit carries NO
# verdict is a hard read failure and maps the entry to UNIQUE: `status --porcelain`,
# `hash-object`, `log --find-object`, and `diff-index --cached --quiet` exiting above 1.
# Collapsing the two into one blanket rule would make CONTENT_IN_HISTORY unreachable.
#
# EMPTY PATHSPEC IS NOT A MATCH. `diff --quiet main -- <path>` exits 0 for two distinct
# reasons: the compared content is identical, and the pathspec selected no paths at all.
# Only the first supports a disposable verdict, so rung 4's tracked positive answer is
# conditional. The reachable case is an AD entry, added to the index and then removed
# from the working tree, whose content exists only as a staged blob; reading its exit 0
# as a match would clear content held nowhere else. The answer is therefore gated on
# `rev-parse --verify --quiet main:<path>`, which asks whether main has content at that
# path to be equal to. A non-zero answer emits nothing and advances the ladder to the
# hash-object guard. This is the vacuous confinement rung 3 already guards against.
#
# INDEX AND WORKING TREE ARE TWO LOCATIONS. M, A, R, C, T, U are content-bearing: each
# says that side holds a blob (T is a typechange, U holds content at index stages 2 and
# 3); a space, ?, ! or D does not. Two content-bearing columns mean TWO distinct blobs,
# and rung 4's tracked half and rung 5 read only the working-tree one, so both fail
# closed there. Rung 3 is not gated: dirt_is_build_artifact reads the cached diff too.
#
# Record contract (pipe-delimited, one record per line; the file path is LAST so a path
# containing the delimiter is still recoverable):
#   DIRTFILE|<worktree-path>|<verdict>|<detail>|<status-code>|<file-path>
#   DIRTSUM|<worktree-path>|ALL_DISPOSABLE|HAS_UNIQUE|<detail>
# <verdict> is one of DISPOSABLE_BUILD_ARTIFACT, DISPOSABLE_SESSION_ARTIFACT,
# CONTENT_ON_MAIN, CONTENT_IN_HISTORY, STAGED_TREE_IS_COMMIT, UNIQUE. <detail> carries a
# commit sha for STAGED_TREE_IS_COMMIT and CONTENT_IN_HISTORY and is empty otherwise.
#   ACTION|dirt-clear|<worktree-path>|OK|REFUSED-UNIQUE|FAILED

# Opt-in gate for the destructive clearing path. Defaulted through parameter expansion
# so the bats suites can drive these functions without the CLI wrapper.
CLEANUP_WT_CLEAR_DISPOSABLE=${CLEANUP_WT_CLEAR_DISPOSABLE:-0}

# Upper bound on how far back the staged-tree probe walks from HEAD. One worktree in the
# 2026-09-06 run carried 86 staged changes that were exactly an earlier commit.
CLEANUP_WT_STAGED_TREE_DEPTH=200

# Upper bound on the history window scanned for a blob. Exceeding the available history
# is not an error: the probe falls back to the full `main` ref.
CLEANUP_WT_HISTORY_SCAN_DEPTH=1000

# The authorization list for an irreversible action, hard-coded with no environment
# override, following the CLEANUP_WT_CONSOLIDATION_BRANCH precedent in
# cleanup_worktrees_actions_lib.sh. A runtime override would let any path be declared
# disposable and would defeat the UNIQUE gate this feature's safety rests on. Matching
# is EXACT and never by prefix: artifacts/** at large must not be auto-cleared.
CLEANUP_WT_SESSION_ARTIFACT_PATHS=(
	"artifacts/pr_context.summary.txt"
	"artifacts/pr_context.appendix.txt"
	"artifacts/orchestration/orchestrator-state.json"
)

dirt_staged_tree_commit() {
	# Bounded staged-tree probe: is the current index exactly the tree of some recent
	# commit on this worktree's history?
	#
	# Walks CLEANUP_WT_STAGED_TREE_DEPTH commits back from HEAD and compares the index
	# against each with `diff-index --cached --quiet`. The bound is passed as depth plus
	# one because the FIRST line rev-list returns is HEAD itself, which is dropped: HEAD's
	# tree is what an unmodified index already equals, so probing it would match for every
	# worktree and label every staged entry disposable.
	#
	# Args: $1 = worktree path.
	# Echoes the matching commit sha and returns 0 on a match; returns 1 with no output
	# when no candidate matched; returns 2 on a hard read failure (a rev-list failure, or
	# a diff-index exit above 1, neither of which carries a verdict).
	local wt="$1" out rc=0 first=1 sha drc
	out=$(cleanup_wt_git --no-optional-locks -C "$wt" rev-list \
		--max-count=$((CLEANUP_WT_STAGED_TREE_DEPTH + 1)) HEAD) || rc=$?
	if ((rc != 0)); then # guard:staged-probe-revlist-hard-fail
		return 2
	fi
	while IFS= read -r sha || [[ -n $sha ]]; do
		[[ -z $sha ]] && continue
		if ((first == 1)); then # guard:staged-probe-skip-head
			first=0
			continue
		fi
		drc=0
		cleanup_wt_git --no-optional-locks -C "$wt" diff-index --cached --quiet \
			"$sha" -- >/dev/null || drc=$?
		if ((drc == 0)); then # guard:staged-probe-tree-match
			printf '%s\n' "$sha"
			return 0
		fi
		if ((drc > 1)); then # guard:staged-probe-diffindex-hard-fail
			return 2
		fi
	done <<<"$out"
	return 1
}

dirt_is_session_artifact() {
	# Rung 2. Pure string comparison against the hard-coded array; no git call, so no
	# invocation ever names a session-artifact path.
	#
	# Args: $1 = repo-relative path. Returns 0 on an exact match, 1 otherwise.
	local rel="$1" p
	for p in "${CLEANUP_WT_SESSION_ARTIFACT_PATHS[@]}"; do
		[[ $rel == "$p" ]] && return 0
	done
	return 1
}

dirt_diff_is_hintpath_confined() {
	# Read one diff and decide whether every changed content line in it is an analyzer
	# HintPath rewrite.
	#
	# The `---` and `+++` file headers are excluded before the test: both begin with a
	# changed-line marker and neither is content, so counting them would make every diff
	# unconfined. Hunk headers and the `diff --git`/`index` lines are not changed lines
	# and are ignored.
	#
	# The header skip is ANCHORED to the four forms the diff emits, not written as a
	# bare `+++ `/`--- ` prefix test. A space does not end a diff line's content, so an
	# ADDED line whose own text begins `+++ ` matches a bare prefix test and is dropped
	# before it is counted and before it is tested for HintPath. A project file carrying
	# one such hand edit then reads as confined and resolves DISPOSABLE_BUILD_ARTIFACT,
	# which is the fail-open direction and makes the entry clearable.
	#
	# The `a/` and `b/` prefixes are assumed because this library issues
	# `diff --no-color -U0` without `--no-prefix`, so the default prefixes are always
	# present; the two `/dev/null` forms cover an added or a deleted file. A header form
	# the pattern does not match is counted as a changed content line, which fails the
	# HintPath test and resolves the entry UNIQUE — the safe direction.
	#
	# Args: $1 = worktree path, $2 = repo-relative path, $3 = "cached" for the staged
	# diff or the empty string for the worktree diff.
	# Echoes the number of changed content lines seen and returns 0 when every one of
	# them is a HintPath line; returns 1 when any is not; returns 2 on a hard read
	# failure.
	local wt="$1" rel="$2" mode="${3:-}" out rc=0 line changed=0
	if [[ $mode == "cached" ]]; then
		out=$(cleanup_wt_git --no-optional-locks -C "$wt" diff --no-color -U0 \
			--cached -- "$rel") || rc=$?
	else
		out=$(cleanup_wt_git --no-optional-locks -C "$wt" diff --no-color -U0 \
			-- "$rel") || rc=$?
	fi
	if ((rc != 0)); then # guard:hintpath-diff-read-hard-fail
		return 2
	fi
	while IFS= read -r line || [[ -n $line ]]; do
		case "$line" in
		"--- a/"* | "+++ b/"* | "--- /dev/null" | "+++ /dev/null") continue ;; # guard:diff-header-skip
		"+"* | "-"*) ;;
		*) continue ;;
		esac
		changed=$((changed + 1))
		if [[ $line != *"HintPath"* ]]; then
			printf '%s\n' "$changed"
			return 1
		fi
	done <<<"$out"
	printf '%s\n' "$changed"
	return 0
}

dirt_is_build_artifact() {
	# Rung 3. A tracked, modified project file whose every changed content line, in BOTH
	# the worktree diff and the cached diff, is an analyzer HintPath rewrite.
	#
	# The rule is deliberately NOT path-pattern-only. In a legacy non-SDK C# project every
	# added source file is an explicit <Compile Include=...> edit to the project file, and
	# a path-only rule would label that hand edit disposable and destroy it. The content
	# confinement is the definition of the class, not extra scope.
	#
	# A diff pair with zero changed content lines is NOT a build artifact. Vacuous
	# confinement would let a read that returned nothing resolve to a disposable verdict,
	# which is the fail-open direction.
	#
	# Args: $1 = worktree path, $2 = repo-relative path.
	# Returns 0 for a build artifact, 1 for not one, 2 on a hard read failure.
	local wt="$1" rel="$2" wout crc=0 wrc=0 cout total=0
	case "$rel" in
	*.csproj | packages.config | */packages.config | app.config | */app.config) ;;
	*) return 1 ;;
	esac
	wout=$(dirt_diff_is_hintpath_confined "$wt" "$rel" "") || wrc=$?
	((wrc == 2)) && return 2 # guard:build-artifact-worktree-diff-hard-fail
	((wrc == 1)) && return 1 # guard:build-artifact-worktree-diff-unconfined
	cout=$(dirt_diff_is_hintpath_confined "$wt" "$rel" "cached") || crc=$?
	((crc == 2)) && return 2 # guard:build-artifact-cached-diff-hard-fail
	((crc == 1)) && return 1 # guard:build-artifact-cached-diff-unconfined
	total=$((wout + cout))
	((total == 0)) && return 1 # guard:build-artifact-vacuous-confinement
	return 0
}

classify_dirt_entry() {
	# The six-rung, first-match-wins precedence ladder for one status entry.
	#
	# Rung order: staged-tree, session artifact, build artifact, content-on-main,
	# content-in-history, unique. Rung 1 is answered from the once-per-worktree probe
	# result passed in by the caller rather than re-probed here, so the probe is issued at
	# most once per worktree however many entries it has.
	#
	# Args: $1 = worktree path, $2 = two-character status code, $3 = repo-relative path,
	# $4 = the staged-tree probe result: a commit sha on a match, the empty string on no
	# match, or the literal ERROR on a hard probe failure.
	# Echoes exactly one `<verdict>|<detail>` line and always returns 0.
	local wt="$1" xy="$2" rel="$3" staged="$4"
	local x="${xy:0:1}" y="${xy:1:1}" untracked=0 blob="" hrc=0 mrc=0 mainblob="" brc=0
	local drc=0 erc=0 vrc=0 lrc=0 bothloc=0 range found

	# A C-quoted payload is returned UNIQUE immediately with no unquoting attempt. One
	# branch, deterministic, and failing in the safe direction; attempting to unquote
	# would add an escape-decoding surface whose only use is to widen what can be
	# cleared.
	if [[ $rel == \"* ]]; then
		printf 'UNIQUE|\n'
		return 0
	fi

	# Rung 1. Staged entries only: the X column is the index status, and a space,
	# question mark, or exclamation mark there means the entry is not staged.
	#
	# THE Y COLUMN GATE IS LOAD-BEARING. The X column answers a question about the
	# INDEX, and the probe that answers rung 1 is `diff-index --cached --quiet`, which
	# compares the index against a candidate commit and says nothing whatever about a
	# non-space Y column. An MM entry has a staged change that matches an ancestor tree
	# AND an unstaged working-tree modification on top of it; that unstaged delta exists
	# in no commit and not in the index, so nothing recovers it once
	# --clear-disposable runs reset --hard. Rung 1 therefore fires only when the Y
	# column is a single space.
	#
	# An entry with a non-space Y column falls through to the rungs that compare
	# WORKING-TREE content, which is the comparison its unstaged delta actually needs.
	# See INDEX AND WORKING TREE ARE TWO LOCATIONS in the header: when the Y column is
	# itself content-bearing that comparison alone is not enough and rungs 4 and 5 fail
	# closed. An MM entry on a HintPath-confined project file can still reach
	# DISPOSABLE_BUILD_ARTIFACT, because dirt_is_build_artifact reads both the worktree
	# diff and the cached diff; an MM entry on any other path reaches rung 6 and is UNIQUE.
	if [[ $x != " " && $x != "?" && $x != "!" && $y == " " ]]; then # guard:rung1-y-column-gate
		if [[ $staged == "ERROR" ]]; then
			printf 'UNIQUE|\n'
			return 0
		fi
		if [[ -n $staged ]]; then
			printf 'STAGED_TREE_IS_COMMIT|%s\n' "$staged"
			return 0
		fi
	fi

	# Rung 2.
	if dirt_is_session_artifact "$rel"; then
		printf 'DISPOSABLE_SESSION_ARTIFACT|\n'
		return 0
	fi

	[[ $xy == "??" ]] && untracked=1
	# See INDEX AND WORKING TREE ARE TWO LOCATIONS in the header: two content-bearing
	# columns mean the index blob and the working-tree blob hold different content.
	[[ $x == [MARCTU] && $y == [MARCTU] ]] && bothloc=1 # guard:index-and-worktree-both-hold-content

	# Rung 3. Tracked, modified entries only; an untracked file has no diff to confine.
	if ((untracked == 0)); then # guard:rung3-tracked-only-gate
		dirt_is_build_artifact "$wt" "$rel" || brc=$?
		if ((brc == 0)); then # guard:rung3-build-artifact-match
			printf 'DISPOSABLE_BUILD_ARTIFACT|\n'
			return 0
		fi
		if ((brc > 1)); then # guard:rung3-build-artifact-hard-fail
			printf 'UNIQUE|\n'
			return 0
		fi
	fi

	# Rung 4, tracked half. Exit 1 is this probe's defined negative answer (the contents
	# differ) and advances the ladder; an exit above 1 carries no verdict.
	if ((untracked == 0)); then # guard:rung4-tracked-gate
		cleanup_wt_git --no-optional-locks -C "$wt" diff --quiet main -- "$rel" \
			>/dev/null || drc=$?
		if ((drc == 0)); then # guard:rung4-tracked-content-equal
			# See EMPTY PATHSPEC IS NOT A MATCH in the header. Only stdout is
			# discarded: the stub logs its argv to stderr, and redirecting that
			# stream too would hide this read from the argv assertions. --quiet
			# already suppresses git's own diagnostic on the negative answer.
			cleanup_wt_git --no-optional-locks -C "$wt" rev-parse --verify --quiet \
				"main:$rel" >/dev/null || erc=$?
			if ((erc == 0)); then      # guard:rung4-tracked-path-in-main
				if ((bothloc == 0)); then # guard:rung4-index-blob-unaccounted
					printf 'CONTENT_ON_MAIN|\n'
					return 0
				fi
			fi
		fi
		if ((drc > 1)); then # guard:rung4-tracked-hard-fail
			printf 'UNIQUE|\n'
			return 0
		fi
	fi

	# The working-tree blob id, needed by rung 4's untracked half and by rung 5. A
	# hash-object failure carries no verdict, so it fails closed.
	blob=$(cleanup_wt_git --no-optional-locks -C "$wt" hash-object -- "$rel") || hrc=$?
	blob=${blob%%$'\n'*}
	if ((hrc != 0)) || [[ -z $blob ]]; then # guard:hash-object-hard-fail
		printf 'UNIQUE|\n'
		return 0
	fi

	# Rung 4, untracked half. A rev-parse failure means the path is absent from main,
	# which is this probe's defined negative answer, so the ladder continues.
	if ((untracked == 1)); then # guard:rung4-untracked-gate
		mainblob=$(cleanup_wt_git --no-optional-locks -C "$wt" rev-parse "main:$rel") || mrc=$?
		mainblob=${mainblob%%$'\n'*}
		if ((mrc == 0)) && [[ -n $mainblob && $mainblob == "$blob" ]]; then # guard:rung4-untracked-main-present
			printf 'CONTENT_ON_MAIN|\n'
			return 0
		fi
	fi

	# Rung 5. The bounded range endpoint is probed first; a repository with less history
	# than the bound cannot resolve it, and that is not an error, so the scan falls back
	# to the full ref rather than reporting a read failure.
	range="main"
	cleanup_wt_git --no-optional-locks -C "$wt" rev-parse --verify --quiet \
		"main~$CLEANUP_WT_HISTORY_SCAN_DEPTH" >/dev/null 2>&1 || vrc=$?
	((vrc == 0)) && range="main~$CLEANUP_WT_HISTORY_SCAN_DEPTH..main" # guard:history-scan-bounded-range
	found=$(cleanup_wt_git --no-optional-locks -C "$wt" log "$range" \
		--find-object="$blob" --format=%H --max-count=1) || lrc=$?
	if ((lrc != 0)); then # guard:find-object-hard-fail
		printf 'UNIQUE|\n'
		return 0
	fi
	found=${found%%$'\n'*}
	if [[ -n $found ]]; then   # guard:history-hit-nonempty
		if ((bothloc == 0)); then # guard:rung5-index-blob-unaccounted
			printf 'CONTENT_IN_HISTORY|%s\n' "$found"
			return 0
		fi
	fi

	# Rung 6.
	printf 'UNIQUE|\n'
	return 0
}

classify_worktree_dirt() {
	# Per-worktree driver: one guarded status read, one DIRTFILE record per entry in
	# porcelain order, then exactly one DIRTSUM aggregate.
	#
	# A worktree with zero status entries emits NEITHER record: there is nothing to
	# classify and an aggregate over an empty set would assert a property of no data.
	# A hard status read emits no record at all and returns git's non-zero exit code, so
	# a read failure can never be mistaken for a clean worktree.
	#
	# The aggregate is ALL_DISPOSABLE if and only if the entry count is at least one and
	# the UNIQUE count is zero, and HAS_UNIQUE otherwise. Its detail field carries the
	# first non-empty per-entry detail, which is the staged-tree commit sha when the
	# worktree's dirt is a staged tree.
	#
	# Args: $1 = worktree path. Returns 0 normally; git's exit code on a hard status
	# read failure.
	local wt="$1" out srrc=0 line xy rel res verdict detail
	local staged="" any_staged=0 prc=0 x
	local entry_count=0 unique_count=0 agg_detail=""
	out=$(cleanup_wt_git --no-optional-locks -C "$wt" status --porcelain) || srrc=$?
	if ((srrc != 0)); then # guard:status-read-hard-fail
		return "$srrc"
	fi
	[[ -z $out ]] && return 0
	# The staged-tree probe is issued only when at least one entry is actually staged, so
	# a worktree carrying only unstaged or untracked dirt costs no rev-list walk.
	while IFS= read -r line || [[ -n $line ]]; do
		[[ -z $line ]] && continue
		x="${line:0:1}"
		if [[ $x != " " && $x != "?" && $x != "!" ]]; then
			any_staged=1
			break
		fi
	done <<<"$out"
	if ((any_staged == 1)); then # guard:staged-probe-issue-gate
		staged=$(dirt_staged_tree_commit "$wt") || prc=$?
		((prc == 1)) && staged=""     # guard:staged-probe-no-match
		((prc > 1)) && staged="ERROR" # guard:staged-probe-error-map
	fi
	while IFS= read -r line || [[ -n $line ]]; do
		[[ -z $line ]] && continue
		xy="${line:0:2}"
		rel="${line:3}"
		# A rename or copy entry's payload is `OLD -> NEW`; the destination is the path
		# that exists in the working tree and is therefore the one classified. Porcelain
		# status uses that payload form ONLY for the two codes R and C, and a space does
		# not trigger C-quoting, so an ordinary path may contain the literal ` -> `.
		# Splitting unconditionally truncated such a path, issued every probe against a
		# file that does not exist, and emitted a record naming the wrong file. The X
		# column is therefore the gate. The variable read here is `xy`, not the enclosing
		# function's `x`: `x` holds the last value the any_staged pre-scan loop assigned,
		# not this entry's X column.
		if [[ ${xy:0:1} == R || ${xy:0:1} == C ]]; then # guard:rename-payload-split-gate
			rel="${rel#* -> }"
		fi
		res=$(classify_dirt_entry "$wt" "$xy" "$rel" "$staged")
		verdict="${res%%|*}"
		detail="${res#*|}"
		entry_count=$((entry_count + 1))
		[[ $verdict == "UNIQUE" ]] && unique_count=$((unique_count + 1)) # guard:unique-verdict-tally
		[[ -z $agg_detail && -n $detail ]] && agg_detail="$detail"
		printf 'DIRTFILE|%s|%s|%s|%s|%s\n' "$wt" "$verdict" "$detail" "$xy" "$rel"
	done <<<"$out"
	((entry_count == 0)) && return 0 # guard:aggregate-empty-entry-set
	if ((unique_count == 0)); then   # guard:aggregate-all-disposable-gate
		printf 'DIRTSUM|%s|ALL_DISPOSABLE|%s\n' "$wt" "$agg_detail"
	else
		printf 'DIRTSUM|%s|HAS_UNIQUE|\n' "$wt"
	fi
	return 0
}

clear_disposable_dirt() {
	# The opt-in clearing sequence: classify, require the aggregate to be exactly
	# ALL_DISPOSABLE, reset --hard, clean -fd, then emit the result record.
	#
	# `git worktree remove` is never invoked from here. The caller retries the existing
	# unforced removal through remove_worktree_safe, so this path introduces no new
	# removal call site and is not force-removal: it clears the working tree first and
	# then retries the same unforced removal.
	#
	# `git clean` is never given -x, -X, or -ff. Without -x the ignored files that -fd
	# leaves alone stay on disk, which matches the classifier's refusal to classify them.
	#
	# Anything other than an exact ALL_DISPOSABLE aggregate refuses: a HAS_UNIQUE
	# aggregate, a hard status read (which emits no aggregate at all), and a worktree with
	# no entries all take the same fail-closed branch, because none of them establishes
	# that every entry is disposable.
	#
	# Args: $1 = worktree path. Returns 0 on a completed clear, 1 on a refusal or a
	# failure.
	local wt="$1" cout crc=0 agg="" rrc=0 clrc=0
	cout=$(classify_worktree_dirt "$wt") || crc=$?
	if ((crc == 0)); then # guard:clear-classify-exit-gate
		agg=$(printf '%s\n' "$cout" | awk -F'|' '/^DIRTSUM\|/{print $3; exit}')
	fi
	if [[ $agg != "ALL_DISPOSABLE" ]]; then # guard:clear-requires-all-disposable
		printf 'ACTION|dirt-clear|%s|REFUSED-UNIQUE\n' "$wt"
		return 1
	fi
	cleanup_wt_git -C "$wt" reset --hard >/dev/null || rrc=$?
	if ((rrc != 0)); then # guard:clear-reset-hard-fail
		printf 'ACTION|dirt-clear|%s|FAILED\n' "$wt"
		return 1
	fi
	cleanup_wt_git -C "$wt" clean -fd >/dev/null || clrc=$?
	if ((clrc != 0)); then # guard:clear-clean-hard-fail
		printf 'ACTION|dirt-clear|%s|FAILED\n' "$wt"
		return 1
	fi
	printf 'ACTION|dirt-clear|%s|OK\n' "$wt"
	return 0
}
