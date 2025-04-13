namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public static class HttpJobKeys
    {
        public const string PropertyRequestAction = "requestAction";
        public const string PropertyRequestUrl = "requestUrl";
        public const string PropertyRequestParameters = "requestParams";
        public const string PropertyRequestHeaders = "requestHeaders";
        public const string PropertyIgnoreVerifySsl = "ignoreSsl";
        /// <summary>
        /// HTTP request timeout. Negative value to indicate infinite timeout.
        /// </summary>
        public const string PropertyRequestTimeoutInSec = "requestTimeout";
    }

}

