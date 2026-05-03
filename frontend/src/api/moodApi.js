import axiosClient from './axiosClient';

// ─── Mood API ────────────────────────────────────────────────────────────────
export const moodApi = {
  recordDailyMood: (data) =>
    axiosClient.post('/mood/daily', data),

  getTodayMood: () =>
    axiosClient.get('/mood/today'),

  getMoodHistory: () =>
    axiosClient.get('/mood/history'),
};

// ─── Daily Task Pool API ──────────────────────────────────────────────────────
export const taskPoolApi = {
  getDailyPool: (count = 6) =>
    axiosClient.get(`/user-tasks/daily-pool?count=${count}`),

  selectTask: (userTaskId) =>
    axiosClient.post(`/user-tasks/${userTaskId}/select`),

  postponeTask: (userTaskId, postponeTo, skipReason) =>
    axiosClient.post(`/user-tasks/${userTaskId}/postpone`, { postponeTo, skipReason }),

  pauseTask: (userTaskId) =>
    axiosClient.post(`/user-tasks/${userTaskId}/pause`),

  resumeTask: (userTaskId) =>
    axiosClient.post(`/user-tasks/${userTaskId}/resume`),

  abandonTask: (userTaskId, reason) =>
    axiosClient.post(`/user-tasks/${userTaskId}/abandon`, { reason }),
};

