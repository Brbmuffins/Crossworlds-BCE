-- Apply to the auth database before deploying stack-aware item endpoints.
-- Existing items preserve the prior behavior: stackable, 99 per slot.
ALTER TABLE items
  ADD COLUMN stackable TINYINT(1) NOT NULL DEFAULT 1,
  ADD COLUMN max_stack_size INT UNSIGNED NOT NULL DEFAULT 99;

UPDATE items
SET max_stack_size = 1
WHERE stackable = 0;
