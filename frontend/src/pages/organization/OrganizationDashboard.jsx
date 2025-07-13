import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  HeartIcon,
  UserGroupIcon,
  CurrencyDollarIcon,
  ArrowTrendingUpIcon,
  CalendarDaysIcon,
  EyeIcon,
  QrCodeIcon,
  ChartBarIcon
} from '@heroicons/react/24/outline';
import { useAuth } from '../../contexts/AuthContext';
import { useTranslation } from '../../contexts/LanguageContext';

export function OrganizationDashboard() {
  const { user } = useAuth();
  const { t } = useTranslation();
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // API call to get dashboard stats
      const response = await fetch('/api/organization/dashboard-stats', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });
      
      if (!response.ok) {
        throw new Error('Failed to load dashboard data');
      }
      
      const data = await response.json();
      setDashboardData(data);
    } catch (err) {
      setError(err.message);
      // Mock data for development
      setDashboardData({
        organization: {
          id: 'org_123',
          name: 'Przykładowa Organizacja',
          target_amount: 10000,
          collected_amount: 6500,
          primary_color: '#3B82F6',
          secondary_color: '#EF4444'
        },
        stats: {
          total_donations: 6500,
          total_donors: 25,
          average_donation: 260,
          progress_percentage: 65,
          donations_today: 3,
          amount_today: 450
        },
        recent_donations: [
          { id: '1', amount: 100, donor_name: 'Jan Kowalski', created_at: '2024-07-09T10:30:00' },
          { id: '2', amount: 250, donor_name: 'Maria Nowak', created_at: '2024-07-09T09:15:00' },
          { id: '3', amount: 100, donor_name: 'Anonymous', created_at: '2024-07-08T16:45:00' }
        ],
        daily_stats: []
      });
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('pl-PL', {
      style: 'currency',
      currency: 'PLN'
    }).format(amount);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('pl-PL', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="animate-pulse">
          <div className="h-8 bg-slate-200 rounded w-1/4 mb-4"></div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="bg-white rounded-xl p-6 shadow-sm">
                <div className="h-4 bg-slate-200 rounded w-3/4 mb-2"></div>
                <div className="h-8 bg-slate-200 rounded w-1/2"></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (error && !dashboardData) {
    return (
      <div className="text-center py-12">
        <div className="bg-red-50 border border-red-200 rounded-xl p-6 max-w-md mx-auto">
          <h3 className="text-lg font-semibold text-red-800 mb-2">{t('common.error')}</h3>
          <p className="text-red-600 mb-4">{error}</p>
          <button
            onClick={loadDashboardData}
            className="btn-primary"
          >
            {t('common.retry')}
          </button>
        </div>
      </div>
    );
  }

  const { organization, stats, recent_donations } = dashboardData;

  const statCards = [
    {
      name: t('organization.totalDonations'),
      value: formatCurrency(stats.total_donations),
      icon: CurrencyDollarIcon,
      color: 'text-green-600',
      bgColor: 'bg-green-50'
    },
    {
      name: t('organization.totalDonors'),
      value: stats.total_donors,
      icon: UserGroupIcon,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50'
    },
    {
      name: t('organization.averageDonation'),
      value: formatCurrency(stats.average_donation),
      icon: ArrowTrendingUpIcon,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50'
    },
    {
      name: t('organization.todaysDonations'),
      value: `${stats.donations_today} (${formatCurrency(stats.amount_today)})`,
      icon: CalendarDaysIcon,
      color: 'text-orange-600',
      bgColor: 'bg-orange-50'
    }
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-start">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">
            {t('organization.welcomeBack')}, {organization.name}!
          </h1>
          <p className="text-slate-600 mt-1">
            {t('organization.dashboardSubtitle')}
          </p>
        </div>
        <div className="flex space-x-3">
          <Link
            to={`/organization/${organization.id}`}
            className="btn-secondary flex items-center"
          >
            <EyeIcon className="w-5 h-5 mr-2" />
            {t('organization.viewPage')}
          </Link>
          <Link
            to="/organization/qr"
            className="btn-primary flex items-center"
          >
            <QrCodeIcon className="w-5 h-5 mr-2" />
            {t('organization.qrCode')}
          </Link>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {statCards.map((stat) => (
          <div key={stat.name} className="bg-white rounded-xl p-6 shadow-sm">
            <div className="flex items-center">
              <div className={`p-3 rounded-lg ${stat.bgColor}`}>
                <stat.icon className={`w-6 h-6 ${stat.color}`} />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-slate-600">{stat.name}</p>
                <p className="text-2xl font-bold text-slate-900">{stat.value}</p>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Progress Bar */}
      <div className="bg-white rounded-xl p-6 shadow-sm">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-slate-900">
            {t('organization.fundraisingProgress')}
          </h3>
          <span className="text-sm text-slate-500">
            {stats.progress_percentage.toFixed(1)}% {t('organization.completed')}
          </span>
        </div>
        <div className="w-full bg-slate-200 rounded-full h-3 mb-4">
          <div 
            className="bg-gradient-to-r from-primary-500 to-charity-500 h-3 rounded-full transition-all duration-300"
            style={{ width: `${Math.min(stats.progress_percentage, 100)}%` }}
          />
        </div>
        <div className="flex justify-between text-sm text-slate-600">
          <span>{formatCurrency(organization.collected_amount)} {t('organization.raised')}</span>
          <span>{t('organization.goal')}: {formatCurrency(organization.target_amount)}</span>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Donations */}
        <div className="bg-white rounded-xl p-6 shadow-sm">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-slate-900">
              {t('organization.recentDonations')}
            </h3>
            <Link
              to="/organization/payments"
              className="text-sm text-primary-600 hover:text-primary-700 font-medium"
            >
              {t('organization.viewAll')}
            </Link>
          </div>
          <div className="space-y-3">
            {recent_donations.slice(0, 5).map((donation) => (
              <div key={donation.id} className="flex items-center justify-between p-3 bg-slate-50 rounded-lg">
                <div className="flex items-center space-x-3">
                  <div className="w-8 h-8 bg-primary-100 rounded-full flex items-center justify-center">
                    <HeartIcon className="w-4 h-4 text-primary-600" />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-slate-900">
                      {donation.donor_name || 'Anonymous'}
                    </p>
                    <p className="text-xs text-slate-500">
                      {formatDate(donation.created_at)}
                    </p>
                  </div>
                </div>
                <span className="text-sm font-semibold text-green-600">
                  {formatCurrency(donation.amount)}
                </span>
              </div>
            ))}
            {recent_donations.length === 0 && (
              <div className="text-center py-8 text-slate-500">
                <HeartIcon className="w-12 h-12 mx-auto mb-3 text-slate-300" />
                <p>{t('organization.noDonations')}</p>
              </div>
            )}
          </div>
        </div>

        {/* Quick Actions */}
        <div className="bg-white rounded-xl p-6 shadow-sm">
          <h3 className="text-lg font-semibold text-slate-900 mb-4">
            {t('organization.quickActions')}
          </h3>
          <div className="space-y-3">
            <Link
              to="/organization/profile"
              className="flex items-center p-3 text-slate-700 hover:bg-slate-50 rounded-lg transition-colors"
            >
              <UserGroupIcon className="w-5 h-5 mr-3 text-slate-400" />
              <span>{t('organization.updateProfile')}</span>
            </Link>
            <Link
              to="/organization/qr"
              className="flex items-center p-3 text-slate-700 hover:bg-slate-50 rounded-lg transition-colors"
            >
              <QrCodeIcon className="w-5 h-5 mr-3 text-slate-400" />
              <span>{t('organization.downloadQR')}</span>
            </Link>
            <Link
              to="/organization/payments"
              className="flex items-center p-3 text-slate-700 hover:bg-slate-50 rounded-lg transition-colors"
            >
              <ChartBarIcon className="w-5 h-5 mr-3 text-slate-400" />
              <span>{t('organization.viewAnalytics')}</span>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}