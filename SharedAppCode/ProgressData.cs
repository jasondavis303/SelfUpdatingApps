namespace SelfUpdatingApp
{
    public class ProgressData
    {
        public ProgressData(string status)
        {
            Status = status;
        }

        public ProgressData(string status, int percent)
        {
            Status = status;
            Percent = percent;
        }

        public int Percent { get; set; }
        public string Status { get; set; }
    }
}
