CREATE TABLE IF NOT EXISTS lookup_interactions (
	id TEXT NOT NULL PRIMARY KEY,

	message_id TEXT NOT NULL,
	user_id TEXT NOT NULL,

	created_at TEXT NOT NULL,
	updated_at TEXT NOT NULL,
	deleted_at TEXT,

	UNIQUE(message_id, user_id)
);