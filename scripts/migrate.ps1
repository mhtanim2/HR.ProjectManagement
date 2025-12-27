# Database migration script for Windows

Write-Host "Starting database migrations..." -ForegroundColor Green

try {
    dotnet ef database update --project HR.ProjectManagement --startup-project HR.ProjectManagement
    Write-Host "Migrations completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Migration failed: $_" -ForegroundColor Red
    exit 1
}
