namespace NetAirflow.Web.Comun
{
    public class CronJob
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ExpresionCron { get; set; } = string.Empty;
        public string? OnSuccesed { get; set; }
        public string? OnCanceled { get; set; }
        public string? OnError { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime? DateModify { get; set; }
        public DateTime? DateSuccesed { get; set; }
        public DateTime? DateCanceled { get; set; }
        public DateTime? DateError { get; set; }
    }
}
