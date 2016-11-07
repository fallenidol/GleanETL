namespace Glean.Core.Enumerations
{
    public enum WhiteSpaceHandling
    {
        DefaultDoNothing = 0,

        TrimLeadingAndTrailingWhiteSpace = 1,

        RemoveAllWhiteSpace = 2,

        TrimAndRemoveConsecutiveWhiteSpace = 3
    }
}