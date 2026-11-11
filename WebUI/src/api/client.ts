import axios from "axios";

export const api = axios.create({
  baseURL: "/api",
  headers: {
    "Content-Type": "application/json"
  }
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("X-Auth-Token") || localStorage.getItem("token");
  if (token) {
    config.headers["X-Auth-Token"] = token;
    config.headers["Authorization"] = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => {
    const resData = response.data;
    if (resData && typeof resData === "object") {
      if (resData.success === "777") {
        return response;
      } else if (resData.success === "401") {
        console.warn(`[API ErrorCode: 401] ${resData.message}`);
        localStorage.removeItem("X-Auth-Token");
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        if (window.location.pathname !== "/login") {
          window.location.href = "/login";
        }
        return response;
      } else if (resData.success && resData.success !== "777") {
        console.warn(`[API ErrorCode: ${resData.success}] ${resData.message}`);
      }
    }
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("X-Auth-Token");
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  }
);
