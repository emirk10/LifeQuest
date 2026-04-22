Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "LifeQuest Docker Servisleri Baslatiliyor..." -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Cyan

docker-compose up -d --build

Write-Host ""
Write-Host "Servisler calisiyor." -ForegroundColor Green
Write-Host "Frontend: https://localhost:5173" -ForegroundColor Yellow
Write-Host "API: https://localhost:7071/swagger" -ForegroundColor Yellow
Write-Host "AI Service: https://localhost:5001/health" -ForegroundColor Yellow
Write-Host "Veritabani (pgAdmin): http://localhost:5050" -ForegroundColor Magenta
Write-Host "pgAdmin Giris: admin@lifequest.com / admin" -ForegroundColor Gray
