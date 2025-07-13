import React, { useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { 
  XCircleIcon,
  ArrowLeftIcon,
  ArrowPathIcon,
  QuestionMarkCircleIcon 
} from '@heroicons/react/24/outline';
import { deviceDetection } from '../utils/deviceDetection';

export function PaymentFailure() {
  const [searchParams] = useSearchParams();
  const reason = searchParams.get('reason') || 'Unknown error';
  const paymentId = searchParams.get('payment_id');
  const organizationId = searchParams.get('organization_id');

  useEffect(() => {
    // Error vibration on mobile
    if (deviceDetection.isMobile()) {
      deviceDetection.vibrate([100, 50, 100, 50, 100]);
    }
  }, []);

  const getErrorMessage = (reason) => {
    const messages = {
      'cancelled': 'You cancelled the payment process.',
      'declined': 'Your payment was declined by your bank or card issuer.',
      'expired': 'The payment session has expired.',
      'insufficient_funds': 'Insufficient funds in your account.',
      'invalid_card': 'Invalid card information provided.',
      'network_error': 'Network connection error occurred.',
      'server_error': 'A server error occurred. Please try again.',
      'unknown': 'An unexpected error occurred during payment processing.',
    };
    
    return messages[reason.toLowerCase()] || messages['unknown'];
  };

  const getHelpText = (reason) => {
    const helpTexts = {
      'cancelled': 'You can try again when you\'re ready to complete the donation.',
      'declined': 'Please check with your bank or try a different payment method.',
      'expired': 'Please start a new donation process.',
      'insufficient_funds': 'Please check your account balance or try a different payment method.',
      'invalid_card': 'Please verify your card details and try again.',
      'network_error': 'Please check your internet connection and try again.',
      'server_error': 'Our team has been notified. Please try again in a few minutes.',
      'unknown': 'Please try again or contact support if the problem persists.',
    };
    
    return helpTexts[reason.toLowerCase()] || helpTexts['unknown'];
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-red-50 via-white to-slate-50 flex items-center justify-center px-4">
      <div className="max-w-md w-full">
        <div className="bg-white rounded-2xl shadow-lg border border-slate-200 p-8 text-center">
          {/* Error Icon */}
          <div className="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-red-100 mb-6">
            <XCircleIcon className="h-10 w-10 text-red-600" />
          </div>

          {/* Error Message */}
          <h1 className="text-2xl font-bold text-slate-900 mb-2">
            Payment Failed
          </h1>
          
          <p className="text-slate-600 mb-4">
            {getErrorMessage(reason)}
          </p>

          <p className="text-sm text-slate-500 mb-6">
            {getHelpText(reason)}
          </p>

          {/* Error Details */}
          {paymentId && (
            <div className="bg-slate-50 rounded-xl p-4 mb-6">
              <div className="text-sm text-slate-500 mb-1">Payment ID</div>
              <div className="font-mono text-sm text-slate-900 break-all">
                {paymentId}
              </div>
            </div>
          )}

          {/* Common Issues */}
          <div className="bg-blue-50 rounded-xl p-4 mb-6 text-left">
            <div className="flex items-center mb-2">
              <QuestionMarkCircleIcon className="h-5 w-5 text-blue-500 mr-2" />
              <span className="font-semibold text-blue-900 text-sm">Common Solutions</span>
            </div>
            <ul className="text-sm text-blue-800 space-y-1">
              <li>• Check your internet connection</li>
              <li>• Verify your card details are correct</li>
              <li>• Ensure sufficient funds are available</li>
              <li>• Try a different payment method</li>
              <li>• Contact your bank if issues persist</li>
            </ul>
          </div>

          {/* Action Buttons */}
          <div className="space-y-3">
            {organizationId ? (
              <Link 
                to={`/${organizationId}`} 
                className="btn-primary w-full group"
              >
                <ArrowPathIcon className="w-5 h-5 mr-2 group-hover:rotate-45 transition-transform" />
                <span>Try Again</span>
              </Link>
            ) : (
              <Link 
                to="/organizations" 
                className="btn-primary w-full"
              >
                <span>Choose Organization</span>
              </Link>
            )}

            <Link 
              to="/organizations" 
              className="btn-secondary w-full group"
            >
              <ArrowLeftIcon className="w-5 h-5 mr-2 group-hover:-translate-x-1 transition-transform" />
              <span>Back to Organizations</span>
            </Link>
          </div>

          {/* Support */}
          <div className="mt-6 pt-6 border-t border-slate-200">
            <p className="text-sm text-slate-600 mb-2">
              Still having trouble?
            </p>
            <a 
              href="mailto:support@charitypay.com"
              className="text-primary-600 hover:text-primary-700 font-medium text-sm"
            >
              Contact Support
            </a>
          </div>
        </div>
      </div>
    </div>
  );
}