import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { 
  ArrowLeftIcon,
  HeartIcon,
  CreditCardIcon,
  DevicePhoneMobileIcon,
  QrCodeIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon
} from '@heroicons/react/24/outline';
import { 
  CreditCardIcon as CreditCardSolid,
  DevicePhoneMobileIcon as PhoneSolid 
} from '@heroicons/react/24/solid';
import QRCode from 'qrcode';
import { organizationsAPI, paymentsAPI, utils } from '../utils/api';
import { deviceDetection } from '../utils/deviceDetection';

const PRESET_AMOUNTS = [10, 25, 50, 100];

export function DonationPage() {
  const { organizationId } = useParams();
  const [organization, setOrganization] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Form state
  const [selectedAmount, setSelectedAmount] = useState(25);
  const [customAmount, setCustomAmount] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('card');
  const [donorEmail, setDonorEmail] = useState('');
  const [donorName, setDonorName] = useState('');
  const [showQR, setShowQR] = useState(false);
  const [qrCodeUrl, setQrCodeUrl] = useState('');
  
  // Payment state
  const [processing, setProcessing] = useState(false);
  const [paymentError, setPaymentError] = useState(null);

  const preferredMethods = deviceDetection.getPreferredPaymentMethods();

  useEffect(() => {
    loadOrganization();
  }, [organizationId]);

  useEffect(() => {
    // Set default payment method based on device
    if (preferredMethods.length > 0) {
      setPaymentMethod(preferredMethods[0]);
    }
  }, []);

  const loadOrganization = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await organizationsAPI.getById(organizationId);
      setOrganization(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const getEffectiveAmount = () => {
    if (customAmount && parseFloat(customAmount) > 0) {
      return parseFloat(customAmount);
    }
    return selectedAmount;
  };

  const generateQRCode = async (url) => {
    try {
      const qrDataUrl = await QRCode.toDataURL(url, {
        width: 200,
        margin: 2,
        color: {
          dark: '#1e293b',
          light: '#ffffff'
        }
      });
      setQrCodeUrl(qrDataUrl);
      setShowQR(true);
    } catch (err) {
      console.error('Failed to generate QR code:', err);
    }
  };

  const handleAmountSelect = (amount) => {
    setSelectedAmount(amount);
    setCustomAmount('');
  };

  const handleCustomAmountChange = (e) => {
    const value = e.target.value;
    setCustomAmount(value);
    if (value) {
      setSelectedAmount(null);
    }
  };

  const validateForm = () => {
    const amount = getEffectiveAmount();
    
    if (!amount || amount <= 0) {
      return 'Please enter a valid donation amount';
    }
    
    if (amount > 100000) {
      return 'Maximum donation amount is 100,000 PLN';
    }
    
    if (donorEmail && !utils.isValidEmail(donorEmail)) {
      return 'Please enter a valid email address';
    }
    
    return null;
  };

  const handleDonate = async () => {
    const validationError = validateForm();
    if (validationError) {
      setPaymentError(validationError);
      return;
    }

    try {
      setProcessing(true);
      setPaymentError(null);
      
      // Vibration feedback on mobile
      if (deviceDetection.isMobile()) {
        deviceDetection.vibrate(50);
      }

      const paymentData = {
        organization_id: organizationId,
        amount: getEffectiveAmount(),
        method: paymentMethod,
        donor_email: donorEmail || null,
        donor_name: donorName || null,
      };

      let response;
      try {
        response = await paymentsAPI.initiate(paymentData);
      } catch (apiError) {
        // Demo mode: If API fails, show QR code with demo URL
        if (import.meta.env.DEV) {
          console.error('Payment API failed, showing demo QR code', apiError);
        }
        const demoUrl = `https://demo-payment.fiserv.com/pay/${Math.random().toString(36).substr(2, 9)}?amount=${getEffectiveAmount()}&org=${organizationId}`;
        await generateQRCode(demoUrl);
        return; // Exit early for demo
      }
      
      if (response && response.redirect_url) {
        // Generate QR code for the payment URL
        await generateQRCode(response.redirect_url);
        
        // On mobile, redirect immediately
        if (deviceDetection.isMobile()) {
          setTimeout(() => {
            window.location.href = response.redirect_url;
          }, 1000);
        }
      } else {
        throw new Error('Payment link not received');
      }
    } catch (err) {
      setPaymentError(err.message);
      
      // Error vibration on mobile
      if (deviceDetection.isMobile()) {
        deviceDetection.vibrate([100, 50, 100]);
      }
    } finally {
      setProcessing(false);
    }
  };

  const getPaymentMethodIcon = (method) => {
    switch (method) {
      case 'apple_pay':
        return '🍎';
      case 'google_pay':
        return '🟢';
      case 'blik':
        return '📱';
      case 'card':
      default:
        return <CreditCardIcon className="w-5 h-5" />;
    }
  };

  const getPaymentMethodLabel = (method) => {
    const labels = {
      card: 'Credit/Debit Card',
      blik: 'BLIK',
      apple_pay: 'Apple Pay',
      google_pay: 'Google Pay'
    };
    return labels[method] || method;
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
          <p className="mt-4 text-slate-600">Loading organization details...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center px-4">
        <div className="max-w-md w-full text-center">
          <div className="bg-red-50 border border-red-200 rounded-xl p-6">
            <h2 className="text-lg font-semibold text-red-800 mb-2">Organization Not Found</h2>
            <p className="text-red-600 mb-4">{error}</p>
            <Link to="/organizations" className="btn-primary">
              Back to Organizations
            </Link>
          </div>
        </div>
      </div>
    );
  }

  const progress = utils.calculateProgress(organization.collected_amount, organization.target_amount);

  // Apply custom branding if available
  const customStyles = organization.primary_color ? {
    '--primary-color': organization.primary_color,
    '--secondary-color': organization.secondary_color || organization.primary_color
  } : {};

  return (
    <div className="min-h-screen bg-slate-50" style={customStyles}>
      {/* Header */}
      <div className="bg-white border-b border-slate-200">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <Link 
            to={`/organization/${organizationId}`}
            className="inline-flex items-center text-slate-600 hover:text-slate-900 transition-colors"
          >
            <ArrowLeftIcon className="w-5 h-5 mr-2" />
            Back to Organization
          </Link>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Organization Details */}
          <div className="space-y-6">
            <div className="card p-6">
              <div className="flex items-center mb-4">
                {organization.logo_url && (
                  <img 
                    src={organization.logo_url} 
                    alt={`${organization.name} logo`}
                    className="w-12 h-12 rounded-lg mr-3"
                  />
                )}
                <h1 className="text-2xl font-bold text-slate-900">
                  {organization.name}
                </h1>
              </div>
              
              {organization.custom_message && (
                <div 
                  className="bg-gradient-to-r from-primary-600 to-secondary-600 text-white p-4 rounded-lg mb-4"
                  style={{
                    backgroundImage: `linear-gradient(to right, ${organization.primary_color || 'var(--primary-color)'}, ${organization.secondary_color || 'var(--secondary-color)'})`
                  }}
                >
                  <p className="text-sm italic">"{organization.custom_message}"</p>
                </div>
              )}
              
              <p className="text-slate-600 mb-6">
                {organization.description}
              </p>

              {/* Progress */}
              <div className="mb-6">
                <div className="flex justify-between items-center mb-2">
                  <span className="text-sm font-medium text-slate-700">
                    {utils.formatCurrency(organization.collected_amount)} raised
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
                  Goal: {utils.formatCurrency(organization.target_amount)}
                </div>
              </div>

              {/* Organization Info */}
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-slate-500">Location:</span>
                  <div className="font-medium">{organization.location}</div>
                </div>
                <div>
                  <span className="text-slate-500">Category:</span>
                  <div className="font-medium capitalize">{organization.category}</div>
                </div>
              </div>
            </div>
          </div>

          {/* Donation Form */}
          <div className="space-y-6">
            <div className="card p-6">
              <h2 className="text-xl font-semibold text-slate-900 mb-6 flex items-center">
                <HeartIcon className="w-6 h-6 mr-2 text-charity-500" />
                Make a Donation
              </h2>

              {/* Amount Selection */}
              <div className="mb-6">
                <label className="block text-sm font-medium text-slate-700 mb-3">
                  Choose Amount (PLN)
                </label>
                
                {/* Preset Amounts - Optimized for mobile */}
                <div className="grid grid-cols-2 gap-4 mb-4">
                  {PRESET_AMOUNTS.map((amount) => (
                    <button
                      key={amount}
                      onClick={() => handleAmountSelect(amount)}
                      className={`p-4 border-2 rounded-2xl text-center font-semibold transition-all text-lg ${
                        selectedAmount === amount && !customAmount
                          ? 'border-primary-500 bg-primary-50 text-primary-700 ring-2 ring-primary-200'
                          : 'border-slate-200 hover:border-slate-300 text-slate-700 hover:bg-slate-50'
                      }`}
                    >
                      {utils.formatCurrency(amount)}
                    </button>
                  ))}
                </div>

                {/* Custom Amount */}
                <div>
                  <input
                    type="number"
                    placeholder="Enter custom amount"
                    value={customAmount}
                    onChange={handleCustomAmountChange}
                    className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    min="1"
                    max="100000"
                  />
                </div>
              </div>

              {/* Payment Methods - Mobile Optimized */}
              <div className="mb-6">
                <label className="block text-sm font-medium text-slate-700 mb-3">
                  Payment Method
                </label>
                <div className="space-y-3">
                  {preferredMethods.map((method, index) => {
                    const isRecommended = (method === 'apple_pay' && deviceDetection.isIOS()) ||
                                        (method === 'google_pay' && deviceDetection.isAndroid());
                    const isFirst = index === 0;
                    
                    return (
                      <button
                        key={method}
                        onClick={() => setPaymentMethod(method)}
                        className={`w-full p-5 border-2 rounded-2xl flex items-center justify-between transition-all ${
                          paymentMethod === method
                            ? 'border-primary-500 bg-primary-50 ring-2 ring-primary-200'
                            : isRecommended
                            ? 'border-green-300 bg-green-50 hover:border-green-400'
                            : 'border-slate-200 hover:border-slate-300 hover:bg-slate-50'
                        } ${isFirst && isRecommended ? 'ring-2 ring-green-200' : ''}`}
                      >
                        <div className="flex items-center">
                          <span className="mr-4 text-2xl">
                            {getPaymentMethodIcon(method)}
                          </span>
                          <div className="text-left">
                            <span className="font-semibold text-slate-900 block text-lg">
                              {getPaymentMethodLabel(method)}
                            </span>
                            {isRecommended && (
                              <span className="text-green-700 text-sm font-medium">
                                ⚡ Fastest on your device
                              </span>
                            )}
                          </div>
                        </div>
                        {paymentMethod === method && (
                          <CheckCircleIcon className="w-6 h-6 text-primary-600" />
                        )}
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* Optional Donor Info */}
              <div className="mb-6 space-y-4">
                <h3 className="text-sm font-medium text-slate-700">
                  Donor Information (Optional)
                </h3>
                <input
                  type="text"
                  placeholder="Your name"
                  value={donorName}
                  onChange={(e) => setDonorName(e.target.value)}
                  className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                />
                <input
                  type="email"
                  placeholder="Your email"
                  value={donorEmail}
                  onChange={(e) => setDonorEmail(e.target.value)}
                  className="w-full p-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                />
              </div>

              {/* Error Display */}
              {paymentError && (
                <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-xl flex items-start">
                  <ExclamationTriangleIcon className="w-5 h-5 text-red-500 mr-2 mt-0.5 flex-shrink-0" />
                  <span className="text-red-700 text-sm">{paymentError}</span>
                </div>
              )}

              {/* Donate Button - Mobile Optimized */}
              <button
                onClick={handleDonate}
                disabled={processing}
                className={`w-full py-4 px-6 text-lg font-semibold rounded-2xl transition-all duration-200 ${
                  processing 
                    ? 'bg-slate-400 cursor-not-allowed' 
                    : 'bg-gradient-to-r from-primary-600 to-secondary-600 hover:from-primary-700 hover:to-secondary-700 active:scale-95'
                } text-white shadow-lg hover:shadow-xl`}
                style={!processing && organization.primary_color ? {
                  backgroundImage: `linear-gradient(to right, ${organization.primary_color}, ${organization.secondary_color || organization.primary_color})`
                } : {}}
              >
                {processing ? (
                  <div className="flex items-center justify-center">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-white mr-3"></div>
                    Processing...
                  </div>
                ) : (
                  <div className="flex items-center justify-center">
                    <HeartIcon className="w-6 h-6 mr-3" />
                    <span>Donate {utils.formatCurrency(getEffectiveAmount())}</span>
                  </div>
                )}
              </button>

              {/* Mobile-first messaging */}
              {deviceDetection.isMobile() ? (
                <div className="mt-4 p-3 bg-blue-50 rounded-xl">
                  <p className="text-sm text-blue-800 text-center">
                    📱 <strong>Quick donation:</strong> Complete payment in just a few taps!
                  </p>
                </div>
              ) : (
                <p className="mt-3 text-xs text-slate-500 text-center">
                  Secure payment powered by Fiserv. Your donation goes directly to the organization.
                </p>
              )}
            </div>
          </div>
        </div>

        {/* QR Code Modal */}
        {showQR && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
            <div className="bg-white rounded-2xl p-6 max-w-sm w-full text-center">
              <h3 className="text-lg font-semibold text-slate-900 mb-4">
                Complete Your Donation
              </h3>
              
              {qrCodeUrl && (
                <img 
                  src={qrCodeUrl} 
                  alt="Payment QR Code" 
                  className="mx-auto mb-4 rounded-lg border border-slate-200"
                />
              )}
              
              <p className="text-slate-600 mb-6 text-sm">
                Scan this QR code or tap the button below to complete your donation of{' '}
                <strong>{utils.formatCurrency(getEffectiveAmount())}</strong>
              </p>
              
              <div className="space-y-3">
                <a
                  href={qrCodeUrl ? qrCodeUrl.replace('data:image/png;base64,', '') : '#'}
                  className="btn-primary w-full"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Open Payment Page
                </a>
                
                <button
                  onClick={() => setShowQR(false)}
                  className="btn-secondary w-full"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}