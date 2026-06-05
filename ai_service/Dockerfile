# LifeQuest AI Mikro Servis — Dockerfile
# Python 3.11 slim tabanlı imaj

FROM python:3.11-slim

# Çalışma dizini
WORKDIR /app

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Sistem bağımlılıkları (Openssl ve CA sertifikaları için)
RUN apt-get update && apt-get install -y --no-install-recommends \
    openssl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Python bağımlılıkları (önce kopyala → cache kullan)
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Uygulama dosyaları
COPY . .

# Models klasörünü oluştur ve izinleri ayarla
RUN mkdir -p /app/models && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Port
EXPOSE 5001

# Sağlık kontrolü (HTTP)
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD python -c "import urllib.request; urllib.request.urlopen('http://localhost:5001/health')" || exit 1

# Başlatma komutu (HTTP)
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "5001"]
