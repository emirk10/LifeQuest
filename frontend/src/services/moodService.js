import axiosClient from '../api/axiosClient';

const moodService = {
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

export default moodService;
