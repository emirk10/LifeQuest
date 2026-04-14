import axiosClient from "../api/axiosClient";

const goalService = {
  getGoals: async () => {
    const response = await axiosClient.get("/goals");
    return response.data?.goals || [];
  },

  createGoal: async (goalData) => {
    const response = await axiosClient.post("/goals", goalData);
    return response.data;
  }
};

export default goalService;
