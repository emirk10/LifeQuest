"""
LifeQuest AI - Image Verifier (Görsel Doğrulayıcı)

Görev tamamlama fotoğraflarını MobileNetV2 (ImageNet) kullanarak doğrular.
Transfer learning temelini hazır tutar; eğitilmiş özel model yoksa
ImageNet ön-eğitilmiş ağırlıkları ile fallback kategori eşleştirmesi yapar.

Mimari: MobileNetV2 → GlobalAveragePooling → Dense katmanları
"""

import os
import base64
import logging
from io import BytesIO

import numpy as np

logger = logging.getLogger(__name__)

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MODEL_PATH = os.path.join(BASE_DIR, "models", "image_verifier.h5")

# Görev kategorisi → ImageNet etiket eşleştirmeleri
# MobileNetV2 ImageNet top-5 çıktılarından kategori tespiti
CATEGORY_IMAGENET_MAP: dict[str, list[str]] = {
    "spor": [
        "running_shoe", "tennis_ball", "basketball", "soccer_ball",
        "barbell", "dumbbell", "gym", "swimming_trunks", "bicycle",
        "mountain_bike", "punching_bag", "ping-pong_ball", "volleyball",
        "rugby_ball", "racket", "ski", "snowboard", "surfboard",
        "paddleboard", "treadmill", "exercise", "jogging",
    ],
    "eğitim": [
        "book_jacket", "notebook", "laptop", "desktop_computer",
        "monitor", "keyboard", "library", "pencil_case", "pen",
        "binder", "envelope", "paper_towel", "screen", "projector",
        "desk", "bookshop", "comic_book", "magazine", "newspaper",
    ],
    "sağlık": [
        "broccoli", "banana", "orange", "lemon", "strawberry",
        "apple", "salad", "soup_bowl", "water_bottle", "water_jug",
        "pill_bottle", "stethoscope", "face_powder", "cucumber",
        "mushroom", "cauliflower", "zucchini", "bell_pepper",
    ],
    "kariyer": [
        "laptop", "desktop_computer", "monitor", "keyboard",
        "mouse", "notebook", "binder", "projector", "screen",
        "desk", "office", "printer", "photocopier", "whiteboard",
    ],
    "sosyal": [
        "restaurant", "coffee_mug", "cup", "wine_glass", "beer_glass",
        "plate", "dining_table", "park_bench", "church", "cinema",
        "stage", "throne", "balloon", "confetti", "candle",
    ],
    "meditasyon": [
        "prayer_rug", "candle", "church", "monastery", "zen_garden",
        "fountain", "park", "lakeside", "beach", "mountain",
    ],
}


class ImageVerifier:
    """MobileNetV2 tabanlı görsel doğrulama servisi."""

    def __init__(self):
        self.model = None
        self.preprocess_fn = None
        self.decode_fn = None
        self._initialized = False

    def _ensure_initialized(self):
        """TensorFlow'u lazy-load et (başlangıç hızı için)."""
        if self._initialized:
            return

        try:
            # TF import'ları sadece ilk kullanımda
            import tensorflow as tf
            from tensorflow.keras.applications.mobilenet_v2 import (
                MobileNetV2,
                preprocess_input,
                decode_predictions,
            )

            self.preprocess_fn = preprocess_input
            self.decode_fn = decode_predictions

            # Eğitilmiş özel model var mı?
            if os.path.exists(MODEL_PATH):
                logger.info("Eğitilmiş özel model yükleniyor: %s", MODEL_PATH)
                self.model = tf.keras.models.load_model(MODEL_PATH)
            else:
                logger.info("ImageNet ön-eğitilmiş MobileNetV2 yükleniyor (fallback)")
                self.model = MobileNetV2(
                    weights="imagenet",
                    include_top=True,
                    input_shape=(224, 224, 3),
                )

            self._initialized = True

        except ImportError:
            logger.warning(
                "TensorFlow yüklü değil. Image Verifier devre dışı."
            )
            self._initialized = False

    # ------------------------------------------------------------------ #
    #  DOĞRULAMA
    # ------------------------------------------------------------------ #

    def verify(self, image_base64: str, expected_category: str) -> dict:
        """Base64 görüntüyü doğrula.

        Args:
            image_base64:      Base64 kodlanmış görüntü
            expected_category: Beklenen görev kategorisi (ör: "spor", "eğitim")

        Returns:
            {
                "is_valid": bool,
                "confidence": float,
                "detected_labels": list[str],
                "expected_category": str,
                "message": str,
            }
        """
        self._ensure_initialized()

        if not self._initialized or self.model is None:
            return {
                "is_valid": False,
                "confidence": 0.0,
                "detected_labels": [],
                "expected_category": expected_category,
                "message": "⚠️ Görüntü doğrulama servisi kullanılamıyor (TensorFlow yüklü değil).",
            }

        try:
            # Base64 → numpy array
            img_array = self._preprocess_image(image_base64)

            # Tahmin yap
            predictions = self.model.predict(img_array, verbose=0)
            top_labels = self.decode_fn(predictions, top=10)[0]

            detected_labels = [
                {"label": label, "confidence": round(float(score), 4)}
                for (_, label, score) in top_labels
            ]

            # Kategori eşleştirmesi: ImageNet etiketleri ile karşılaştır
            is_valid, match_confidence = self._match_category(
                top_labels, expected_category
            )

            if is_valid:
                message = (
                    f"✅ Görüntü '{expected_category}' kategorisi ile eşleşiyor "
                    f"(güven: %{match_confidence * 100:.0f})"
                )
            else:
                message = (
                    f"❌ Görüntü '{expected_category}' kategorisi ile eşleşmiyor. "
                    f"Tespit edilen: {top_labels[0][1]} (%{top_labels[0][2] * 100:.0f})"
                )

            return {
                "is_valid": is_valid,
                "confidence": round(match_confidence, 4),
                "detected_labels": detected_labels,
                "expected_category": expected_category,
                "message": message,
            }

        except Exception as e:
            logger.error("Görüntü doğrulama hatası: %s", e)
            return {
                "is_valid": False,
                "confidence": 0.0,
                "detected_labels": [],
                "expected_category": expected_category,
                "message": f"⚠️ Görüntü işlenirken hata oluştu: {str(e)}",
            }

    # ------------------------------------------------------------------ #
    #  TRANSFER LEARNING MODELİ OLUŞTURMA (İleride kullanılacak)
    # ------------------------------------------------------------------ #

    def build_custom_model(self, num_classes: int):
        """MobileNetV2 üzerine özel sınıflandırıcı katmanları ekle.

        Bu metod, gerçek kullanıcı verileri toplandığında
        fine-tuning için kullanılacaktır.
        """
        import tensorflow as tf
        from tensorflow.keras.applications import MobileNetV2 as MV2
        from tensorflow.keras.layers import Dense, GlobalAveragePooling2D, Dropout
        from tensorflow.keras.models import Model

        base_model = MV2(
            weights="imagenet",
            include_top=False,
            input_shape=(224, 224, 3),
        )

        # Son 20 katmanı eğitilebilir yap
        for layer in base_model.layers[:-20]:
            layer.trainable = False
        for layer in base_model.layers[-20:]:
            layer.trainable = True

        x = base_model.output
        x = GlobalAveragePooling2D()(x)
        x = Dense(256, activation="relu")(x)
        x = Dropout(0.5)(x)
        x = Dense(128, activation="relu")(x)
        x = Dropout(0.3)(x)
        predictions = Dense(num_classes, activation="softmax")(x)

        model = Model(inputs=base_model.input, outputs=predictions)
        model.compile(
            optimizer=tf.keras.optimizers.Adam(learning_rate=0.0001),
            loss="categorical_crossentropy",
            metrics=["accuracy"],
        )

        return model

    # ------------------------------------------------------------------ #
    #  YARDIMCI METODLAR
    # ------------------------------------------------------------------ #

    def _preprocess_image(self, img_base64: str) -> np.ndarray:
        """Base64 → (1, 224, 224, 3) numpy array."""
        from PIL import Image

        img_bytes = base64.b64decode(img_base64)
        img = Image.open(BytesIO(img_bytes)).convert("RGB")
        img = img.resize((224, 224))
        img_array = np.array(img, dtype=np.float32)
        img_array = np.expand_dims(img_array, axis=0)
        img_array = self.preprocess_fn(img_array)
        return img_array

    def _match_category(
        self,
        top_labels: list,
        expected_category: str,
    ) -> tuple[bool, float]:
        """ImageNet etiketlerini görev kategorisi ile karşılaştır.

        Returns:
            (is_valid, confidence)
        """
        category_key = expected_category.lower().strip()

        # Eşleşecek anahtar kelimeleri bul
        match_keywords: list[str] = []
        for cat, keywords in CATEGORY_IMAGENET_MAP.items():
            if cat in category_key or category_key in cat:
                match_keywords = keywords
                break

        if not match_keywords:
            # Bilinmeyen kategori → kabul et (güven düşük)
            return True, 0.3

        # Top-10 etiketlerden herhangi biri eşleşiyor mu?
        best_confidence = 0.0
        for _, label, score in top_labels:
            label_lower = label.lower().replace(" ", "_")
            for keyword in match_keywords:
                if keyword in label_lower or label_lower in keyword:
                    best_confidence = max(best_confidence, float(score))

        is_valid = best_confidence > 0.05  # %5 güven eşiği
        return is_valid, best_confidence
