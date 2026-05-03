import axiosClient from "../api/axiosClient";

const goalService = {
  getGoals: async () => {
    const response = await axiosClient.get("/goals");
    return response.data?.goals || [];
  },

  getGoalDashboard: async () => {
    const response = await axiosClient.get("/goals/dashboard");
    return response.data;
  },

  createGoal: async (goalData) => {
    const response = await axiosClient.post("/goals", goalData);
    return response.data;
  },

  updateGoalProgress: async (goalId, incrementBy, note) => {
    const response = await axiosClient.patch(`/goals/${goalId}/progress`, { incrementBy, note });
    return response.data;
  },

  archiveGoal: async (goalId) => {
    const response = await axiosClient.patch(`/goals/${goalId}/archive`);
    return response.data;
  },

  addMilestone: async (goalId, milestone) => {
    const response = await axiosClient.post(`/goals/${goalId}/milestones`, milestone);
    return response.data;
  }
};

export default goalService;

