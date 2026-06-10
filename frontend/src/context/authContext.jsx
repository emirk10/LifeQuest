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
    if (userData.onboardingCompleted) {
      navigate("/dashboard");
    } else {
      navigate("/onboarding");
    }
  };

  const updateUser = (updatedData) => {
    setUser((prev) => {
      const newUser = prev ? { ...prev, ...updatedData } : null;
      if (newUser) {
        sessionStorage.setItem("user_data", JSON.stringify(newUser));
      }
      return newUser;
    });
  };

  const logout = async () => {
    try {
      
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
    <AuthContext.Provider value={{ user, login, logout, loading, updateUser }}>
      {!loading && children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);