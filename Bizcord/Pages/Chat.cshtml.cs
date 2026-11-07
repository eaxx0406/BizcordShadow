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
        private readonly CurrentUser _currentUser;

        public ChatModel(GatewayClient gateway, CurrentUser currentUser)
        {
            _gateway = gateway;
            _currentUser = currentUser;
        }

        public string UserName => _currentUser.Name;
        public Guid UserId => _currentUser.Id;

        // Chatlisten i venstre side, hentet fra ChatService (GET api/chats?userId=...)
        public IReadOnlyList<ChatPreview> Chats { get; private set; } = [];

        // Fejl, hvis listen ikke kunne hentes (f.eks. hvis gatewayen ikke kører)
        public string? ChatsError { get; private set; }

        // Brugere, der kan tilføjes (alle testbrugere undtagen en selv)
        public IReadOnlyList<TestUser> OtherUsers => TestUsers.All.Where(u => u.Id != _currentUser.Id).ToList();

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

        // Beskeden i skrivefeltet. Tom eller kun mellemrum sendes ikke ("." er fint).
        [BindProperty]
        public string? NewMessage { get; set; }

        public string? StatusMessage { get; private set; }
        public bool IsError { get; private set; }

        // Hvor fejlen fra et mislykket POST skal vises: dialogen "create" eller "add", eller skrivefeltet "message"
        public string? OpenDialog { get; private set; }

        public async Task OnGet()
        {
            await LoadPageAsync();
        }

        public async Task<IActionResult> OnPostCreateChat()
        {
            OpenDialog = "create";

            if (NewChatMemberIds.Count == 0)
            {
                SetError("Vælg mindst ét medlem. En chat kun med dig selv er ikke så interessant.");
                await LoadPageAsync();
                return Page();
            }

            var result = await _gateway.CreateChatAsync(
                new CreateChatRequest(NewChatName ?? string.Empty, _currentUser.Id, NewChatMemberIds));

            if (!result.IsSuccess)
            {
                SetError($"Kunne ikke oprette chat. {result.Error}");
                await LoadPageAsync();
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
                await LoadPageAsync();
                return Page();
            }

            var name = TestUsers.NameOf(NewParticipantUserId);
            var result = await _gateway.AddParticipantAsync(ChatId.Value, NewParticipantUserId);

            if (!result.IsSuccess)
            {
                SetError($"Kunne ikke tilføje {name}. {result.Error}");
                await LoadPageAsync();
                return Page();
            }

            // Succes: vis chatten, så den nye deltager kan ses i medlemslisten
            FlashMessage = $"{name} er tilføjet til chatten.";
            return RedirectToPage("/Chat", new { id = ChatId });
        }

        public async Task<IActionResult> OnPostSendMessage()
        {
            OpenDialog = "message";

            if (OpenChatId is null)
            {
                return RedirectToPage("/Chat");
            }

            // Sendknappen er slået fra, når feltet er tomt, men tjek alligevel (JavaScript kan være slået fra)
            if (string.IsNullOrWhiteSpace(NewMessage))
            {
                SetError("Beskeden er tom.");
                await LoadPageAsync();
                return Page();
            }

            var result = await _gateway.SendMessageAsync(OpenChatId.Value, new SendMessageRequest(_currentUser.Id, NewMessage));

            if (!result.IsSuccess)
            {
                // Beholder teksten i feltet, så den ikke går tabt
                SetError($"Kunne ikke sende beskeden. {result.Error}");
                await LoadPageAsync();
                return Page();
            }

            // Succes: tilbage til chatten med et tomt skrivefelt (beskederne vises her, når GetMessages findes)
            FlashMessage = "Beskeden er sendt.";
            return RedirectToPage("/Chat", new { id = OpenChatId });
        }

        // Henter det, siden viser: chatlisten og (hvis der er en i URL'en) den åbne chat
        private async Task LoadPageAsync()
        {
            await LoadChatsAsync();
            await LoadOpenChatAsync();
        }

        private async Task LoadChatsAsync()
        {
            var result = await _gateway.GetMyChatsAsync(_currentUser.Id);
            if (!result.IsSuccess)
            {
                ChatsError = result.Error;
                return;
            }

            Chats = result.Value!.Select(ChatPreview.From).ToList();
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
