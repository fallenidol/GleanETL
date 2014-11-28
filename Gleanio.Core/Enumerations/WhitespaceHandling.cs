namespace Gleanio.Core.Enumerations
{
    #region Enumerations

    public enum WhitespaceHandling
    {
        DefaultDoNothing = 0,
        TrimLeadingAndTrailingWhitespace = 1,
        RemoveAllWhitespace = 2,
        TrimAndRemoveConsecutiveWhitespace = 3
    }

    #endregion Enumerations
}