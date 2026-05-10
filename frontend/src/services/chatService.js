import axiosClient from '../api/axiosClient';

const chatService = {
  sendMessage: async (message) => {
    const response = await axiosClient.post('/chat/send', { message });
    return response.data;
  },

  // Bağlamsal motivasyon mesajı (mood + level dahil)
  getMotivation: async ({ moodHint, level, totalXp }) => {
    const parts = [];
    if (moodHint) parts.push(`Kullanıcı şu an ${moodHint} hissediyor.`);
    if (level) parts.push(`Seviye: ${level}.`);
    if (totalXp) parts.push(`Toplam XP: ${totalXp}.`);
    const message = `${parts.join(' ')} Kullanıcıya kısa, içten ve güçlü bir Türkçe motivasyon mesajı ver. Sadece mesajı yaz, başka şey ekleme.`;
    const response = await axiosClient.post('/chat/send', { message });
    return response.data;
  },
};

export default chatService;

