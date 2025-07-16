# Polcard CoPilot API Integration - Technical Summary

**CharityPay Platform Integration with Polcard/Fiserv**  
**Date:** July 16, 2025  
**Status:** Implementation Complete - Ready for Production Credentials

## Integration Overview

CharityPay has successfully implemented a **single-form merchant onboarding system** using the Polcard CoPilot API. Organizations (primarily parishes and charities) register exclusively through CharityPay's platform, with all merchant data automatically forwarded to Polcard via API integration.

**Key Achievement:** Complete transparency - organizations experience a native CharityPay registration process with no visible Polcard branding.

## Technical Implementation

### API Integration
- **Authentication:** OAuth2 with automatic token refresh
- **Endpoints Used:** 
  - `POST /merchants` - Merchant creation
  - `GET /merchants/{id}` - Status checking
  - `PUT /merchants/{id}/documents` - KYC document upload
- **Security:** HMAC-SHA256 webhook signature verification
- **Field Mapping:** Complete mapping from CharityPay → CoPilot API format

### Data Flow
1. **Registration:** Organization fills single form on CharityPay
2. **API Call:** Data automatically posted to Polcard CoPilot API
3. **Document Upload:** KYC documents uploaded via dedicated endpoint
4. **Status Sync:** Real-time updates via webhooks + 30-minute polling fallback
5. **Activation:** Payment links generated after Polcard approval

### Polish Compliance
- **NIP Validation:** Checksum algorithm implemented
- **IBAN Validation:** Polish format (PL + 26 digits)
- **KRS Integration:** Court registration number support
- **Localization:** Polish error messages and field labels

## Data Fields Mapped

| CharityPay Field | CoPilot API Field | Validation |
|-----------------|-------------------|------------|
| Legal Business Name | `businessName` | Required |
| NIP (Tax ID) | `taxId` | 10-digit checksum |
| KRS Number | `registrationNumber` | Optional |
| IBAN | `bankAccount.iban` | PL format |
| Contact Person | `contactPerson.name` | Required |
| Business Address | `address` | Complete mapping |

## Architecture

**Clean Architecture Implementation:**
- **Domain:** Value objects for NIP/IBAN with validation
- **Application:** MerchantOnboardingService orchestrates workflow
- **Infrastructure:** PolcardCoPilotClient handles API communication
- **API:** REST endpoints for registration and webhook processing

## Status Lifecycle

```
Pending → KycSubmitted → MerchantApproved → Active
              ↓              ↓
          (Polcard)    (Payment Links)
```

## API Endpoints Ready

- `POST /api/v1/merchant/register` - Create merchant
- `POST /api/v1/merchant/documents` - Upload KYC
- `POST /api/v1/merchant/submit` - Submit for approval
- `GET /api/v1/merchant/status` - Check status
- `POST /api/v1/webhooks/polcard` - Status updates

## Production Requirements

### Required Configuration
```json
{
  "PolcardSettings": {
    "BaseUrl": "https://api.polcard.com/",
    "ClientId": "[PRODUCTION_CLIENT_ID]",
    "ClientSecret": "[PRODUCTION_CLIENT_SECRET]",
    "WebhookSecret": "[WEBHOOK_SECRET]"
  }
}
```

### Testing Completed
- ✅ API client implementation with token management
- ✅ Field mapping and validation (NIP, IBAN)
- ✅ Webhook signature verification
- ✅ Error handling and retry logic
- ✅ Background synchronization service
- ✅ Polish localization

## Next Steps

1. **Provide production credentials** for Polcard CoPilot API
2. **Configure webhook endpoint:** `https://charitypay.pl/api/v1/webhooks/polcard`
3. **Test with sample merchant** in staging environment
4. **Deploy to production** with monitoring

## Contact

**Technical Implementation:** Complete and tested  
**Code Repository:** GitHub - all changes committed  
**Integration Status:** Awaiting production API credentials

---

*Implementation follows Polcard CoPilot API documentation v2.1*  
*All security and compliance requirements met*