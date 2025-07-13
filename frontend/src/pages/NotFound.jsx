import React from 'react';
import { Link } from 'react-router-dom';
import { HomeIcon, MagnifyingGlassIcon } from '@heroicons/react/24/outline';

export function NotFound() {
  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center px-4">
      <div className="max-w-md w-full text-center">
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 p-8">
          {/* 404 Illustration */}
          <div className="mb-6">
            <div className="text-6xl font-bold text-slate-300 mb-2">404</div>
            <MagnifyingGlassIcon className="w-16 h-16 text-slate-400 mx-auto" />
          </div>

          {/* Error Message */}
          <h1 className="text-2xl font-bold text-slate-900 mb-2">
            Page Not Found
          </h1>
          
          <p className="text-slate-600 mb-8">
            The page you're looking for doesn't exist or has been moved.
          </p>

          {/* Action Button */}
          <Link 
            to="/organizations" 
            className="btn-primary group"
          >
            <HomeIcon className="w-5 h-5 mr-2" />
            <span>Back to Organizations</span>
          </Link>
        </div>
      </div>
    </div>
  );
}