namespace sfa.sale.generator.web.Configuration
{
    public class AppSettings
    {
        public bool UseWorker { get; set; }
        public bool UseDatabase { get; set; }

        public string ConnectionString { get; set; }

        public string WorkDirectory { get; set; }
    }
}