// Authentication utilities for frontend

const API_BASE_URL = 'http://localhost:8000/api';

// Token management
export const getToken = () => {
  return localStorage.getItem('access_token');
};

export const setToken = (token) => {
  localStorage.setItem('access_token', token);
};

export const removeToken = () => {
  localStorage.removeItem('access_token');
  localStorage.removeItem('user_info');
};

export const getAuthHeaders = () => {
  const token = getToken();
  return token ? {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  } : {
    'Content-Type': 'application/json'
  };
};

// User info management
export const getUserInfo = () => {
  const userInfo = localStorage.getItem('user_info');
  return userInfo ? JSON.parse(userInfo) : null;
};

export const setUserInfo = (userInfo) => {
  localStorage.setItem('user_info', JSON.stringify(userInfo));
};

export const isAuthenticated = () => {
  const token = getToken();
  const userInfo = getUserInfo();
  return !!(token && userInfo);
};

export const isAdmin = () => {
  const userInfo = getUserInfo();
  return userInfo && userInfo.role === 'admin';
};

export const isOrganization = () => {
  const userInfo = getUserInfo();
  return userInfo && userInfo.role === 'organization';
};

// API functions
export const login = async (email, password) => {
  try {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.detail || 'Login failed');
    }

    const data = await response.json();
    
    // Store token
    setToken(data.access_token);
    
    // Get user info
    const userInfo = await getCurrentUser();
    setUserInfo(userInfo);
    
    return { success: true, user: userInfo };
  } catch (error) {
    console.error('Login error:', error);
    return { success: false, error: error.message };
  }
};

export const getCurrentUser = async () => {
  try {
    const response = await fetch(`${API_BASE_URL}/auth/me`, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error('Failed to get user info');
    }

    return await response.json();
  } catch (error) {
    console.error('Get user info error:', error);
    removeToken(); // Clear invalid token
    throw error;
  }
};

export const logout = async () => {
  try {
    // Call logout endpoint (optional, since JWT is stateless)
    await fetch(`${API_BASE_URL}/auth/logout`, {
      method: 'POST',
      headers: getAuthHeaders(),
    });
  } catch (error) {
    console.error('Logout error:', error);
  } finally {
    // Always clear local storage
    removeToken();
  }
};

export const registerOrganization = async (formData) => {
  try {
    const response = await fetch(`${API_BASE_URL}/auth/register-organization`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(formData),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.detail || 'Registration failed');
    }

    return await response.json();
  } catch (error) {
    console.error('Registration error:', error);
    throw error;
  }
};

// Admin API functions
export const getAdminStats = async () => {
  try {
    const response = await fetch(`${API_BASE_URL}/admin/stats`, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error('Failed to get admin stats');
    }

    return await response.json();
  } catch (error) {
    console.error('Admin stats error:', error);
    throw error;
  }
};

export const getAdminOrganizations = async (filters = {}) => {
  try {
    const params = new URLSearchParams();
    
    if (filters.status_filter) params.append('status_filter', filters.status_filter);
    if (filters.search) params.append('search', filters.search);
    if (filters.limit) params.append('limit', filters.limit);
    if (filters.offset) params.append('offset', filters.offset);
    
    const response = await fetch(`${API_BASE_URL}/admin/organizations?${params}`, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error('Failed to get organizations');
    }

    return await response.json();
  } catch (error) {
    console.error('Admin organizations error:', error);
    throw error;
  }
};

export const approveOrganization = async (organizationId, status, adminNotes = '') => {
  try {
    const response = await fetch(`${API_BASE_URL}/admin/organizations/${organizationId}/approve`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify({
        organization_id: organizationId,
        status: status,
        admin_notes: adminNotes
      }),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.detail || 'Failed to update organization status');
    }

    return await response.json();
  } catch (error) {
    console.error('Organization approval error:', error);
    throw error;
  }
};

export const getAdminUsers = async (filters = {}) => {
  try {
    const params = new URLSearchParams();
    
    if (filters.role_filter) params.append('role_filter', filters.role_filter);
    if (filters.active_only !== undefined) params.append('active_only', filters.active_only);
    if (filters.search) params.append('search', filters.search);
    if (filters.limit) params.append('limit', filters.limit);
    if (filters.offset) params.append('offset', filters.offset);
    
    const response = await fetch(`${API_BASE_URL}/admin/users?${params}`, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error('Failed to get users');
    }

    return await response.json();
  } catch (error) {
    console.error('Admin users error:', error);
    throw error;
  }
};

export const toggleUserActive = async (userId) => {
  try {
    const response = await fetch(`${API_BASE_URL}/admin/users/${userId}/toggle-active`, {
      method: 'PUT',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.detail || 'Failed to toggle user status');
    }

    return await response.json();
  } catch (error) {
    console.error('Toggle user active error:', error);
    throw error;
  }
};