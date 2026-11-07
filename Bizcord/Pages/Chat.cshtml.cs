using Bizcord.Models;
using Bizcord.Services;
using ChatService.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bizcord.Pages
{
    public class ChatModel : PageModel
    {
        private readonly GatewayClient _gateway;

        public ChatModel(GatewayClient gateway)
        {
            _gateway = gateway;
        }

        public string UserName => CurrentUser.Name;
        public Guid UserId => CurrentUser.Id;

        // Chatlisten i venstre side. Dummydata, indtil ChatService kan returnere "mine chats".
        public IReadOnlyList<ChatPreview> Chats => DummyChats.All;

        // Brugere, der kan tilføjes (alle testbrugere undtagen en selv)
        public IReadOnlyList<TestUser> OtherUsers => TestUsers.All.Where(u => u.Id != CurrentUser.Id).ToList();

        // Ny chat: navn + medlemmer i ét trin (ADR 0012). Navnet kan sættes tomt for at teste validering i ChatService.
        [BindProperty]
        public string NewChatName { get; set; } = string.Empty;

        // De valgte medlemmer (user-id'er) ud over en selv
        [BindProperty]
        public List<Guid> NewChatMemberIds { get; set; } = new();

        // Den chat, der tilføjes deltagere til senere. Udfyldes, når en chat oprettes, men kan også indsættes manuelt.
        [BindProperty]
        public Guid? ChatId { get; set; }

        [BindProperty]
        public string? ChatName { get; set; }

        [BindProperty]
        public Guid NewParticipantUserId { get; set; }

        public string? StatusMessage { get; private set; }
        public bool IsError { get; private set; }

        // Hvilken dialog der skal åbnes igen efter et POST, så resultatet kan ses ("create" eller "add")
        public string? OpenDialog { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateChat()
        {
            OpenDialog = "create";

            if (NewChatMemberIds.Count == 0)
            {
                SetError("Vælg mindst ét medlem. En chat kun med dig selv er ikke så interessant.");
                return Page();
            }

            var result = await _gateway.CreateChatAsync(
                new CreateChatRequest(NewChatName ?? string.Empty, CurrentUser.Id, NewChatMemberIds));

            if (result.IsSuccess)
            {
                ChatId = result.Value!.Id;
                ChatName = result.Value.Name;

                // Tag helpers viser værdien fra ModelState frem for modellen, så de gamle postede værdier skal fjernes.
                ModelState.Remove(nameof(ChatId));
                ModelState.Remove(nameof(ChatName));

                var members = string.Join(", ", NewChatMemberIds.Select(TestUsers.NameOf));
                SetStatus($"Chat \"{ChatName}\" oprettet med {members}.");

                // Tøm formularen, så den er klar til en ny chat
                NewChatName = string.Empty;
                NewChatMemberIds.Clear();
                ModelState.Remove(nameof(NewChatName));
            }
            else
            {
                SetError($"Kunne ikke oprette chat. {result.Error}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddParticipant()
        {
            OpenDialog = "add";

            if (ChatId is null)
            {
                SetError("Angiv hvilken chat (opret en chat først, eller indsæt et chat-id).");
                return Page();
            }

            var name = TestUsers.NameOf(NewParticipantUserId);
            var result = await _gateway.AddParticipantAsync(ChatId.Value, NewParticipantUserId);

            if (result.IsSuccess)
            {
                SetStatus($"{name} er tilføjet til chatten.");
            }
            else
            {
                SetError($"Kunne ikke tilføje {name}. {result.Error}");
            }

            return Page();
        }

        private void SetStatus(string message)
        {
            StatusMessage = message;
            IsError = false;
        }

        private void SetError(string message)
        {
            StatusMessage = message;
            IsError = true;
        }
    }
}
