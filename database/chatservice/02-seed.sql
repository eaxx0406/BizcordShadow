-- Testdata indtil UserService findes og kan levere participants (read model) via events.
-- Matcher den hardcodede bruger i BizcordShadow/Models/CurrentUser.cs.
-- OBS: id = user_id her med vilje, så den nuværende CreateChatHandler (som sender user-id som participant-id) kan testes.
insert into participant (id, user_id, name)
values ('11111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111', 'Test Bruger');
