import axiosClient from "../api/axiosClient";

const userService = {
  getProfile: async () => {
    const response = await axiosClient.get("/users/profile");
    return response.data;
  },
  getLeaderboard: async () => {
    const response = await axiosClient.get("/users/leaderboard");
    return response.data;
  },
  assignRole: async (data) => {
    const response = await axiosClient.put("/roles/assign-user", data);
    return response.data;
  },
  getActivities: async () => {
    const response = await axiosClient.get("/users/activities");
    return response.data;
  },
  getUserStats: async () => {
    const response = await axiosClient.get("/users/stats");
    return response.data;
  },
  updateProfile: async (data) => {
    const response = await axiosClient.patch("/users/profile-fields", data);
    return response.data;
  },
};

export default userService;