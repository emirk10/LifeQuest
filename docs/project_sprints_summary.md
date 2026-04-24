# LifeQuest Sprint Özetleri (SP-01 - SP-04)

Bu doküman, LifeQuest projesine eklenen temel özellikleri ve 4 aşamalı sprint sürecinin özetini içerir.

## 🚀 SP-01: Profil Anketi ve Özelleştirme
**Odak:** Kullanıcıyı tanıma ve kişiselleştirilmiş deneyim sağlama.
* **Profil Anketi:** Kullanıcıların hedefleri, zorluk tercihleri, güçlü/zayıf yönleri ve uygun zamanları gibi bilgileri toplandı.
* **ChatBot Karakteri:** Asistanın karakteri (Motivasyon Koçu, Arkadaşça vb.) kullanıcının tercihine bırakıldı.
* **Proaktif Bildirimler (Nudge):** Kullanıcıyı motive etmek için bildirim tercihleri eklendi.

## 🤖 SP-02: Akıllı ChatBot Entegrasyonu
**Odak:** Sadece soru cevaplayan değil, sistemi yönetebilen asistan.
* **Aksiyon Alabilen Chatbot:** Chatbot, sadece metin döndürmek yerine görev atama, görev erteleme, zorluk seçimi gibi butonlu aksiyonlar (ChatAction) üretebilir hale geldi.
* **Mini Task (Anlık Görev):** Kullanıcının yorgun veya motive olmasına göre, küçük parçalara ayrılmış (MiniTask) kısa görevler önerebilme yeteneği eklendi.
* **Proaktif Asistan:** Kullanıcı uzun süre girmediğinde veya bir görev başarısız olduğunda asistanın kendi kendine mesaj atabilmesi sağlandı.

## 📊 SP-03: Davranış Analizi ve Raporlama
**Odak:** Kullanıcının geçmiş performansını analiz edip görselleştirme.
* **Haftalık Rapor İşçisi (WeeklyReportWorker):** Her pazar günü çalışan ve kullanıcının tüm haftasını (kazanılan XP, tamamlanan/atlanan görevler) özetleyen bir sistem eklendi.
* **Davranış Analizi:** Kullanıcının en çok hangi gün aktif olduğu, hangi kategoriyi ertelediği gibi eğilimler (BehaviorSummary) analiz edildi.
* **Grafikler:** Frontend tarafına (Recharts kullanılarak) XP kazanım grafikleri ve detaylı davranış analizi kartları (BehaviorInsightCard) eklendi.

## 🏖️ SP-04: Adaptif Öğrenme ve Tatil Modu
**Odak:** Esneklik, öğrenen sistem ve kullanıcıyı sıkmama.
* **Tatil/Mola Modu:** Kullanıcının ara vermek istediğinde "Streak" (seri) değerini kaybetmemesi için tatil modu eklendi.
* **Hedef Revizyonu:** Uzun süre başarısız olunan (tamamlama oranı %20 altı) hedefler Pazar akşamları Chatbot tarafından fark edilip, kullanıcıya "Bu hedefi değiştirelim mi?" diye sorulması sağlandı.
* **İşbirlikçi Filtreleme (Collaborative Filtering):** Python AI servisine, kullanıcının seviyesine ve hedeflerine benzeyen diğer kullanıcıların bitirdiği görevleri önerebilme yeteneği (makine öğrenmesi altyapısı) eklendi.
