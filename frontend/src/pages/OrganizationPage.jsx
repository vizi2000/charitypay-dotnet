import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { 
  ArrowLeftIcon,
  HeartIcon,
  ShareIcon,
  PhoneIcon,
  EnvelopeIcon,
  GlobeAltIcon,
  MapPinIcon,
  CalendarIcon,
  CheckCircleIcon
} from '@heroicons/react/24/outline';
import { useTranslation } from '../contexts/LanguageContext';
import { organizationsAPI } from '../utils/api';

export function OrganizationPage() {
  const { organizationId } = useParams();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [organization, setOrganization] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadOrganization();
  }, [organizationId]);

  const loadOrganization = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await organizationsAPI.getById(organizationId);
      // Map API response fields to frontend expected fields
      const mappedOrg = {
        id: response.data.id,
        name: response.data.name,
        description: response.data.description,
        category: response.data.category,
        location: response.data.location,
        target_amount: response.data.targetAmount,
        collected_amount: response.data.collectedAmount,
        contact_email: response.data.contactEmail,
        website: response.data.website,
        phone: response.data.phone,
        address: response.data.address,
        logo_url: response.data.logoUrl,
        cover_image_url: null, // Not in API yet
        primary_color: response.data.primaryColor,
        secondary_color: response.data.secondaryColor,
        custom_message: response.data.customMessage,
        status: response.data.status === 3 ? 'approved' : 'pending',
        created_at: response.data.createdAt
      };
      setOrganization(mappedOrg);
    } catch (err) {
      setError(err.message);
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
      month: 'long',
      day: 'numeric'
    });
  };

  const calculateProgress = () => {
    if (!organization || organization.target_amount === 0) return 0;
    return Math.min((organization.collected_amount / organization.target_amount) * 100, 100);
  };

  const handleDonate = () => {
    navigate(`/donate/${organizationId}`);
  };

  const handleShare = async () => {
    if (navigator.share) {
      try {
        await navigator.share({
          title: organization.name,
          text: organization.description,
          url: window.location.href
        });
      } catch (err) {
        if (import.meta.env.DEV) {
          console.error('Error sharing:', err);
        }
      }
    } else {
      // Fallback: copy to clipboard
      navigator.clipboard.writeText(window.location.href);
      // You could show a toast notification here
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-50">
        <div className="animate-pulse">
          <div className="h-64 bg-slate-200"></div>
          <div className="max-w-4xl mx-auto px-4 py-8">
            <div className="h-8 bg-slate-200 rounded w-1/3 mb-4"></div>
            <div className="h-4 bg-slate-200 rounded w-full mb-2"></div>
            <div className="h-4 bg-slate-200 rounded w-2/3"></div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center px-4">
        <div className="max-w-md w-full text-center">
          <div className="bg-red-50 border border-red-200 rounded-xl p-6">
            <h2 className="text-lg font-semibold text-red-800 mb-2">
              {t('common.organizationNotFound')}
            </h2>
            <p className="text-red-600 mb-4">{error}</p>
            <Link to="/organizations" className="btn-primary">
              {t('common.backToOrganizations')}
            </Link>
          </div>
        </div>
      </div>
    );
  }

  const progress = calculateProgress();
  const customStyles = organization.primary_color ? {
    '--primary-color': organization.primary_color,
    '--secondary-color': organization.secondary_color || organization.primary_color
  } : {};

  return (
    <div className="min-h-screen bg-slate-50" style={customStyles}>
      {/* Header */}
      <div className="bg-white border-b border-slate-200">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <Link 
              to="/organizations" 
              className="inline-flex items-center text-slate-600 hover:text-slate-900 transition-colors"
            >
              <ArrowLeftIcon className="w-5 h-5 mr-2" />
              {t('common.backToOrganizations')}
            </Link>
            
            <button
              onClick={handleShare}
              className="flex items-center px-3 py-2 text-slate-600 hover:text-slate-900 hover:bg-slate-100 rounded-lg transition-colors"
            >
              <ShareIcon className="w-5 h-5 mr-2" />
              {t('common.share')}
            </button>
          </div>
        </div>
      </div>

      {/* Hero Section */}
      <div 
        className="relative bg-gradient-to-r from-primary-600 to-secondary-600 text-white"
        style={{
          backgroundImage: organization.cover_image_url 
            ? `linear-gradient(rgba(0,0,0,0.5), rgba(0,0,0,0.5)), url(${organization.cover_image_url})`
            : `linear-gradient(to right, ${organization.primary_color || 'var(--primary-color)'}, ${organization.secondary_color || 'var(--secondary-color)'})`
        }}
      >
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
            <div>
              <div className="flex items-center mb-4">
                {organization.logo_url && (
                  <img 
                    src={organization.logo_url} 
                    alt={`${organization.name} logo`}
                    className="w-16 h-16 rounded-lg mr-4 bg-white p-2"
                  />
                )}
                <div>
                  <h1 className="text-4xl font-bold mb-2">{organization.name}</h1>
                  <div className="flex items-center text-sm opacity-90">
                    <MapPinIcon className="w-4 h-4 mr-1" />
                    <span>{organization.location}</span>
                    <span className="mx-2">•</span>
                    <span className="capitalize">{t(`categories.${organization.category}`)}</span>
                  </div>
                </div>
              </div>
              
              <p className="text-lg mb-6 opacity-90">
                {organization.description}
              </p>
              
              {organization.custom_message && (
                <div className="bg-white/20 backdrop-blur-sm rounded-xl p-4 mb-6">
                  <p className="text-sm italic">"{organization.custom_message}"</p>
                </div>
              )}
              
              <div className="flex items-center space-x-4">
                <button
                  onClick={handleDonate}
                  className="bg-white text-primary-600 hover:bg-primary-50 px-8 py-3 rounded-xl font-semibold transition-colors flex items-center"
                >
                  <HeartIcon className="w-5 h-5 mr-2" />
                  {t('common.donateNow')}
                </button>
                
                <div className="text-sm opacity-90">
                  <CalendarIcon className="w-4 h-4 inline mr-1" />
                  {t('common.activeSince')} {formatDate(organization.created_at)}
                </div>
              </div>
            </div>
            
            {/* Progress Card */}
            <div className="bg-white/10 backdrop-blur-sm rounded-2xl p-6">
              <div className="flex justify-between items-center mb-4">
                <span className="text-sm font-medium">
                  {formatCurrency(organization.collected_amount)} {t('common.raised')}
                </span>
                <span className="text-sm opacity-90">
                  {progress.toFixed(1)}% {t('common.completed')}
                </span>
              </div>
              
              <div className="w-full bg-white/20 rounded-full h-3 mb-4">
                <div 
                  className="bg-white h-3 rounded-full transition-all duration-500"
                  style={{ width: `${progress}%` }}
                />
              </div>
              
              <div className="flex justify-between text-sm">
                <span className="opacity-90">{t('common.goal')}: {formatCurrency(organization.target_amount)}</span>
                <span className="opacity-90">{formatCurrency(organization.target_amount - organization.collected_amount)} {t('common.remaining')}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left Column - Main Content */}
          <div className="lg:col-span-2 space-y-8">
            {/* About */}
            <div className="bg-white rounded-xl p-6 shadow-sm">
              <h2 className="text-xl font-semibold text-slate-900 mb-4">
                {t('organization.aboutOrganization')}
              </h2>
              <div className="prose prose-slate max-w-none">
                <p className="text-slate-600 leading-relaxed">
                  {organization.description}
                </p>
              </div>
            </div>

            {/* Statistics */}
            <div className="bg-white rounded-xl p-6 shadow-sm">
              <h2 className="text-xl font-semibold text-slate-900 mb-4">
                {t('organization.impact')}
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="text-center">
                  <div className="text-3xl font-bold text-primary-600 mb-2">
                    {formatCurrency(organization.collected_amount)}
                  </div>
                  <div className="text-sm text-slate-600">{t('common.totalRaised')}</div>
                </div>
                <div className="text-center">
                  <div className="text-3xl font-bold text-secondary-600 mb-2">
                    {Math.round(progress)}%
                  </div>
                  <div className="text-sm text-slate-600">{t('common.goalReached')}</div>
                </div>
                <div className="text-center">
                  <div className="text-3xl font-bold text-green-600 mb-2">
                    {organization.status === 'approved' ? (
                      <CheckCircleIcon className="w-8 h-8 mx-auto" />
                    ) : (
                      '⏳'
                    )}
                  </div>
                  <div className="text-sm text-slate-600">
                    {organization.status === 'approved' ? t('common.verified') : t('common.pending')}
                  </div>
                </div>
              </div>
            </div>

            {/* Recent Activity */}
            <div className="bg-white rounded-xl p-6 shadow-sm">
              <h2 className="text-xl font-semibold text-slate-900 mb-4">
                {t('organization.recentActivity')}
              </h2>
              <div className="space-y-4">
                {/* This would be populated with recent donations */}
                <div className="text-center py-8 text-slate-500">
                  <HeartIcon className="w-12 h-12 mx-auto mb-3 text-slate-300" />
                  <p>{t('organization.noRecentActivity')}</p>
                </div>
              </div>
            </div>
          </div>

          {/* Right Column - Sidebar */}
          <div className="space-y-6">
            {/* Donation Card */}
            <div className="bg-white rounded-xl p-6 shadow-sm">
              <h3 className="text-lg font-semibold text-slate-900 mb-4">
                {t('common.supportThisCause')}
              </h3>
              <button
                onClick={handleDonate}
                className="w-full bg-gradient-to-r from-primary-600 to-secondary-600 text-white py-3 px-4 rounded-xl font-semibold hover:from-primary-700 hover:to-secondary-700 transition-all flex items-center justify-center"
              >
                <HeartIcon className="w-5 h-5 mr-2" />
                {t('common.makeADonation')}
              </button>
              <p className="text-xs text-slate-500 text-center mt-2">
                {t('common.securePayment')}
              </p>
            </div>

            {/* Contact Information */}
            <div className="bg-white rounded-xl p-6 shadow-sm">
              <h3 className="text-lg font-semibold text-slate-900 mb-4">
                {t('common.contactInfo')}
              </h3>
              <div className="space-y-3">
                {organization.contact_email && (
                  <div className="flex items-center text-sm text-slate-600">
                    <EnvelopeIcon className="w-4 h-4 mr-3 text-slate-400" />
                    <a 
                      href={`mailto:${organization.contact_email}`}
                      className="hover:text-primary-600 transition-colors"
                    >
                      {organization.contact_email}
                    </a>
                  </div>
                )}
                
                {organization.phone && (
                  <div className="flex items-center text-sm text-slate-600">
                    <PhoneIcon className="w-4 h-4 mr-3 text-slate-400" />
                    <a 
                      href={`tel:${organization.phone}`}
                      className="hover:text-primary-600 transition-colors"
                    >
                      {organization.phone}
                    </a>
                  </div>
                )}
                
                {organization.website && (
                  <div className="flex items-center text-sm text-slate-600">
                    <GlobeAltIcon className="w-4 h-4 mr-3 text-slate-400" />
                    <a 
                      href={organization.website}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="hover:text-primary-600 transition-colors"
                    >
                      {t('common.visitWebsite')}
                    </a>
                  </div>
                )}
                
                {organization.address && (
                  <div className="flex items-start text-sm text-slate-600">
                    <MapPinIcon className="w-4 h-4 mr-3 text-slate-400 mt-0.5" />
                    <span>{organization.address}</span>
                  </div>
                )}
              </div>
            </div>

            {/* Trust & Safety */}
            <div className="bg-slate-50 rounded-xl p-6">
              <h3 className="text-lg font-semibold text-slate-900 mb-4">
                {t('common.trustAndSafety')}
              </h3>
              <div className="space-y-2 text-sm text-slate-600">
                <div className="flex items-center">
                  <CheckCircleIcon className="w-4 h-4 mr-2 text-green-500" />
                  <span>{t('common.verifiedOrganization')}</span>
                </div>
                <div className="flex items-center">
                  <CheckCircleIcon className="w-4 h-4 mr-2 text-green-500" />
                  <span>{t('common.securePayments')}</span>
                </div>
                <div className="flex items-center">
                  <CheckCircleIcon className="w-4 h-4 mr-2 text-green-500" />
                  <span>{t('common.transparentReporting')}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}