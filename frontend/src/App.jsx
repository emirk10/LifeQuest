import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/authContext';
import { SignalRProvider } from './context/SignalRContext';
import ErrorBoundary from './components/ErrorBoundary';

import DashBoard from './pages/DashBoard';
import TaskList from './pages/TaskList';
import LoginPage from './pages/LoginPage';
import Register from './pages/Register';
import BadgeGallery from './pages/BadgeGallery';
import ProfilePage from './pages/ProfilePage';
import AdminPage from './pages/AdminPage';
import GoalsPage from './pages/GoalsPage';
import ReportsPage from './pages/ReportsPage';
import SocialPage from './pages/SocialPage';
import SocialFeedPage from './pages/SocialFeedPage';
import DailyTaskPage from './pages/DailyTaskPage';
import OnboardingPage from './pages/OnboardingPage';
import VerifyEmailPage from './pages/VerifyEmailPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import ResetPasswordPage from './pages/ResetPasswordPage';

import TaskPoolPage from './pages/TaskPoolPage';
import SharePage from './pages/SharePage';

import PrivateRoute from './components/PrivateRoute';
import ChatBotFlow from './components/chatbot/ChatBotFlow';

function App() {
  return (
    <ErrorBoundary>
      <Router>
        <AuthProvider>
          <SignalRProvider>
            <Routes>
              {/* ... routes ... */}
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<Register />} />
              <Route path="/verify-email" element={<VerifyEmailPage />} />
              <Route path="/forgot-password" element={<ForgotPasswordPage />} />
              <Route path="/reset-password" element={<ResetPasswordPage />} />
              <Route path="/share/:token" element={<SharePage />} />

              <Route path="/" element={<Navigate to="/dashboard" replace />} />

              <Route path="/dashboard" element={<PrivateRoute><DashBoard /></PrivateRoute>} />
              <Route path="/tasks" element={<PrivateRoute><TaskList /></PrivateRoute>} />
              <Route path="/task-pool" element={<PrivateRoute><TaskPoolPage /></PrivateRoute>} />
              <Route path="/badges" element={<PrivateRoute><BadgeGallery /></PrivateRoute>} />
              <Route path="/profile" element={<PrivateRoute><ProfilePage /></PrivateRoute>} />
              <Route path="/goals" element={<PrivateRoute><GoalsPage /></PrivateRoute>} />
              <Route path="/reports" element={<PrivateRoute><ReportsPage /></PrivateRoute>} />
              <Route path="/social" element={<PrivateRoute><SocialPage /></PrivateRoute>} />
              <Route path="/feed" element={<PrivateRoute><SocialFeedPage /></PrivateRoute>} />
              <Route path="/daily" element={<PrivateRoute><DailyTaskPage /></PrivateRoute>} />
              <Route path="/admin" element={<PrivateRoute><AdminPage /></PrivateRoute>} />
              <Route path="/onboarding" element={<PrivateRoute><OnboardingPage /></PrivateRoute>} />
            </Routes>
            <ChatBotFlow />
          </SignalRProvider>
        </AuthProvider>
      </Router>
    </ErrorBoundary>
  );
}

export default App;