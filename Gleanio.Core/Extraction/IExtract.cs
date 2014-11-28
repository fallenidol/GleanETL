namespace Gleanio.Core.Extraction
{
    public interface IExtract
    {
        #region Methods

        void AfterExtract();

        void BeforeExtract();

        void ExtractToTarget();

        #endregion Methods
    }
}