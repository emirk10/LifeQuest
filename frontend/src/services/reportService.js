import axiosClient from "../api/axiosClient";

const reportService = {
  getReports: async () => {
    const response = await axiosClient.get("/reports");
    return response.data?.reports || [];
  }
};

export default reportService;
