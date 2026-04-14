import axios from "axios";

// Python AI Mikro Servis (port 5001)
const aiClient = axios.create({
  baseURL: "http://localhost:5001",
  headers: {
    "Content-Type": "application/json",
  },
});

const aiService = {
  // Sağlık kontrolü
  healthCheck: async () => {
    const response = await aiClient.get("/health");
    return response.data;
  },

  // Chatbot — mesaj gönder
  chat: async (userId, message) => {
    const response = await aiClient.post("/api/chat", {
      user_id: userId,
      message: message,
    });
    return response.data;
  },

  // Yansıma metni doğrulama
  verifyReflection: async (text, minWordCount = 10, taskCategory = null) => {
    const response = await aiClient.post("/api/verify-reflection", {
      text,
      min_word_count: minWordCount,
      task_category: taskCategory,
    });
    return response.data;
  },

  // Görüntü doğrulama
  verifyImage: async (userId, taskId, taskCategory, imageBase64) => {
    const response = await aiClient.post("/api/verify-image", {
      user_id: userId,
      task_id: taskId,
      task_category: taskCategory,
      image_base64: imageBase64,
    });
    return response.data;
  },

  // Modelleri yeniden eğit (Admin)
  trainModels: async () => {
    const response = await aiClient.post("/api/train");
    return response.data;
  },
};

export default aiService;
