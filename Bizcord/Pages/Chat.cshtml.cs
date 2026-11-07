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

        // Den åbne chat fra URL'en (/Chat/{id}). Null = ingen chat åben, så vises velkomsten.
        [BindProperty(SupportsGet = true, Name = "id")]
        public Guid? OpenChatId { get; set; }

        // Den åbne chat med medlemmer, hentet gennem gatewayen (GET api/chats/{id})
        public ChatDetailsDto? OpenChat { get; private set; }

        // Fejl, hvis den åbne chat ikke kunne hentes (f.eks. 404)
        public string? OpenChatError { get; private set; }

        // Besked fra forrige request (f.eks. "Chat oprettet"), overlever redirect via TempData
        [TempData]
        public string? FlashMessage { get; set; }

        // Ny chat: navn + medlemmer i ét trin (ADR 0012). Navnet kan sættes tomt for at teste validering i ChatService.
        [BindProperty]
        public string NewChatName { get; set; } = string.Empty;

        // De valgte medlemmer (user-id'er) ud over en selv
        [BindProperty]
        public List<Guid> NewChatMemberIds { get; set; } = new();

        // Den chat, der tilføjes deltagere til. Er en chat åben, bruges den; ellers kan et id indsættes manuelt.
        [BindProperty]
        public Guid? ChatId { get; set; }

        [BindProperty]
        public string? ChatName { get; set; }

        [BindProperty]
        public Guid NewParticipantUserId { get; set; }

        public string? StatusMessage { get; private set; }
        public bool IsError { get; private set; }

        // Hvilken dialog der skal åbnes igen efter et mislykket POST, så fejlen kan ses ("create" eller "add")
        public string? OpenDialog { get; private set; }

        public async Task OnGet()
        {
            await LoadOpenChatAsync();
        }

        public async Task<IActionResult> OnPostCreateChat()
        {
            OpenDialog = "create";

            if (NewChatMemberIds.Count == 0)
            {
                SetError("Vælg mindst ét medlem. En chat kun med dig selv er ikke så interessant.");
                await LoadOpenChatAsync();
                return Page();
            }

            var result = await _gateway.CreateChatAsync(
                new CreateChatRequest(NewChatName ?? string.Empty, CurrentUser.Id, NewChatMemberIds));

            if (!result.IsSuccess)
            {
                SetError($"Kunne ikke oprette chat. {result.Error}");
                await LoadOpenChatAsync();
                return Page();
            }

            // Succes: gå direkte til den nye chat (redirect efter POST, så F5 ikke opretter chatten igen)
            FlashMessage = $"Chat \"{result.Value!.Name}\" er oprettet.";
            return RedirectToPage("/Chat", new { id = result.Value.Id });
        }

        public async Task<IActionResult> OnPostAddParticipant()
        {
            OpenDialog = "add";

            if (ChatId is null)
            {
                SetError("Angiv hvilken chat (opret en chat først, eller indsæt et chat-id).");
                await LoadOpenChatAsync();
                return Page();
            }

            var name = TestUsers.NameOf(NewParticipantUserId);
            var result = await _gateway.AddParticipantAsync(ChatId.Value, NewParticipantUserId);

            if (!result.IsSuccess)
            {
                SetError($"Kunne ikke tilføje {name}. {result.Error}");
                await LoadOpenChatAsync();
                return Page();
            }

            // Succes: vis chatten, så den nye deltager kan ses i medlemslisten
            FlashMessage = $"{name} er tilføjet til chatten.";
            return RedirectToPage("/Chat", new { id = ChatId });
        }

        // Henter den chat, der står i URL'en, og forudfylder "Tilføj deltager" med den
        private async Task LoadOpenChatAsync()
        {
            if (OpenChatId is null)
            {
                return;
            }

            var result = await _gateway.GetChatAsync(OpenChatId.Value);
            if (!result.IsSuccess)
            {
                OpenChatError = result.Error;
                return;
            }

            OpenChat = result.Value;

            // Kun hvis brugeren ikke selv har postet et andet chat-id
            if (ChatId is null)
            {
                ChatId = OpenChat!.Id;
                ChatName = OpenChat.Name;
            }
        }

        private void SetError(string message)
        {
            StatusMessage = message;
            IsError = true;
        }
    }
}
