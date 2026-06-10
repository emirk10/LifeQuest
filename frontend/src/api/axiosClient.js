import axios from "axios";
import { toast } from "react-toastify";

const axiosClient = axios.create({
  baseURL: window?._env_?.VITE_API_BASE_URL || import.meta.env.VITE_API_BASE_URL,
  withCredentials: true, 
  headers: {
    "Content-Type": "application/json",
  },
});

axiosClient.interceptors.request.use(
  (config) => {
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    
    if (!error.response) {
      toast.error("Bağlantı sorunu, lütfen internetinizi kontrol edip tekrar deneyin. 🌐");
      return Promise.reject(error);
    }

    
    if (error.response.status === 401 && !originalRequest._retry) {
      const url = originalRequest.url.toLowerCase();
      if (url.includes("/auth/refresh-token") || url.includes("/auth/login")) {
        sessionStorage.removeItem("user_data");
        window.location.href = "/login";
        return Promise.reject(error);
      }

      originalRequest._retry = true;
      try {
        await axiosClient.post("/Auth/refresh-token");
        return axiosClient(originalRequest);
      } catch (refreshError) {
        sessionStorage.removeItem("user_data");
        window.location.href = "/login";
        return Promise.reject(refreshError);
      }
    }

    
    if (error.response.status === 403) {
      toast.error("Bu işlem için yetkiniz bulunmuyor. 🔒");
    }

    
    if (error.response.status >= 500) {
      toast.error("Sunucu tarafında bir sorun oluştu, ekip bilgilendirildi. 🛠️");
    }

    
    const errorCode = error.response.data?.errorCode || error.response.data?.ErrorCode;
    if (errorCode) {
       const { getFriendlyErrorMessage } = await import("../utils/errorMap");
       const message = getFriendlyErrorMessage(errorCode);
       toast.error(message);
       console.warn(`[API Error] ${errorCode}:`, error.response.data);
    }

    return Promise.reject(error);
  }
);

export default axiosClient;
