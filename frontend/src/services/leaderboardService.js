import axiosClient from '../api/axiosClient';

const leaderboardService = {
  // Genel sıralama (P4-T6: Filtreleme desteği eklendi)
  getLeaderboard: async (pageNumber = 1, pageSize = 20, filter = 'global', timeRange = 'alltime') => {
    const response = await axiosClient.get(`/users/leaderboard?PageNumber=${pageNumber}&PageSize=${pageSize}&Filter=${filter}&TimeRange=${timeRange}`);
    return response.data || { leaderboard: [], totalCount: 0 };
  },
};

export default leaderboardService;
