namespace Glean.Core.EventArgs
{
    using System;

    public class ExtractCompleteArgs : EventArgs
    {
        public ExtractCompleteArgs(
            long linesExtractedFromSource,
            long linesPassedToTarget,
            long linesCommittedToTarget,
            long extractDurationMs,
            long commitDurationMs,
            long durationInMs)
        {
            DurationInMs = durationInMs;
            CommitDurationMs = commitDurationMs;
            ExtractDurationMs = extractDurationMs;
            LinesCommittedToTarget = linesCommittedToTarget;
            LinesPassedToTarget = linesPassedToTarget;
            LinesExtractedFromSource = linesExtractedFromSource;
        }

        public long LinesExtractedFromSource { get; }

        public long LinesPassedToTarget { get; }

        public long LinesCommittedToTarget { get; }

        public long ExtractDurationMs { get; }

        public long CommitDurationMs { get; }

        public long DurationInMs { get; }

        public override string ToString()
        {
            return string.Format(
                "Total Duration={1}ms; Extract Duration={2}ms; Commit Duration={0}ms; Lines Extracted={3}, Lines Passed={4}; Lines Committed={5};",
                CommitDurationMs,
                DurationInMs,
                ExtractDurationMs,
                LinesExtractedFromSource,
                LinesPassedToTarget,
                LinesCommittedToTarget);
        }
    }
}