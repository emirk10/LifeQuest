"""
SP-04: Basit Item-Based Collaborative Filtering
Benzer profildeki kullanıcıların tamamladığı görevlere dayalı öneri.
Yeni kullanıcılar (<7 gün) için content-based filtering'e fallback yapılır.
"""

from collections import defaultdict
from datetime import datetime, timedelta
from typing import Optional


def get_collaborative_recommendations(
    user_profile: dict,
    all_interactions: list[dict],
    top_n: int = 3
) -> list[str]:
    """
    Item-based collaborative filtering.
    
    Args:
        user_profile: {
            "user_id": str,
            "joined_days_ago": int,
            "general_level": float,
            "difficulty_preference": str,   # easy/medium/hard/mixed
            "user_goals": list[str],
            "completed_task_ids": list[str]
        }
        all_interactions: [
            {
                "user_id": str,
                "task_id": str,
                "task_category": str,
                "difficulty": int,    # 0=Easy, 1=Medium, 2=Hard
                "was_completed": bool,
                "user_level": float,
                "user_goals": list[str]
            },
            ...
        ]
        top_n: Kaç öneri döndürülsün
    
    Returns:
        Önerilen AppTaskId listesi (str)
    """
    # Yeni kullanıcı kontrolü — 7 günden az kayıtlıysa CF için yeterli veri yok
    if user_profile.get("joined_days_ago", 0) < 7:
        return []  # Caller content-based filtering kullanır

    user_id = user_profile.get("user_id", "")
    user_goals = set(g.lower() for g in (user_profile.get("user_goals") or []))
    user_level = user_profile.get("general_level", 1.0)
    already_completed = set(user_profile.get("completed_task_ids") or [])
    diff_pref = (user_profile.get("difficulty_preference") or "mixed").lower()

    if not all_interactions:
        return []

    # ── Benzer kullanıcıları bul ─────────────────────────────────────────────
    # Benzerlik kriterleri: level yakınlığı (±2), hedef örtüşmesi
    similar_user_completions: dict[str, int] = defaultdict(int)

    for interaction in all_interactions:
        if not interaction.get("was_completed"):
            continue
        if interaction.get("user_id") == user_id:
            continue  # kendisi değil

        # Level benzerliği (±2 aralık)
        other_level = interaction.get("user_level", 1.0)
        if abs(other_level - user_level) > 2.0:
            continue

        # Hedef örtüşmesi
        other_goals = set(g.lower() for g in (interaction.get("user_goals") or []))
        if user_goals and other_goals and not user_goals.intersection(other_goals):
            continue

        task_id = interaction.get("task_id", "")
        if not task_id or task_id in already_completed:
            continue

        # Zorluk filtresi (mixed hepsini alır)
        if diff_pref != "mixed":
            diff_map = {"easy": 0, "medium": 1, "hard": 2}
            if interaction.get("difficulty") != diff_map.get(diff_pref, 1):
                continue

        similar_user_completions[task_id] += 1

    # En çok tamamlanan görevleri sırala
    sorted_tasks = sorted(
        similar_user_completions.items(),
        key=lambda x: x[1],
        reverse=True
    )

    return [task_id for task_id, _ in sorted_tasks[:top_n]]


def build_user_profile_from_context(context: dict) -> dict:
    """
    AI servisi context dict'inden collaborative filtering profili çıkar.
    main.py içinde kullanılmak üzere yardımcı fonksiyon.
    """
    created_at_str = context.get("user_created_at")
    joined_days = 999  # default: eski kullanıcı
    if created_at_str:
        try:
            created_at = datetime.fromisoformat(created_at_str.replace("Z", "+00:00"))
            joined_days = (datetime.now(created_at.tzinfo) - created_at).days
        except Exception:
            pass

    return {
        "user_id": context.get("user_id", ""),
        "joined_days_ago": joined_days,
        "general_level": context.get("general_level", 1.0),
        "difficulty_preference": context.get("difficulty_preference", "mixed"),
        "user_goals": context.get("user_goals", []),
        "completed_task_ids": [
            t.get("app_task_id") for t in (context.get("tasks") or [])
            if t.get("status") == "Completed"
        ]
    }
