import axiosClient from '../api/axiosClient';

const leaderboardService = {
  // Genel sıralama
  getLeaderboard: async () => {
    const response = await axiosClient.get('/users/leaderboard');
    return response.data.leaderboard || response.data || [];
  },
};

export default leaderboardService;
