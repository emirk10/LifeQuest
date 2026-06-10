import axiosClient from '../api/axiosClient';

const socialService = {
  
  getFriends: async () => {
    const response = await axiosClient.get('/social/friends');
    return response.data.friends || [];
  },

  
  getFriendRequests: async () => {
    const response = await axiosClient.get('/social/friend-requests');
    return response.data.requests || [];
  },

  
  searchUsers: async (query) => {
    const response = await axiosClient.get(`/social/users/search?query=${encodeURIComponent(query)}`);
    return response.data.users || [];
  },

  
  sendFriendRequest: async (targetUserId) => {
    const response = await axiosClient.post(`/social/friend-request/${targetUserId}`);
    return response.data;
  },

  
  acceptFriendRequest: async (requestId) => {
    const response = await axiosClient.patch(`/social/friend-request/${requestId}/accept`);
    return response.data;
  },

  
  rejectFriendRequest: async (requestId) => {
    const response = await axiosClient.patch(`/social/friend-request/${requestId}/reject`);
    return response.data;
  },

  
  removeFriend: async (friendshipId) => {
    const response = await axiosClient.delete(`/social/friends/${friendshipId}`);
    return response.data;
  },

  
  getChallenges: async () => {
    const response = await axiosClient.get('/social/challenges');
    return response.data.challenges || [];
  },

  
  createChallenge: async (challengeData) => {
    const response = await axiosClient.post('/social/challenges', challengeData);
    return response.data;
  },
  
  
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
  
  sendKudos: async (userTaskId) => {
    const response = await axiosClient.post('/social/kudos', { userTaskId });
    return response.data;
  },
};

export default socialService;
