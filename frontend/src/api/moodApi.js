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

// ─── Mood Service (Merged) ───────────────────────────────────────────────────
export const moodService = {
  recordDailyMood: async (moodData) => {
    const response = await axiosClient.post('/mood/daily', moodData);
    return response.data;
  },

  getTodayMood: async () => {
    const response = await axiosClient.get('/mood/today');
    return response.data;
  },

  getMoodHistory: async () => {
    const response = await axiosClient.get('/mood/history');
    return response.data;
  }
};

// ─── Daily Task Pool API ──────────────────────────────────────────────────────
export const taskPoolApi = {
  getDailyPool: (count = 6) =>
    axiosClient.get(`/user-tasks/daily-pool?count=${count}`),

  selectTask: (userTaskId) =>
    axiosClient.post(`/user-tasks/${userTaskId}/select`),

  startTask: (userTaskId) =>
    axiosClient.post(`/user-tasks/${userTaskId}/start`),

  postponeTask: (userTaskId, postponeTo, skipReason) =>
    axiosClient.post(`/user-tasks/${userTaskId}/postpone`, { postponeTo, skipReason }),

  cancelPostponeTask: (userTaskId) =>
    axiosClient.post(`/user-tasks/${userTaskId}/cancel-postpone`),

  pauseTask: (userTaskId) =>
    axiosClient.post(`/user-tasks/${userTaskId}/pause`),

  resumeTask: (userTaskId) =>
    axiosClient.post(`/user-tasks/${userTaskId}/resume`),

  abandonTask: (userTaskId, reason) =>
    axiosClient.post(`/user-tasks/${userTaskId}/abandon`, { reason }),
};
