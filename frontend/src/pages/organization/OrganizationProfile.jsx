import React, { useState, useEffect } from 'react';
import {
  UserIcon,
  CameraIcon,
  SwatchIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  XMarkIcon
} from '@heroicons/react/24/outline';
import { useAuth } from '../../contexts/AuthContext';
import { useTranslation } from '../../contexts/LanguageContext';

export function OrganizationProfile() {
  const { user } = useAuth();
  const { t } = useTranslation();
  const [organization, setOrganization] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [activeTab, setActiveTab] = useState('basic');

  // Form state
  const [formData, setFormData] = useState({
    description: '',
    contact_email: '',
    website: '',
    phone: '',
    address: '',
    primary_color: '#3B82F6',
    secondary_color: '#EF4444',
    custom_message: ''
  });

  // Logo upload
  const [logoFile, setLogoFile] = useState(null);
  const [logoPreview, setLogoPreview] = useState(null);
  const [uploadingLogo, setUploadingLogo] = useState(false);

  useEffect(() => {
    loadOrganization();
  }, []);

  const loadOrganization = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await fetch('/api/organization/profile', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });
      
      if (!response.ok) {
        throw new Error('Failed to load organization data');
      }
      
      const data = await response.json();
      setOrganization(data);
      setFormData({
        description: data.description || '',
        contact_email: data.contact_email || '',
        website: data.website || '',
        phone: data.phone || '',
        address: data.address || '',
        primary_color: data.primary_color || '#3B82F6',
        secondary_color: data.secondary_color || '#EF4444',
        custom_message: data.custom_message || ''
      });
    } catch (err) {
      setError(err.message);
      // Mock data for development
      const mockData = {
        id: 'org_123',
        name: 'Przykładowa Organizacja',
        description: 'Nasza organizacja pomaga potrzebującym...',
        contact_email: 'kontakt@organizacja.pl',
        website: 'https://organizacja.pl',
        phone: '+48 123 456 789',
        address: 'ul. Przykładowa 1, 00-000 Warszawa',
        primary_color: '#3B82F6',
        secondary_color: '#EF4444',
        custom_message: 'Dziękujemy za wsparcie!',
        logo_url: null
      };
      setOrganization(mockData);
      setFormData(mockData);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleLogoSelect = (e) => {
    const file = e.target.files[0];
    if (file) {
      // Validate file type
      if (!file.type.match(/image\/(jpeg|jpg|png)/)) {
        setError('Please select a valid image file (JPG, PNG)');
        return;
      }
      
      // Validate file size (2MB)
      if (file.size > 2 * 1024 * 1024) {
        setError('File size must be less than 2MB');
        return;
      }
      
      setLogoFile(file);
      
      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        setLogoPreview(e.target.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleLogoUpload = async () => {
    if (!logoFile) return;
    
    try {
      setUploadingLogo(true);
      setError(null);
      
      const formData = new FormData();
      formData.append('file', logoFile);
      
      const response = await fetch('/api/organization/logo', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: formData
      });
      
      if (!response.ok) {
        throw new Error('Failed to upload logo');
      }
      
      const result = await response.json();
      setOrganization(prev => ({ ...prev, logo_url: result.data.logo_url }));
      setLogoFile(null);
      setLogoPreview(null);
      setSuccess('Logo uploaded successfully!');
      
      // Clear success message after 3 seconds
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      setError(err.message);
    } finally {
      setUploadingLogo(false);
    }
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      setError(null);
      
      const response = await fetch('/api/organization/profile', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(formData)
      });
      
      if (!response.ok) {
        throw new Error('Failed to update profile');
      }
      
      setSuccess('Profile updated successfully!');
      
      // Clear success message after 3 seconds
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const tabs = [
    { id: 'basic', name: t('organization.basicInfo'), icon: UserIcon },
    { id: 'branding', name: t('organization.branding'), icon: SwatchIcon }
  ];

  const colorPresets = [
    { primary: '#3B82F6', secondary: '#EF4444', name: 'Blue & Red' },
    { primary: '#10B981', secondary: '#F59E0B', name: 'Green & Yellow' },
    { primary: '#8B5CF6', secondary: '#EC4899', name: 'Purple & Pink' },
    { primary: '#F59E0B', secondary: '#EF4444', name: 'Orange & Red' },
    { primary: '#6B7280', secondary: '#374151', name: 'Gray & Dark' }
  ];

  if (loading) {
    return (
      <div className="animate-pulse space-y-6">
        <div className="h-8 bg-slate-200 rounded w-1/4"></div>
        <div className="bg-white rounded-xl p-6 shadow-sm">
          <div className="h-4 bg-slate-200 rounded w-1/2 mb-4"></div>
          <div className="space-y-3">
            <div className="h-4 bg-slate-200 rounded"></div>
            <div className="h-4 bg-slate-200 rounded w-3/4"></div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-start">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">
            {t('organization.profileSettings')}
          </h1>
          <p className="text-slate-600 mt-1">
            {t('organization.profileSubtitle')}
          </p>
        </div>
        <button
          onClick={handleSave}
          disabled={saving}
          className={`btn-primary ${saving ? 'opacity-50 cursor-not-allowed' : ''}`}
        >
          {saving ? t('common.saving') : t('common.save')}
        </button>
      </div>

      {/* Alerts */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-4 flex items-start">
          <ExclamationTriangleIcon className="w-5 h-5 text-red-500 mr-3 mt-0.5 flex-shrink-0" />
          <div>
            <h3 className="text-sm font-medium text-red-800">{t('common.error')}</h3>
            <p className="text-sm text-red-700 mt-1">{error}</p>
          </div>
          <button
            onClick={() => setError(null)}
            className="ml-auto text-red-500 hover:text-red-700"
          >
            <XMarkIcon className="w-5 h-5" />
          </button>
        </div>
      )}

      {success && (
        <div className="bg-green-50 border border-green-200 rounded-xl p-4 flex items-start">
          <CheckCircleIcon className="w-5 h-5 text-green-500 mr-3 mt-0.5 flex-shrink-0" />
          <div>
            <h3 className="text-sm font-medium text-green-800">{t('common.success')}</h3>
            <p className="text-sm text-green-700 mt-1">{success}</p>
          </div>
          <button
            onClick={() => setSuccess(null)}
            className="ml-auto text-green-500 hover:text-green-700"
          >
            <XMarkIcon className="w-5 h-5" />
          </button>
        </div>
      )}

      {/* Tabs */}
      <div className="bg-white rounded-xl shadow-sm">
        <div className="border-b border-slate-200">
          <nav className="flex space-x-8 px-6">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center py-4 px-1 border-b-2 font-medium text-sm ${
                  activeTab === tab.id
                    ? 'border-primary-500 text-primary-600'
                    : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300'
                }`}
              >
                <tab.icon className="w-5 h-5 mr-2" />
                {tab.name}
              </button>
            ))}
          </nav>
        </div>

        <div className="p-6">
          {activeTab === 'basic' && (
            <div className="space-y-6">
              {/* Organization Name (Read-only) */}
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">
                  {t('organization.name')}
                </label>
                <input
                  type="text"
                  value={organization?.name || ''}
                  disabled
                  className="w-full p-3 border border-slate-300 rounded-xl bg-slate-50 text-slate-500"
                />
                <p className="text-xs text-slate-500 mt-1">
                  {t('organization.nameNote')}
                </p>
              </div>

              {/* Description */}
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">
                  {t('organization.description')}
                </label>
                <textarea
                  name="description"
                  value={formData.description}
                  onChange={handleInputChange}
                  rows={4}
                  className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  placeholder={t('organization.descriptionPlaceholder')}
                />
              </div>

              {/* Contact Information */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('organization.contactEmail')}
                  </label>
                  <input
                    type="email"
                    name="contact_email"
                    value={formData.contact_email}
                    onChange={handleInputChange}
                    className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('organization.phone')}
                  </label>
                  <input
                    type="tel"
                    name="phone"
                    value={formData.phone}
                    onChange={handleInputChange}
                    className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">
                  {t('organization.website')}
                </label>
                <input
                  type="url"
                  name="website"
                  value={formData.website}
                  onChange={handleInputChange}
                  className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  placeholder="https://..."
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">
                  {t('organization.address')}
                </label>
                <input
                  type="text"
                  name="address"
                  value={formData.address}
                  onChange={handleInputChange}
                  className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                />
              </div>
            </div>
          )}

          {activeTab === 'branding' && (
            <div className="space-y-6">
              {/* Logo Upload */}
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">
                  {t('organization.logo')}
                </label>
                <div className="flex items-center space-x-4">
                  <div className="w-20 h-20 bg-slate-100 rounded-lg flex items-center justify-center overflow-hidden">
                    {logoPreview ? (
                      <img src={logoPreview} alt="Logo preview" className="w-full h-full object-cover" />
                    ) : organization?.logo_url ? (
                      <img src={organization.logo_url} alt="Current logo" className="w-full h-full object-cover" />
                    ) : (
                      <CameraIcon className="w-8 h-8 text-slate-400" />
                    )}
                  </div>
                  <div className="flex-1">
                    <input
                      type="file"
                      accept="image/*"
                      onChange={handleLogoSelect}
                      className="hidden"
                      id="logo-upload"
                    />
                    <label
                      htmlFor="logo-upload"
                      className="btn-secondary cursor-pointer inline-flex items-center mr-3"
                    >
                      <CameraIcon className="w-5 h-5 mr-2" />
                      {t('organization.selectLogo')}
                    </label>
                    {logoFile && (
                      <button
                        onClick={handleLogoUpload}
                        disabled={uploadingLogo}
                        className="btn-primary"
                      >
                        {uploadingLogo ? t('organization.uploading') : t('organization.uploadLogo')}
                      </button>
                    )}
                  </div>
                </div>
                <p className="text-xs text-slate-500 mt-1">
                  {t('organization.logoNote')}
                </p>
              </div>

              {/* Color Presets */}
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">
                  {t('organization.colorPresets')}
                </label>
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-3">
                  {colorPresets.map((preset, index) => (
                    <button
                      key={index}
                      onClick={() => setFormData(prev => ({
                        ...prev,
                        primary_color: preset.primary,
                        secondary_color: preset.secondary
                      }))}
                      className={`p-3 rounded-lg border-2 transition-all ${
                        formData.primary_color === preset.primary && formData.secondary_color === preset.secondary
                          ? 'border-primary-500 ring-2 ring-primary-200'
                          : 'border-slate-200 hover:border-slate-300'
                      }`}
                    >
                      <div className="flex space-x-1 mb-2">
                        <div 
                          className="w-6 h-6 rounded-md"
                          style={{ backgroundColor: preset.primary }}
                        />
                        <div 
                          className="w-6 h-6 rounded-md"
                          style={{ backgroundColor: preset.secondary }}
                        />
                      </div>
                      <p className="text-xs text-slate-600">{preset.name}</p>
                    </button>
                  ))}
                </div>
              </div>

              {/* Custom Colors */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('organization.primaryColor')}
                  </label>
                  <div className="flex items-center space-x-3">
                    <input
                      type="color"
                      name="primary_color"
                      value={formData.primary_color}
                      onChange={handleInputChange}
                      className="w-12 h-12 border border-slate-300 rounded-lg cursor-pointer"
                    />
                    <input
                      type="text"
                      name="primary_color"
                      value={formData.primary_color}
                      onChange={handleInputChange}
                      className="flex-1 p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                      placeholder="#3B82F6"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('organization.secondaryColor')}
                  </label>
                  <div className="flex items-center space-x-3">
                    <input
                      type="color"
                      name="secondary_color"
                      value={formData.secondary_color}
                      onChange={handleInputChange}
                      className="w-12 h-12 border border-slate-300 rounded-lg cursor-pointer"
                    />
                    <input
                      type="text"
                      name="secondary_color"
                      value={formData.secondary_color}
                      onChange={handleInputChange}
                      className="flex-1 p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                      placeholder="#EF4444"
                    />
                  </div>
                </div>
              </div>

              {/* Custom Message */}
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">
                  {t('organization.customMessage')}
                </label>
                <textarea
                  name="custom_message"
                  value={formData.custom_message}
                  onChange={handleInputChange}
                  rows={3}
                  className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  placeholder={t('organization.customMessagePlaceholder')}
                />
                <p className="text-xs text-slate-500 mt-1">
                  {t('organization.customMessageNote')}
                </p>
              </div>

              {/* Preview */}
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">
                  {t('organization.preview')}
                </label>
                <div className="border-2 border-dashed border-slate-300 rounded-xl p-6">
                  <div 
                    className="bg-gradient-to-r p-6 rounded-lg text-white"
                    style={{ 
                      backgroundImage: `linear-gradient(to right, ${formData.primary_color}, ${formData.secondary_color})` 
                    }}
                  >
                    <h3 className="text-lg font-semibold mb-2">{organization?.name}</h3>
                    <p className="text-sm opacity-90">
                      {formData.custom_message || t('organization.defaultMessage')}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}