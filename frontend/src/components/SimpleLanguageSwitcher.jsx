import React from 'react';
import { useTranslation } from '../contexts/LanguageContext';

export function SimpleLanguageSwitcher() {
  const { language, changeLanguage } = useTranslation();

  const toggleLanguage = () => {
    changeLanguage(language === 'pl' ? 'en' : 'pl');
  };

  return (
    <button
      onClick={toggleLanguage}
      className="flex items-center space-x-2 px-3 py-2 text-slate-600 hover:text-slate-900 hover:bg-slate-100 rounded-lg transition-colors"
      title="Zmień język / Change language"
    >
      <span className="text-lg">
        {language === 'pl' ? '🇵🇱' : '🇺🇸'}
      </span>
      <span className="hidden sm:inline text-sm font-medium">
        {language === 'pl' ? 'PL' : 'EN'}
      </span>
    </button>
  );
}