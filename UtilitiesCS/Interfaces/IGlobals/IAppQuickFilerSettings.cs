namespace UtilitiesCS
{
    public interface IAppQuickFilerSettings
    {
        bool MoveEntireConversation { get; }
        bool SaveAttachments { get; }
        bool SaveEmailCopy { get; }
        bool SavePictures { get; }
    }
}