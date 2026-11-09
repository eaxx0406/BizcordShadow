namespace ChatService.Contracts
{
    // Ordforrådet for beskeder (ADR 0022). Strengene står i databasen og i ChatMessageDto,
    // så ChatService og klienterne skal bruge præcis de samme værdier.
    public static class MessageKinds
    {
        public const string User = "user";       // en person skrev beskeden
        public const string System = "system";   // noget skete i chatten (se SystemMessageTypes)
    }

    public static class SystemMessageTypes
    {
        public const string MemberAdded = "member_added";   // afsenderen blev tilføjet
        public const string MemberLeft = "member_left";     // afsenderen forlod chatten
        public const string ChatRenamed = "chat_renamed";   // afsenderen omdøbte chatten. Details: oldName, newName
    }
}
