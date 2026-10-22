# P0-T9 search-can-match control

Timestamp: 2026-10-01T12-11
Command: git grep -n -I -F ".bak" origin/main -- .gitignore
EXIT_CODE: 0
Output Summary: One hit beginning `origin/main:.gitignore:`, proving the P0-T8 search shape can match on origin/main.

```
origin/main:.gitignore:257:*.rptproj.bak
```
