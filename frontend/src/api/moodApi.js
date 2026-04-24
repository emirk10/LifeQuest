import axiosClient from './axiosClient';

// ─── Mood API ────────────────────────────────────────────────────────────────
export const moodApi = {
  recordDailyMood: (data) =>
    axiosClient.post('/api/mood/daily', data),

  getTodayMood: () =>
    axiosClient.get('/api/mood/today'),

  getMoodHistory: () =>
    axiosClient.get('/api/mood/history'),
};

// ─── Daily Task Pool API ──────────────────────────────────────────────────────
export const taskPoolApi = {
  getDailyPool: (count = 6) =>
    axiosClient.get(`/api/user-tasks/daily-pool?count=${count}`),

  selectTask: (userTaskId) =>
    axiosClient.post(`/api/user-tasks/${userTaskId}/select`),

  postponeTask: (userTaskId, postponeTo, skipReason) =>
    axiosClient.post(`/api/user-tasks/${userTaskId}/postpone`, { postponeTo, skipReason }),

  pauseTask: (userTaskId) =>
    axiosClient.post(`/api/user-tasks/${userTaskId}/pause`),

  resumeTask: (userTaskId) =>
    axiosClient.post(`/api/user-tasks/${userTaskId}/resume`),

  abandonTask: (userTaskId, reason) =>
    axiosClient.post(`/api/user-tasks/${userTaskId}/abandon`, { reason }),
};
