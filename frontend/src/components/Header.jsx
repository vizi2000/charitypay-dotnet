import React from 'react';
import { Link } from 'react-router-dom';
import { HeartIcon, UserIcon, UserPlusIcon } from '@heroicons/react/24/solid';
import { useTranslation } from '../contexts/LanguageContext';
import { SimpleLanguageSwitcher } from './SimpleLanguageSwitcher';
import { useAuth } from '../contexts/AuthContext';

export function Header() {
  const { t } = useTranslation();
  const { isAuthenticated, user, isAdmin } = useAuth();

  return (
    <header className="bg-white border-b border-slate-200 sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <Link 
            to="/organizations" 
            className="flex items-center space-x-2 text-primary-600 hover:text-primary-700 transition-colors group"
          >
            <div className="flex items-center justify-center w-8 h-8 bg-gradient-to-br from-primary-500 to-charity-500 rounded-lg group-hover:scale-105 transition-transform">
              <HeartIcon className="w-5 h-5 text-white" />
            </div>
            <span className="text-xl font-bold">
              CharityPay
            </span>
          </Link>

          {/* Navigation */}
          <nav className="hidden md:flex items-center space-x-6">
            <Link 
              to="/organizations" 
              className="text-slate-600 hover:text-slate-900 font-medium transition-colors"
            >
              {t('nav.organizations')}
            </Link>
            <a 
              href="#how-it-works" 
              className="text-slate-600 hover:text-slate-900 font-medium transition-colors"
            >
              {t('nav.howItWorks')}
            </a>
            <a 
              href="#about" 
              className="text-slate-600 hover:text-slate-900 font-medium transition-colors"
            >
              {t('nav.about')}
            </a>
          </nav>

          {/* Right side actions */}
          <div className="flex items-center space-x-4">
            {/* Language Switcher */}
            <SimpleLanguageSwitcher />

            {/* Auth Buttons */}
            {isAuthenticated ? (
              <div className="flex items-center space-x-3">
                {isAdmin() && (
                  <Link
                    to="/admin"
                    className="hidden sm:inline-flex items-center px-3 py-2 text-sm font-medium text-primary-600 hover:text-primary-700 hover:bg-primary-50 rounded-lg transition-colors"
                  >
                    Admin Panel
                  </Link>
                )}
                <div className="flex items-center space-x-2 text-slate-700">
                  <div className="w-8 h-8 bg-gradient-to-br from-primary-500 to-charity-500 rounded-full flex items-center justify-center">
                    <span className="text-white font-medium text-sm">
                      {user?.full_name?.charAt(0)?.toUpperCase() || 'U'}
                    </span>
                  </div>
                  <span className="hidden sm:inline text-sm font-medium">
                    {user?.full_name || 'User'}
                  </span>
                </div>
              </div>
            ) : (
              <div className="flex items-center space-x-3">
                <Link
                  to="/login"
                  className="flex items-center space-x-1 px-3 py-2 text-sm font-medium text-slate-600 hover:text-slate-900 hover:bg-slate-100 rounded-lg transition-colors"
                >
                  <UserIcon className="w-4 h-4" />
                  <span className="hidden sm:inline">{t('nav.login')}</span>
                </Link>
                <Link
                  to="/register"
                  className="flex items-center space-x-1 px-3 py-2 text-sm font-medium text-white bg-gradient-to-r from-primary-600 to-charity-600 hover:from-primary-700 hover:to-charity-700 rounded-lg transition-colors"
                >
                  <UserPlusIcon className="w-4 h-4" />
                  <span className="hidden sm:inline">{t('nav.register')}</span>
                </Link>
              </div>
            )}

            {/* Mobile menu button */}
            <div className="md:hidden">
              <button className="text-slate-600 hover:text-slate-900 p-2">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}