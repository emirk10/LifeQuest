"""
ActionExtractor — Kullanıcı mesajından hangi göreve atıfta bulunulduğunu çıkarır.
SP-02: Fuzzy matching ile görev adı/id eşleştirmesi.
"""

from difflib import SequenceMatcher
from dataclasses import dataclass


@dataclass
class ActionExtractionResult:
    task_id: str | None
    task_title: str | None
    confidence: float
    extraction_method: str  # "exact" | "fuzzy" | "latest_active" | "none"


class ActionExtractor:
    """
    Kullanıcı mesajından hangi göreve atıfta bulunulduğunu çıkarır.
    Önce exact match, sonra fuzzy match, son çare latest active görev.
    """

    def extract(self, message: str, user_tasks: list[dict]) -> ActionExtractionResult:
        """
        Args:
            message: Kullanıcı mesajı
            user_tasks: [{"id": str, "title": str, "status": str, ...}]
        """
        if not user_tasks:
            return ActionExtractionResult(None, None, 0.0, "none")

        msg_lower = message.lower()

        # 1. Exact title match
        for task in user_tasks:
            title = task.get("title", "").lower()
            if title and title in msg_lower:
                return ActionExtractionResult(
                    task_id=task["id"],
                    task_title=task["title"],
                    confidence=0.95,
                    extraction_method="exact"
                )

        # 2. Fuzzy match (SequenceMatcher)
        best_task = None
        best_ratio = 0.0
        for task in user_tasks:
            title = task.get("title", "")
            ratio = SequenceMatcher(None, msg_lower, title.lower()).ratio()
            if ratio > best_ratio:
                best_ratio = ratio
                best_task = task

        if best_task and best_ratio >= 0.45:
            return ActionExtractionResult(
                task_id=best_task["id"],
                task_title=best_task["title"],
                confidence=best_ratio,
                extraction_method="fuzzy"
            )

        # 3. Fallback: En son aktif görev
        active_tasks = [t for t in user_tasks if t.get("status") in ("InProgress", "Selected", "Suggested")]
        if active_tasks:
            t = active_tasks[0]
            return ActionExtractionResult(
                task_id=t["id"],
                task_title=t["title"],
                confidence=0.3,
                extraction_method="latest_active"
            )

        return ActionExtractionResult(None, None, 0.0, "none")
