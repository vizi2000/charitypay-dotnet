import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  BuildingOfficeIcon,
  UsersIcon,
  CurrencyDollarIcon,
  ChartBarIcon,
  ArrowUpIcon,
  ArrowDownIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  ClockIcon,
  XCircleIcon
} from '@heroicons/react/24/outline';
import { getAdminStats, getAdminOrganizations } from '../../utils/auth';
import { useTranslation } from '../../contexts/LanguageContext';

export function AdminDashboard() {
  const [stats, setStats] = useState(null);
  const [recentOrganizations, setRecentOrganizations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const { t } = useTranslation();

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Load stats and recent organizations in parallel
      const [statsData, orgsData] = await Promise.all([
        getAdminStats(),
        getAdminOrganizations({ limit: 5, offset: 0 })
      ]);

      setStats(statsData);
      setRecentOrganizations(orgsData.items || []);
    } catch (err) {
      setError(err.message);
      console.error('Dashboard load error:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('pl-PL', {
      style: 'currency',
      currency: 'PLN',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  };

  const getStatusIcon = (status) => {
    switch (status) {
      case 'approved':
        return <CheckCircleIcon className="w-5 h-5 text-green-500" />;
      case 'pending':
        return <ClockIcon className="w-5 h-5 text-yellow-500" />;
      case 'rejected':
        return <XCircleIcon className="w-5 h-5 text-red-500" />;
      case 'suspended':
        return <ExclamationTriangleIcon className="w-5 h-5 text-orange-500" />;
      default:
        return <ClockIcon className="w-5 h-5 text-gray-500" />;
    }
  };

  const getStatusBadge = (status) => {
    const statusConfig = {
      approved: 'bg-green-100 text-green-800',
      pending: 'bg-yellow-100 text-yellow-800',
      rejected: 'bg-red-100 text-red-800',
      suspended: 'bg-orange-100 text-orange-800'
    };

    return statusConfig[status] || 'bg-gray-100 text-gray-800';
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
              <div className="animate-pulse">
                <div className="w-8 h-8 bg-slate-200 rounded-lg mb-4"></div>
                <div className="h-4 bg-slate-200 rounded mb-2"></div>
                <div className="h-6 bg-slate-200 rounded"></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-xl bg-red-50 border border-red-200 p-6">
        <div className="flex items-start">
          <ExclamationTriangleIcon className="w-6 h-6 text-red-500 mr-3 mt-0.5" />
          <div>
            <h3 className="text-lg font-semibold text-red-800">{t('admin.errorLoading')}</h3>
            <p className="text-red-700 mt-1">{error}</p>
            <button
              onClick={loadDashboardData}
              className="mt-4 btn-secondary"
            >
              {t('common.tryAgain')}
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-slate-900">{t('admin.dashboard')}</h1>
        <p className="text-slate-600 mt-1">
          {t('admin.dashboardSubtitle')}
        </p>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* Total Organizations */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-slate-600">{t('admin.totalOrganizations')}</p>
              <p className="text-3xl font-bold text-slate-900 mt-2">
                {stats?.total_organizations || 0}
              </p>
            </div>
            <div className="w-12 h-12 bg-blue-100 rounded-xl flex items-center justify-center">
              <BuildingOfficeIcon className="w-6 h-6 text-blue-600" />
            </div>
          </div>
          <div className="mt-4 flex items-center text-sm">
            <span className="text-green-600 font-medium">
              {stats?.active_organizations || 0} {t('admin.active')}
            </span>
            <span className="text-slate-500 ml-2">
              • {(stats?.total_organizations || 0) - (stats?.active_organizations || 0)} {t('admin.pending')}
            </span>
          </div>
        </div>

        {/* Total Donations */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-slate-600">{t('admin.totalDonations')}</p>
              <p className="text-3xl font-bold text-slate-900 mt-2">
                {formatCurrency(stats?.total_donations || 0)}
              </p>
            </div>
            <div className="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center">
              <CurrencyDollarIcon className="w-6 h-6 text-green-600" />
            </div>
          </div>
          <div className="mt-4 flex items-center text-sm">
            <span className="text-green-600 font-medium">
              {stats?.total_payments || 0} {t('admin.payments')}
            </span>
            <span className="text-slate-500 ml-2">
              • {formatCurrency(stats?.avg_donation || 0)} {t('admin.avg')}
            </span>
          </div>
        </div>

        {/* Today's Payments */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-slate-600">{t('admin.todaysPayments')}</p>
              <p className="text-3xl font-bold text-slate-900 mt-2">
                {stats?.payments_today || 0}
              </p>
            </div>
            <div className="w-12 h-12 bg-purple-100 rounded-xl flex items-center justify-center">
              <ChartBarIcon className="w-6 h-6 text-purple-600" />
            </div>
          </div>
          <div className="mt-4 flex items-center text-sm">
            {stats?.payments_today > 0 ? (
              <div className="flex items-center text-green-600">
                <ArrowUpIcon className="w-4 h-4 mr-1" />
                <span className="font-medium">{t('admin.active')}</span>
              </div>
            ) : (
              <div className="flex items-center text-slate-500">
                <ArrowDownIcon className="w-4 h-4 mr-1" />
                <span>{t('admin.noPaymentsToday')}</span>
              </div>
            )}
          </div>
        </div>

        {/* Active Users */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6 hover:shadow-md transition-shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-slate-600">{t('admin.activeUsers')}</p>
              <p className="text-3xl font-bold text-slate-900 mt-2">
                {stats?.active_organizations || 0}
              </p>
            </div>
            <div className="w-12 h-12 bg-orange-100 rounded-xl flex items-center justify-center">
              <UsersIcon className="w-6 h-6 text-orange-600" />
            </div>
          </div>
          <div className="mt-4 flex items-center text-sm">
            <span className="text-slate-600">{t('admin.organizationAccounts')}</span>
          </div>
        </div>
      </div>

      {/* Two Column Layout */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Recent Organizations */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-lg font-semibold text-slate-900">{t('admin.recentOrganizations')}</h2>
            <Link
              to="/admin/organizations"
              className="text-sm font-medium text-primary-600 hover:text-primary-700 transition-colors"
            >
              {t('admin.viewAll')}
            </Link>
          </div>

          <div className="space-y-4">
            {recentOrganizations.length > 0 ? (
              recentOrganizations.map((org) => (
                <div key={org.id} className="flex items-center justify-between p-4 rounded-lg border border-slate-100 hover:border-slate-200 transition-colors">
                  <div className="flex items-center space-x-3">
                    <div className="flex-shrink-0">
                      {getStatusIcon(org.status)}
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-medium text-slate-900 truncate">
                        {org.name}
                      </p>
                      <p className="text-xs text-slate-500 truncate">
                        {org.location} • {org.category}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center space-x-3">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusBadge(org.status)}`}>
                      {org.status || 'pending'}
                    </span>
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-8">
                <BuildingOfficeIcon className="w-12 h-12 text-slate-300 mx-auto mb-3" />
                <p className="text-slate-500">{t('admin.noOrganizationsYet')}</p>
              </div>
            )}
          </div>
        </div>

        {/* Top Performing Organizations */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-lg font-semibold text-slate-900">{t('admin.topPerformers')}</h2>
            <Link
              to="/admin/analytics"
              className="text-sm font-medium text-primary-600 hover:text-primary-700 transition-colors"
            >
              {t('admin.viewAnalytics')}
            </Link>
          </div>

          <div className="space-y-4">
            {stats?.top_organizations && stats.top_organizations.length > 0 ? (
              stats.top_organizations.map((org, index) => (
                <div key={org.id} className="flex items-center justify-between p-4 rounded-lg border border-slate-100">
                  <div className="flex items-center space-x-3">
                    <div className="flex-shrink-0">
                      <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold text-white ${
                        index === 0 ? 'bg-yellow-500' : 
                        index === 1 ? 'bg-gray-400' : 
                        index === 2 ? 'bg-orange-400' : 'bg-slate-400'
                      }`}>
                        {index + 1}
                      </div>
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-medium text-slate-900 truncate">
                        {org.name}
                      </p>
                      <p className="text-xs text-slate-500">
                        {formatCurrency(org.total_donations)}
                      </p>
                    </div>
                  </div>
                  <div className="flex-shrink-0">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusBadge(org.status)}`}>
                      {org.status || 'active'}
                    </span>
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-8">
                <ChartBarIcon className="w-12 h-12 text-slate-300 mx-auto mb-3" />
                <p className="text-slate-500">{t('admin.noDonationData')}</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
        <h2 className="text-lg font-semibold text-slate-900 mb-4">{t('admin.quickActions')}</h2>
        
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <Link
            to="/admin/organizations?status=pending"
            className="p-4 rounded-lg border border-slate-200 hover:border-primary-300 hover:bg-primary-50 transition-all group"
          >
            <ClockIcon className="w-8 h-8 text-yellow-500 mb-3 group-hover:text-yellow-600" />
            <h3 className="font-medium text-slate-900 group-hover:text-primary-700">{t('admin.reviewPending')}</h3>
            <p className="text-sm text-slate-500 mt-1">{t('admin.reviewPendingDesc')}</p>
          </Link>

          <Link
            to="/admin/organizations"
            className="p-4 rounded-lg border border-slate-200 hover:border-primary-300 hover:bg-primary-50 transition-all group"
          >
            <BuildingOfficeIcon className="w-8 h-8 text-blue-500 mb-3 group-hover:text-blue-600" />
            <h3 className="font-medium text-slate-900 group-hover:text-primary-700">{t('admin.manageOrganizations')}</h3>
            <p className="text-sm text-slate-500 mt-1">{t('admin.manageOrganizationsDesc')}</p>
          </Link>

          <Link
            to="/admin/users"
            className="p-4 rounded-lg border border-slate-200 hover:border-primary-300 hover:bg-primary-50 transition-all group"
          >
            <UsersIcon className="w-8 h-8 text-green-500 mb-3 group-hover:text-green-600" />
            <h3 className="font-medium text-slate-900 group-hover:text-primary-700">{t('admin.userManagement')}</h3>
            <p className="text-sm text-slate-500 mt-1">{t('admin.userManagementDesc')}</p>
          </Link>

          <Link
            to="/admin/analytics"
            className="p-4 rounded-lg border border-slate-200 hover:border-primary-300 hover:bg-primary-50 transition-all group"
          >
            <ChartBarIcon className="w-8 h-8 text-purple-500 mb-3 group-hover:text-purple-600" />
            <h3 className="font-medium text-slate-900 group-hover:text-primary-700">{t('admin.analytics')}</h3>
            <p className="text-sm text-slate-500 mt-1">{t('admin.viewAnalyticsDesc')}</p>
          </Link>
        </div>
      </div>
    </div>
  );
}