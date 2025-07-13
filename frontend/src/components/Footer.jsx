import React from 'react';
import { HeartIcon } from '@heroicons/react/24/solid';

export function Footer() {
  return (
    <footer className="bg-white border-t border-slate-200 mt-auto">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex flex-col md:flex-row justify-between items-center">
          {/* Logo and description */}
          <div className="flex items-center space-x-2 mb-4 md:mb-0">
            <div className="flex items-center justify-center w-6 h-6 bg-gradient-to-br from-primary-500 to-charity-500 rounded">
              <HeartIcon className="w-4 h-4 text-white" />
            </div>
            <span className="text-lg font-semibold text-slate-900">CharityPay</span>
          </div>

          {/* Links */}
          <div className="flex items-center space-x-6 text-sm text-slate-600">
            <a href="#privacy" className="hover:text-slate-900 transition-colors">
              Privacy Policy
            </a>
            <a href="#terms" className="hover:text-slate-900 transition-colors">
              Terms of Service
            </a>
            <a href="#contact" className="hover:text-slate-900 transition-colors">
              Contact
            </a>
          </div>
        </div>

        {/* Copyright */}
        <div className="mt-6 pt-6 border-t border-slate-200 text-center text-sm text-slate-500">
          <p>© 2024 CharityPay. Making donations simple and secure.</p>
          <p className="mt-1">Powered by Fiserv payment technology.</p>
        </div>
      </div>
    </footer>
  );
}