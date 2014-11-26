using gleanio.framework.Source;
using gleanio.framework.Target;

namespace gleanio.framework.Extraction
{
    public abstract class Extract : IExtract
    {
        #region Constructors

        protected Extract(TextFile source, IExtractTarget target)
        {
            Source = source;
            Target = target;
        }

        #endregion Constructors

        #region Properties

        public TextFile Source
        {
            get; private set;
        }

        public IExtractTarget Target
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        public abstract void ExtractToTarget();

        #endregion Methods
    }
}