-- Testdata indtil UserService findes og kan levere participants (read model) via events.
-- Matcher de hardcodede brugere i Bizcord/Models/CurrentUser.cs og Bizcord/Models/TestUsers.cs.
-- id (participant) er med vilje FORSKELLIG fra user_id, så det kan ses, at oversættelsen user-id -> participant-id virker.
-- "Ukendt bruger" (9999...) er med vilje IKKE med, så fejlen "bruger findes ikke" kan testes.
insert into participant (id, user_id, name)
values
    ('aaaaaaaa-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111111', 'Test Bruger'),
    ('aaaaaaaa-0000-0000-0000-000000000002', '22222222-2222-2222-2222-222222222222', 'Anna'),
    ('aaaaaaaa-0000-0000-0000-000000000003', '33333333-3333-3333-3333-333333333333', 'Bo'),
    ('aaaaaaaa-0000-0000-0000-000000000004', '44444444-4444-4444-4444-444444444444', 'Carla');
