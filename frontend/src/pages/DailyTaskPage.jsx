import { useState, useEffect } from 'react';
import DailyMoodSelector from '../components/DailyMoodSelector';
import TaskPoolSelector from '../components/TaskPoolSelector';
import { moodApi, taskPoolApi } from '../api/moodApi';
import MainLayout from '../layouts/MainLayout';
import '../styles/DailyTaskPage.css';

export default function DailyTaskPage() {
  const [phase, setPhase] = useState('loading'); // loading | mood | pool
  const [poolData, setPoolData] = useState(null);
  const [poolLoading, setPoolLoading] = useState(false);

  // Sayfa açıldığında bugün zaten mood girildi mi kontrol et
  useEffect(() => {
    checkTodayMood();
  }, []);

  const checkTodayMood = async () => {
    try {
      const res = await moodApi.getTodayMood();
      if (res.data?.data) {
        // Bugün mood girilmiş — direkt pool'a geç
        await loadPool();
        setPhase('pool');
      } else {
        setPhase('mood');
      }
    } catch {
      setPhase('mood');
    }
  };

  const handleMoodRecorded = async () => {
    await loadPool();
    setPhase('pool');
  };

  const loadPool = async () => {
    setPoolLoading(true);
    try {
      const res = await taskPoolApi.getDailyPool(6);
      setPoolData(res.data?.data || res.data || null);
    } catch (err) {
      console.error('Pool yüklenemedi:', err);
    } finally {
      setPoolLoading(false);
    }
  };

  const handleTaskSelect = async (task) => {
    try {
      await taskPoolApi.selectTask(task.userTaskId);
    } catch (err) {
      console.error('Görev seçilemedi:', err);
    }
  };

  return (
    <MainLayout>
      <div className="daily-task-page">
        {/* Page Header */}
        <div className="daily-task-page__header">
          <div>
            <h1 className="text-3xl font-black text-white italic tracking-tighter mb-2">🎯 Günlük Görevler</h1>
            <p className="text-slate-400 text-sm">Ruh haline göre kişiselleştirilmiş görevler</p>
          </div>
          {phase === 'pool' && (
            <button
              className="mood-reset-btn bg-indigo-600 hover:bg-indigo-500 text-white font-bold px-4 py-2 rounded-xl transition-colors"
              onClick={() => setPhase('mood')}
            >
              😊 Moodunu Güncelle
            </button>
          )}
        </div>

        {/* Content */}
        {phase === 'loading' && (
          <div className="daily-task-page__loading flex flex-col items-center justify-center p-12">
            <div className="loading-spinner w-8 h-8 border-t-2 border-indigo-500 rounded-full animate-spin mb-4" />
            <p className="text-slate-400">Yükleniyor...</p>
          </div>
        )}

        {phase === 'mood' && (
          <DailyMoodSelector onMoodRecorded={handleMoodRecorded} />
        )}

        {phase === 'pool' && (
          poolLoading ? (
            <div className="daily-task-page__loading flex flex-col items-center justify-center p-12">
              <div className="loading-spinner w-8 h-8 border-t-2 border-indigo-500 rounded-full animate-spin mb-4" />
              <p className="text-slate-400">Görevler hazırlanıyor...</p>
            </div>
          ) : (
            <TaskPoolSelector
              poolData={poolData}
              onTaskSelect={handleTaskSelect}
              onRefresh={loadPool}
            />
          )
        )}
      </div>
    </MainLayout>
  );
}

