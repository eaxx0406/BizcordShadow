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

        // Navn på den chat, der skal oprettes. Kan sættes tom for at teste validering.
        [BindProperty]
        public string NewChatName { get; set; } = "Ny chat";

        // Den chat, der tilføjes deltagere til. Udfyldes, når en chat oprettes, men kan også indsættes manuelt.
        [BindProperty]
        public Guid? ChatId { get; set; }

        [BindProperty]
        public string? ChatName { get; set; }

        [BindProperty]
        public Guid NewParticipantUserId { get; set; }

        public string? StatusMessage { get; private set; }
        public bool IsError { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateChat()
        {
            var result = await _gateway.CreateChatAsync(new CreateChatRequest(NewChatName ?? string.Empty, CurrentUser.Id));

            if (result.IsSuccess)
            {
                ChatId = result.Value!.Id;
                ChatName = result.Value.Name;

                // Tag helpers viser værdien fra ModelState frem for modellen, så de gamle postede værdier skal fjernes.
                ModelState.Remove(nameof(ChatId));
                ModelState.Remove(nameof(ChatName));

                SetStatus($"Chat \"{ChatName}\" oprettet.");
            }
            else
            {
                SetError($"Kunne ikke oprette chat. {result.Error}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddParticipant()
        {
            if (ChatId is null)
            {
                SetError("Opret en chat først (eller indsæt et chat-id).");
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
