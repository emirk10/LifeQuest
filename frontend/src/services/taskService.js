import axiosClient from '../api/axiosClient';

const taskService = {
  getAllTasks: async (categoryId = null) => {
    const url = categoryId ? `/user-tasks?categoryId=${categoryId}` : '/user-tasks';
    const response = await axiosClient.get(url);
    return response.data.tasks || [];
  },

  completeTask: async (userTaskId) => {
    const response = await axiosClient.patch(`/user-tasks/${userTaskId}/complete`);
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
  }
};

export default taskService;