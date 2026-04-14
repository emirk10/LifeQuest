import json
import random
import uuid
import datetime

# Sentetik eğitim verisi üretici
# Bu betik, DB'ye doğrudan SQL olarak yazılabilecek sahte 'UserTaskInteractions' satırları üretir.

num_records = 5000
output_sql_file = "seed_interactions.sql"

# Örnek Guid'ler (Sizin gerçek db'de farklı olabilir ama rastgele guid yetecektir)
users = [str(uuid.uuid4()) for _ in range(50)]
tasks = [str(uuid.uuid4()) for _ in range(20)]

def calculate_completion_chance(user_level, user_cat_level, difficulty, hour_of_day, streak):
    # ML'in öğrenebileceği bir patern (pattern) yarat:
    # 1. Seviye uyumu iyiyse olasılık artar
    # 2. Streak yüksekse olasılık artar
    # 3. Öğlen ve akşam saatlerinde yapma olasılığı daha yüksek (örneğin 18:00 - 20:00)
    
    chance = 50 
    
    # Zorluk uyumu
    if difficulty == 1 and user_cat_level >= 1: chance += 20
    if difficulty == 2 and user_cat_level < 2: chance -= 30
    if difficulty == 2 and user_cat_level >= 2: chance += 10
    if difficulty == 3 and user_cat_level < 4: chance -= 40
    if difficulty == 3 and user_cat_level >= 4: chance += 15
    
    if streak > 5: chance += 10
    if streak > 30: chance += 20
    
    if 17 <= hour_of_day <= 21: chance += 15
    if 2 <= hour_of_day <= 6: chance -= 40
    
    return max(5, min(95, chance))


with open(output_sql_file, "w", encoding="utf-8") as f:
    f.write("INSERT INTO \"UserTaskInteractions\" (\"Id\", \"AppUserId\", \"AppTaskId\", \"UserLevel\", \"UserCategoryLevel\", \"TaskDifficulty\", \"TaskBaseXP\", \"DayOfWeek\", \"HourOfDay\", \"UserCompletedTaskCount\", \"UserStreakDays\", \"WasCompleted\", \"CompletionTimeHours\", \"Rating\", \"CreatedDate\", \"UpdatedDate\") VALUES\n")

    values = []
    
    for i in range(num_records):
        user_id = random.choice(users)
        task_id = random.choice(tasks)
        
        user_level = random.randint(1, 10)
        user_cat_level = max(1, user_level - random.randint(0, 3))
        task_difficulty = random.choice([1, 2, 3]) # 1: Easy, 2: Medium, 3: Hard
        task_base_xp = task_difficulty * 10
        
        day_of_week = random.randint(0, 6)
        hour_of_day = random.randint(0, 23)
        user_completed_count = user_level * random.randint(5, 10)
        streak = random.randint(0, 50)
        
        chance = calculate_completion_chance(user_level, user_cat_level, task_difficulty, hour_of_day, streak)
        was_completed = random.randint(1, 100) <= chance
        
        completion_time = round(random.uniform(0.5, 24.0), 2) if was_completed else 0.0
        rating = round(random.uniform(3.0, 5.0), 1) if was_completed else 0.0
        
        created_date = datetime.datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S')
        updated_date = created_date
        
        was_completed_sql = "true" if was_completed else "false"
        
        row_uuid = str(uuid.uuid4())
        
        values.append(f"('{row_uuid}', '{user_id}', '{task_id}', {user_level}, {user_cat_level}, {task_difficulty}, {task_base_xp}, {day_of_week}, {hour_of_day}, {user_completed_count}, {streak}, {was_completed_sql}, {completion_time}, {rating}, '{created_date}', '{updated_date}')")

    f.write(",\n".join(values) + ";")

print(f"{num_records} satır {output_sql_file} adlı dosyaya kaydedildi. (PostgreSQL ile içeri aktarılabilir)")
