-- Fix column type mismatch in OrganizationInformations table
-- Tax_number and Recipient_phone were incorrectly created as INT,
-- but the domain model and application code treat them as strings.

-- Step 1: Convert existing INT values to NVARCHAR (preserve data)
ALTER TABLE OrganizationInformations
    ALTER COLUMN Tax_number NVARCHAR(50) NOT NULL;
GO

ALTER TABLE OrganizationInformations
    ALTER COLUMN Recipient_phone NVARCHAR(20) NOT NULL;
GO
