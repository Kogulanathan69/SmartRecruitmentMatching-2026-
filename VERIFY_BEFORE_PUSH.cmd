@echo off
setlocal
cd /d "%~dp0"

echo [1/5] Cleaning solution...
dotnet clean NexHire.sln || goto :failed

echo [2/5] Restoring packages...
dotnet restore NexHire.sln || goto :failed

echo [3/5] Building solution...
dotnet build NexHire.sln --no-restore || goto :failed

echo [4/5] Running tests...
dotnet test NexHire.sln --no-build || goto :failed

echo [5/5] Updating database...
dotnet ef database update --project src\NexHire.Infrastructure --startup-project src\NexHire.API || goto :failed

echo.
echo ALL CHECKS PASSED. Review git status before committing.
git status --short
echo.
pause
exit /b 0

:failed
echo.
echo CHECK FAILED. Do not push this branch yet.
echo Read the first error shown above.
echo.
pause
exit /b 1
