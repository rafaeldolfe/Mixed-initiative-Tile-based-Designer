namespace ExpressiveRange
{
    public class Sample
    {
        public Level level;
        public double leniency;
        public double linearity;

        public override string ToString()
        {
            return $"Rooms: {level.rooms.Count}, leniency: {leniency}, linearity: {linearity}";
        }
    }
}
