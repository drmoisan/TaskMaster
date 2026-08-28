#!/usr/bin/env bash
# shell_qc_lib.sh: sourceable function library for the native bash shell-qc
# toolchain. Provides script discovery, shebang parsing, tool resolution (with
# the SHELL_QC_<TOOL>_BIN test seam), the missing-tool message block, the
# check/format/test/coverage implementations, and Cobertura line-rate parsing.
# Contains no Python and no Poetry. Intended to be sourced by scripts/bash/shell-qc.sh
# and by bats tests. All external tools (shfmt, shellcheck, bats, kcov) legitimately
# return non-zero, so every invocation is captured with `|| rc=$?` under set -e.

# EXCLUDED_DIRS are pruned during discovery traversal at any depth.
# SEARCH_DIRS and TEST_DIR_CANDIDATES mirror the removed Python module's constants.

extract_shebang_command() {
	# Extract the interpreter command from a file's shebang line.
	#
	# Reads only the first line (parity with reading one line in Python), lowercases
	# it (so `#!/usr/bin/env BASH` qualifies), strips the `#!` and surrounding
	# whitespace, word-splits the payload, and takes the basename of the first token.
	# When that token is `env`, it is dropped along with any leading `-`-prefixed
	# option tokens (covering `env -S bash` and `env -flag` forms) and the basename of
	# the next token is used. Shebang lines never legitimately contain quotes, so plain
	# word-splitting is acceptable parity for Python's shlex.split.
	#
	# Args: $1 = path to inspect.
	# Echoes the resolved command name on success; prints nothing and returns 1 when
	# the file is unreadable, has no shebang, or has an empty payload.
	local file="$1"
	local first_line payload
	# Unreadable file (open failure) is treated as not a shell script, no error.
	IFS= read -r first_line <"$file" 2>/dev/null || return 1
	first_line=${first_line,,}
	[[ $first_line == '#!'* ]] || return 1
	payload=${first_line:2}
	# Trim leading and trailing whitespace, mirroring Python's str.strip().
	payload=${payload#"${payload%%[![:space:]]*}"}
	payload=${payload%"${payload##*[![:space:]]}"}
	[[ -n $payload ]] || return 1
	local -a parts
	read -ra parts <<<"$payload"
	((${#parts[@]})) || return 1
	local cmd=${parts[0]##*/}
	if [[ $cmd == env ]]; then
		local i=1
		# Skip env's own option flags before the interpreter token.
		while ((i < ${#parts[@]})) && [[ ${parts[$i]} == -* ]]; do
			i=$((i + 1))
		done
		((i < ${#parts[@]})) || return 1
		cmd=${parts[$i]##*/}
	fi
	printf '%s\n' "$cmd"
}

is_shell_script() {
	# Return success when a path looks like a shell script.
	#
	# A path qualifies when its basename lowercased ends in `.sh` (parity with
	# Path.suffix.lower() == ".sh") or when its shebang resolves to bash or sh.
	#
	# Args: $1 = path to inspect. Returns 0 when shell, 1 otherwise.
	local file="$1"
	local base=${file##*/}
	local lower=${base,,}
	if [[ $lower == *.sh ]]; then
		return 0
	fi
	local cmd
	if cmd=$(extract_shebang_command "$file"); then
		[[ $cmd == bash || $cmd == sh ]]
		return
	fi
	return 1
}

discover_shell_scripts() {
	# Discover shell scripts under tools/, scripts/, and .claude/lib/bash/ relative to
	# the current dir.
	#
	# Missing roots are silently skipped. Traversal prunes .venv, .git, node_modules,
	# dist, and build at any depth. Output is de-duplicated and sorted with LC_ALL=C
	# for deterministic codepoint ordering, matching the removed module's sorted set.
	local -a roots=()
	local root
	# Collect only the search roots that exist, mirroring Path.exists() skipping.
	for root in tools scripts .claude/lib/bash; do
		if [[ -d $root ]]; then
			roots+=("$root")
		fi
	done
	((${#roots[@]})) || return 0
	local f
	# Walk each root, pruning excluded directories, and emit qualifying files.
	# NUL-delimited output makes the pipeline robust to unusual path characters.
	while IFS= read -r -d '' f; do
		if is_shell_script "$f"; then
			printf '%s\n' "$f"
		fi
	done < <(find "${roots[@]}" \
		-type d \( -name .venv -o -name .git -o -name node_modules \
		-o -name dist -o -name build \) -prune \
		-o -type f -print0) | LC_ALL=C sort -u
}

find_bats_test_dirs() {
	# Echo existing bats test directories, tests/shell then tests/bash, in that order.
	# Emits one path per line; emits nothing when neither directory exists.
	local -a dirs=()
	local candidate
	# Preserve the documented precedence order of the two candidate directories.
	for candidate in "tests/shell" "tests/bash"; do
		if [[ -d $candidate ]]; then
			dirs+=("$candidate")
		fi
	done
	((${#dirs[@]})) || return 0
	local d
	for d in "${dirs[@]}"; do
		printf '%s\n' "$d"
	done
}

resolve_tool() {
	# Resolve an external tool path, honoring the SHELL_QC_<TOOL>_BIN override seam.
	#
	# When SHELL_QC_<TOOL>_BIN is set to a non-empty value, that value must point to an
	# existing executable; an empty or nonexistent override is treated as missing.
	# Without an override, the tool is resolved from PATH via command -v.
	#
	# Args: $1 = tool name (lowercase: shfmt|shellcheck|bats|kcov).
	# Echoes the resolved path on success; prints nothing and returns 1 when missing.
	local tool="$1"
	local var="SHELL_QC_${tool^^}_BIN"
	local override=${!var:-}
	if [[ -n $override ]]; then
		# An explicit override must be a runnable executable to count as present.
		if [[ -x $override ]]; then
			printf '%s\n' "$override"
			return 0
		fi
		return 1
	fi
	local resolved
	if resolved=$(command -v "$tool" 2>/dev/null); then
		printf '%s\n' "$resolved"
		return 0
	fi
	return 1
}

print_missing_tool_block() {
	# Print the exact five-line missing-tool block. The package name defaults to the
	# tool name. This block is a byte-identical contract with the removed module.
	#
	# Args: $1 = tool name; $2 = optional package name (defaults to $1).
	local tool="$1"
	local package="${2:-$1}"
	printf 'Missing required tool: %s\n' "$tool"
	printf 'Devcontainer install (apt-get): apt-get update && apt-get install -y %s\n' "$package"
	printf 'macOS (Homebrew): brew install %s\n' "$package"
	printf 'Ubuntu/Debian: sudo apt-get update && sudo apt-get install -y %s\n' "$package"
	printf 'On Windows, use WSL for best results.\n'
}

run_check() {
	# Run shfmt in diff mode and shellcheck across discovered scripts.
	#
	# No scripts prints the skip message and returns 0. Both tools are preflighted in
	# order (shfmt then shellcheck); a missing tool prints the block and returns 127.
	# shfmt runs once over the full file list; shellcheck runs once per file so
	# findings stay attributable. The return value is the maximum observed exit code.
	local -a files=()
	mapfile -t files < <(discover_shell_scripts)
	if ((${#files[@]} == 0)); then
		printf 'No shell scripts found; skipping.\n'
		return 0
	fi
	local shfmt_bin shellcheck_bin
	if ! shfmt_bin=$(resolve_tool shfmt); then
		print_missing_tool_block shfmt
		return 127
	fi
	if ! shellcheck_bin=$(resolve_tool shellcheck); then
		print_missing_tool_block shellcheck
		return 127
	fi
	local exit_code=0 rc=0
	# shfmt legitimately returns non-zero when a file differs; capture, do not abort.
	"$shfmt_bin" -d "${files[@]}" || rc=$?
	if ((rc > exit_code)); then
		exit_code=$rc
	fi
	local file
	# Lint each discovered file independently to keep failures localized.
	for file in "${files[@]}"; do
		rc=0
		"$shellcheck_bin" "$file" || rc=$?
		if ((rc > exit_code)); then
			exit_code=$rc
		fi
	done
	return "$exit_code"
}

run_format() {
	# Run shfmt in write mode across discovered scripts.
	#
	# No scripts prints the skip message and returns 0. A missing shfmt prints the
	# block and returns 127. Otherwise shfmt -w runs over the full list and its exit
	# code is returned.
	local -a files=()
	mapfile -t files < <(discover_shell_scripts)
	if ((${#files[@]} == 0)); then
		printf 'No shell scripts found; skipping.\n'
		return 0
	fi
	local shfmt_bin
	if ! shfmt_bin=$(resolve_tool shfmt); then
		print_missing_tool_block shfmt
		return 127
	fi
	local rc=0
	"$shfmt_bin" -w "${files[@]}" || rc=$?
	return "$rc"
}

run_test() {
	# Run bats against tests/shell and tests/bash without coverage.
	#
	# No test directory prints the exact skip marker consumed by fix_all.py and
	# returns 0. A missing bats prints the exact non-coverage skip marker and returns
	# 0. Otherwise bats runs once per directory; all directories run even if one
	# fails, and the maximum exit code is returned.
	local -a test_dirs=()
	mapfile -t test_dirs < <(find_bats_test_dirs)
	if ((${#test_dirs[@]} == 0)); then
		printf 'No shell test directories found; skipping.\n'
		return 0
	fi
	local bats_bin
	if ! bats_bin=$(resolve_tool bats); then
		printf 'bats not installed; skipping shell tests.\n'
		return 0
	fi
	local exit_code=0 rc=0 test_dir
	# Run every directory even on failure so all suites report, matching prior behavior.
	for test_dir in "${test_dirs[@]}"; do
		rc=0
		"$bats_bin" "$test_dir" || rc=$?
		if ((rc > exit_code)); then
			exit_code=$rc
		fi
	done
	return "$exit_code"
}

extract_cobertura_line_rate() {
	# Echo the first line-rate attribute value from a Cobertura cov.xml.
	#
	# Matches line-rate="0.75" or line-rate='0.75'. Prints nothing and returns 1 when
	# the file is missing or no attribute is found, so callers can treat absence as a
	# silent no-op (parity with the removed module returning None).
	#
	# Args: $1 = path to cov.xml.
	local cov_xml="$1"
	[[ -f $cov_xml ]] || return 1
	local match
	match=$(grep -oE "line-rate=[\"'][0-9.]+[\"']" "$cov_xml" 2>/dev/null | head -n1) || true
	[[ -n $match ]] || return 1
	# Strip the attribute name and the surrounding single-or-double quotes.
	local value=${match#line-rate=}
	value=${value#?}
	value=${value%?}
	[[ -n $value ]] || return 1
	printf '%s\n' "$value"
}

print_coverage_summary() {
	# Print "Bash coverage (lines): NN.N%" using the cov.xml line-rate.
	#
	# Prints nothing when cov.xml is missing or unparseable, matching the removed
	# module's no-op behavior. The percentage carries exactly one decimal place.
	#
	# Args: $1 = path to cov.xml.
	local cov_xml="$1"
	local rate
	if ! rate=$(extract_cobertura_line_rate "$cov_xml"); then
		return 0
	fi
	local percent
	percent=$(awk -v r="$rate" 'BEGIN { printf "%.1f", r * 100 }')
	printf 'Bash coverage (lines): %s%%\n' "$percent"
}

run_test_coverage() {
	# Run bats under kcov, merge the reports, and print the coverage summary.
	#
	# No test directory prints the exact skip marker and returns 0. A missing bats
	# prints the coverage-mode message and returns 127. A missing kcov prints its
	# message plus the missing-tool block and returns 127. The output directory
	# (SHELL_QC_KCOV_OUT_DIR, default artifacts/pester/kcov, resolved against the repo
	# root when relative) is deleted and recreated. Each directory runs under kcov with
	# a stable run dir, stopping at the first failure; successful runs are merged into a
	# single cov.xml. The transient .kcov_runs directory is always removed.
	local -a test_dirs=()
	mapfile -t test_dirs < <(find_bats_test_dirs)
	if ((${#test_dirs[@]} == 0)); then
		printf 'No shell test directories found; skipping.\n'
		return 0
	fi
	local bats_bin
	if ! bats_bin=$(resolve_tool bats); then
		printf 'bats not installed; cannot run shell tests with coverage.\n'
		return 127
	fi
	local kcov_bin
	if ! kcov_bin=$(resolve_tool kcov); then
		printf 'kcov not installed; cannot run shell tests with coverage.\n'
		print_missing_tool_block kcov
		return 127
	fi
	local repo_root
	repo_root=$(pwd)
	local out_dir=${SHELL_QC_KCOV_OUT_DIR:-artifacts/pester/kcov}
	# Resolve a relative output directory against the repo root for stable placement.
	if [[ $out_dir != /* ]]; then
		out_dir="$repo_root/$out_dir"
	fi
	# Start from a clean slate so stale coverage is never merged or displayed.
	rm -rf "$out_dir"
	mkdir -p "$out_dir"
	local runs_dir="$out_dir/.kcov_runs"
	mkdir -p "$runs_dir"
	# Scope coverage to repo scripts/tools and the Claude bash library; exclude the test
	# sources themselves.
	local include_pattern="$repo_root/tools,$repo_root/scripts,$repo_root/.claude/lib/bash"
	local exclude_pattern="$repo_root/tests"
	local exit_code=0 rc=0 test_dir
	local -a run_dirs=()
	# Run directories in order, stopping on the first failure to keep logs readable.
	for test_dir in "${test_dirs[@]}"; do
		local run_dir="$runs_dir/${test_dir##*/}"
		run_dirs+=("$run_dir")
		rc=0
		# Do not pass --cobertura-only here: that flag suppresses the per-run coverage
		# database that `kcov --merge` consumes, producing an empty merged report. kcov
		# still emits Cobertura output in the merged directory without it.
		"$kcov_bin" \
			"--include-pattern=$include_pattern" \
			"--exclude-pattern=$exclude_pattern" \
			"$run_dir" "$bats_bin" "$test_dir" || rc=$?
		if ((rc > exit_code)); then
			exit_code=$rc
		fi
		if ((exit_code != 0)); then
			break
		fi
	done
	# Merge per-directory runs into one report directory containing a single cov.xml.
	if ((exit_code == 0)) && ((${#run_dirs[@]} > 0)); then
		rc=0
		"$kcov_bin" --merge "$out_dir" "${run_dirs[@]}" || rc=$?
		if ((rc > exit_code)); then
			exit_code=$rc
		fi
		# kcov --merge writes the combined Cobertura report to <out_dir>/kcov-merged/cov.xml.
		# Copy it to the canonical <out_dir>/cov.xml so the summary parse below and
		# downstream tooling (Coverage Gutters) find a single cov.xml at the documented path.
		if [[ -f "$out_dir/kcov-merged/cov.xml" ]]; then
			cp -f "$out_dir/kcov-merged/cov.xml" "$out_dir/cov.xml" || true
		fi
	fi
	# Always remove the intermediate run directories, on success or failure.
	rm -rf "$runs_dir"
	# On overall success, emit the one-line coverage summary when cov.xml is parseable.
	if ((exit_code == 0)); then
		print_coverage_summary "$out_dir/cov.xml"
	fi
	return "$exit_code"
}
