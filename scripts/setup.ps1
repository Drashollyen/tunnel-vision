# One-time setup after cloning, for people who prefer PowerShell. Same as scripts/setup.sh.
#   powershell -ExecutionPolicy Bypass -File scripts/setup.ps1
$ErrorActionPreference = 'Stop'
$bash = Join-Path $env:ProgramFiles 'Git\bin\bash.exe'
if (-not (Test-Path $bash)) { throw 'Git for Windows is required: https://git-scm.com/download/win' }
& $bash scripts/setup.sh
