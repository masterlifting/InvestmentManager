namespace InvestmentManager.Client.Configurations
{
    public static class EnumConfig
    {
        public enum ColorCustom
        {
            isuccess,
            idanger,
            iwarning,
            iinfo,
            isecondary,
            idark
        }
        public enum ColorBootstrap
        {
            light,
            success,
            danger,
            primary,
            warning,
            info,
            secondary,
            dark,
            muted
        }
        public enum HtmlDataType
        {
            NotSet,
            String,
            Number,
            Date,
            Boolean,
            Currency
        }
        public enum AlignType
        {
            Left,
            Right,
            Center
        }
    }
}
