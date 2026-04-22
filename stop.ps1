Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "LifeQuest Docker Servisleri Durduruluyor..." -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Cyan

docker-compose down

Write-Host ""
Write-Host "Servisler durduruldu." -ForegroundColor Green
