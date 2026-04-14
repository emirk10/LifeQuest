import axiosClient from '../api/axiosClient';

const chatService = {
  sendMessage: async (message) => {
    const response = await axiosClient.post('/chat/send', { message });
    return response.data;
  },
};

export default chatService;
