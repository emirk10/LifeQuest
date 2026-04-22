@echo off
echo ========================================================
echo LifeQuest Docker Servisleri Baslatiliyor...
echo ========================================================
docker-compose up -d --build
echo.
echo Servisler calisiyor. 
echo Frontend: https://localhost:5173
echo API: https://localhost:7071/swagger
echo AI Service: https://localhost:5001/health
echo Veritabani (pgAdmin): http://localhost:5050
echo pgAdmin Giris: admin@lifequest.com / admin
