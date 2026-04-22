# LifeQuest AI Mikro Servis — Dockerfile
# Python 3.11 slim tabanlı imaj

FROM python:3.11-slim

# Çalışma dizini
WORKDIR /app

# Sistem bağımlılıkları (TensorFlow ve PIL için gerekli + openssl SSL için)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libglib2.0-0 \
    libsm6 \
    libxext6 \
    libxrender-dev \
    libgl1 \
    openssl \
    && rm -rf /var/lib/apt/lists/*

# Python bağımlılıkları (önce kopyala → cache kullan)
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Uygulama dosyaları
COPY . .

# Models klasörünü oluştur
RUN mkdir -p /app/models

# Generate self-signed SSL certs for HTTPS
RUN openssl req -x509 -newkey rsa:4096 -nodes -out /app/cert.pem -keyout /app/key.pem -days 365 -subj "/CN=localhost"

# Port
EXPOSE 5001

# Sağlık kontrolü (HTTPS ve SSL verify bypass ile)
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD python -c "import urllib.request; import ssl; ctx = ssl.create_default_context(); ctx.check_hostname = False; ctx.verify_mode = ssl.CERT_NONE; urllib.request.urlopen('https://localhost:5001/health', context=ctx)" || exit 1

# Başlatma komutu (HTTPS)
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "5001", "--ssl-keyfile", "/app/key.pem", "--ssl-certfile", "/app/cert.pem"]
