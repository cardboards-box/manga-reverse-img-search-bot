CREATE TABLE IF NOT EXISTS guild_configs (
	id TEXT NOT NULL PRIMARY KEY,

	guild_id TEXT NOT NULL UNIQUE,
	message_idiots TEXT,
	message_loading TEXT,
	channels TEXT NOT NULL DEFAULT '[]',
	channels_type INTEGER NOT NULL DEFAULT 0,
	emotes TEXT NOT NULL DEFAULT '[]',
	emotes_enabled INTEGER NOT NULL DEFAULT 1,
	pings_enabled INTEGER NOT NULL DEFAULT 1,

	created_at TEXT NOT NULL,
	updated_at TEXT NOT NULL,
	deleted_at TEXT
);