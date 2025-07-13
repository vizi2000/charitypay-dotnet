import axios from 'axios';

// Create axios instance with base configuration
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for debugging
api.interceptors.request.use(
  (config) => {
    console.log(`Making ${config.method?.toUpperCase()} request to ${config.url}`);
    return config;
  },
  (error) => {
    console.error('Request error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    console.error('Response error:', error);
    
    // Handle common errors
    if (error.response?.status === 404) {
      throw new Error('Resource not found');
    } else if (error.response?.status === 500) {
      throw new Error('Server error. Please try again later.');
    } else if (error.code === 'NETWORK_ERROR' || !error.response) {
      throw new Error('Network error. Please check your connection.');
    }
    
    throw error;
  }
);

// Organizations API
export const organizationsAPI = {
  // Get all organizations
  async getAll() {
    try {
      const response = await api.get('/demo/organizations');
      return response.data;
    } catch (error) {
      throw new Error(`Failed to fetch organizations: ${error.message}`);
    }
  },

  // Get organization by ID
  async getById(id) {
    try {
      const response = await api.get(`/demo/organizations`);
      // Find the organization by ID from the demo data
      const organizations = response.data.data || [];
      const organization = organizations.find(org => org.id === parseInt(id));
      if (!organization) {
        throw new Error('Organization not found');
      }
      return { data: organization };
    } catch (error) {
      throw new Error(`Failed to fetch organization: ${error.message}`);
    }
  },
};

// Payments API
export const paymentsAPI = {
  // Initiate a new payment
  async initiate(paymentData) {
    try {
      const response = await api.post('/demo/payments', paymentData);
      return response.data;
    } catch (error) {
      if (error.response?.data?.detail) {
        throw new Error(error.response.data.detail);
      }
      throw new Error(`Failed to initiate payment: ${error.message}`);
    }
  },

  // Get payment status
  async getStatus(paymentId) {
    try {
      const response = await api.get('/demo/payments');
      // Find the payment by ID from the demo data
      const payments = response.data.data || [];
      const payment = payments.find(p => p.id === paymentId);
      if (!payment) {
        throw new Error('Payment not found');
      }
      return { data: payment };
    } catch (error) {
      throw new Error(`Failed to get payment status: ${error.message}`);
    }
  },
};

// Utility functions
export const utils = {
  // Format currency for display
  formatCurrency(amount, currency = 'PLN') {
    return new Intl.NumberFormat('pl-PL', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(amount);
  },

  // Calculate progress percentage
  calculateProgress(collected, target) {
    if (target === 0) return 0;
    return Math.min((collected / target) * 100, 100);
  },

  // Format relative date
  formatRelativeDate(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMs = now - date;
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));

    if (diffInDays === 0) {
      return 'Today';
    } else if (diffInDays === 1) {
      return 'Yesterday';
    } else if (diffInDays < 7) {
      return `${diffInDays} days ago`;
    } else if (diffInDays < 30) {
      const weeks = Math.floor(diffInDays / 7);
      return `${weeks} week${weeks > 1 ? 's' : ''} ago`;
    } else {
      return date.toLocaleDateString('pl-PL');
    }
  },

  // Validate email
  isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  },

  // Validate amount
  isValidAmount(amount) {
    const num = parseFloat(amount);
    return !isNaN(num) && num > 0 && num <= 100000;
  },
};

export default api;