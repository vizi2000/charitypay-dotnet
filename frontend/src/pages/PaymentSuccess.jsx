import React, { useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { 
  CheckCircleIcon,
  HeartIcon,
  ArrowRightIcon,
  ShareIcon 
} from '@heroicons/react/24/solid';
import { deviceDetection } from '../utils/deviceDetection';

export function PaymentSuccess() {
  const [searchParams] = useSearchParams();
  const paymentId = searchParams.get('payment_id');
  const amount = searchParams.get('amount');
  const organizationName = searchParams.get('organization_name') || 'the organization';

  useEffect(() => {
    // Success vibration on mobile
    if (deviceDetection.isMobile()) {
      deviceDetection.vibrate([200, 100, 200]);
    }
  }, []);

  const shareMessage = `I just donated${amount ? ` ${amount} PLN` : ''} to ${organizationName} through CharityPay! 💚 #charity #donation`;

  const handleShare = async () => {
    if (navigator.share) {
      try {
        await navigator.share({
          title: 'Donation Successful',
          text: shareMessage,
          url: window.location.origin,
        });
      } catch (err) {
        console.log('Share cancelled or failed');
      }
    } else {
      // Fallback to copying to clipboard
      try {
        await navigator.clipboard.writeText(shareMessage + ' ' + window.location.origin);
        alert('Message copied to clipboard!');
      } catch (err) {
        console.log('Copy failed');
      }
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-50 via-white to-primary-50 flex items-center justify-center px-4">
      <div className="max-w-md w-full">
        <div className="bg-white rounded-2xl shadow-lg border border-slate-200 p-8 text-center">
          {/* Success Icon */}
          <div className="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-green-100 mb-6">
            <CheckCircleIcon className="h-10 w-10 text-green-600" />
          </div>

          {/* Success Message */}
          <h1 className="text-2xl font-bold text-slate-900 mb-2">
            Donation Successful!
          </h1>
          
          <p className="text-slate-600 mb-6">
            Thank you for your generous donation{amount && ` of ${amount} PLN`} to {organizationName}. 
            Your contribution will make a real difference.
          </p>

          {/* Payment Details */}
          {paymentId && (
            <div className="bg-slate-50 rounded-xl p-4 mb-6">
              <div className="text-sm text-slate-500 mb-1">Payment ID</div>
              <div className="font-mono text-sm text-slate-900 break-all">
                {paymentId}
              </div>
            </div>
          )}

          {/* Impact Message */}
          <div className="bg-gradient-to-r from-charity-50 to-primary-50 rounded-xl p-4 mb-6">
            <div className="flex items-center justify-center mb-2">
              <HeartIcon className="h-6 w-6 text-charity-500 mr-2" />
              <span className="font-semibold text-slate-900">Making an Impact</span>
            </div>
            <p className="text-sm text-slate-700">
              Your donation has been securely processed and will be transferred directly to the organization. 
              You'll receive a confirmation email shortly.
            </p>
          </div>

          {/* Action Buttons */}
          <div className="space-y-3">
            <Link 
              to="/organizations" 
              className="btn-primary w-full group"
            >
              <span>Donate to Another Organization</span>
              <ArrowRightIcon className="w-5 h-5 ml-2 group-hover:translate-x-1 transition-transform" />
            </Link>

            <button
              onClick={handleShare}
              className="btn-secondary w-full group"
            >
              <ShareIcon className="w-5 h-5 mr-2" />
              <span>Share Your Good Deed</span>
            </button>
          </div>

          {/* Footer */}
          <p className="mt-6 text-xs text-slate-500">
            Need help? Contact us at support@charitypay.com
          </p>
        </div>

        {/* Confetti Animation */}
        <div className="fixed inset-0 pointer-events-none overflow-hidden">
          {[...Array(20)].map((_, i) => (
            <div
              key={i}
              className="absolute animate-bounce"
              style={{
                left: `${Math.random() * 100}%`,
                top: `-10px`,
                animationDelay: `${Math.random() * 3}s`,
                animationDuration: `${3 + Math.random() * 2}s`,
                fontSize: `${1 + Math.random()}rem`,
              }}
            >
              {['🎉', '💚', '🌟', '❤️', '🎊'][Math.floor(Math.random() * 5)]}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}