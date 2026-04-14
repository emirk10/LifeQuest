# Sprint 2: Python AI Mikro Servis Kurulumu ve Modellerin Eğitilmesi

Bu sprint'in temel amacı, .NET backend'in yanında çalışacak, asıl "Yapay Zeka" iş yükünü (Chatbot, NLP, Görsel Analiz) üstlenecek bağımsız bir Python mikro servisi kurmaktır. API kullanmadan kendi modellerimizi sıfırdan eğiteceğiz.

## 🎯 Hedefler

- Python + FastAPI kullanarak mikro servis mimarisini ve klasör yapısını kurmak.
- Chatbot için **Intent Classification (Niyet Sınıflandırma)** modelini eğitmek.
- Kullanıcı mesajları için **Sentiment Analysis (Duygu Analizi)** modelini eğitmek.
- Metin (Reflection) doğrulaması için metin analizi altyapısını kurmak.
- Görüntü doğrulaması (Image Verification) için MobileNetV2 tabanlı Transfer Learning altyapısını hazırlamak.
- .NET tarafının bu servise istek (HTTP Request) atabileceği API endpoint'lerini oluşturmak.

---

## 📋 Görev Listesi ve İş Kırılımı

| Kod | Görev | Detay |
| :--- | :--- | :--- |
| **2.1** | **Python Proje Yapısı** | `ai_service` klasörünün oluşturulması, `requirements.txt` ile bağımlılıkların (FastAPI, scikit-learn, TensorFlow, pydantic) belirlenmesi. |
| **2.2** | **Intent Eğitim Verisi** | 7 farklı niyet (motivasyon, görev sorgulama, istatistik, öneri, selamlama, sağlık, rozet) için 200+ Türkçe örnek cümlenin hazırlanması (`data/intents_tr.json`). |
| **2.3** | **Intent Classifier Eğitimi** | TF-IDF ve Naive Bayes/SVM kullanarak niyet sınıflandırma modelinin (`intent_classifier.py`) kodlanması, eğitilmesi ve `.pkl` olarak kaydedilmesi. |
| **2.4** | **Sentiment Eğitim Verisi** | Pozitif, negatif ve nötr kategorilerde 300+ Türkçe cümlenin hazırlanması (`data/sentiments_tr.json`). |
| **2.5** | **Sentiment Analyzer Eğitimi**| TF-IDF ve SVM ile duygu analizi modelinin (`sentiment_analyzer.py`) kodlanması, eğitilmesi ve `.pkl` olarak kaydedilmesi. |
| **2.6** | **Reflection Verifier (Yansıma)** | Görev tamamlama metinlerinin (reflection) uzunluğunu, kalitesini ve spama karşı durumunu kontrol edecek servisin (`reflection_verifier.py`) yazılması. |
| **2.7** | **Image Verifier (Görsel)** | TF/Keras MobileNetV2 ile transfer learning temelinin kurulması (`image_verifier.py`). Model başlangıçta ön-eğitilmiş ağırlıkları kullanacak. |
| **2.8** | **FastAPI Endpoint'leri** | `main.py` içinde `/api/chat`, `/api/verify-image`, `/api/verify-reflection` ve model yeniden eğitim için `/api/train` endpoint'lerinin yazılması. |
| **2.9** | **Docker Kurulumu** | Sistemin kolayca deploy edilebilmesi ve .NET ile haberleşebilmesi için `Dockerfile` ve `docker-compose.yml` dosyalarının oluşturulması. |
| **2.10** | **Testler** | Yazılan endpoint'lerin çalışabilirliğinin test edilmesi (örnek istekler atılarak modellerin doğru cevap verip vermediğinin kontrolü). |

---

## 📁 Mimari ve Klasör Yapısı

Sprint tamamlandığında ana dizinde aşağıdaki Python mikro servis yapısı oluşacak:

```text
LifeQuest/
├── LifeQuestAPI/                # .NET Backend (Sprint 1'de güncellendi)
└── ai_service/                  # Python AI Mikro Servis (SPRINT 2 ODAK NOKTASI)
    ├── main.py                  # API giriş noktası (FastAPI)
    ├── requirements.txt         # Paket listesi
    ├── Dockerfile               # İzolasyon ve Deployment için
    ├── models/                  # Eğitilmiş AI modellerinin tutulduğu klasör (.pkl, .h5)
    ├── services/                # İş mantığı ve Model sınıfları
    │   ├── intent_classifier.py
    │   ├── sentiment_analyzer.py
    │   ├── reflection_verifier.py
    │   └── image_verifier.py
    ├── training/                # Metrikleri, confusion matrix vb. üreten araçlar
    │   └── evaluate_models.py
    └── data/                    # Eğitim verileri (JSON formatında)
        ├── intents_tr.json
        └── sentiments_tr.json
```

---

## 🛠️ Kullanılacak Teknolojiler

- **FastAPI & Uvicorn:** Yüksek performanslı ve async destekli API sunucusu.
- **scikit-learn:** NLP işlemleri, TF-IDF vectorization, Intent ve Sentiment modelleri için (Naive Bayes, SVM).
- **TensorFlow/Keras:** Görüntü sınıflandırma ve Transfer Learning modelleri için.
- **Pydantic:** Gelen/giden JSON verilerinin tipe bağlı olarak doğrulanması.

---

> [!NOTE]
> Bu sprint, projenizi sıradan bir CRUD (Ekle/Sil/Güncelle) uygulamasından çıkarıp asıl **"Yapay Zeka Destekli"** kimliğine kavuşturacak en kritik geliştirme aşamasıdır.
