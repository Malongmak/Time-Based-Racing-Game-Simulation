namespace CSharpSpeedRush.Models
{
    public class Track
    {
        public int TotalLaps { get; } = 5;
        public int CurrentLap { get; private set; } = 1;
        public double LapProgress { get; private set; } = 0;

        public void Advance(double amount)
        {
            LapProgress += amount;
            if (LapProgress >= 100)
            {
                LapProgress = 0;
                CurrentLap++;
            }
        }

        public bool IsRaceFinished => CurrentLap > TotalLaps;
    }
}
// CSharpSpeedRush.Models.Track.cs