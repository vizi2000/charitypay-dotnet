# Changelog

All notable changes to the CharityPay .NET project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Polcard/Fiserv Integration**:
  - Complete implementation of Polcard CoPilot client for merchant onboarding
  - Document upload support for KYC verification
  - Webhook controller for handling Polcard events
  - Comprehensive request/response models for API integration
- **Database Seeding**:
  - DatabaseSeeder class with comprehensive test data
  - Automatic seeding on application startup in development
  - Sample organizations, users, and payments for testing
- **Frontend Improvements**:
  - Fixed API base URL configuration
  - Improved error handling in API client
  - Better integration with backend endpoints
- Created missing `OrganizationCategory` enum with values: Religia, Dzieci, Zwierzeta, Edukacja, Zdrowie, Inne
- Implemented comprehensive TestDataBuilder infrastructure for tests:
  - `UserBuilder` for creating test users
  - `OrganizationBuilder` for creating test organizations  
  - `PaymentBuilder` for creating test payments
- Added automatic database migration on startup in development environment
- Added database schema creation fallback using `EnsureCreatedAsync()` for development

### Fixed
- **Frontend-Backend Integration**:
  - Fixed CORS issues preventing frontend API calls
  - Corrected API base URL from 5001 to 8081
  - Resolved authentication context issues
  - Fixed organization data loading from PostgreSQL
- Fixed entity configuration default values to use proper enum types instead of strings:
  - `OrganizationStatus.Pending` instead of "pending" in OrganizationConfiguration
  - `PaymentStatus.Pending` instead of "pending" in PaymentConfiguration
  - Fixed duplicate configuration in CharityPayDbContext
- Fixed test method names from `GetOrganizationByUserIdAsync` to `GetMyOrganizationAsync`
- Fixed all references from `ApplicationDbContext` to `CharityPayDbContext` in test files
- Fixed compilation errors in `AuthDemoController` with proper null checks and type casting
- Fixed admin organizations endpoint returning 500 errors
- Resolved database table creation issues during development

### Changed
- Migrated organization data from JSON files to PostgreSQL database
- Updated quick-test.sh to properly check for JSON response in health endpoint
- Modified Program.cs to include database initialization logic
- Replaced string default values with proper enum values in database migrations
- Enhanced security headers middleware configuration

### Documentation
- Created comprehensive SUMMARY.md with current project status
- Updated CHANGELOG.md with detailed recent changes
- Added "Remaining TODOs from Code Review" section to TASK.md documenting:
  - Unimplemented refresh token functionality
  - Pending Fiserv payment gateway integration
  - Missing admin statistics implementation
- Updated TASK.md with completed code review fixes
- Marked numerous completed tasks in TASK.md

## [0.1.0] - 2025-07-14

### Added
- Initial .NET 8 project structure with Clean Architecture
- Domain entities (User, Organization, Payment) with enums
- Entity Framework Core configuration with PostgreSQL
- Basic API controllers and demo endpoints
- React TypeScript frontend with Vite
- Docker compose setup for local development
- JWT authentication scaffolding
- Repository pattern implementation
- Initial database migrations