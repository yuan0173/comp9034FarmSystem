-- Frontend-Backend Alignment Migration Script
-- This script aligns the backend database with frontend expectations
-- Created: 2025-08-28
-- Purpose: Ensure consistent field naming between frontend and backend

-- ==============================================
-- Phase 1: Add new AdminId column to Events table
-- ==============================================

BEGIN TRANSACTION;

-- Add AdminId column to Events table
ALTER TABLE Events ADD COLUMN AdminId INTEGER;

-- Add foreign key constraint for AdminId (references Staffs table)
-- Note: In SQLite, we cannot add foreign key constraints after table creation
-- We'll handle this in the application logic

-- Add index for AdminId for better query performance
CREATE INDEX IF NOT EXISTS IX_Events_AdminId ON Events(AdminId);

COMMIT;

-- ==============================================
-- Phase 2: Create view for frontend compatibility
-- ==============================================

BEGIN TRANSACTION;

-- Drop existing view if it exists
DROP VIEW IF EXISTS v_EventsAligned;

-- Create a view that maps backend fields to frontend expected names
CREATE VIEW v_EventsAligned AS
SELECT 
    Id as eventId,
    StaffId as staffId,
    DeviceId as deviceId,
    AdminId as adminId,
    EventType as eventType,
    Timestamp as timeStamp,
    Notes as reason,
    CreatedAt as createdAt
FROM Events;

COMMIT;

-- ==============================================
-- Phase 3: Update existing data (if needed)
-- ==============================================

BEGIN TRANSACTION;

-- Set default AdminId for existing events (optional)
-- You can set this to a specific admin ID or leave as NULL
UPDATE Events 
SET AdminId = NULL 
WHERE AdminId IS NULL;

COMMIT;

-- ==============================================
-- Verification Queries
-- ==============================================

-- Check the schema after migration
PRAGMA table_info(Events);

-- Verify the view works correctly
SELECT * FROM v_EventsAligned LIMIT 5;

-- Check data integrity
SELECT 
    COUNT(*) as total_events,
    COUNT(DISTINCT StaffId) as unique_staff,
    COUNT(DISTINCT DeviceId) as unique_devices,
    COUNT(AdminId) as events_with_admin
FROM Events;

-- ==============================================
-- Rollback Plan (if needed)
-- ==============================================

/*
-- To rollback this migration:

BEGIN TRANSACTION;

-- Drop the view
DROP VIEW IF EXISTS v_EventsAligned;

-- Drop the index
DROP INDEX IF EXISTS IX_Events_AdminId;

-- Remove the AdminId column (Note: SQLite doesn't support DROP COLUMN)
-- You would need to recreate the table without AdminId column
-- This is a complex operation, so plan carefully

COMMIT;
*/

-- ==============================================
-- Success Message
-- ==============================================

SELECT 'Frontend-Backend alignment migration completed successfully!' as Status;
