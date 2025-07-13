// Device and OS detection utilities
export const deviceDetection = {
  // Detect if user is on iOS
  isIOS() {
    return /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
  },

  // Detect if user is on Android
  isAndroid() {
    return /Android/.test(navigator.userAgent);
  },

  // Detect if user is on mobile
  isMobile() {
    return this.isIOS() || this.isAndroid() || /Mobi|Mobile/.test(navigator.userAgent);
  },

  // Detect if user is on tablet
  isTablet() {
    return /iPad/.test(navigator.userAgent) || 
           (this.isAndroid() && !/Mobile/.test(navigator.userAgent));
  },

  // Detect if user is on desktop
  isDesktop() {
    return !this.isMobile() && !this.isTablet();
  },

  // Get preferred payment methods based on device
  getPreferredPaymentMethods() {
    const methods = ['card', 'blik']; // Default methods
    
    if (this.isIOS()) {
      // Apple Pay should be first on iOS
      methods.unshift('apple_pay');
    } else if (this.isAndroid()) {
      // Google Pay should be first on Android
      methods.unshift('google_pay');
    }
    
    return methods;
  },

  // Check if Apple Pay is available
  isApplePayAvailable() {
    return this.isIOS() && window.ApplePaySession && ApplePaySession.canMakePayments();
  },

  // Check if Google Pay is available
  isGooglePayAvailable() {
    return this.isAndroid() && window.google && window.google.payments;
  },

  // Get device info for analytics
  getDeviceInfo() {
    return {
      userAgent: navigator.userAgent,
      platform: navigator.platform,
      isIOS: this.isIOS(),
      isAndroid: this.isAndroid(),
      isMobile: this.isMobile(),
      isTablet: this.isTablet(),
      isDesktop: this.isDesktop(),
      screenWidth: window.screen.width,
      screenHeight: window.screen.height,
      viewport: {
        width: window.innerWidth,
        height: window.innerHeight,
      },
    };
  },

  // Get optimal button size based on device
  getButtonSize() {
    if (this.isMobile()) {
      return 'large'; // Larger buttons for touch
    } else {
      return 'medium';
    }
  },

  // Check if device supports vibration
  supportsVibration() {
    return 'vibrate' in navigator;
  },

  // Vibrate device (for feedback)
  vibrate(pattern = 100) {
    if (this.supportsVibration()) {
      navigator.vibrate(pattern);
    }
  },
};