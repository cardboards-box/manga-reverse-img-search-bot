CREATE TABLE IF NOT EXISTS guild_configs (
	id TEXT NOT NULL PRIMARY KEY,

	guild_id TEXT NOT NULL UNIQUE,
	message_idiots TEXT,
	message_loading TEXT,
	message_download_failed TEXT,
	message_no_results TEXT,
	message_succeeded TEXT,
	message_error TEXT,
	channels_whitelist TEXT NOT NULL DEFAULT '[]',
	channels_blacklist TEXT NOT NULL DEFAULT '[]',
	emotes TEXT NOT NULL DEFAULT '[]',
	emotes_enabled INTEGER NOT NULL DEFAULT 1,
	pings_enabled INTEGER NOT NULL DEFAULT 1,

	created_at TEXT NOT NULL,
	updated_at TEXT NOT NULL,
	deleted_at TEXT
);