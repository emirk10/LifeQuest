"""Part 4: Simgeler/kisaltmalar, icerik BSM referanslari, ve son kontroller."""
import os
from docx import Document

DST = os.path.join(os.path.dirname(__file__), "yeni_updated.docx")
doc = Document(DST)

def rp(para, new_text):
    if not para.runs:
        para.text = new_text
        return
    for run in para.runs:
        run.text = ""
    para.runs[0].text = new_text

def fp(doc, search, start=0):
    for i in range(start, len(doc.paragraphs)):
        if search in doc.paragraphs[i].text:
            return i
    return -1

# === BSM 401 referanslarini guncelle (icerik ve son sayfa) ===
for i, p in enumerate(doc.paragraphs):
    if "BSM 401" in p.text:
        new_text = p.text.replace("BSM 401", "BSM 498")
        new_text = new_text.replace("BİLGİSAYAR MÜHENDİSLİĞİ TASARIMI", "BİTİRME ÇALIŞMASI")
        rp(p, new_text)
        print(f"[{i}] BSM 401 -> BSM 498")

# === "hedeflenmektedir/planlanmaktadır/öngörülmüştür" gibi gelecek zaman kalıplarını düzelt ===
future_to_past = {
    "hedeflenmektedir": "gerçekleştirilmiştir",
    "planlanmaktadır": "uygulanmıştır",
    "planlanmıştır": "gerçekleştirilmiştir",
    "hedeflenmiştir": "gerçekleştirilmiştir",
    "öngörülmüştür": "geliştirilmiştir",
    "öngörülmektedir": "geliştirilmiştir",
    "tasarlanacaktır": "tasarlanmıştır",
    "kodlanacaktır": "kodlanmıştır",
    "sağlanacaktır": "sağlanmıştır",
    "yapılacaktır": "yapılmıştır",
    "geliştirilecektir": "geliştirilmiştir",
    "entegre edilecektir": "entegre edilmiştir",
    "kullanılacaktır": "kullanılmıştır",
    "gerçekleştirilecektir": "gerçekleştirilmiştir",
    "oluşturulacaktır": "oluşturulmuştur",
    "sunulacaktır": "sunulmuştur",
    "uygulanacaktır": "uygulanmıştır",
    "çalışacaktır": "çalışmıştır",
    "çalışacak olup": "çalışmış olup",
    "yapacaktır": "yapmıştır",
    "atanacaktır": "atanmıştır",
    "gönderilecektir": "gönderilmiştir",
    "doğrulanacaktır": "doğrulanmıştır",
    "barındıracaktır": "barındırmıştır",
    "işleyecektir": "işlemiştir",
    "olması planlanan": "olan",
    "oluşturulması planlanan": "oluşturulan",
    "içerecek olan": "içeren",
    "somutlaştırılacağı": "somutlaştırıldığı",
    "yönetileceği": "yönetildiği",
    "erişim sağlayacağı": "erişim sağladığı",
    "entegre edilmesi öngörülmüştür": "entegre edilmiştir",
    "geliştirilmesi hedeflenmiştir": "geliştirilmiştir",
    "yapılandırılması planlanmıştır": "yapılandırılmıştır",
    "geliştirilmesi öngörülmüştür": "geliştirilmiştir",
    "çalışacak olup": "çalışmış olup",
}

count = 0
for i, p in enumerate(doc.paragraphs):
    original = p.text
    modified = original
    for old, new in future_to_past.items():
        if old in modified:
            modified = modified.replace(old, new)
    if modified != original:
        rp(p, modified)
        count += 1

print(f"\n{count} paragrafta gelecek zaman -> gecmis zaman donusumu yapildi")

# === Anahtar kelimeler guncelle ===
i = fp(doc, "Anahtar Kelimeler:")
if i >= 0:
    rp(doc.paragraphs[i], 'Anahtar Kelimeler: Oyunlaştırma, Yapay Zeka, Kişisel Gelişim, Temiz Mimari, Sosyal Platform, Mikro Servis')
    print(f"[{i}] Anahtar kelimeler guncellendi")

# === Kaydet ===
doc.save(DST)
print(f"\nPart 4 tamamlandi! Dosya: {DST}")
