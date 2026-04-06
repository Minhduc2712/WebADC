namespace ErpOnlineOrder.Application.Constants
{
    public static class SettingKeys
    {
        // SMTP
        public const string SmtpHost      = "SMTP_HOST";
        public const string SmtpPort      = "SMTP_PORT";
        public const string SmtpUseSsl    = "SMTP_USE_SSL";
        public const string SmtpFromName  = "SMTP_FROM_NAME";
        public const string SmtpFromEmail = "SMTP_FROM_EMAIL";
        public const string SmtpPassword  = "SMTP_PASSWORD";

        // Company info (dùng cho in ấn)
        public const string CompanyName    = "COMPANY_NAME";
        public const string CompanyAddress = "COMPANY_ADDRESS";
        public const string CompanyPhone   = "COMPANY_PHONE";
        public const string CompanyEmail   = "COMPANY_EMAIL";
    }
}
