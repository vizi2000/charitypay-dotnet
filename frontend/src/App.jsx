import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ErrorBoundary } from './components/ErrorBoundary';
import { Header } from './components/Header';
import { Footer } from './components/Footer';
import { AuthProvider, withAuth } from './contexts/AuthContext';
import { LanguageProvider } from './contexts/LanguageContext';
import { OrganizationList } from './pages/OrganizationList';
import { DonationPage } from './pages/DonationPage';
import { PaymentSuccess } from './pages/PaymentSuccess';
import { PaymentFailure } from './pages/PaymentFailure';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { AdminLayout } from './pages/admin/AdminLayout';
import { AdminDashboard } from './pages/admin/AdminDashboard';
import { OrganizationManagement } from './pages/admin/OrganizationManagement';
import { OrganizationLayout } from './pages/organization/OrganizationLayout';
import { OrganizationDashboard } from './pages/organization/OrganizationDashboard';
import { OrganizationProfile } from './pages/organization/OrganizationProfile';
import { OrganizationPage } from './pages/OrganizationPage';
import { NotFound } from './pages/NotFound';

// Protected admin components
const ProtectedAdminLayout = withAuth(AdminLayout, ['admin']);
const ProtectedAdminDashboard = withAuth(AdminDashboard, ['admin']);
const ProtectedOrganizationManagement = withAuth(OrganizationManagement, ['admin']);

// Protected organization components
const ProtectedOrganizationLayout = withAuth(OrganizationLayout, ['organization']);
const ProtectedOrganizationDashboard = withAuth(OrganizationDashboard, ['organization']);
const ProtectedOrganizationProfile = withAuth(OrganizationProfile, ['organization']);

function App() {
  return (
    <ErrorBoundary>
      <LanguageProvider>
        <AuthProvider>
          <Router>
          <Routes>
            {/* Auth routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            
            {/* Admin routes */}
            <Route path="/admin" element={<ProtectedAdminLayout />}>
              <Route index element={<ProtectedAdminDashboard />} />
              <Route path="organizations" element={<ProtectedOrganizationManagement />} />
              <Route path="users" element={<div className="p-6 text-center"><h2 className="text-xl font-bold text-slate-900">User Management</h2><p className="text-slate-600 mt-2">Coming soon...</p></div>} />
              <Route path="analytics" element={<div className="p-6 text-center"><h2 className="text-xl font-bold text-slate-900">Analytics</h2><p className="text-slate-600 mt-2">Coming soon...</p></div>} />
              <Route path="settings" element={<div className="p-6 text-center"><h2 className="text-xl font-bold text-slate-900">Settings</h2><p className="text-slate-600 mt-2">Coming soon...</p></div>} />
            </Route>
            
            {/* Organization routes */}
            <Route path="/organization" element={<ProtectedOrganizationLayout />}>
              <Route path="dashboard" element={<ProtectedOrganizationDashboard />} />
              <Route path="profile" element={<ProtectedOrganizationProfile />} />
              <Route path="payments" element={<div className="p-6 text-center"><h2 className="text-xl font-bold text-slate-900">Payment History</h2><p className="text-slate-600 mt-2">Coming soon...</p></div>} />
              <Route path="qr" element={<div className="p-6 text-center"><h2 className="text-xl font-bold text-slate-900">QR Code Management</h2><p className="text-slate-600 mt-2">Coming soon...</p></div>} />
              <Route path="settings" element={<div className="p-6 text-center"><h2 className="text-xl font-bold text-slate-900">Settings</h2><p className="text-slate-600 mt-2">Coming soon...</p></div>} />
            </Route>
            
            {/* Public routes with header/footer */}
            <Route path="/*" element={
              <div className="min-h-screen flex flex-col bg-slate-50">
                <Header />
                <main className="flex-1">
                  <Routes>
                    <Route path="/organizations" element={<OrganizationList />} />
                    <Route path="/organization/:organizationId" element={<OrganizationPage />} />
                    <Route path="/donate/:organizationId" element={<DonationPage />} />
                    <Route path="/payment/success" element={<PaymentSuccess />} />
                    <Route path="/payment/failure" element={<PaymentFailure />} />
                    <Route path="/:organizationId" element={<DonationPage />} />
                    <Route path="/" element={<OrganizationList />} />
                    <Route path="*" element={<NotFound />} />
                  </Routes>
                </main>
                <Footer />
              </div>
            } />
          </Routes>
          </Router>
        </AuthProvider>
      </LanguageProvider>
    </ErrorBoundary>
  );
}

export default App;
