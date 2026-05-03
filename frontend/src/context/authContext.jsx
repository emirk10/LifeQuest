import React, { createContext, useState, useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(sessionStorage.getItem("token"));
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const storedToken = sessionStorage.getItem("token");
    const storedUser = sessionStorage.getItem("user_data");

    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(JSON.parse(storedUser));
    }
    setLoading(false);
  }, []);

  const login = (userData, userToken) => {
    sessionStorage.setItem("token", userToken);
    sessionStorage.setItem("user_data", JSON.stringify(userData));
    setToken(userToken);
    setUser(userData);
    navigate("/dashboard");
  };

  const logout = () => {
    sessionStorage.removeItem("token");
    sessionStorage.removeItem("user_data");
    setToken(null);
    setUser(null);
    navigate("/login");
  };

  return (
    <AuthContext.Provider value={{ user, token, login, logout, loading }}>
      {!loading && children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);