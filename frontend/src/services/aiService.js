import axios from "axios";


const aiClient = axios.create({
  baseURL: import.meta.env.VITE_AI_SERVICE_URL || "http://localhost:5001",
  headers: {
    "Content-Type": "application/json",
  },
});

const aiService = {
  
  healthCheck: async () => {
    const response = await aiClient.get("/health");
    return response.data;
  },

  
  chat: async (userId, message) => {
    const response = await aiClient.post("/api/chat", {
      user_id: userId,
      message: message,
    });
    return response.data;
  },

  
  verifyReflection: async (text, minWordCount = 10, taskCategory = null) => {
    const response = await aiClient.post("/api/verify-reflection", {
      text,
      min_word_count: minWordCount,
      task_category: taskCategory,
    });
    return response.data;
  },

  
  verifyImage: async (userId, taskId, taskCategory, imageBase64) => {
    const response = await aiClient.post("/api/verify-image", {
      user_id: userId,
      task_id: taskId,
      task_category: taskCategory,
      image_base64: imageBase64,
    });
    return response.data;
  },

  
  trainModels: async () => {
    const response = await aiClient.post("/api/train");
    return response.data;
  },
};

export default aiService;
