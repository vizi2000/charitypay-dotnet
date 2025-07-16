import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  BuildingOfficeIcon,
  UserIcon,
  EnvelopeIcon,
  LockClosedIcon,
  MapPinIcon,
  CurrencyDollarIcon,
  DocumentTextIcon,
  GlobeAltIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  EyeIcon,
  EyeSlashIcon
} from '@heroicons/react/24/outline';
import { registerOrganization } from '../utils/auth';
import { useTranslation } from '../contexts/LanguageContext';

const CATEGORIES = [
  { value: 'religia', icon: '⛪' },
  { value: 'dzieci', icon: '👶' },
  { value: 'zwierzeta', icon: '🐾' },
  { value: 'edukacja', icon: '📚' },
  { value: 'zdrowie', icon: '🏥' },
  { value: 'inne', icon: '🤝' }
];

export function RegisterPage() {
  const { t } = useTranslation();
  const [formData, setFormData] = useState({
    // Organization details
    name: '',
    description: '',
    category: 'religia',
    location: '',
    target_amount: '',
    contact_email: '',
    website: '',
    
    // Merchant/Payment details
    legal_business_name: '',
    tax_id: '',
    krs_number: '',
    bank_account: '',
    
    // Admin user details
    admin_full_name: '',
    admin_password: ''
  });

  const [showPassword, setShowPassword] = useState(false);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [registrationResult, setRegistrationResult] = useState(null);

  const navigate = useNavigate();

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear field error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {};

    // Organization validation
    if (!formData.name.trim()) {
      newErrors.name = t('register.validation.nameRequired');
    }

    if (!formData.description.trim()) {
      newErrors.description = t('register.validation.descriptionRequired');
    } else if (formData.description.length < 10) {
      newErrors.description = t('register.validation.descriptionMin');
    }

    if (!formData.location.trim()) {
      newErrors.location = t('register.validation.locationRequired');
    }

    if (!formData.target_amount) {
      newErrors.target_amount = t('register.validation.targetAmountRequired');
    } else if (parseFloat(formData.target_amount) <= 0) {
      newErrors.target_amount = t('register.validation.targetAmountPositive');
    }

    if (!formData.contact_email.trim()) {
      newErrors.contact_email = t('register.validation.emailRequired');
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.contact_email)) {
      newErrors.contact_email = t('register.validation.emailInvalid');
    }

    if (formData.website && !/^https?:\/\/.+\..+/.test(formData.website)) {
      newErrors.website = t('register.validation.websiteInvalid');
    }

    // Merchant validation
    if (!formData.legal_business_name.trim()) {
      newErrors.legal_business_name = 'Oficjalna nazwa organizacji jest wymagana';
    }

    if (!formData.tax_id.trim()) {
      newErrors.tax_id = 'NIP jest wymagany';
    } else if (!/^\d{10}$/.test(formData.tax_id.replace(/[-\s]/g, ''))) {
      newErrors.tax_id = 'NIP musi składać się z 10 cyfr';
    }

    if (!formData.bank_account.trim()) {
      newErrors.bank_account = 'Numer konta bankowego jest wymagany';
    } else if (!/^PL\d{26}$/.test(formData.bank_account.replace(/\s/g, ''))) {
      newErrors.bank_account = 'Numer konta musi być w formacie polskiego IBAN (PL + 26 cyfr)';
    }

    if (formData.krs_number && !/^\d{1,20}$/.test(formData.krs_number)) {
      newErrors.krs_number = 'Numer KRS może zawierać tylko cyfry (maksymalnie 20)';
    }

    // Admin user validation
    if (!formData.admin_full_name.trim()) {
      newErrors.admin_full_name = t('register.validation.adminNameRequired');
    }

    if (!formData.admin_password) {
      newErrors.admin_password = t('register.validation.passwordRequired');
    } else if (formData.admin_password.length < 6) {
      newErrors.admin_password = t('register.validation.passwordMin');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setLoading(true);

    try {
      const registrationData = {
        ...formData,
        target_amount: parseFloat(formData.target_amount)
      };

      const result = await registerOrganization(registrationData);
      setRegistrationResult(result);
      setSuccess(true);
    } catch (error) {
      setErrors({ submit: error.message });
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 via-white to-primary-50 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
        <div className="max-w-md w-full space-y-8">
          <div className="text-center">
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <CheckCircleIcon className="w-8 h-8 text-green-600" />
            </div>
            
            <h2 className="text-3xl font-bold text-slate-900 mb-2">
              {t('register.success')}
            </h2>
            <p className="text-slate-600">
              {t('register.successSubtitle')}
            </p>
          </div>

          <div className="bg-white shadow-xl rounded-2xl p-8">
            <div className="space-y-4">
              <div className="bg-green-50 border border-green-200 rounded-xl p-4">
                <h3 className="font-medium text-green-800 mb-2">{t('register.whatNext')}</h3>
                <ul className="text-sm text-green-700 space-y-1">
                  {t('register.reviewProcess').map((step, index) => (
                    <li key={index}>• {step}</li>
                  ))}
                </ul>
              </div>

              {registrationResult && (
                <div className="bg-slate-50 rounded-xl p-4">
                  <h4 className="font-medium text-slate-800 mb-2">{t('register.registrationDetails')}</h4>
                  <div className="text-sm text-slate-600 space-y-1">
                    <p><strong>{t('register.organizationId')}</strong> {registrationResult.organization_id}</p>
                    <p><strong>{t('common.status')}:</strong> {registrationResult.status}</p>
                  </div>
                </div>
              )}

              <div className="flex space-x-3">
                <Link
                  to="/login"
                  className="flex-1 btn-primary text-center"
                >
                  {t('register.goToLogin')}
                </Link>
                <Link
                  to="/organizations"
                  className="flex-1 btn-secondary text-center"
                >
                  {t('register.viewOrganizations')}
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-white to-primary-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-2xl mx-auto">
        {/* Header */}
        <div className="text-center mb-8">
          <Link to="/organizations" className="inline-flex items-center space-x-2 text-2xl font-bold text-primary-600 mb-8">
            <div className="flex items-center justify-center w-10 h-10 bg-gradient-to-br from-primary-500 to-charity-500 rounded-lg">
              <span className="text-white font-bold">C</span>
            </div>
            <span>CharityPay</span>
          </Link>
          
          <h1 className="text-3xl font-bold text-slate-900 mb-2">
            {t('register.title')}
          </h1>
          <p className="text-slate-600">
            {t('register.subtitle')}
          </p>
        </div>

        {/* Registration Form */}
        <div className="bg-white shadow-xl rounded-2xl p-8">
          <form onSubmit={handleSubmit} className="space-y-8">
            {/* Submit Error */}
            {errors.submit && (
              <div className="bg-red-50 border border-red-200 rounded-xl p-4 flex items-start">
                <ExclamationTriangleIcon className="w-5 h-5 text-red-500 mr-3 mt-0.5 flex-shrink-0" />
                <div>
                  <h3 className="text-sm font-medium text-red-800">{t('register.registrationFailed')}</h3>
                  <p className="text-sm text-red-700 mt-1">{errors.submit}</p>
                </div>
              </div>
            )}

            {/* Organization Details */}
            <div>
              <h2 className="text-lg font-semibold text-slate-900 mb-4 flex items-center">
                <BuildingOfficeIcon className="w-6 h-6 mr-2 text-primary-600" />
                {t('register.orgDetails')}
              </h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Organization Name */}
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('register.orgName')} *
                  </label>
                  <input
                    type="text"
                    name="name"
                    value={formData.name}
                    onChange={handleInputChange}
                    className={`w-full p-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.name ? 'border-red-300' : 'border-slate-300'}`}
                    placeholder={t('register.orgNamePlaceholder')}
                  />
                  {errors.name && <p className="text-red-600 text-sm mt-1">{errors.name}</p>}
                </div>

                {/* Category */}
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('common.category')} *
                  </label>
                  <select
                    name="category"
                    value={formData.category}
                    onChange={handleInputChange}
                    className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500"
                  >
                    {CATEGORIES.map((cat) => (
                      <option key={cat.value} value={cat.value}>
                        {cat.icon} {t(`categories.${cat.value}`)}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Location */}
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('common.location')} *
                  </label>
                  <div className="relative">
                    <MapPinIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-slate-400" />
                    <input
                      type="text"
                      name="location"
                      value={formData.location}
                      onChange={handleInputChange}
                      className={`w-full pl-10 pr-3 py-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.location ? 'border-red-300' : 'border-slate-300'}`}
                      placeholder="np. Warszawa, Polska"
                    />
                  </div>
                  {errors.location && <p className="text-red-600 text-sm mt-1">{errors.location}</p>}
                </div>

                {/* Target Amount */}
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('register.targetAmount')} *
                  </label>
                  <div className="relative">
                    <CurrencyDollarIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-slate-400" />
                    <input
                      type="number"
                      name="target_amount"
                      value={formData.target_amount}
                      onChange={handleInputChange}
                      className={`w-full pl-10 pr-3 py-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.target_amount ? 'border-red-300' : 'border-slate-300'}`}
                      placeholder="10000"
                      min="1"
                    />
                  </div>
                  {errors.target_amount && <p className="text-red-600 text-sm mt-1">{errors.target_amount}</p>}
                </div>

                {/* Contact Email */}
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('register.contactEmail')} *
                  </label>
                  <div className="relative">
                    <EnvelopeIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-slate-400" />
                    <input
                      type="email"
                      name="contact_email"
                      value={formData.contact_email}
                      onChange={handleInputChange}
                      className={`w-full pl-10 pr-3 py-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.contact_email ? 'border-red-300' : 'border-slate-300'}`}
                      placeholder="kontakt@parafia.pl"
                    />
                  </div>
                  {errors.contact_email && <p className="text-red-600 text-sm mt-1">{errors.contact_email}</p>}
                </div>

                {/* Website */}
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('register.website')}
                  </label>
                  <div className="relative">
                    <GlobeAltIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-slate-400" />
                    <input
                      type="url"
                      name="website"
                      value={formData.website}
                      onChange={handleInputChange}
                      className={`w-full pl-10 pr-3 py-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.website ? 'border-red-300' : 'border-slate-300'}`}
                      placeholder={t('register.websitePlaceholder')}
                    />
                  </div>
                  {errors.website && <p className="text-red-600 text-sm mt-1">{errors.website}</p>}
                </div>

                {/* Description */}
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('common.description')} *
                  </label>
                  <div className="relative">
                    <DocumentTextIcon className="absolute left-3 top-3 w-5 h-5 text-slate-400" />
                    <textarea
                      name="description"
                      value={formData.description}
                      onChange={handleInputChange}
                      className={`w-full pl-10 pr-3 py-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.description ? 'border-red-300' : 'border-slate-300'}`}
                      placeholder="Opowiedz ludziom o swojej organizacji i tym, co robicie..."
                      rows={4}
                    />
                  </div>
                  {errors.description && <p className="text-red-600 text-sm mt-1">{errors.description}</p>}
                </div>
              </div>
            </div>

            {/* Merchant Details */}
            <div>
              <h2 className="text-lg font-semibold text-slate-900 mb-4 flex items-center">
                <CurrencyDollarIcon className="w-6 h-6 mr-2 text-primary-600" />
                Dane do płatności
              </h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Legal Business Name */}
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    Oficjalna nazwa organizacji *
                  </label>
                  <input
                    type="text"
                    name="legal_business_name"
                    value={formData.legal_business_name}
                    onChange={handleInputChange}
                    className={`w-full p-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.legal_business_name ? 'border-red-300' : 'border-slate-300'}`}
                    placeholder="Parafia Św. Jana w Warszawie"
                  />
                  {errors.legal_business_name && <p className="text-red-600 text-sm mt-1">{errors.legal_business_name}</p>}
                  <p className="text-sm text-slate-500 mt-1">Nazwa zgodna z KRS lub dokumentami rejestracyjnymi</p>
                </div>

                {/* Tax ID (NIP) */}
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    NIP *
                  </label>
                  <input
                    type="text"
                    name="tax_id"
                    value={formData.tax_id}
                    onChange={handleInputChange}
                    className={`w-full p-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.tax_id ? 'border-red-300' : 'border-slate-300'}`}
                    placeholder="1234567890"
                    maxLength="12"
                  />
                  {errors.tax_id && <p className="text-red-600 text-sm mt-1">{errors.tax_id}</p>}
                  <p className="text-sm text-slate-500 mt-1">10 cyfr bez kresek</p>
                </div>

                {/* KRS Number */}
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    Numer KRS
                  </label>
                  <input
                    type="text"
                    name="krs_number"
                    value={formData.krs_number}
                    onChange={handleInputChange}
                    className={`w-full p-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.krs_number ? 'border-red-300' : 'border-slate-300'}`}
                    placeholder="0000123456"
                    maxLength="20"
                  />
                  {errors.krs_number && <p className="text-red-600 text-sm mt-1">{errors.krs_number}</p>}
                  <p className="text-sm text-slate-500 mt-1">Opcjonalnie - jeśli organizacja jest zarejestrowana</p>
                </div>

                {/* Bank Account */}
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    Numer konta bankowego (IBAN) *
                  </label>
                  <input
                    type="text"
                    name="bank_account"
                    value={formData.bank_account}
                    onChange={handleInputChange}
                    className={`w-full p-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.bank_account ? 'border-red-300' : 'border-slate-300'}`}
                    placeholder="PL61109010140000071219812874"
                    maxLength="30"
                  />
                  {errors.bank_account && <p className="text-red-600 text-sm mt-1">{errors.bank_account}</p>}
                  <p className="text-sm text-slate-500 mt-1">Konto na które będą przekazywane darowizny</p>
                </div>
              </div>
            </div>

            {/* Admin Account */}
            <div>
              <h2 className="text-lg font-semibold text-slate-900 mb-4 flex items-center">
                <UserIcon className="w-6 h-6 mr-2 text-primary-600" />
                {t('register.adminAccount')}
              </h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Admin Name */}
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('register.yourFullName')} *
                  </label>
                  <input
                    type="text"
                    name="admin_full_name"
                    value={formData.admin_full_name}
                    onChange={handleInputChange}
                    className={`w-full p-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.admin_full_name ? 'border-red-300' : 'border-slate-300'}`}
                    placeholder="Jan Kowalski"
                  />
                  {errors.admin_full_name && <p className="text-red-600 text-sm mt-1">{errors.admin_full_name}</p>}
                </div>

                {/* Admin Password */}
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-2">
                    {t('common.password')} *
                  </label>
                  <div className="relative">
                    <LockClosedIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-slate-400" />
                    <input
                      type={showPassword ? 'text' : 'password'}
                      name="admin_password"
                      value={formData.admin_password}
                      onChange={handleInputChange}
                      className={`w-full pl-10 pr-12 py-3 border rounded-xl focus:ring-2 focus:ring-primary-500 ${errors.admin_password ? 'border-red-300' : 'border-slate-300'}`}
                      placeholder={t('register.atLeast6Chars')}
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-3 top-1/2 transform -translate-y-1/2 text-slate-400 hover:text-slate-600"
                    >
                      {showPassword ? <EyeSlashIcon className="w-5 h-5" /> : <EyeIcon className="w-5 h-5" />}
                    </button>
                  </div>
                  {errors.admin_password && <p className="text-red-600 text-sm mt-1">{errors.admin_password}</p>}
                </div>
              </div>
            </div>

            {/* Submit Button */}
            <div className="flex flex-col sm:flex-row gap-4">
              <Link
                to="/organizations"
                className="flex-1 btn-secondary text-center"
              >
                {t('common.cancel')}
              </Link>
              <button
                type="submit"
                disabled={loading}
                className={`flex-1 py-3 px-6 text-white font-semibold rounded-xl transition-all duration-200 ${
                  loading
                    ? 'bg-slate-400 cursor-not-allowed'
                    : 'bg-gradient-to-r from-charity-600 to-primary-600 hover:from-charity-700 hover:to-primary-700 shadow-lg hover:shadow-xl'
                }`}
              >
                {loading ? (
                  <div className="flex items-center justify-center">
                    <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white mr-3"></div>
                    {t('register.submitting')}
                  </div>
                ) : (
                  t('register.registerButton')
                )}
              </button>
            </div>
          </form>

          {/* Footer */}
          <div className="mt-8 text-center">
            <p className="text-sm text-slate-600">
              {t('register.alreadyHaveAccount')}{' '}
              <Link 
                to="/login" 
                className="font-medium text-primary-600 hover:text-primary-500 transition-colors"
              >
                {t('register.signInHere')}
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}