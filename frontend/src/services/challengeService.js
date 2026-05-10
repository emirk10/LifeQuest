import axiosClient from '../api/axiosClient';

export const challengeService = {
  sendInvite: async (inviteData) => {
    const { data } = await axiosClient.post('/social/challenges/invite', inviteData);
    return data;
  },
  respondToChallenge: async (challengeId, accept, message) => {
    const { data } = await axiosClient.patch(`/social/challenges/${challengeId}/respond`, { accept, message });
    return data;
  },
  getActiveChallenges: async (status = 'active') => {
    const { data } = await axiosClient.get(`/social/challenges?status=${status}`);
    return data;
  },
  getChallengeDetail: async (challengeId) => {
    const { data } = await axiosClient.get(`/social/challenges/${challengeId}`);
    return data;
  },
  cancelChallenge: async (challengeId) => {
    const { data } = await axiosClient.delete(`/social/challenges/${challengeId}`);
    return data;
  },
  getChallengeDetails: async (challengeId) => {
    const { data } = await axiosClient.get(`/social/challenges/${challengeId}/details`);
    return data;
  }
};

