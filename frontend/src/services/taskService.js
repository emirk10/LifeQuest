import axiosClient from '../api/axiosClient';

const taskService = {
  getAllTasks: async (categoryId = null, page = 1, pageSize = 10) => {
    let url = `/user-tasks?page=${page}&pageSize=${pageSize}`;
    if (categoryId) url += `&categoryId=${categoryId}`;
    const response = await axiosClient.get(url);
    return response.data;
  },

  completeTask: async (userTaskId, reflectionText = null, customTags = []) => {
    const response = await axiosClient.patch(`/user-tasks/${userTaskId}/complete`, {
      reflectionText,
      customTags
    });
    return response.data;
  },

  requestNewQuests: async () => {
    const response = await axiosClient.post('/user-tasks/request-new');
    return response.data;
  },

  assignSpecificTask: async (appTaskId) => {
    const response = await axiosClient.post(`/user-tasks/assign/${appTaskId}`);
    return response.data;
  },

  createTask: async (taskData) => {
    return await axiosClient.post('/Tasks/create-task', taskData);
  },

  createBulkTasks: async (tasksArray) => {
    return await axiosClient.post('/Tasks/create-bulk-tasks', { tasks: tasksArray });
  },

  // ─── SP-01: Yeni Görev Aksiyonları ─────────────────────────────────────
  getDailyPool: async () => {
    const response = await axiosClient.get('/user-tasks/daily-pool');
    return response.data;
  },

  selectTask: async (taskId) => {
    const response = await axiosClient.post(`/user-tasks/${taskId}/select`);
    return response.data;
  },

  startTask: async (taskId) => {
    const response = await axiosClient.post(`/user-tasks/${taskId}/start`);
    return response.data;
  },

  postponeTask: async (taskId, postponeTo, reason) => {
    const response = await axiosClient.post(`/user-tasks/${taskId}/postpone`, {
      postponeTo,
      reason
    });
    return response.data;
  },

  pauseTask: async (taskId) => {
    const response = await axiosClient.post(`/user-tasks/${taskId}/pause`);
    return response.data;
  },

  resumeTask: async (taskId) => {
    const response = await axiosClient.post(`/user-tasks/${taskId}/resume`);
    return response.data;
  },

  abandonTask: async (taskId, reason) => {
    const response = await axiosClient.post(`/user-tasks/${taskId}/abandon`, {
      reason
    });
    return response.data;
  },

  cancelTask: async (taskId) => {
    const response = await axiosClient.post(`/user-tasks/${taskId}/cancel`);
    return response.data;
  }
};

export default taskService;