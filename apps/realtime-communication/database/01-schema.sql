-- RealTimeCommunicationService schema (DUMMY). Én tabel: en primitiv log over modtagne MessageSent-events.

-- message_id er primary key: RabbitMQ lover "mindst én gang", så samme event kan komme to gange.
-- Med message_id som nøgle bliver en dublet bare ignoreret (on conflict do nothing i repositoriet).
create table received_message (
    message_id     uuid primary key,
    chat_id        uuid not null,
    sender_user_id uuid not null,
    recipient_user_ids uuid[] not null, -- dem ChatService sagde skulle have beskeden
    sent_at        timestamptz not null,
    received_at    timestamptz not null
);
