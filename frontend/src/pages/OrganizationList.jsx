import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { 
  HeartIcon,
  MapPinIcon,
  UsersIcon,
  SparklesIcon,
  ArrowRightIcon,
  QrCodeIcon
} from '@heroicons/react/24/outline';
import { organizationsAPI, utils } from '../utils/api';
import { useTranslation } from '../contexts/LanguageContext';

export function OrganizationList() {
  const { t } = useTranslation();
  const [organizations, setOrganizations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadOrganizations();
  }, []);

  const loadOrganizations = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await organizationsAPI.getAll();
      setOrganizations(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const getCategoryIcon = (category) => {
    const icons = {
      dzieci: '👶',
      religia: '⛪',
      zwierzeta: '🐾',
      edukacja: '📚',
      zdrowie: '🏥',
      inne: '🤝'
    };
    return icons[category] || '❤️';
  };

  const getCategoryColor = (category) => {
    const colors = {
      dzieci: 'bg-blue-100 text-blue-800',
      religia: 'bg-purple-100 text-purple-800',
      zwierzeta: 'bg-green-100 text-green-800',
      edukacja: 'bg-yellow-100 text-yellow-800',
      zdrowie: 'bg-red-100 text-red-800',
      inne: 'bg-gray-100 text-gray-800'
    };
    return colors[category] || 'bg-gray-100 text-gray-800';
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
          <p className="mt-4 text-slate-600">{t('common.loading')}</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center px-4">
        <div className="max-w-md w-full text-center">
          <div className="bg-red-50 border border-red-200 rounded-xl p-6">
            <h2 className="text-lg font-semibold text-red-800 mb-2">{t('common.error')}</h2>
            <p className="text-red-600 mb-4">{error}</p>
            <button
              onClick={loadOrganizations}
              className="btn-primary"
            >
              {t('common.tryAgain')}
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="bg-gradient-to-br from-primary-50 via-white to-charity-50 py-16 sm:py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold text-slate-900 mb-6">
              {t('home.hero.title')}
            </h1>
            <p className="text-xl sm:text-2xl text-slate-600 mb-8 max-w-3xl mx-auto">
              {t('home.hero.subtitle')}
            </p>
            <div className="flex items-center justify-center space-x-8 text-sm text-slate-500">
              <div className="flex items-center space-x-2">
                <SparklesIcon className="w-5 h-5" />
                <span>{t('home.hero.securePayments')}</span>
              </div>
              <div className="flex items-center space-x-2">
                <UsersIcon className="w-5 h-5" />
                <span>{t('home.hero.verifiedOrgs')}</span>
              </div>
              <div className="flex items-center space-x-2">
                <HeartIcon className="w-5 h-5" />
                <span>{t('home.hero.goesToCharity')}</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Organizations Grid */}
      <section className="py-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-slate-900 mb-4">
              {t('home.chooseOrg')}
            </h2>
            <p className="text-lg text-slate-600">
              {t('home.chooseOrgSubtitle')}
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {organizations.map((org) => {
              const progress = utils.calculateProgress(org.collected_amount, org.target_amount);
              
              return (
                <div
                  key={org.id}
                  className="card p-6 group hover:scale-[1.02] transition-all duration-200"
                >
                  {/* Category Badge */}
                  <div className="flex items-center justify-between mb-4">
                    <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${getCategoryColor(org.category)}`}>
                      <span className="mr-1">{getCategoryIcon(org.category)}</span>
                      {t(`categories.${org.category}`)}
                    </span>
                    <div className="flex items-center text-slate-500 text-sm">
                      <MapPinIcon className="w-4 h-4 mr-1" />
                      {org.location}
                    </div>
                  </div>

                  {/* Organization Name */}
                  <h3 className="text-xl font-semibold text-slate-900 mb-3 group-hover:text-primary-600 transition-colors">
                    {org.name}
                  </h3>

                  {/* Description */}
                  <p className="text-slate-600 mb-6 line-clamp-3">
                    {org.description}
                  </p>

                  {/* Progress */}
                  <div className="mb-6">
                    <div className="flex justify-between items-center mb-2">
                      <span className="text-sm font-medium text-slate-700">
                        {utils.formatCurrency(org.collected_amount)} {t('home.raised')}
                      </span>
                      <span className="text-sm text-slate-500">
                        {progress.toFixed(1)}%
                      </span>
                    </div>
                    <div className="progress-bar">
                      <div 
                        className="progress-fill"
                        style={{ width: `${Math.min(progress, 100)}%` }}
                      />
                    </div>
                    <div className="mt-1 text-sm text-slate-500">
                      {t('home.goal')} {utils.formatCurrency(org.target_amount)}
                    </div>
                  </div>

                  {/* Action Buttons */}
                  <div className="space-y-3">
                    <Link
                      to={`/${org.id}`}
                      className="btn-charity w-full group/btn"
                    >
                      <span>{t('home.donateNow')}</span>
                      <ArrowRightIcon className="w-5 h-5 ml-2 group-hover/btn:translate-x-1 transition-transform" />
                    </Link>
                    
                    {/* QR Code Management */}
                    <div className="flex space-x-2">
                      <a
                        href={`http://localhost:8000/api/organizations/${org.id}/qr`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex-1 btn-secondary text-sm py-2 flex items-center justify-center"
                        title="View QR Code"
                      >
                        <QrCodeIcon className="w-4 h-4 mr-1" />
                        {t('home.viewQR')}
                      </a>
                      <a
                        href={`http://localhost:8000/api/organizations/${org.id}/qr/download?size=400`}
                        download
                        className="flex-1 btn-secondary text-sm py-2 flex items-center justify-center"
                        title="Download QR Code for printing"
                      >
                        <QrCodeIcon className="w-4 h-4 mr-1" />
                        {t('home.downloadQR')}
                      </a>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>

          {organizations.length === 0 && !loading && (
            <div className="text-center py-12">
              <p className="text-slate-600 text-lg">{t('home.noOrganizations')}</p>
            </div>
          )}
        </div>
      </section>

      {/* Stats Section */}
      <section className="bg-slate-900 py-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 text-center">
            <div>
              <div className="text-4xl font-bold text-white mb-2">
                {organizations.length}+
              </div>
              <div className="text-slate-300">
                {t('home.stats.verifiedOrganizations')}
              </div>
            </div>
            <div>
              <div className="text-4xl font-bold text-white mb-2">
                {utils.formatCurrency(
                  organizations.reduce((sum, org) => sum + org.collected_amount, 0)
                )}
              </div>
              <div className="text-slate-300">
                {t('home.stats.totalDonations')}
              </div>
            </div>
            <div>
              <div className="text-4xl font-bold text-white mb-2">
                100%
              </div>
              <div className="text-slate-300">
                {t('home.stats.goesToCharity')}
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}