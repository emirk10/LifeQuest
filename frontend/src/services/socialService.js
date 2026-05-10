import axiosClient from '../api/axiosClient';

const socialService = {
  // Arkadaş listesi
  getFriends: async () => {
    const response = await axiosClient.get('/social/friends');
    return response.data.friends || [];
  },

  // Bekleyen istekler
  getFriendRequests: async () => {
    const response = await axiosClient.get('/social/friend-requests');
    return response.data.requests || [];
  },

  // Kullanıcı ara
  searchUsers: async (query) => {
    const response = await axiosClient.get(`/social/users/search?query=${encodeURIComponent(query)}`);
    return response.data.users || [];
  },

  // Arkadaşlık isteği gönder
  sendFriendRequest: async (targetUserId) => {
    const response = await axiosClient.post(`/social/friend-request/${targetUserId}`);
    return response.data;
  },

  // İstek kabul et
  acceptFriendRequest: async (requestId) => {
    const response = await axiosClient.patch(`/social/friend-request/${requestId}/accept`);
    return response.data;
  },

  // İstek reddet
  rejectFriendRequest: async (requestId) => {
    const response = await axiosClient.patch(`/social/friend-request/${requestId}/reject`);
    return response.data;
  },

  // Arkadaşlık sil
  removeFriend: async (friendshipId) => {
    const response = await axiosClient.delete(`/social/friends/${friendshipId}`);
    return response.data;
  },

  // Mücadeleler
  getChallenges: async () => {
    const response = await axiosClient.get('/social/challenges');
    return response.data.challenges || [];
  },

  // Mücadele oluştur
  createChallenge: async (challengeData) => {
    const response = await axiosClient.post('/social/challenges', challengeData);
    return response.data;
  },
  
  // P4-T1: Engelleme işlemleri
  getBlockedUsers: async () => {
    const response = await axiosClient.get('/social/blocked-users');
    return response.data.blockedUsers || [];
  },
  blockUser: async (targetUserId) => {
    const response = await axiosClient.post(`/social/block/${targetUserId}`);
    return response.data;
  },
  unblockUser: async (targetUserId) => {
    const response = await axiosClient.post(`/social/unblock/${targetUserId}`);
    return response.data;
  },
  // P4-T2: Kudos gönder
  sendKudos: async (userTaskId) => {
    const response = await axiosClient.post('/social/kudos', { userTaskId });
    return response.data;
  },
};

export default socialService;
