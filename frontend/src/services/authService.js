import axiosClient from '../api/axiosClient';

const authService = {
  login: async (credentials) => {
    const response = await axiosClient.post('/Auth/Login', credentials);
    return response.data;
  },

  register: async (userData) => {
    const response = await axiosClient.post('/Auth/Register', userData);
    return response.data;
  },

  verifyEmail: async (token) => {
    const response = await axiosClient.patch(`/Auth/verify-email?token=${token}`);
    return response.data;
  },

  forgotPassword: async (email) => {
    const response = await axiosClient.post('/Auth/forgot-password', { email });
    return response.data;
  },

  resetPassword: async (resetData) => {
    const response = await axiosClient.post('/Auth/reset-password', resetData);
    return response.data;
  }
};

export default authService;