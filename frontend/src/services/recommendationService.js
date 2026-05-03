import axiosClient from '../api/axiosClient';

const recommendationService = {
  getDailyPool: async () => {
    const response = await axiosClient.get('/user-tasks/daily-pool');
    return response.data;
  },

  selectRecommendation: async (taskId) => {
    const response = await axiosClient.post(`/user-tasks/${taskId}/select`);
    return response.data;
  }
};

export default recommendationService;
