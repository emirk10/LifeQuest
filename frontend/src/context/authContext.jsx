import React, { createContext, useState, useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const storedUser = sessionStorage.getItem("user_data");

    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
    setLoading(false);
  }, []);

  const login = (userData) => {
    sessionStorage.setItem("user_data", JSON.stringify(userData));
    setUser(userData);
    navigate("/dashboard");
  };

  const logout = async () => {
    try {
      // Import and call API to clear cookies
      const axiosClient = (await import("../api/axiosClient")).default;
      await axiosClient.post("/Auth/revoke");
    } catch (err) {
      console.error("Logout API failed", err);
    }
    sessionStorage.removeItem("user_data");
    setUser(null);
    navigate("/login");
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, loading }}>
      {!loading && children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);