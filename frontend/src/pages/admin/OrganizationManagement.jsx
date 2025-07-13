import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  CheckCircleIcon,
  XCircleIcon,
  ClockIcon,
  ExclamationTriangleIcon,
  EyeIcon,
  QrCodeIcon,
  MapPinIcon,
  BuildingOfficeIcon,
  UserIcon,
  CalendarIcon
} from '@heroicons/react/24/outline';
import { getAdminOrganizations, approveOrganization } from '../../utils/auth';
import { useTranslation } from '../../contexts/LanguageContext';

// STATUS_CONFIG will be defined inside component to access t() function

export function OrganizationManagement() {
  const { t } = useTranslation();

  const STATUS_CONFIG = {
    pending: {
      label: t('admin.orgManagement.pending'),
      color: 'bg-yellow-100 text-yellow-800 border-yellow-200',
      icon: ClockIcon,
      iconColor: 'text-yellow-600'
    },
    approved: {
      label: t('admin.orgManagement.approved'),
      color: 'bg-green-100 text-green-800 border-green-200',
      icon: CheckCircleIcon,
      iconColor: 'text-green-600'
    },
    rejected: {
      label: t('admin.orgManagement.rejected'),
      color: 'bg-red-100 text-red-800 border-red-200',
      icon: XCircleIcon,
      iconColor: 'text-red-600'
    },
    suspended: {
      label: t('admin.orgManagement.suspended'),
      color: 'bg-orange-100 text-orange-800 border-orange-200',
      icon: ExclamationTriangleIcon,
      iconColor: 'text-orange-600'
    }
  };
  const [organizations, setOrganizations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedOrg, setSelectedOrg] = useState(null);
  const [showApprovalModal, setShowApprovalModal] = useState(false);
  const [approvalNotes, setApprovalNotes] = useState('');
  const [approvalAction, setApprovalAction] = useState('');
  const [processing, setProcessing] = useState(false);

  // Filters
  const [filters, setFilters] = useState({
    search: '',
    status_filter: '',
    limit: 20,
    offset: 0
  });

  useEffect(() => {
    loadOrganizations();
  }, [filters]);

  const loadOrganizations = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getAdminOrganizations(filters);
      setOrganizations(data.items || []);
    } catch (err) {
      setError(err.message);
      console.error('Load organizations error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (key, value) => {
    setFilters(prev => ({
      ...prev,
      [key]: value,
      offset: 0 // Reset pagination when filters change
    }));
  };

  const openApprovalModal = (org, action) => {
    setSelectedOrg(org);
    setApprovalAction(action);
    setApprovalNotes('');
    setShowApprovalModal(true);
  };

  const closeApprovalModal = () => {
    setSelectedOrg(null);
    setApprovalAction('');
    setApprovalNotes('');
    setShowApprovalModal(false);
  };

  const handleApproval = async () => {
    if (!selectedOrg || !approvalAction) return;

    try {
      setProcessing(true);
      await approveOrganization(selectedOrg.id, approvalAction, approvalNotes);
      
      // Refresh the list
      await loadOrganizations();
      closeApprovalModal();
    } catch (err) {
      setError(err.message);
      console.error('Approval error:', err);
    } finally {
      setProcessing(false);
    }
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('pl-PL', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('pl-PL', {
      style: 'currency',
      currency: 'PLN',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  };

  const getStatusBadge = (status) => {
    const config = STATUS_CONFIG[status] || STATUS_CONFIG.pending;
    const Icon = config.icon;
    
    return (
      <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium border ${config.color}`}>
        <Icon className={`w-4 h-4 mr-1.5 ${config.iconColor}`} />
        {config.label}
      </span>
    );
  };

  if (loading && organizations.length === 0) {
    return (
      <div className="space-y-6">
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
          <div className="animate-pulse space-y-4">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="flex items-center space-x-4">
                <div className="w-12 h-12 bg-slate-200 rounded-lg"></div>
                <div className="flex-1 space-y-2">
                  <div className="h-4 bg-slate-200 rounded w-1/3"></div>
                  <div className="h-3 bg-slate-200 rounded w-1/2"></div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">{t('admin.orgManagement.title')}</h1>
          <p className="text-slate-600 mt-1">
            {t('admin.orgManagement.subtitle')}
          </p>
        </div>
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-4 flex items-start">
          <ExclamationTriangleIcon className="w-5 h-5 text-red-500 mr-3 mt-0.5 flex-shrink-0" />
          <div>
            <h3 className="text-sm font-medium text-red-800">{t('common.error')}</h3>
            <p className="text-sm text-red-700 mt-1">{error}</p>
            <button
              onClick={loadOrganizations}
              className="mt-2 text-sm text-red-700 hover:text-red-800 font-medium"
            >
              {t('common.tryAgain')}
            </button>
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
        <div className="flex flex-col sm:flex-row gap-4">
          {/* Search */}
          <div className="flex-1">
            <div className="relative">
              <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-slate-400" />
              <input
                type="text"
                placeholder={t('admin.orgManagement.searchPlaceholder')}
                value={filters.search}
                onChange={(e) => handleFilterChange('search', e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              />
            </div>
          </div>

          {/* Status Filter */}
          <div className="sm:w-48">
            <div className="relative">
              <FunnelIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-slate-400" />
              <select
                value={filters.status_filter}
                onChange={(e) => handleFilterChange('status_filter', e.target.value)}
                className="w-full pl-10 pr-8 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 appearance-none bg-white"
              >
                <option value="">{t('admin.orgManagement.allStatus')}</option>
                <option value="pending">{t('admin.orgManagement.pending')}</option>
                <option value="approved">{t('admin.orgManagement.approved')}</option>
                <option value="rejected">{t('admin.orgManagement.rejected')}</option>
                <option value="suspended">{t('admin.orgManagement.suspended')}</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      {/* Organizations List */}
      <div className="bg-white rounded-xl shadow-sm border border-slate-200">
        <div className="overflow-hidden">
          {organizations.length > 0 ? (
            <div className="divide-y divide-slate-200">
              {organizations.map((org) => (
                <div key={org.id} className="p-6 hover:bg-slate-50 transition-colors">
                  <div className="flex items-start justify-between">
                    <div className="flex items-start space-x-4 flex-1">
                      {/* Organization Icon */}
                      <div className="flex-shrink-0">
                        <div className="w-12 h-12 bg-gradient-to-br from-primary-500 to-charity-500 rounded-xl flex items-center justify-center">
                          <BuildingOfficeIcon className="w-6 h-6 text-white" />
                        </div>
                      </div>

                      {/* Organization Details */}
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center space-x-3 mb-2">
                          <h3 className="text-lg font-semibold text-slate-900 truncate">
                            {org.name}
                          </h3>
                          {getStatusBadge(org.status)}
                        </div>

                        <p className="text-slate-600 mb-3 line-clamp-2">
                          {org.description}
                        </p>

                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 text-sm">
                          <div className="flex items-center text-slate-500">
                            <MapPinIcon className="w-4 h-4 mr-2" />
                            {org.location}
                          </div>
                          <div className="flex items-center text-slate-500">
                            <UserIcon className="w-4 h-4 mr-2" />
                            {org.contact_email}
                          </div>
                          <div className="flex items-center text-slate-500">
                            <span className="font-medium text-slate-700">{t('admin.orgManagement.target')}</span>
                            <span className="ml-1">{formatCurrency(org.target_amount)}</span>
                          </div>
                          <div className="flex items-center text-slate-500">
                            <CalendarIcon className="w-4 h-4 mr-2" />
                            {formatDate(org.created_at)}
                          </div>
                        </div>

                        {org.admin_notes && (
                          <div className="mt-3 p-3 bg-slate-100 rounded-lg">
                            <p className="text-sm text-slate-700">
                              <span className="font-medium">{t('admin.orgManagement.adminNotes')}</span> {org.admin_notes}
                            </p>
                          </div>
                        )}
                      </div>
                    </div>

                    {/* Actions */}
                    <div className="flex-shrink-0 ml-4">
                      <div className="flex items-center space-x-2">
                        {/* QR Code */}
                        {org.status === 'approved' && (
                          <a
                            href={`http://localhost:8000/api/organizations/${org.id}/qr`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-lg transition-colors"
                            title={t('admin.orgManagement.viewQRCode')}
                          >
                            <QrCodeIcon className="w-5 h-5" />
                          </a>
                        )}

                        {/* View Details */}
                        <Link
                          to={`/${org.id}`}
                          target="_blank"
                          className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-lg transition-colors"
                          title={t('admin.orgManagement.viewPublicPage')}
                        >
                          <EyeIcon className="w-5 h-5" />
                        </Link>

                        {/* Approval Actions */}
                        {org.status === 'pending' && (
                          <div className="flex space-x-2">
                            <button
                              onClick={() => openApprovalModal(org, 'approved')}
                              className="px-3 py-1.5 bg-green-600 text-white text-sm font-medium rounded-lg hover:bg-green-700 transition-colors"
                            >
                              {t('admin.orgManagement.approve')}
                            </button>
                            <button
                              onClick={() => openApprovalModal(org, 'rejected')}
                              className="px-3 py-1.5 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors"
                            >
                              {t('admin.orgManagement.reject')}
                            </button>
                          </div>
                        )}

                        {org.status === 'approved' && (
                          <button
                            onClick={() => openApprovalModal(org, 'suspended')}
                            className="px-3 py-1.5 bg-orange-600 text-white text-sm font-medium rounded-lg hover:bg-orange-700 transition-colors"
                          >
                            {t('admin.orgManagement.suspend')}
                          </button>
                        )}

                        {org.status === 'suspended' && (
                          <button
                            onClick={() => openApprovalModal(org, 'approved')}
                            className="px-3 py-1.5 bg-green-600 text-white text-sm font-medium rounded-lg hover:bg-green-700 transition-colors"
                          >
                            {t('admin.orgManagement.reactivate')}
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-12">
              <BuildingOfficeIcon className="w-12 h-12 text-slate-300 mx-auto mb-3" />
              <p className="text-slate-500">{t('home.noOrganizations')}</p>
            </div>
          )}
        </div>
      </div>

      {/* Approval Modal */}
      {showApprovalModal && selectedOrg && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl p-6 max-w-md w-full">
            <h3 className="text-lg font-semibold text-slate-900 mb-4">
              {approvalAction === 'approved' ? t('admin.orgManagement.approveOrg') : 
               approvalAction === 'rejected' ? t('admin.orgManagement.rejectOrg') : 
               approvalAction === 'suspended' ? t('admin.orgManagement.suspendOrg') : t('admin.orgManagement.updateOrg')}
            </h3>

            <div className="mb-4 p-4 bg-slate-50 rounded-lg">
              <p className="font-medium text-slate-900">{selectedOrg.name}</p>
              <p className="text-sm text-slate-600">{selectedOrg.contact_email}</p>
            </div>

            <div className="mb-6">
              <label className="block text-sm font-medium text-slate-700 mb-2">
                {t('admin.orgManagement.adminNotesOptional')}
              </label>
              <textarea
                value={approvalNotes}
                onChange={(e) => setApprovalNotes(e.target.value)}
                placeholder={t('admin.orgManagement.addNotesPlaceholder')}
                className="w-full p-3 border border-slate-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                rows={3}
              />
            </div>

            <div className="flex space-x-3">
              <button
                onClick={closeApprovalModal}
                disabled={processing}
                className="flex-1 px-4 py-2 text-slate-700 bg-slate-100 hover:bg-slate-200 rounded-lg transition-colors disabled:opacity-50"
              >
                {t('common.cancel')}
              </button>
              <button
                onClick={handleApproval}
                disabled={processing}
                className={`flex-1 px-4 py-2 text-white rounded-lg transition-colors disabled:opacity-50 ${
                  approvalAction === 'approved' ? 'bg-green-600 hover:bg-green-700' :
                  approvalAction === 'rejected' ? 'bg-red-600 hover:bg-red-700' :
                  'bg-orange-600 hover:bg-orange-700'
                }`}
              >
                {processing ? t('admin.orgManagement.processing') : `${t('common.confirm')} ${approvalAction === 'approved' ? t('admin.orgManagement.confirmApproved') : approvalAction === 'rejected' ? t('admin.orgManagement.confirmRejected') : t('admin.orgManagement.confirmSuspended')}`}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}