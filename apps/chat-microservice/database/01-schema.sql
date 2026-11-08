-- ChatService schema (database first). Postgres-konvention: små bogstaver + snake_case, ingen anførselstegn.

create table chat (
    id   uuid primary key,
    name text not null
);

-- Read model af en User fra UserService (source of truth). 1:1 med user -> user_id er unik.
create table participant (
    id      uuid primary key,
    user_id uuid not null unique,
    name    text not null
);

create table participation (
    id             uuid primary key,
    chat_id        uuid not null references chat(id) on delete restrict,
    participant_id uuid not null references participant(id) on delete restrict,
    joined_at      timestamptz not null,
    left_at        timestamptz
);

-- Kun ét aktivt medlemskab per participant per chat
create unique index ux_participation_active
    on participation (chat_id, participant_id)
    where left_at is null;

create table message (
    id        uuid primary key,
    chat_id   uuid not null references chat(id) on delete restrict,
    sender_id uuid not null references participation(id) on delete restrict,
    content   text not null,
    sent_at   timestamptz not null,
    -- Soft delete (ADR 0020): sat, når afsenderen har slettet beskeden. content er så tom.
    deleted_at timestamptz
);

-- Keyset paging (ADR 0014): "de nyeste beskeder i en chat, ældre end X" bliver et opslag i dette index,
-- uanset hvor mange beskeder chatten har. Bruges også af "seneste besked" i listen over mine chats.
create index ix_message_chat_sent
    on message (chat_id, sent_at desc, id desc);

create table message_receipt (
    message_id   uuid not null references message(id) on delete restrict,
    receiver_id  uuid not null references participant(id) on delete restrict,
    delivered_at timestamptz,
    seen_at      timestamptz,
    primary key (message_id, receiver_id)
);
