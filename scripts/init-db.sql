-- init-db.sql - Database initialization script for development

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create development database if it doesn't exist
SELECT 'CREATE DATABASE charitypay_dev'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'charitypay_dev')\gexec

-- Connect to the development database
\c charitypay_dev;

-- Create schemas
CREATE SCHEMA IF NOT EXISTS public;
CREATE SCHEMA IF NOT EXISTS audit;

-- Set default privileges
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO postgres;
ALTER DEFAULT PRIVILEGES IN SCHEMA audit GRANT SELECT, INSERT ON TABLES TO postgres;

-- Create audit trigger function for change tracking
CREATE OR REPLACE FUNCTION audit.audit_trigger()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO audit.audit_log (
            table_name,
            operation,
            new_values,
            changed_by,
            changed_at
        ) VALUES (
            TG_TABLE_NAME,
            TG_OP,
            row_to_json(NEW),
            current_user,
            NOW()
        );
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO audit.audit_log (
            table_name,
            operation,
            old_values,
            new_values,
            changed_by,
            changed_at
        ) VALUES (
            TG_TABLE_NAME,
            TG_OP,
            row_to_json(OLD),
            row_to_json(NEW),
            current_user,
            NOW()
        );
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO audit.audit_log (
            table_name,
            operation,
            old_values,
            changed_by,
            changed_at
        ) VALUES (
            TG_TABLE_NAME,
            TG_OP,
            row_to_json(OLD),
            current_user,
            NOW()
        );
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create audit log table
CREATE TABLE IF NOT EXISTS audit.audit_log (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    table_name TEXT NOT NULL,
    operation TEXT NOT NULL,
    old_values JSONB,
    new_values JSONB,
    changed_by TEXT NOT NULL DEFAULT current_user,
    changed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Create index for performance
CREATE INDEX IF NOT EXISTS idx_audit_log_table_name ON audit.audit_log(table_name);
CREATE INDEX IF NOT EXISTS idx_audit_log_changed_at ON audit.audit_log(changed_at);
CREATE INDEX IF NOT EXISTS idx_audit_log_operation ON audit.audit_log(operation);

-- Create function to add audit trigger to a table
CREATE OR REPLACE FUNCTION audit.add_audit_trigger(target_table TEXT)
RETURNS VOID AS $$
BEGIN
    EXECUTE format('
        CREATE TRIGGER %I_audit_trigger
        AFTER INSERT OR UPDATE OR DELETE ON %I
        FOR EACH ROW EXECUTE FUNCTION audit.audit_trigger();
    ', target_table, target_table);
END;
$$ LANGUAGE plpgsql;

-- Create development user (if needed for application)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_user WHERE usename = 'charitypay_dev') THEN
        CREATE USER charitypay_dev WITH PASSWORD 'dev_password_123';
        GRANT CONNECT ON DATABASE charitypay_dev TO charitypay_dev;
        GRANT USAGE ON SCHEMA public TO charitypay_dev;
        GRANT USAGE ON SCHEMA audit TO charitypay_dev;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO charitypay_dev;
        GRANT SELECT ON ALL TABLES IN SCHEMA audit TO charitypay_dev;
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO charitypay_dev;
        ALTER DEFAULT PRIVILEGES IN SCHEMA audit GRANT SELECT ON TABLES TO charitypay_dev;
    END IF;
END
$$;

-- Create application configuration table
CREATE TABLE IF NOT EXISTS public.app_configuration (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    key TEXT UNIQUE NOT NULL,
    value TEXT NOT NULL,
    description TEXT,
    is_sensitive BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Insert default configuration values
INSERT INTO public.app_configuration (key, value, description, is_sensitive) VALUES
    ('app.name', 'CharityPay', 'Application name', FALSE),
    ('app.version', '1.0.0', 'Application version', FALSE),
    ('app.environment', 'Development', 'Current environment', FALSE),
    ('features.registration_enabled', 'true', 'Enable organization registration', FALSE),
    ('features.payment_enabled', 'true', 'Enable payment processing', FALSE),
    ('limits.max_payment_amount', '100000', 'Maximum payment amount in PLN', FALSE),
    ('limits.min_payment_amount', '5', 'Minimum payment amount in PLN', FALSE)
ON CONFLICT (key) DO NOTHING;

-- Create updated_at trigger function
CREATE OR REPLACE FUNCTION public.update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply updated_at trigger to app_configuration
CREATE TRIGGER app_configuration_updated_at_trigger
    BEFORE UPDATE ON public.app_configuration
    FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();

-- Add audit trigger to app_configuration
SELECT audit.add_audit_trigger('app_configuration');

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_app_configuration_key ON public.app_configuration(key);
CREATE INDEX IF NOT EXISTS idx_app_configuration_created_at ON public.app_configuration(created_at);

-- Display initialization summary
\echo '✅ Database initialization completed successfully!'
\echo '📊 Database: charitypay_dev'
\echo '👤 User: postgres (superuser), charitypay_dev (app user)'
\echo '🗂️  Schemas: public (application), audit (audit logs)'
\echo '⚙️  Configuration: app_configuration table initialized'
\echo '🔍 Audit: audit triggers and logging enabled'