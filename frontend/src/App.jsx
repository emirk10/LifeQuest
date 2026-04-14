import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/authContext';

import DashBoard from './pages/DashBoard';
import TaskList from './pages/TaskList';
import LoginPage from './pages/LoginPage';
import Register from './pages/Register';
import BadgeGallery from './pages/BadgeGallery';
import ProfilePage from './pages/ProfilePage';
import AdminPage from './pages/AdminPage';
import AITestPage from './pages/AITestPage';
import GoalsPage from './pages/GoalsPage';
import ReportsPage from './pages/ReportsPage';

import PrivateRoute from './components/PrivateRoute';
import ChatBot from './components/ChatBot';

function App() {
  return (
    <Router>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<Register />} />
          
          <Route path="/" element={<Navigate to="/dashboard" replace />} />

          <Route 
            path="/dashboard" 
            element={
              <PrivateRoute>
                <DashBoard />
              </PrivateRoute>
            } 
          />
          
          <Route 
            path="/tasks" 
            element={
              <PrivateRoute>
                <TaskList />
              </PrivateRoute>
            } 
          />
          
          <Route 
            path="/badges" 
            element={
              <PrivateRoute>
                <BadgeGallery />
              </PrivateRoute>
            } 
          />
          
          <Route 
            path="/profile" 
            element={
              <PrivateRoute>
                <ProfilePage />
              </PrivateRoute>
            } 
          />

          <Route 
            path="/goals" 
            element={
              <PrivateRoute>
                <GoalsPage />
              </PrivateRoute>
            } 
          />

          <Route 
            path="/reports" 
            element={
              <PrivateRoute>
                <ReportsPage />
              </PrivateRoute>
            } 
          />
          
          <Route 
            path="/admin" 
            element={
              <PrivateRoute>
                <AdminPage />
              </PrivateRoute>
            } 
          />

          <Route 
            path="/ai-test" 
            element={
              <PrivateRoute>
                <AITestPage />
              </PrivateRoute>
            } 
          />

        </Routes>
        <ChatBot />
      </AuthProvider>
    </Router>
  );
}

export default App;