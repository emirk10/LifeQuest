import axiosClient from '../api/axiosClient';

const chatService = {
  sendMessage: async (message, saveToHistory = true) => {
    const response = await axiosClient.post('/chat/send', { message, saveToHistory });
    return response.data;
  },

  getHistory: async (limit = 50) => {
    const response = await axiosClient.get(`/chat/history?limit=${limit}`);
    return response.data;
  },

  
  getMotivation: async ({ moodHint, level, totalXp }) => {
    const parts = [];
    if (moodHint) parts.push(`Kullanıcı şu an ${moodHint} hissediyor.`);
    if (level) parts.push(`Seviye: ${level}.`);
    if (totalXp) parts.push(`Toplam XP: ${totalXp}.`);
    const message = `${parts.join(' ')} Kullanıcıya kısa, içten ve güçlü bir Türkçe motivasyon mesajı ver. Sadece mesajı yaz, başka şey ekleme.`;
    const response = await axiosClient.post('/chat/send', { message });
    return response.data;
  },

  processAction: async (messageId, actionType, taskId, payload) => {
    const response = await axiosClient.post('/chat/action', {
      messageId,
      actionType,
      taskId,
      payload
    });
    return response.data;
  },
};

export default chatService;

