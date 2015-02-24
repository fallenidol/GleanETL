namespace Gleanio.Core.EventArgs
{
    public class ExtractCompleteArgs : System.EventArgs
    {
        public override string ToString()
        {
            return string.Format("Total Duration={1}ms; Extract Duration={2}ms; Commit Duration={0}ms; Lines Extracted={3}, Lines Passed={4}; Lines Committed={5};", this.CommitDurationMs, this.DurationInMs, this.ExtractDurationMs, this.LinesExtractedFromSource, this.LinesPassedToTarget, this.LinesCommittedToTarget);
        }

        #region Constructors

        public ExtractCompleteArgs(long linesExtractedFromSource, long linesPassedToTarget, long linesCommittedToTarget,
            long extractDurationMs, long commitDurationMs, long durationInMs)
        {
            DurationInMs = durationInMs;
            CommitDurationMs = commitDurationMs;
            ExtractDurationMs = extractDurationMs;
            LinesCommittedToTarget = linesCommittedToTarget;
            LinesPassedToTarget = linesPassedToTarget;
            LinesExtractedFromSource = linesExtractedFromSource;
        }

        #endregion Constructors

        public long LinesExtractedFromSource { get; private set; }
        public long LinesPassedToTarget { get; private set; }
        public long LinesCommittedToTarget { get; private set; }
        public long ExtractDurationMs { get; private set; }
        public long CommitDurationMs { get; private set; }
        public long DurationInMs { get; private set; }
    }
}