namespace SelfUpdatingApp
{
    public class ProgressData
    {
        internal ProgressData(string status) : this(status, 0, false) { }

        internal ProgressData(string status, int percent) : this(status, percent, false) { }

        internal ProgressData(string status, int percent, bool done)
        {
            Status = status;
            Percent = percent;
            Done = done;
        }

        public int Percent { get; }
        public string Status { get; }
        public bool Done { get; }
    }
}
