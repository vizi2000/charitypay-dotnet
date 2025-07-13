import React, { createContext, useContext, useState, useEffect } from 'react';
import enTranslations from '../locales/en.json';
import plTranslations from '../locales/pl.json';

const translations = {
  en: enTranslations,
  pl: plTranslations
};

const LanguageContext = createContext();

export function LanguageProvider({ children }) {
  const [language, setLanguage] = useState(() => {
    // Get language from localStorage or default to Polish
    const savedLanguage = localStorage.getItem('charitypay-language');
    return savedLanguage || 'pl';
  });

  useEffect(() => {
    // Save language preference to localStorage
    localStorage.setItem('charitypay-language', language);
  }, [language]);

  const changeLanguage = (newLanguage) => {
    if (translations[newLanguage]) {
      setLanguage(newLanguage);
    }
  };

  const t = (key, defaultValue = key) => {
    const keys = key.split('.');
    let value = translations[language];
    
    for (const k of keys) {
      if (value && typeof value === 'object' && k in value) {
        value = value[k];
      } else {
        // Fallback to English if key not found in current language
        value = translations['en'];
        for (const k of keys) {
          if (value && typeof value === 'object' && k in value) {
            value = value[k];
          } else {
            return defaultValue;
          }
        }
        break;
      }
    }
    
    return typeof value === 'string' ? value : defaultValue;
  };

  const getAvailableLanguages = () => {
    return [
      { code: 'pl', name: 'Polski', flag: '🇵🇱' },
      { code: 'en', name: 'English', flag: '🇺🇸' }
    ];
  };

  const getCurrentLanguage = () => {
    return getAvailableLanguages().find(lang => lang.code === language);
  };

  const value = {
    language,
    changeLanguage,
    t,
    getAvailableLanguages,
    getCurrentLanguage
  };

  return (
    <LanguageContext.Provider value={value}>
      {children}
    </LanguageContext.Provider>
  );
}

export function useTranslation() {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error('useTranslation must be used within a LanguageProvider');
  }
  return context;
}

// Export default hook for convenience
export default useTranslation;