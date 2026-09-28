---
name: bash-heredoc-backslash-and-tmp-traps
description: Two silent-false-negative traps when verifying via Bash+python3 - quoted heredocs still collapse doubled backslashes, and Windows python3 cannot see /tmp
metadata:
  type: project
---

Two traps hit on the #911 review (2026-09-20). Both produce a **silently wrong PASS**, which is the
worst failure mode for a reviewer.

**1. A quoted heredoc (`python3 - <<'PY'`) still collapses `\\`.** A regex written as
`re.compile(r'packages[\\/]([^\\/]+)[\\/]')` reached python as `[\/]` (slash only) and matched
**zero** of 1,498 Windows restore paths in `.csproj` files. Nothing errored; the script printed
`total refs: 0  disagreements: 0` and would have been read as "no version disagreements anywhere."

**Why:** the Bash tool's own command-string handling de-doubles backslashes before `bash` sees the
heredoc body, so the quoted-heredoc guarantee does not hold. Same root cause as
`bash-tool-collapses-double-backslash-in-sed` in the orchestrator memory.

**How to apply:** never write a literal backslash in a heredoc. Build it from its character code and
assemble the pattern by concatenation:

```python
B = chr(92)
SEP = '[' + re.escape(B) + '/]'
pat = re.compile('packages' + SEP + '([^' + re.escape(B) + '/]+)' + SEP)
```

Then **prove the pattern is live** before trusting a zero result: print a match count on one known
file and a `repr()` slice of the text around the first occurrence. A census that returns zero
findings must be falsified before it is reported. (This is the [[feedback_gates-can-pass-for-reasons-unrelated-to-correctness]] pattern
applied to my own verification scripts.)

**2. The `python3` on PATH here is a Windows build and cannot resolve `/tmp`.** `git archive ... |
tar -x -C /tmp/x` works (msys tar), but `glob.glob('/tmp/x/*/*.csproj')` returns `[]` with no error,
so a baseline-versus-head comparison silently compares head against nothing. Extract to a
Windows-visible path instead, e.g. `/c/Users/<user>/AppData/Local/Temp/claude/<name>` for the shell
and `C:\Users\<user>\AppData\Local\Temp\claude\<name>` for python. Always print the file count the
glob found before looping.

**3. `cat > file <<'EOF'` fails on large markdown containing apostrophes** with
`unexpected EOF while looking for matching '`. Use the Write tool for review artifacts; reserve Bash
heredocs for short scripts.

**4. `cd X && grep|head|awk|cat` is refused outright** by the permission engine. Use `git -C <path>`,
absolute paths with no leading `cd`, or the Grep/Read tools. `rm -rf` is blocked too.
