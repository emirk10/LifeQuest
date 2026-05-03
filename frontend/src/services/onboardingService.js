import axiosClient from '../api/axiosClient';

export const onboardingService = {
  completeOnboarding: async (answers) => {
    const { data } = await axiosClient.post('/onboarding/complete', answers);
    return data;
  },
  getStatus: async () => {
    const { data } = await axiosClient.get('/onboarding/status');
    return data;
  }
};
